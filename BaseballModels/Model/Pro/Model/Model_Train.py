import matplotlib.pyplot as plt
import torch
from tqdm import tqdm
from Constants import device
from Pro.Model.Player_Model import Stats_Loss, Position_Classification_Loss, Classification_Loss, Mlb_Value_Loss_Hitter, Mlb_Value_Loss_Pitcher, MLB_Stat_Classification_Loss, Pt_Loss
from Pro.Model.Model_Scheduler import Model_Scheduler_ReduceOnPlateauGroups as Scheduler

from Utilities import profiler

ELEMENT_LIST = ["WarClass", "Level", "PA", "Stats", "Position", "MLBValue", "PlayingTime", "MLBStat"]
NUM_ELEMENTS = len(ELEMENT_LIST)
ELEMENT_LOSS_SCALES = [1] * NUM_ELEMENTS

@profiler
def GetLosses(network, data : tuple, targets : tuple, masks : tuple, h0 : torch.Tensor, shouldBackprop : bool, is_hitter: bool) -> tuple:
  # Get Model Output
  data, length, pt_levelYearGames = data
  mask_valid = length > 0
  data = data[mask_valid].to(device, non_blocking=True)
  length = length[mask_valid].to(device, non_blocking=True)
  pt_levelYearGames = pt_levelYearGames[mask_valid].to(device, non_blocking=True)
  h0 = h0[mask_valid].transpose(0, 1).to(device, non_blocking=True)
  output_war, output_level, output_pa, output_stats, output_pos, output_mlbValue, output_pt, output_mlbstat = network(data, length, pt_levelYearGames, h0)
  
  # Move targets and masks to GPU
  target_war, target_level, target_pa, target_yearStats, target_yearPos, target_mlbValue, target_pt, target_mlbstat = targets
  mask_labels, mask_stats, mask_year, mask_mlbValue, mask_mlbstat = masks
  
  target_war = target_war[mask_valid].to(device, non_blocking=True)
  target_level = target_level[mask_valid].to(device, non_blocking=True)
  target_pa = target_pa[mask_valid].to(device, non_blocking=True)
  target_yearStats = target_yearStats[mask_valid].to(device, non_blocking=True)
  target_yearPos = target_yearPos[mask_valid].to(device, non_blocking=True)
  target_mlbValue = target_mlbValue[mask_valid].to(device, non_blocking=True)
  target_pt = target_pt[mask_valid].to(device, non_blocking=True)
  target_mlbstat = target_mlbstat[mask_valid].to(device, non_blocking=True)
  
  mask_labels = mask_labels[mask_valid].to(device, non_blocking=True)
  mask_year = mask_year[mask_valid].to(device, non_blocking=True)
  mask_stats = mask_stats[mask_valid].to(device, non_blocking=True)
  mask_mlbValue = mask_mlbValue[mask_valid].to(device, non_blocking=True)
  mask_mlbstat = mask_mlbstat[mask_valid].to(device, non_blocking=True)
  
  # Get losses
  loss_war, loss_level, loss_pa = Classification_Loss(output_war, output_level, output_pa, target_war, target_level, target_pa, mask_labels)
  loss_yearStats = Stats_Loss(output_stats, target_yearStats, mask_stats)
  loss_yearPt = Pt_Loss(output_pt, target_pt)
  loss_yearPos = Position_Classification_Loss(output_pos, target_yearPos, mask_year)
  loss_mlbValue = Mlb_Value_Loss_Hitter(output_mlbValue, target_mlbValue, mask_mlbValue) if is_hitter else Mlb_Value_Loss_Pitcher(output_mlbValue, target_mlbValue, mask_mlbValue)
  loss_mlbStat = MLB_Stat_Classification_Loss(output_mlbstat, target_mlbstat, mask_mlbstat)
  
  # Scale how much each loss should effect the model
  losses = [loss_war, loss_level, loss_pa, loss_yearStats, loss_yearPos, loss_yearPt, loss_mlbValue, loss_mlbStat]
  for i in range(NUM_ELEMENTS):
    els = ELEMENT_LOSS_SCALES[i]
    if els != 1:
      losses[i] *= els
  
  if shouldBackprop:
    torch.autograd.backward(losses)
  
  for i in range(NUM_ELEMENTS):
    els = ELEMENT_LOSS_SCALES[i]
    if els != 1:
      losses[i] /= els
  
  return (loss_war, loss_level, loss_pa, loss_yearStats, loss_yearPos, loss_mlbValue, loss_yearPt, loss_mlbStat)

def train(network, data_generator, num_elements, optimizer, is_hitter : bool, trainingFraction : float):
  network.train() #updates any network layers that behave differently in training and execution
  avg_loss = [0] * NUM_ELEMENTS
  for batch, (data, length, pt_levelYearGames, targets, masks, fake_data) in enumerate(data_generator):
    # Train on real data
    optimizer.zero_grad()
    loss_war, loss_level, loss_pa, loss_yearStats, loss_yearPos, loss_mlbValue, loss_yearPt = GetLosses(network, data, length, pt_levelYearGames, targets, masks, True, is_hitter, trainingFraction)
    torch.nn.utils.clip_grad_norm_(network.parameters(), max_norm=0.05)
    optimizer.step()
    avg_loss[0] += loss_war.item()
    avg_loss[1] += loss_level.item()
    avg_loss[2] += loss_pa.item()
    avg_loss[3] += loss_yearStats.item()
    avg_loss[4] += loss_yearPos.item()
    avg_loss[5] += loss_mlbValue.item()
    avg_loss[6] += loss_yearPt.item()
    
  for n in range(NUM_ELEMENTS):
    avg_loss[n] /= num_elements
  return avg_loss

def test(network, test_loader, num_elements, is_hitter : bool, trainingFraction):
  network.eval() #updates any network layers that behave differently in training and execution
  avg_loss = [0] * NUM_ELEMENTS
  num_batches = 0
  with torch.no_grad():
    for data, length, pt_levelYearGames, targets, masks, fake_data in test_loader:
      loss_war, loss_level, loss_pa, loss_yearStats, loss_yearPos, loss_mlbValue, loss_yearPt = GetLosses(network, data, length, pt_levelYearGames, targets, masks, False, is_hitter, trainingFraction)
      
      avg_loss[0] += loss_war.item()
      avg_loss[1] += loss_level.item()
      avg_loss[2] += loss_pa.item()
      avg_loss[3] += loss_yearStats.item()
      avg_loss[4] += loss_yearPos.item()
      avg_loss[5] += loss_mlbValue.item()
      avg_loss[6] += loss_yearPt.item()
      num_batches += 1
  
  for n in range(NUM_ELEMENTS):
    avg_loss[n] /= num_elements
  return avg_loss

def count_parameters(model):
    return sum(p.numel() for p in model.parameters() if p.requires_grad)

def logResults(epoch, num_epochs, train_loss, test_loss, print_interval=1000, should_output=True):
  if should_output and (epoch%print_interval == 0):  
    print('Epoch [%d/%d], Train Loss: %.4f, Test Loss: %.4f' %(epoch+1, num_epochs, train_loss, test_loss))

def graphLoss(epoch_counter, train_loss_hist, test_loss_hist, loss_name="Loss", start = 2, graph_y_range=None, title=""):
  fig = plt.figure()
  plt.plot(epoch_counter[start:], train_loss_hist[start:], color='blue')
  plt.plot(epoch_counter[start:], test_loss_hist[start:], color='red')
  plt.title(title)
  if graph_y_range is not None:
    plt.ylim(graph_y_range)
  plt.legend(['Train Loss', 'Test Loss'], loc='upper right')
  plt.xlabel('#Epochs')
  plt.ylabel(loss_name)

def trainAndGraph(network, 
                  training_dataset, 
                  testing_dataset,
                  batch_size : int,
                  num_epochs, 
                  logging_interval=1, 
                  early_stopping_cutoff=20, 
                  should_output=True, 
                  model_name="no_name", 
                  save_last=False, 
                  is_hitter=True,
                  elements_to_save : list[int] = [0],
                  get_end_loss : bool = False):
  #Arrays to store training history
  test_loss_history = [[] for _ in range(NUM_ELEMENTS)]
  epoch_counter = []
  train_loss_history = [[] for _ in range(NUM_ELEMENTS)]
  best_losses = [99999999 for _ in elements_to_save]
  best_epochs = [0 for _ in elements_to_save]
  epochsSinceLastImprove = 0
  
  train_generator = torch.utils.data.DataLoader(training_dataset, batch_size=batch_size, shuffle=True)
  test_generator = torch.utils.data.DataLoader(testing_dataset, batch_size=batch_size, shuffle=False)
  
  scheduler = Scheduler(network.optimizer, [[0,1], [2], [3], [4], [5], [6], [7]], verbose=False,
                        factor=0.5, patience=5, cooldown=5)
  
  iterable = range(num_epochs)
  if not should_output:
    iterable = tqdm(iterable, leave=False, desc="Training")
  for epoch in iterable:
    avg_loss = train(network, train_generator, len(training_dataset), network.optimizer, is_hitter=is_hitter, trainingFraction=epoch / num_epochs)
    test_loss = test(network, test_generator, len(testing_dataset), is_hitter=is_hitter, trainingFraction=epoch / num_epochs)

    training_dataset.increase_variant()

    scheduler.step(test_loss)
    logResults(epoch, num_epochs, avg_loss[elements_to_save[0]], test_loss[elements_to_save[0]], logging_interval, should_output)
    
    for n in range(NUM_ELEMENTS):
      train_loss_history[n].append(avg_loss[n])
      test_loss_history[n].append(test_loss[n])
    epoch_counter.append(epoch)
    
    anyImproved : bool = False
    for i, el in enumerate(elements_to_save):
      if (test_loss[el] < best_losses[i]):
        best_losses[i] = test_loss[el]
        torch.save(network.state_dict(), model_name + "_" + ELEMENT_LIST[el] + ".pt")
        anyImproved = True
        best_epochs[i] = epoch
    if anyImproved:
      epochsSinceLastImprove = 0
    else:
      epochsSinceLastImprove += 1
      
    if epochsSinceLastImprove >= early_stopping_cutoff:
      if should_output:
        print("Stopped Training Early")
      break

  if should_output:
    for i, el in enumerate(elements_to_save):
      print(f"Best result for {ELEMENT_LIST[el]} at epoch={best_epochs[i]} with loss={best_losses[i]}")

    for n in range(NUM_ELEMENTS):
      graphLoss(epoch_counter, train_loss_history[n], test_loss_history[n], title=ELEMENT_LIST[n], start=1)
  
  if save_last:
    for el in elements_to_save:
      torch.save(network.state_dict(), model_name + "_" + ELEMENT_LIST[el] + ".pt")
  
  if get_end_loss:
    return test_loss
  
  return best_losses