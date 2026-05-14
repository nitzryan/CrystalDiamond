import matplotlib.pyplot as plt
import matplotlib.ticker as mticker
import torch
from tqdm import tqdm
from Constants import device
from Stuff.Model.PitchModel import PitchModel
from Stuff.DataPrep.PitchDataset import PitchDataset
from Stuff.Model.LossFunctions import *
from Stuff.Model.ModelScheduler import Model_Scheduler_ReduceOnPlateauGroups as Scheduler

from Constants import profiler

_MODEL_VARIANTS = ["Location", "Stuff", "Combined"]
_MODEL_OUTPUTS = ["Result", "SwingResults", "InPlay"]
_TOTAL_OUTPUTS = [v + " " + o for v in _MODEL_VARIANTS for o in _MODEL_OUTPUTS]

SHOULD_PROFILE = False

def TrainAndGraph(
    network : PitchModel,
    train_dataset : PitchDataset,
    test_dataset : PitchDataset,
    batch_size : int = 30000,
    num_epochs : int = 1001,
    logging_interval : int = 50,
    early_stopping_cutoff : int = 20,
    base_element : int = 2 * len(_MODEL_OUTPUTS),
    should_output : bool = True,
    model_name : str = "no_name",
) -> list[float]:
    
    if SHOULD_PROFILE:
        profiler.enable()
    
    test_loss_history : list[list[float]] = [[] for _ in range(len(_TOTAL_OUTPUTS))]
    train_loss_history : list[list[float]] = [[] for _ in range(len(_TOTAL_OUTPUTS))]
    epoch_counter : list[int] = []
    
    best_loss = 99999999
    best_epoch = 0
    epochs_since_improve = 0

    
    scheduler = Scheduler(
        optimizer=network.optimizer,
        parameter_map=[[0], [1], [2], [3], [4], [5], [6], [7], [8]],
        verbose=False,
        factor=0.5,
        patience=5,
        cooldown=5)
    
    iterable = range(num_epochs)
    if not should_output:
        iterable = tqdm(iterable, leave=False, desc="Training")
    for epoch in iterable:
        train_losses = TrainTest(network=network, 
            dataset=train_dataset, 
            optimizer=network.optimizer, 
            batch_size=batch_size,
            total_size=len(train_dataset),
            is_train=True)
        test_losses = TrainTest(network=network, 
            dataset=test_dataset, 
            optimizer=None,
            batch_size=batch_size,
            total_size=len(test_dataset),
            is_train=False)
        
        LogResults(epoch, num_epochs, train_losses[base_element], test_losses[base_element], logging_interval, should_output)
        scheduler.step(test_losses)
        
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
        
    if SHOULD_PROFILE:    
        profiler.disable()
        profiler.dump_stats("train_profile.lprof")
        
    if should_output:
        print(f"Best result at epoch={best_epoch} with loss={best_loss}")
        for n in range(len(_TOTAL_OUTPUTS)):
            GraphLoss(epoch_counter, train_loss_history[n], test_loss_history[n], title=_TOTAL_OUTPUTS[n], start=1)
        
    return test_losses


@profiler
def TrainTest(network : PitchModel, 
              dataset : PitchDataset, 
              optimizer : torch.optim.Optimizer | None, 
              total_size : int,
              batch_size : int,
              is_train : bool) -> list[float]:
    
    
    if is_train:
        network.train()
        indices = torch.randperm(total_size, device='cpu')
        if optimizer is None:
            raise RuntimeError("Optimizer is none for Train")
    else:
        network.eval()
        indices = torch.arange(total_size, device='cpu')
        
    avg_losses = [0] * len(_TOTAL_OUTPUTS)
    sizes = [0] * len(_TOTAL_OUTPUTS)
    for i in range(0, total_size, batch_size):
        # Fetch Data
        batch_idx = indices[i:i + batch_size]
        _, data, targets, masks = dataset.GetEntries(batch_idx)
        
        data = tuple(d.to(device, non_blocking=False) for d in data)
        targets = tuple(t.to(device, non_blocking=False) for t in targets)
        masks = tuple(m.to(device, non_blocking=False) for m in masks)
        
        # Run through model, get losses
        if is_train:
            optimizer.zero_grad()
        losses, counts = GetLosses(network, data, targets, masks, is_train)
        
        if is_train:
            torch.nn.utils.clip_grad_norm_(network.parameters(), max_norm=0.05)
            optimizer.step()
        
        for i in range(len(_TOTAL_OUTPUTS)):
            avg_losses[i] += losses[i].item() * counts[i]
            sizes[i] += counts[i]
            
    for i in range(len(_TOTAL_OUTPUTS)):
        avg_losses[i] /= sizes[i]
    return avg_losses
        
@profiler
def GetLosses(network : PitchModel, data : tuple[torch.Tensor, ...], targets : tuple[torch.Tensor, ...], masks : tuple[torch.Tensor, ...], should_backprop : bool) -> tuple[list[torch.Tensor], list[int]]:
    outputs = network(data)
    
    if len(outputs) / len(targets) != len(_MODEL_VARIANTS):
        raise RuntimeError(f"Not the same number of outputs ({len(outputs)}) and targets ({len(targets)})")
    
    losses : list[torch.Tensor] = []
    counts : list[int] = []
    for i in range(len(outputs)):
        loss, count = Classification_Loss(outputs[i], targets[i % len(_MODEL_OUTPUTS)], masks[i % len(_MODEL_OUTPUTS)])
        losses.append(loss)
        counts.append(count)
        
    if should_backprop:
        torch.autograd.backward(losses)
        
    return losses, counts

def LogResults(epoch, num_epochs, train_loss, test_loss, print_interval=1000, should_output=True):
    if should_output and (epoch%print_interval == 0):  
        print('Epoch [%d/%d], Train Loss: %.4f, Test Loss: %.4f' %(epoch+1, num_epochs, train_loss, test_loss))
        
def GraphLoss(epoch_counter, train_loss_hist, test_loss_hist, loss_name="Loss", start = 0, graph_y_range=None, title=""):
    plt.plot(epoch_counter[start:], train_loss_hist[start:], color='blue')
    plt.plot(epoch_counter[start:], test_loss_hist[start:], color='red')
    plt.title(title)
    if graph_y_range is not None:
        plt.ylim(graph_y_range)
    plt.legend(['Train Loss', 'Test Loss'], loc='upper right')
    plt.xlabel('#Epochs')
    plt.ylabel(loss_name)
    plt.yscale('log')
    plt.gca().yaxis.set_major_formatter(mticker.ScalarFormatter())
    plt.gca().yaxis.get_major_formatter().set_scientific(False)
    plt.gca().yaxis.get_major_formatter().set_useOffset(False)
    plt.gca().yaxis.set_minor_formatter(mticker.ScalarFormatter())
    plt.show()
    plt.clf()