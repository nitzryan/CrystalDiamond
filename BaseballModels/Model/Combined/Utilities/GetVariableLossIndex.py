from Pro.Model.Model_Train import ELEMENT_LIST
from College.Model.Model_Train import HITTER_ELEMENT_LIST, PITCHER_ELEMENT_LIST

def GetVariableLossIndex(name : str, is_pro : bool, is_hitter : bool) -> int:
    name_lower = name.lower()
    if is_pro:
        element_list = ELEMENT_LIST
    elif is_hitter:
        element_list = HITTER_ELEMENT_LIST
    else:
        element_list = PITCHER_ELEMENT_LIST
        
    for i in range(len(element_list)):
        if element_list[i].lower() == name_lower:
            if is_pro:
                return i
            else:
                return i + len(ELEMENT_LIST)
        
    raise Exception(f"Did not find Variable Loss Name {name}")