from Combined.Utilities.Types import *
from Combined.Model.TestOrTrain import TestOrTrain

def RunEpoch(
    pro_network, col_network, train_dataset, test_dataset,
    is_hitter: bool, num_pro_elements: int, num_col_elements: int,
    batch_size: int, pro_element_loss_scales
) -> tuple[EpochResult, EpochResult]:
    train_result = TestOrTrain(
            pro_network=pro_network, 
            col_network=col_network, 
            dataset=train_dataset, 
            pro_size=train_dataset.GetProLength(), 
            col_size=train_dataset.GetColLength(), 
            pro_optimizer=pro_network.optimizer, 
            col_optimizer=col_network.optimizer,
            is_hitter=is_hitter,
            pro_elements=num_pro_elements,
            col_elements=num_col_elements,
            is_train=True,
            batch_size=batch_size,
            pro_element_loss_scales=pro_element_loss_scales)
    
    test_result = TestOrTrain(
            pro_network=pro_network,
            col_network=col_network,
            dataset=test_dataset,
            pro_size=test_dataset.GetProLength(),
            col_size=test_dataset.GetColLength(),
            is_hitter=is_hitter,
            pro_elements=num_pro_elements,
            col_elements=num_col_elements,
            is_train=False,
            batch_size=batch_size,
            pro_element_loss_scales=pro_element_loss_scales)
    
    return train_result, test_result