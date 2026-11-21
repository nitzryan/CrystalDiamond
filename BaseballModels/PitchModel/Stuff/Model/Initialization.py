import math
from types import SimpleNamespace
from typing import Callable

import torch
import torch.nn as nn

from PitchModel.Constants import INIT_PITCH_RESULT_RATES, INIT_INPLAY_BUCKET_RATES
from PitchModel.Stuff.DataPrep.DataPrep import DataPrep
from PitchModel.Stuff.Model.ModelOutputType import ModelOutputType

OUTPUT_WEIGHT_SCALE = 0.1

def GetActivationGain(activation_function : Callable[[torch.Tensor], torch.Tensor]) -> float:
    """1 / sqrt(E[f(z)^2]) for z ~ N(0, 1), the multiplier that keeps a layer fed by f(z) at unit scale."""
    z = torch.linspace(-8.0, 8.0, 4001, dtype=torch.float64)
    density = torch.exp(-0.5 * z * z) / math.sqrt(2.0 * math.pi)
    with torch.no_grad():
        second_moment = torch.trapezoid(activation_function(z) ** 2 * density, z)
    return 1.0 / math.sqrt(float(second_moment))

def GetBaseRates(model_output_type : ModelOutputType) -> torch.Tensor:
    """Probability of each class of an output head at the base rates in Constants.py."""
    # Class indices come from DataPrep's own label functions so they cannot drift from the labels
    match model_output_type:
        case ModelOutputType.Result:
            rates = torch.zeros(4, dtype=torch.float64)
            for result, rate in INIT_PITCH_RESULT_RATES.items():
                rates[DataPrep.GetOutputType(SimpleNamespace(Result=result))] += rate
        case ModelOutputType.SwingResults:
            rates = torch.zeros(3, dtype=torch.float64)
            for result, rate in INIT_PITCH_RESULT_RATES.items():
                bucket, was_swing = DataPrep.GetSwingBucket(SimpleNamespace(Result=result))
                if was_swing:
                    rates[bucket] += rate
        case ModelOutputType.InPlay:
            rates = torch.tensor(INIT_INPLAY_BUCKET_RATES, dtype=torch.float64)
    
    return rates / rates.sum()

def InitializeNetwork(
        layers : nn.ModuleList,
        activation_function : Callable[[torch.Tensor], torch.Tensor],
        model_output_type : ModelOutputType) -> None:
    """layers = [input Linear, ResnetBlock x N, output Linear]"""
    input_linear = layers[0]
    blocks = list(layers)[1:-1]
    output_linear = layers[-1]
    
    base_rates = GetBaseRates(model_output_type)
    if base_rates.numel() != output_linear.out_features:
        raise ValueError(f"{model_output_type.name} base rates have {base_rates.numel()} classes but the "
                         f"network has {output_linear.out_features} outputs")
    
    # Inputs are z-scored (then rotated by PCA), so their mean square is about 1 per feature
    nn.init.normal_(input_linear.weight, std=1.0 / math.sqrt(input_linear.in_features))
    nn.init.zeros_(input_linear.bias)
    
    activation_gain = GetActivationGain(activation_function)
    branch_scale = 1.0 / math.sqrt(max(len(blocks), 1))
    for block in blocks:
        block.ResetParameters(activation_gain, branch_scale)
    
    # Log base rates as the bias and small weights: softmax of the output starts very close to the base rates
    nn.init.normal_(output_linear.weight, std=OUTPUT_WEIGHT_SCALE / math.sqrt(output_linear.in_features))
    with torch.no_grad():
        output_linear.bias.copy_(base_rates.log())
