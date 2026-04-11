from tqdm import tqdm

from Pro.Model.Model_Train import ELEMENT_LIST, NUM_ELEMENTS
from College.Model.Model_Train import HITTER_ELEMENT_LIST, PITCHER_ELEMENT_LIST, NUM_ELEMENTS_HITTER, NUM_ELEMENTS_PITCHER

from Pro.Model.Player_Model import RNN_Model as Pro_Model
from College.Model.College_Model import RNN_Model as Col_Model

from College.Model.Model_Train import GetLossesHitter, GetLossesPitcher
from Pro.Model.Model_Train import GetLosses
from Pro.Model.Model_Scheduler import Model_Scheduler_ReduceOnPlateauGroups as Scheduler

from Combined.DataPrep.Player_Dataset import Combined_Player_Dataset

import torch
import matplotlib.pyplot as plt

def TrainAndGraph(
    pro_network : Pro_Model,
    col_network : Col_Model,
    train_dataset : Combined_Player_Dataset,
    test_dataset : Combined_Player_Dataset,
    batch_size : int,
    num_epochs : int,
    is_hitter : bool,
    logging_interval : int = 10,
    should_output : bool = True,
    pro_model_name : str = "no_name_pro",
    col_model_name : str = "no_name_col",
    get_end_loss : bool = False,
    element_to_save : int = 0
) -> float:
    num_pro_elements = NUM_ELEMENTS
    if is_hitter:
        num_col_elements =  + NUM_ELEMENTS_HITTER
        element_list = ELEMENT_LIST + HITTER_ELEMENT_LIST
    else:
        num_col_elements = NUM_ELEMENTS_PITCHER
        element_list = ELEMENT_LIST + PITCHER_ELEMENT_LIST
    num_elements = num_pro_elements + num_col_elements
        
    test_loss_history : list[list[float]] = [[] for _ in range(num_elements)]
    train_loss_history : list[list[float]] = [[] for _ in range(num_elements)]
    epoch_counter : list[int] = []
    best_loss = 999999
    best_epoch = -1
    
    train_generator = torch.utils.data.DataLoader(train_dataset, batch_size=batch_size, shuffle=True)
    test_generator = torch.utils.data.DataLoader(test_dataset, batch_size=batch_size, shuffle=False)
    
    col_optimizer = torch.optim.Adam(col_network.parameters(), lr=0.0025)
    col_scheduler = torch.optim.lr_scheduler.ReduceLROnPlateau(col_optimizer, factor=0.5, patience=5, cooldown=5)
    pro_scheduler = Scheduler(pro_network.optimizer, [[0,1], [2], [3], [4], [5], [6], [7]], verbose=False,
                        factor=0.5, patience=5, cooldown=5)
    
    iterable = range(num_epochs)
    if not should_output:
        iterable = tqdm(iterable, leave=False, desc="Training")
    for epoch in iterable:
        train_loss = Train(
            pro_network=pro_network, 
            col_network=col_network, 
            generator=train_generator, 
            pro_size=train_dataset.GetProLength(), 
            col_size=train_dataset.GetColLength(), 
            pro_optimizer=pro_network.optimizer, 
            col_optimizer=col_optimizer,
            is_hitter=is_hitter,
            pro_elements=num_pro_elements,
            col_elements=num_col_elements,)
        test_loss = Test(
            pro_network=pro_network,
            col_network=col_network,
            generator=test_generator,
            pro_size=test_dataset.GetProLength(),
            col_size=test_dataset.GetColLength(),
            is_hitter=is_hitter,
            pro_elements=num_pro_elements,
            col_elements=num_col_elements,)
        
        for n in range(num_elements):
            train_loss_history[n].append(train_loss[n])
            test_loss_history[n].append(test_loss[n])
        epoch_counter.append(epoch)
        
        col_scheduler.step(test_loss[num_pro_elements])
        pro_scheduler.step(test_loss[:num_pro_elements])
        
        if should_output and (epoch % logging_interval == 0):  
            print('Epoch [%d/%d], Train Loss: %.4f, Test Loss: %.4f' %(epoch, num_epochs, train_loss[element_to_save], test_loss[element_to_save]))
        
        if test_loss[element_to_save] < best_loss:
            best_loss = test_loss[element_to_save]
            best_epoch = epoch
            torch.save(col_network.state_dict(), col_model_name + ".pt")
            torch.save(pro_network.state_dict(), pro_model_name + ".pt")
            
    if should_output:
        print(f"Best result at epoch={best_epoch} loss={best_loss}")
        for n in range(num_elements):
            GraphLoss(epoch_counter, train_loss_history[n], test_loss_history[n], title=element_list[n])
        
    if get_end_loss:
        return test_loss[element_to_save]
    else:
        return best_loss
        
def Train(
    pro_network : Pro_Model,
    col_network : Col_Model,
    generator : torch.utils.data.DataLoader,
    pro_size : int,
    col_size : int,
    pro_optimizer : torch.optim.Optimizer,
    col_optimizer : torch.optim.Optimizer,
    is_hitter : bool,
    pro_elements : int,
    col_elements : int,
) -> list[float]:
    
    pro_network.train()
    col_network.train()
    avg_loss = [0] * (pro_elements + col_elements)
    
    for pro_data, pro_targets, pro_masks, col_data, col_targets, col_masks in generator:
        col_optimizer.zero_grad()
        pro_optimizer.zero_grad()
        if is_hitter:
            col_losses, h0 = GetLossesHitter(col_network, col_data, col_targets, col_masks, shouldBackprop=True)
        else:
            col_losses, h0 = GetLossesPitcher(col_network, col_data, col_targets, col_masks, shouldBackprop=True)
        
        pro_losses = GetLosses(pro_network, pro_data, pro_targets, pro_masks, h0, shouldBackprop=True, is_hitter=is_hitter)
        
        torch.nn.utils.clip_grad_norm_(col_network.parameters(), max_norm=0.05)
        col_optimizer.step()
        torch.nn.utils.clip_grad_norm_(pro_network.parameters(), max_norm=0.05)
        pro_optimizer.step()
        
        for i, loss in enumerate(pro_losses):
            avg_loss[i] += loss.item()
        for i, loss in enumerate(col_losses, start=len(pro_losses)):
            avg_loss[i] += loss.item()
            
    for n in range(pro_elements):
        avg_loss[n] /= pro_size
    for n in range(col_elements):
        avg_loss[n + pro_elements] /= col_size
        
    return avg_loss


def Test(
    pro_network : Pro_Model,
    col_network : Col_Model,
    generator : torch.utils.data.DataLoader,
    pro_size : int,
    col_size : int,
    is_hitter : bool,
    pro_elements : int,
    col_elements : int,
) -> list[float]:
    
    pro_network.eval()
    col_network.eval()
    avg_loss = [0] * (pro_elements + col_elements)
    
    for pro_data, pro_targets, pro_masks, col_data, col_targets, col_masks in generator:
        if is_hitter:
            col_losses, h0 = GetLossesHitter(col_network, col_data, col_targets, col_masks, shouldBackprop=False)
        else:
            col_losses, h0 = GetLossesPitcher(col_network, col_data, col_targets, col_masks, shouldBackprop=False)
        
        pro_losses = GetLosses(pro_network, pro_data, pro_targets, pro_masks, h0, shouldBackprop=False, is_hitter=is_hitter)
        
        for i, loss in enumerate(pro_losses):
            avg_loss[i] += loss.item()
        for i, loss in enumerate(col_losses, start=len(pro_losses)):
            avg_loss[i] += loss.item()
            
    for n in range(pro_elements):
        avg_loss[n] /= pro_size
    for n in range(col_elements):
        avg_loss[n + pro_elements] /= col_size
        
    return avg_loss


def GraphLoss(epoch_counter, train_loss_hist, test_loss_hist, loss_name="Loss", start = 1, graph_y_range=None, title=""):
    fig = plt.figure()
    plt.plot(epoch_counter[start:], train_loss_hist[start:], color='blue')
    plt.plot(epoch_counter[start:], test_loss_hist[start:], color='red')
    plt.title(title)
    if graph_y_range is not None:
        plt.ylim(graph_y_range)
    plt.legend(['Train Loss', 'Test Loss'], loc='upper right')
    plt.xlabel('#Epochs')
    plt.ylabel(loss_name)