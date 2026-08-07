import matplotlib.pyplot as plt
import matplotlib.ticker as mticker
import torch
from tqdm import tqdm
from dataclasses import dataclass

from PitchModel.Constants import device
from PitchModel.Stuff.Model.PitchModel import PitchModel
from PitchModel.Stuff.Model.ModelOutputType import ModelVariantType, ModelOutputType
from PitchModel.Stuff.DataPrep.PitchDataset import PitchDataset, PitchDatasets
from PitchModel.Stuff.Model.LossFunctions import *
from torch.optim.lr_scheduler import CosineAnnealingLR 
from PitchModel.Constants import profiler

SHOULD_PROFILE = False

@dataclass
class TrainResult:
    test_loss : float
    val_seen_loss : float | None
    val_unseen_loss : float | None

def TrainAndGraph(
    network : PitchModel,
    datasets : PitchDatasets,
    batch_size : int = 30000,
    num_epochs : int = 101,
    logging_interval : int = 50,

    should_output : bool = True,
    show_progress : bool = True,
    model_name : str = "no_name",
) -> TrainResult:
    
    if SHOULD_PROFILE:
        profiler.enable()
    
    test_loss_history : list[float] = []
    train_loss_history : list[float] = []
    val_seen_loss_history : list[float] | None = [] if datasets.val_seen is not None else None
    val_unseen_loss_history : list[float] | None = [] if datasets.val_unseen is not None else None
    epoch_counter : list[int] = []
    
    match network.model_variant_type:
        case ModelVariantType.Stuff:
            match network.model_output_type:
                case ModelOutputType.Result:
                    t_max = 200
                case ModelOutputType.SwingResults:
                    t_max = 200
                case ModelOutputType.InPlay:
                    t_max = 200
        case ModelVariantType.Combined:
            match network.model_output_type:
                case ModelOutputType.Result:
                    t_max = 200
                case ModelOutputType.SwingResults:
                    t_max = 200
                case ModelOutputType.InPlay:
                    t_max = 200
    
    scheduler = CosineAnnealingLR(
        network.optimizer,
        T_max=t_max
    )
    
    iterable = range(num_epochs)
    if not should_output and show_progress:
        iterable = tqdm(iterable, leave=False, desc="Training")
    for epoch in iterable:
        train_loss = TrainTest(network=network, 
            dataset=datasets.train, 
            optimizer=network.optimizer, 
            batch_size=batch_size,
            total_size=len(datasets.train),
            is_train=True)
        test_loss = TrainTest(network=network, 
            dataset=datasets.test, 
            optimizer=None,
            batch_size=batch_size,
            total_size=len(datasets.test),
            is_train=False)
        if val_seen_loss_history is not None:
            val_seen_loss_history.append(TrainTest(network=network,
                dataset=datasets.val_seen,
                optimizer=None,
                batch_size=batch_size,
                total_size=len(datasets.val_seen),
                is_train=False))
        if val_unseen_loss_history is not None:
            val_unseen_loss_history.append(TrainTest(network=network,
                dataset=datasets.val_unseen,
                optimizer=None,
                batch_size=batch_size,
                total_size=len(datasets.val_unseen),
                is_train=False))
        
        LogResults(epoch, num_epochs, train_loss, test_loss, logging_interval, should_output)
        scheduler.step()
        
        train_loss_history.append(train_loss)
        test_loss_history.append(test_loss)
        epoch_counter.append(epoch)
        
        # Check to exit early if model blows up
        if epoch == 0:
            first_loss = test_loss
        elif test_loss > 1.2 * first_loss:
            break
        

    torch.save(network.state_dict(), f"{model_name}_{network.model_variant_type.name}_{network.model_output_type.name}.pt")
        
    if SHOULD_PROFILE:    
        profiler.disable()
        profiler.dump_stats("train_profile.lprof")
        
    if should_output:
        print(f"End result with loss={test_loss}")
        GraphLoss(epoch_counter, train_loss_history, test_loss_history, val_seen_loss_history, val_unseen_loss_history, title=f"{network.model_variant_type.name}_{network.model_output_type.name}", start=1)
        
    return TrainResult(
        test_loss=test_loss,
        val_seen_loss=val_seen_loss_history[-1] if val_seen_loss_history else None,
        val_unseen_loss=val_unseen_loss_history[-1] if val_unseen_loss_history else None,
    )


@profiler
def TrainTest(network : PitchModel, 
              dataset : PitchDataset, 
              optimizer : torch.optim.Optimizer | None, 
              total_size : int,
              batch_size : int,
              is_train : bool) -> float:
    
    
    if is_train:
        network.train()
        indices = torch.randperm(total_size, device='cpu')
        if optimizer is None:
            raise RuntimeError("Optimizer is none for Train")
    else:
        network.eval()
        indices = torch.arange(total_size, device='cpu')
        
    avg_loss = 0
    size = 0
    for i in range(0, total_size, batch_size):
        # Fetch Data
        batch_idx = indices[i:i + batch_size]
        _, data, target = dataset.GetEntries(batch_idx, False)
        
        data = tuple(d.to(device, non_blocking=False) for d in data)
        
        # Run through model, get losses
        if is_train:
            optimizer.zero_grad()
        loss, count = GetLosses(network, data, target, is_train)
        
        if is_train:
            torch.nn.utils.clip_grad_norm_(network.parameters(), max_norm=0.05)
            optimizer.step()
        
        
        avg_loss += loss.item() * count
        size += count
            
    avg_loss /= size
    return avg_loss
        
@profiler
def GetLosses(
    network : PitchModel, 
    data : tuple[torch.Tensor, ...], 
    targets : torch.Tensor, 
    should_backprop : bool) -> tuple[list[torch.Tensor], list[int]]:
    
    
    stuff, combined = data
    match network.model_variant_type:
        case ModelVariantType.Stuff:
            input_data = stuff
        case ModelVariantType.Combined:
            input_data = combined
    
    outputs = network(input_data)
    
    loss, count = Classification_Loss(outputs, targets)
        
    if should_backprop:
        torch.autograd.backward(loss)
        
    return loss, count

def LogResults(epoch, num_epochs, train_loss, test_loss, print_interval=1000, should_output=True):
    if should_output and (epoch%print_interval == 0):  
        print('Epoch [%d/%d], Train Loss: %.4f, Test Loss: %.4f' %(epoch+1, num_epochs, train_loss, test_loss))
        
def GraphLoss(epoch_counter : list[int],
            train_loss_hist : list[float],
            test_loss_hist : list[float],
            val_seen_loss_hist : list[float] | None = None,
            val_unseen_loss_hist : list[float] | None = None,
            loss_name : str = "Loss",
            start : int = 0,
            graph_y_range : tuple[float, float] | None = None,
            title : str = ""
            ) -> None:
    
    # Plot series
    plt.plot(epoch_counter[start:], train_loss_hist[start:], color='blue')
    plt.plot(epoch_counter[start:], test_loss_hist[start:], color='red')
    
    legend : list[str] = ['Train Loss', 'Test Loss']
    
    if val_seen_loss_hist is not None:
        plt.plot(epoch_counter[start:], val_seen_loss_hist[start:], color='green')
        legend.append('Val Seen Loss')
    if val_unseen_loss_hist is not None:
        plt.plot(epoch_counter[start:], val_unseen_loss_hist[start:], color='orange')
        legend.append('Val Unseen Loss')
    
    # Format Graph
    plt.title(title)
    if graph_y_range is not None:
        plt.ylim(graph_y_range)
    plt.legend(legend, loc='upper right')
    plt.xlabel('#Epochs')
    plt.ylabel(loss_name)
    plt.gca().yaxis.set_major_formatter(mticker.ScalarFormatter())
    plt.gca().yaxis.get_major_formatter().set_scientific(False)
    plt.gca().yaxis.get_major_formatter().set_useOffset(False)
    plt.gca().yaxis.set_minor_formatter(mticker.ScalarFormatter())
    plt.show()
    plt.clf()