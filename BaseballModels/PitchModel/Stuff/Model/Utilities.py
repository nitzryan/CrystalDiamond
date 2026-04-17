import torch
import torch.nn as nn
from typing import Callable

def GetModuleOutput(data : torch.Tensor, moduleList : nn.ModuleList, nonlin : Callable[[torch.Tensor], torch.Tensor]) -> torch.Tensor:
        for layer in moduleList:
            data = layer(data)
            if layer != moduleList[-1]:
                data = nonlin(data)
                
        return data