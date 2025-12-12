import matplotlib.pyplot as plt
import torch
from tqdm import tqdm
from Constants import device
from Player_Model import Stats_Loss, Position_Classification_Loss, Classification_Loss, Mlb_Value_Loss_Hitter, Mlb_Value_Loss_Pitcher, Prospect_WarRegression_Loss, Pt_Loss
from Model_Scheduler import Model_Scheduler_ReduceOnPlateauGroups as Scheduler

LEVEL_LOSS_MULTIPLIER = 0.6
PA_LOSS_MULTIPLIER = 0.8
STATS_LOSS_MULTIPLIER = 0.2
POSITION_LOSS_MULTIPLIER = 1.0
TWAR_LOSS_MULTIPLIER = 0.8
MLB_VALUE_LOSS_MULTIPLIER = 0.2
WAR_REGRESSION_LOSS_MULTIPLIER = 0.6
PT_LOSS_MULTIPLIER = 0.5

ELEMENT_LIST = ["TotalClassification", "Level", "PA", "YearStats", "YearPos", "MLBValue", "Regression", "PlayingTime"]
NUM_ELEMENTS = len(ELEMENT_LIST)

def GetLosses(network, data, length, targets : tuple, masks : tuple, shouldBackprop : bool, is_hitter: bool) -> tuple:
  # Get Model Output
  data = data.to(device)
  length = length.to(device)
  output_war, output_warregression, output_level, output_pa, output_yearStats, output_yearPos, output_mlbValue, output_pt = network(data, length)
  
  # Move targets and masks to GPU
  target_war, target_warregression, target_level, target_pa, target_yearStats, target_yearPos, target_mlbValue, target_pt = targets
  mask_labels, mask_year, mask_stats, mask_mlbValue = masks
  target_war, target_warregression, target_level, target_pa, target_yearStats, target_yearPos, target_mlbValue, target_pt = target_war.to(device), target_warregression.to(device), target_level.to(device), target_pa.to(device), target_yearStats.to(device), target_yearPos.to(device), target_mlbValue.to(device), target_pt.to(device)
  mask_labels, mask_year, mask_stats, mask_mlbValue = mask_labels.to(device), mask_year.to(device), mask_stats.to(device), mask_mlbValue.to(device)
  
  # Get losses
  loss_war, loss_level, loss_pa = Classification_Loss(output_war, output_level, output_pa, target_war, target_level, target_pa, mask_labels)
  loss_warregression = Prospect_WarRegression_Loss(output_warregression, target_warregression, mask_labels)
  loss_yearStats = Stats_Loss(output_yearStats, target_yearStats, mask_stats)
  loss_yearPt = Pt_Loss(output_pt, target_pt)
  loss_yearPos = Position_Classification_Loss(output_yearPos, target_yearPos, mask_year)
  loss_mlbValue = Mlb_Value_Loss_Hitter(output_mlbValue, target_mlbValue, mask_mlbValue) if is_hitter else Mlb_Value_Loss_Pitcher(output_mlbValue, target_mlbValue, mask_mlbValue)
  
  if shouldBackprop:
    (loss_war * TWAR_LOSS_MULTIPLIER).backward(retain_graph=True)
    (loss_level * LEVEL_LOSS_MULTIPLIER).backward(retain_graph=True)
    (loss_pa * PA_LOSS_MULTIPLIER).backward(retain_graph=True)
    (loss_warregression * WAR_REGRESSION_LOSS_MULTIPLIER).backward(retain_graph=True)
    (loss_yearStats * STATS_LOSS_MULTIPLIER).backward(retain_graph=True)
    (loss_yearPos * POSITION_LOSS_MULTIPLIER).backward(retain_graph=True)
    (loss_yearPt * PT_LOSS_MULTIPLIER).backward(retain_graph=True)
    (loss_mlbValue * MLB_VALUE_LOSS_MULTIPLIER).backward()
  
  return (loss_war, loss_level, loss_pa, loss_warregression, loss_yearStats, loss_yearPos, loss_mlbValue, loss_yearPt)

def train(network, data_generator, num_elements, optimizer, is_hitter : bool, should_output=True):
  
  network.train() #updates any network layers that behave differently in training and execution
  avg_loss = [0] * NUM_ELEMENTS
  num_batches = 0
  for batch, (data, length, target_war, target_warregression, target_level, target_pa, mask_labels, mask_stats, year_mask, target_yearStats, target_year_position, mlb_value_mask, target_mlb_value, target_pt) in enumerate(data_generator):
    optimizer.zero_grad()
    loss_war, loss_level, loss_pa, loss_warregression, loss_yearStats, loss_yearPos, loss_mlbValue, loss_yearPt = GetLosses(network, data, length, (target_war, target_warregression, target_level, target_pa, target_yearStats, target_year_position, target_mlb_value, target_pt), (mask_labels, year_mask, mask_stats, mlb_value_mask), True, is_hitter)
    torch.nn.utils.clip_grad_norm_(network.parameters(), max_norm=0.05)
    optimizer.step()
    avg_loss[0] += loss_war.item()
    avg_loss[1] += loss_level.item()
    avg_loss[2] += loss_pa.item()
    avg_loss[3] += loss_yearStats.item()
    avg_loss[4] += loss_yearPos.item()
    avg_loss[5] += loss_mlbValue.item()
    avg_loss[6] += loss_warregression.item()
    avg_loss[7] += loss_yearPt.item()
    num_batches += 1
  
  for n in range(NUM_ELEMENTS):
    avg_loss[n] /= num_elements
  return avg_loss

def test(network, test_loader, num_elements, is_hitter : bool):
  network.eval() #updates any network layers that behave differently in training and execution
  avg_loss = [0] * NUM_ELEMENTS
  num_batches = 0
  with torch.no_grad():
    for data, length, target_war, target_warregression, target_level, target_pa, mask_labels, mask_stats, year_mask, target_yearStats, target_year_position, mlb_value_mask, target_mlb_value, target_pt in test_loader:
      loss_war, loss_level, loss_pa, loss_warregression, loss_yearStats, loss_yearPos, loss_mlbValue, loss_yearPt = GetLosses(network, data, length, (target_war, target_warregression, target_level, target_pa, target_yearStats, target_year_position, target_mlb_value, target_pt), (mask_labels, year_mask, mask_stats, mlb_value_mask), False, is_hitter)
      
      avg_loss[0] += loss_war.item()
      avg_loss[1] += loss_level.item()
      avg_loss[2] += loss_pa.item()
      avg_loss[3] += loss_yearStats.item()
      avg_loss[4] += loss_yearPos.item()
      avg_loss[5] += loss_mlbValue.item()
      avg_loss[6] += loss_warregression.item()
      avg_loss[7] += loss_yearPt.item()
      num_batches += 1
  
  for n in range(NUM_ELEMENTS):
    avg_loss[n] /= num_elements
  return avg_loss

def count_parameters(model):
    return sum(p.numel() for p in model.parameters() if p.requires_grad)

def logResults(epoch, num_epochs, train_loss, test_loss, print_interval=1000, should_output=True):
  if should_output and (epoch%print_interval == 0):  
    print('Epoch [%d/%d], Train Loss: %.4f, Test Loss: %.4f' %(epoch+1, num_epochs, train_loss, test_loss))

def graphLoss(epoch_counter, train_loss_hist, test_loss_hist, loss_name="Loss", start = 0, graph_y_range=None, title=""):
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
                  elements_to_save : list[int] = [0]):
  #Arrays to store training history
  test_loss_history = [[] for _ in range(NUM_ELEMENTS)]
  epoch_counter = []
  train_loss_history = [[] for _ in range(NUM_ELEMENTS)]
  best_losses = [99999999 for _ in elements_to_save]
  best_epochs = [0 for _ in elements_to_save]
  epochsSinceLastImprove = 0
  
  train_generator = torch.utils.data.DataLoader(training_dataset, batch_size=batch_size, shuffle=True)
  test_generator = torch.utils.data.DataLoader(testing_dataset, batch_size=batch_size, shuffle=False)
  
  scheduler = Scheduler(network.optimizer, [[0,1], [2], [3], [4], [5], [6], [7], [8]], verbose=False,
                        factor = 0.5, patience=20, cooldown=10)
  
  iterable = range(num_epochs)
  if not should_output:
    iterable = tqdm(iterable, leave=False, desc="Training")
  for epoch in iterable:
    avg_loss = train(network, train_generator, len(training_dataset), network.optimizer, should_output=should_output, is_hitter=is_hitter)
    test_loss = test(network, test_generator, len(testing_dataset), is_hitter=is_hitter)

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
    
    # Allow model to change fast to get decent baseline, than adjust slower without waiting for scheduler
    # if optimizer.param_groups[0]['lr'] > 0.0015 and epoch >= 10:
    #   if should_output:
    #     print("Reducing learning rate after intial fast learning period")
    #   for param_group in optimizer.param_groups:
    #     param_group['lr'] = 0.0015

  if should_output:
    for i, el in enumerate(elements_to_save):
      print(f"Best result for {ELEMENT_LIST[el]} at epoch={best_epochs[i]} with loss={best_losses[i]}")

    for n in range(NUM_ELEMENTS):
      graphLoss(epoch_counter, train_loss_history[n], test_loss_history[n], title=ELEMENT_LIST[n], start=3)
  
  if save_last:
    for el in elements_to_save:
      torch.save(network.state_dict(), model_name + "_" + ELEMENT_LIST[el] + ".pt")
  return best_losses