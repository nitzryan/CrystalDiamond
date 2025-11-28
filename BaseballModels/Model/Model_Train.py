import matplotlib.pyplot as plt
import torch
from tqdm import tqdm
from Constants import device
from Player_Model import Stats_Loss, Position_Classification_Loss, Classification_Loss, Mlb_Value_Loss_Hitter, Mlb_Value_Loss_Pitcher, Prospect_Value_Loss, Classification_War_Regression

PWAR_LOSS_MULTIPLIER = 0.4
LEVEL_LOSS_MULTIPLIER = 0.6
PA_LOSS_MULTIPLIER = 0.8
STATS_LOSS_MULTIPLIER = 0.6
POSITION_LOSS_MULTIPLIER = 1.0
TWAR_LOSS_MULTIPLIER = 0.8
VALUE_LOSS_MULTIPLIER = 0.15
MLB_VALUE_LOSS_MULTIPLIER = 0.2
WAR_VALUE_LOSS_MULTIPLIER = 0.6
DEFAULT_LOSS_ITEM = 8

ELEMENT_LIST = ["TotalClassification", "PeakClassification", "Level", "PA", "YearStats", "YearPos", "Value", "MLBValue", "Regression", "ClassToReg"]
NUM_ELEMENTS = len(ELEMENT_LIST)

def train(network,  data_generator, num_elements, optimizer, is_hitter : bool, logging = 200, should_output=True):
  network.train() #updates any network layers that behave differently in training and execution
  avg_loss = [0] * NUM_ELEMENTS
  num_batches = 0
  for batch, (data, length, target_war, target_warvalue, target_value, target_pwar, target_level, target_pa, mask_labels, mask_stats, year_mask, year_stats, year_position, mlb_value_mask, target_mlb_value) in enumerate(data_generator):
    data, length = data.to(device), length.to(device)
    target_war = target_war.to(device)
    target_warvalue = target_warvalue.to(device)
    target_value = target_value.to(device)
    target_pwar = target_pwar.to(device)
    target_level = target_level.to(device)
    target_pa = target_pa.to(device)
    mask_stats = mask_stats.to(device)
    mask_labels = mask_labels.to(device)
    mlb_value_mask, target_mlb_value = mlb_value_mask.to(device), target_mlb_value.to(device)
    year_mask, year_stats, year_position = year_mask.to(device), year_stats.to(device), year_position.to(device)
    optimizer.zero_grad()
    output_war, output_warvalue, output_value, output_pwar, output_level, output_pa, output_yearStats, output_yearPos, output_mlbValue = network(data, length)
    loss_war, loss_value, loss_pwar, loss_level, loss_pa = Classification_Loss(output_war, output_value, output_pwar, output_level, output_pa, target_war, target_value, target_pwar, target_level, target_pa, mask_labels)
    
    loss_war = loss_war * TWAR_LOSS_MULTIPLIER
    loss_value = loss_value * VALUE_LOSS_MULTIPLIER
    loss_pwar = PWAR_LOSS_MULTIPLIER * loss_pwar
    loss_level = LEVEL_LOSS_MULTIPLIER * loss_level
    loss_pa = PA_LOSS_MULTIPLIER * loss_pa
    
    loss_warvalue = Prospect_Value_Loss(output_warvalue, target_warvalue, mask_labels)
    loss_warvalue = WAR_VALUE_LOSS_MULTIPLIER * loss_warvalue
    
    loss_classification_to_regression = WAR_VALUE_LOSS_MULTIPLIER * Classification_War_Regression(network, output_war, target_warvalue, mask_labels)
    loss_classification_to_regression.backward(retain_graph=True)
    
    loss_war.backward(retain_graph=True)
    loss_value.backward(retain_graph=True)
    loss_pwar.backward(retain_graph=True)
    loss_level.backward(retain_graph=True)
    loss_pa.backward(retain_graph=True)
    
    loss_warvalue.backward(retain_graph=True)
    
    loss_yearStats = STATS_LOSS_MULTIPLIER * Stats_Loss(output_yearStats, year_stats, year_mask)
    loss_yearPos = POSITION_LOSS_MULTIPLIER * Position_Classification_Loss(output_yearPos, year_position, year_mask)
    loss_yearStats.backward(retain_graph=True)
    loss_yearPos.backward(retain_graph=True)
    
    loss_mlbValue = (Mlb_Value_Loss_Hitter(output_mlbValue, target_mlb_value, mlb_value_mask) if is_hitter else Mlb_Value_Loss_Pitcher(output_mlbValue, target_mlb_value, mlb_value_mask))
    loss_mlbValue = loss_mlbValue * MLB_VALUE_LOSS_MULTIPLIER
    loss_mlbValue.backward()
    
    optimizer.step()
    avg_loss[0] += loss_war.item() / TWAR_LOSS_MULTIPLIER
    avg_loss[1] += loss_pwar.item() / PWAR_LOSS_MULTIPLIER
    avg_loss[2] += loss_level.item() / LEVEL_LOSS_MULTIPLIER
    avg_loss[3] += loss_pa.item() / PA_LOSS_MULTIPLIER
    avg_loss[4] += loss_yearStats.item() / STATS_LOSS_MULTIPLIER
    avg_loss[5] += loss_yearPos.item() / POSITION_LOSS_MULTIPLIER
    avg_loss[6] += loss_value.item() / VALUE_LOSS_MULTIPLIER
    avg_loss[7] += loss_mlbValue.item() / MLB_VALUE_LOSS_MULTIPLIER
    avg_loss[8] += loss_warvalue.item() / WAR_VALUE_LOSS_MULTIPLIER
    avg_loss[9] += loss_classification_to_regression.item() / WAR_VALUE_LOSS_MULTIPLIER
    num_batches += 1
    if should_output and ((batch+1)%logging == 0): print('Batch [%d/%d], Train Loss: %.4f' %(batch+1, len(data_generator.dataset)/len(output_war), avg_loss/num_batches))
  
  for n in range(NUM_ELEMENTS):
    avg_loss[n] /= num_elements
  return avg_loss

def test(network, test_loader, num_elements, is_hitter : bool):
  network.eval() #updates any network layers that behave differently in training and execution
  avg_loss = [0] * NUM_ELEMENTS
  num_batches = 0
  with torch.no_grad():
    for data, length, target_war, target_warvalue, target_value, target_pwar, target_level, target_pa, mask_labels, mask_stats, year_mask, year_stats, year_position, mlb_value_mask, target_mlb_value in test_loader:
      data, length = data.to(device), length.to(device)
      target_war, target_warvalue, target_value, target_pwar, target_level, target_pa = target_war.to(device), target_warvalue.to(device), target_value.to(device), target_pwar.to(device), target_level.to(device), target_pa.to(device)
      mask_stats = mask_stats.to(device)
      mask_labels = mask_labels.to(device)
      mlb_value_mask, target_mlb_value = mlb_value_mask.to(device), target_mlb_value.to(device)
      year_mask, year_stats, year_position = year_mask.to(device), year_stats.to(device), year_position.to(device)
      output_war, output_warvalue, output_value, output_pwar, output_level, output_pa, output_yearStats, output_yearPos, output_mlbValue = network(data, length)
      
      loss_war, loss_value, loss_pwar, loss_level, loss_pa = Classification_Loss(output_war, output_value, output_pwar, output_level, output_pa, target_war, target_value, target_pwar, target_level, target_pa, mask_labels)
      loss_yearStats = Stats_Loss(output_yearStats, year_stats, year_mask)
      loss_yearPos = Position_Classification_Loss(output_yearPos, year_position, year_mask)
      loss_mlbValue = (Mlb_Value_Loss_Hitter(output_mlbValue, target_mlb_value, mlb_value_mask) if is_hitter else Mlb_Value_Loss_Pitcher(output_mlbValue, target_mlb_value, mlb_value_mask))
      
      loss_warvalue = Prospect_Value_Loss(output_warvalue, target_warvalue, mask_labels)
      
      loss_classification_to_regression = Classification_War_Regression(network, output_war, target_warvalue, mask_labels)
      
      avg_loss[0] += loss_war.item()
      avg_loss[1] += loss_pwar.item()
      avg_loss[2] += loss_level.item()
      avg_loss[3] += loss_pa.item()
      avg_loss[4] += loss_yearStats.item()
      avg_loss[5] += loss_yearPos.item()
      avg_loss[6] += loss_value.item()
      avg_loss[7] += loss_mlbValue.item()
      avg_loss[8] += loss_warvalue.item()
      avg_loss[9] += loss_classification_to_regression.item()
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
                  training_generator, 
                  testing_generator, 
                  num_train : int, 
                  num_test : int, 
                  optimizer, 
                  scheduler, 
                  num_epochs, 
                  logging_interval=1, 
                  early_stopping_cutoff=20, 
                  should_output=True, 
                  graph_y_range=None, 
                  model_name="no_name", 
                  save_last=False, 
                  is_hitter=True,
                  elements_to_save : list[int] = [DEFAULT_LOSS_ITEM]):
  #Arrays to store training history
  test_loss_history = [[] for _ in range(NUM_ELEMENTS)]
  epoch_counter = []
  train_loss_history = [[] for _ in range(NUM_ELEMENTS)]
  best_losses = [99999999 for _ in elements_to_save]
  best_epochs = [0 for _ in elements_to_save]
  epochsSinceLastImprove = 0
  
  iterable = range(num_epochs)
  if not should_output:
    iterable = tqdm(iterable, leave=False, desc="Training")
  for epoch in iterable:
    avg_loss = train(network, training_generator, num_train, optimizer, should_output=should_output, is_hitter=is_hitter)
    test_loss = test(network, testing_generator, num_test, is_hitter=is_hitter)

    scheduler.step(test_loss[elements_to_save[0]])
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