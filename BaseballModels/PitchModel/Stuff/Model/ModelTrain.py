import matplotlib.pyplot as plt
import torch
from tqdm import tqdm
from Constants import device
from Stuff.Model.PitchModel import PitchModel
from Stuff.DataPrep.PitchDataset import PitchDataset
from Stuff.Model.LossFunctions import *

_MODEL_VARIANTS = ["Location", "Stuff", "Combined"]
_MODEL_OUTPUTS = ["Value", "Runs", "Outs", "Swung", "Contact", "InPlay"]
_TOTAL_OUTPUTS = [v + " " + o for v in _MODEL_VARIANTS for o in _MODEL_OUTPUTS]

[x + "hi" for x in _MODEL_OUTPUTS]

def TrainAndGraph(
    network : PitchModel,
    train_dataset : PitchDataset,
    test_dataset : PitchDataset,
    batch_size : int = 100,
    num_epochs : int = 21,
    logging_interval : int = 10,
    early_stopping_cutoff : int = 20,
    base_element : int = 12,
    should_output : bool = True,
    model_name : str = "no_name",
) -> list[float]:
    
    test_loss_history : list[list[float]] = [[] for _ in range(len(_TOTAL_OUTPUTS))]
    train_loss_history : list[list[float]] = [[] for _ in range(len(_TOTAL_OUTPUTS))]
    epoch_counter : list[int] = []
    
    best_loss = 99999999
    best_epoch = 0
    epochs_since_improve = 0
    
    train_generator = torch.utils.data.DataLoader(train_dataset, batch_size=batch_size, shuffle=True)
    test_generator = torch.utils.data.DataLoader(test_dataset, batch_size=batch_size, shuffle=False)
    
    optimizer = torch.optim.Adam(network.parameters(), lr=0.001)
    
    # TODO : Need a custom scheduler for this
    
    iterable = range(num_epochs)
    if not should_output:
        iterable = tqdm(iterable, leave=False, desc="Training")
    for epoch in iterable:
        train_losses = TrainTest(network=network, 
            generator=train_generator, 
            optimizer=optimizer, 
            total_size=len(train_dataset),
            is_train=True)
        test_losses = TrainTest(network=network, 
            generator=test_generator, 
            optimizer=None, 
            total_size=len(test_dataset),
            is_train=False)
        
        LogResults(epoch, num_epochs, train_losses[base_element], test_losses[base_element], logging_interval, should_output)
        
        for n in range(len(_TOTAL_OUTPUTS)):
            train_loss_history[n].append(train_losses[n])
            test_loss_history[n].append(test_losses[n])
        epoch_counter.append(epoch)
        
        if test_losses[base_element] < best_loss:
            best_loss = test_losses[base_element]
            best_epoch = epoch
            torch.save(network.state_dict(), model_name + ".pt")
            epochs_since_improve = 0
        else:
            epochs_since_improve += 1
            
        if epochs_since_improve >= early_stopping_cutoff:
            if should_output:
                print("Stopped Training Early")
            break
        
    if should_output:
        print(f"Best result at epoch={best_epoch} with loss={best_loss}")
        
    for n in range(len(_TOTAL_OUTPUTS)):
        GraphLoss(epoch_counter, train_loss_history[n], test_loss_history[n], title=_TOTAL_OUTPUTS[n], start=1)
        
    return test_losses



def TrainTest(network : PitchModel, generator : torch.utils.data.DataLoader, optimizer : torch.optim.Optimizer | None, total_size : int, is_train : bool) -> list[float]:
    if is_train:
        network.train()
        if optimizer is None:
            raise RuntimeError("Optimizer is none for Train")
    else:
        network.eval()
    avg_losses = [0] * len(_TOTAL_OUTPUTS)
    for data, targets in generator:
        if is_train:
            optimizer.zero_grad()
        losses = GetLosses(network, data, targets, is_train)
        
        if is_train:
            torch.nn.utils.clip_grad_norm_(network.parameters(), max_norm=0.05)
            optimizer.step()
        
        for i in range(len(_TOTAL_OUTPUTS)):
            avg_losses[i] += losses[i].item()
            
    for i in range(len(_TOTAL_OUTPUTS)):
        avg_losses[i] /= total_size
    return avg_losses
        
def GetLosses(network : PitchModel, data : tuple[torch.Tensor, ...], targets : tuple[torch.Tensor, ...], should_backprop : bool) -> list[torch.Tensor]:
    data_overview, data_loc, data_stuff = data
    data_overview = data_overview.to(device, non_blocking=True)
    data_loc = data_loc.to(device, non_blocking=True)
    data_stuff = data_stuff.to(device, non_blocking=True)
    
    for i in range(len(targets)):
        targets[i] = targets[i].to(device, non_blocking=True)
    
    outputs = network(data_overview, data_loc, data_stuff)
    
    if len(outputs) / len(targets) != len(_MODEL_VARIANTS):
        raise RuntimeError(f"Not the same number of outputs ({len(outputs)}) and targets ({len(targets)})")
    
    losses = []
    for i in range(len(outputs)):
        losses.append(Classification_Loss(outputs[i], targets[i % len(_MODEL_OUTPUTS)]))
        
    if should_backprop:
        torch.autograd.backward(losses)
        
    return losses

def LogResults(epoch, num_epochs, train_loss, test_loss, print_interval=1000, should_output=True):
    if should_output and (epoch%print_interval == 0):  
        print('Epoch [%d/%d], Train Loss: %.4f, Test Loss: %.4f' %(epoch+1, num_epochs, train_loss, test_loss))
        
def GraphLoss(epoch_counter, train_loss_hist, test_loss_hist, loss_name="Loss", start = 2, graph_y_range=None, title=""):
    plt.plot(epoch_counter[start:], train_loss_hist[start:], color='blue')
    plt.plot(epoch_counter[start:], test_loss_hist[start:], color='red')
    plt.title(title)
    if graph_y_range is not None:
        plt.ylim(graph_y_range)
    plt.legend(['Train Loss', 'Test Loss'], loc='upper right')
    plt.xlabel('#Epochs')
    plt.ylabel(loss_name)
    plt.show()
    plt.clf()