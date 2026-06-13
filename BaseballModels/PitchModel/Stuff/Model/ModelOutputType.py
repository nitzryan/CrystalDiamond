from enum import Enum

class ModelVariantType(Enum):
    Stuff = 0,
    Combined = 1
    
class ModelOutputType(Enum):
    Result = 0,
    SwingResults = 1,
    InPlay = 2,
    
MODEL_VARIANTS = [ModelVariantType.Stuff, ModelVariantType.Combined]
MODEL_OUTPUTS = [ModelOutputType.Result, ModelOutputType.SwingResults, ModelOutputType.InPlay]