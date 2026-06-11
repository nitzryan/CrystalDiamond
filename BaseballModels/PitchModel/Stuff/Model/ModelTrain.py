import matplotlib.pyplot as plt
import matplotlib.ticker as mticker
import torch
from tqdm import tqdm
from Constants import device
from Stuff.Model.PitchModel import PitchModel, ModelVariantType, ModelOutputType
from Stuff.DataPrep.PitchDataset import PitchDataset
from Stuff.Model.LossFunctions import *
from torch.optim.lr_scheduler import CosineAnnealingLR 
from Constants import profiler



_MODEL_VARIANTS = [ModelVariantType.Stuff, ModelVariantType.Combined]
_MODEL_OUTPUTS = [ModelOutputType.Result, ModelOutputType.SwingResults, ModelOutputType.InPlay]

SHOULD_PROFILE = False

def TrainAndGraph(
    network : PitchModel,
    train_dataset : PitchDataset,
    test_dataset : PitchDataset,
    batch_size : int = 30000,
    num_epochs : int = 1001,
    logging_interval : int = 50,
    early_stopping_cutoff : int = 20,

    should_output : bool = True,
    model_name : str = "no_name",
) -> float:
    
    if SHOULD_PROFILE:
        profiler.enable()
    
    test_loss_history : list[float] = []
    train_loss_history : list[float] = []
    epoch_counter : list[int] = []
    
    best_loss = 99999999
    best_epoch = 0
    epochs_since_improve = 0

    
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
    if not should_output:
        iterable = tqdm(iterable, leave=False, desc="Training")
    for epoch in iterable:
        train_loss = TrainTest(network=network, 
            dataset=train_dataset, 
            optimizer=network.optimizer, 
            batch_size=batch_size,
            total_size=len(train_dataset),
            is_train=True)
        test_loss = TrainTest(network=network, 
            dataset=test_dataset, 
            optimizer=None,
            batch_size=batch_size,
            total_size=len(test_dataset),
            is_train=False)
        
        LogResults(epoch, num_epochs, train_loss, test_loss, logging_interval, should_output)
        scheduler.step()
        
        train_loss_history.append(train_loss)
        test_loss_history.append(test_loss)
        epoch_counter.append(epoch)
        
        if test_loss < best_loss:
            best_loss = test_loss
            best_epoch = epoch
            torch.save(network.state_dict(), f"{model_name}_{network.model_variant_type.name}_{network.model_output_type.name}.pt")
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
        GraphLoss(epoch_counter, train_loss_history, test_loss_history, title=f"{network.model_variant_type.name}_{network.model_output_type.name}", start=1)
        
    return test_loss


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
        _, data, targets, masks = dataset.GetEntries(batch_idx)
        
        data = tuple(d.to(device, non_blocking=False) for d in data)
        targets = tuple(t.to(device, non_blocking=False) for t in targets)
        masks = tuple(m.to(device, non_blocking=False) for m in masks)
        
        # Run through model, get losses
        if is_train:
            optimizer.zero_grad()
        loss, count = GetLosses(network, data, targets, masks, is_train)
        
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
    targets : tuple[torch.Tensor, ...], 
    masks : tuple[torch.Tensor, ...], 
    should_backprop : bool) -> tuple[list[torch.Tensor], list[int]]:
    
    
    overview, loc, stuff, combined, game, league = data
    match network.model_variant_type:
        case ModelVariantType.Stuff:
            input_data = torch.cat((overview, stuff, league), dim=-1)
        case ModelVariantType.Combined:
            input_data = torch.cat((overview, loc, stuff, combined, league), dim=-1)
    
    outputs = network(input_data)
    
    output_result, output_swing, output_inplay = targets
    mask_result, mask_swing, mask_inplay = masks
    match network.model_output_type:
        case ModelOutputType.Result:
            output_targets = output_result
            output_masks = mask_result
        case ModelOutputType.SwingResults:
            output_targets = output_swing
            output_masks = mask_swing
        case ModelOutputType.InPlay:
            output_targets = output_inplay
            output_masks = mask_inplay
    
    loss, count = Classification_Loss(outputs, output_targets, output_masks)
        
    if should_backprop:
        torch.autograd.backward(loss)
        
    return loss, count

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