import matplotlib.pyplot as plt
import torch
from tqdm import tqdm
from Constants import device

PWAR_LOSS_MULTIPLIER = 0.4
LEVEL_LOSS_MULTIPLIER = 0.6
PA_LOSS_MULTIPLIER = 0.8
STATS_LOSS_MULTIPLIER = 0.6
POSITION_LOSS_MULTIPLIER = 1.0
TWAR_LOSS_MULTIPLIER = 0.15
VALUE_LOSS_MULTIPLIER = 0.15
LOSS_ITEM = 0

NUM_ELEMENTS = 7

def train(network,  data_generator, num_elements, loss_function, loss_function_stats, loss_function_position, optimizer, logging = 200, should_output=True):
  network.train() #updates any network layers that behave differently in training and execution
  avg_loss = [0] * NUM_ELEMENTS
  num_batches = 0
  for batch, (data, length, target_war, target_value, target_pwar, target_level, target_pa, mask_labels, mask_stats, year_mask, year_stats, year_position) in enumerate(data_generator):
    data, length = data.to(device), length.to(device)
    target_war, target_value, target_pwar, target_level, target_pa = target_war.to(device), target_value.to(device), target_pwar.to(device), target_level.to(device), target_pa.to(device)
    mask_stats = mask_stats.to(device)
    mask_labels = mask_labels.to(device)
    year_mask, year_stats, year_position = year_mask.to(device), year_stats.to(device), year_position.to(device)
    optimizer.zero_grad()
    output_war, output_value, output_pwar, output_level, output_pa, output_yearStats, output_yearPos = network(data, length)
    loss_war, loss_value, loss_pwar, loss_level, loss_pa = loss_function(output_war, output_value, output_pwar, output_level, output_pa, target_war, target_value, target_pwar, target_level, target_pa, mask_labels)
    
    loss_war = loss_war * TWAR_LOSS_MULTIPLIER
    loss_value = loss_value * VALUE_LOSS_MULTIPLIER
    loss_pwar = PWAR_LOSS_MULTIPLIER * loss_pwar
    loss_level = LEVEL_LOSS_MULTIPLIER * loss_level
    loss_pa = PA_LOSS_MULTIPLIER * loss_pa
    
    loss_war.backward(retain_graph=True)
    loss_value.backward(retain_graph=True)
    loss_pwar.backward(retain_graph=True)
    loss_level.backward(retain_graph=True)
    loss_pa.backward(retain_graph=True)
    
    loss_yearStats = STATS_LOSS_MULTIPLIER * loss_function_stats(output_yearStats, year_stats, year_mask)
    loss_yearPos = POSITION_LOSS_MULTIPLIER * loss_function_position(output_yearPos, year_position, year_mask)
    loss_yearStats.backward(retain_graph=True)
    loss_yearPos.backward()
    
    optimizer.step()
    avg_loss[0] += loss_war.item() / TWAR_LOSS_MULTIPLIER
    avg_loss[1] += loss_pwar.item() / PWAR_LOSS_MULTIPLIER
    avg_loss[2] += loss_level.item() / LEVEL_LOSS_MULTIPLIER
    avg_loss[3] += loss_pa.item() / PA_LOSS_MULTIPLIER
    avg_loss[4] += loss_yearStats.item() / STATS_LOSS_MULTIPLIER
    avg_loss[5] += loss_yearPos.item() / POSITION_LOSS_MULTIPLIER
    avg_loss[6] += loss_value.item() / VALUE_LOSS_MULTIPLIER
    num_batches += 1
    if should_output and ((batch+1)%logging == 0): print('Batch [%d/%d], Train Loss: %.4f' %(batch+1, len(data_generator.dataset)/len(output_war), avg_loss/num_batches))
  
  for n in range(NUM_ELEMENTS):
    avg_loss[n] /= num_elements
  return avg_loss

def test(network, test_loader, num_elements, loss_function, loss_function_stats, loss_function_position):
  network.eval() #updates any network layers that behave differently in training and execution
  avg_loss = [0] * NUM_ELEMENTS
  num_batches = 0
  with torch.no_grad():
    for data, length, target_war, target_value, target_pwar, target_level, target_pa, mask_labels, mask_stats, year_mask, year_stats, year_position in test_loader:
      data, length = data.to(device), length.to(device)
      target_war, target_value, target_pwar, target_level, target_pa = target_war.to(device), target_value.to(device), target_pwar.to(device), target_level.to(device), target_pa.to(device)
      mask_stats = mask_stats.to(device)
      mask_labels = mask_labels.to(device)
      year_mask, year_stats, year_position = year_mask.to(device), year_stats.to(device), year_position.to(device)
      output_war, output_value, output_pwar, output_level, output_pa, output_yearStats, output_yearPos = network(data, length)
      
      loss_war, loss_value, loss_pwar, loss_level, loss_pa = loss_function(output_war, output_value, output_pwar, output_level, output_pa, target_war, target_value, target_pwar, target_level, target_pa, mask_labels)
      loss_yearStats = loss_function_stats(output_yearStats, year_stats, year_mask)
      loss_yearPos = loss_function_position(output_yearPos, year_position, year_mask)
      
      loss_war = TWAR_LOSS_MULTIPLIER * loss_war
      loss_pwar = PWAR_LOSS_MULTIPLIER * loss_pwar
      loss_level = LEVEL_LOSS_MULTIPLIER * loss_level
      loss_pa = PA_LOSS_MULTIPLIER * loss_pa
      loss_value = VALUE_LOSS_MULTIPLIER * loss_value
      loss_yearStats = STATS_LOSS_MULTIPLIER * loss_yearStats
      loss_yearPos = POSITION_LOSS_MULTIPLIER * loss_yearPos
      
      avg_loss[0] += loss_war.item() / TWAR_LOSS_MULTIPLIER
      avg_loss[1] += loss_pwar.item() / PWAR_LOSS_MULTIPLIER
      avg_loss[2] += loss_level.item() / LEVEL_LOSS_MULTIPLIER
      avg_loss[3] += loss_pa.item() / PA_LOSS_MULTIPLIER
      avg_loss[4] += loss_yearStats.item() / STATS_LOSS_MULTIPLIER
      avg_loss[5] += loss_yearPos.item() / POSITION_LOSS_MULTIPLIER
      avg_loss[6] += loss_value.item() / VALUE_LOSS_MULTIPLIER
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

def trainAndGraph(network, training_generator, testing_generator, num_train : int, num_test : int, loss_function, loss_function_stats, loss_function_position, optimizer, scheduler, num_epochs, logging_interval=1, early_stopping_cutoff=20, should_output=True, graph_y_range=None, model_name="no_name.pt", save_last=False):
  #Arrays to store training history
  test_loss_history = [[] for _ in range(NUM_ELEMENTS)]
  epoch_counter = []
  train_loss_history = [[] for _ in range(NUM_ELEMENTS)]
  last_loss = 999999
  best_loss = 999999
  best_epoch = 0
  epochsSinceLastImprove = 0
  
  iterable = range(num_epochs)
  if not should_output:
    iterable = tqdm(iterable, leave=False, desc="Training")
  for epoch in iterable:
    avg_loss = train(network, training_generator, num_train, loss_function, loss_function_stats, loss_function_position, optimizer, should_output=should_output)
    test_loss = test(network, testing_generator, num_test, loss_function, loss_function_stats, loss_function_position)

    scheduler.step(test_loss[LOSS_ITEM])
    logResults(epoch, num_epochs, avg_loss[LOSS_ITEM], test_loss[LOSS_ITEM], logging_interval, should_output)
    
    for n in range(NUM_ELEMENTS):
      train_loss_history[n].append(avg_loss[n])
      test_loss_history[n].append(test_loss[n])
    epoch_counter.append(epoch)
    
    if (test_loss[LOSS_ITEM] < best_loss):
      best_loss = test_loss[LOSS_ITEM]
      best_epoch = epoch
      torch.save(network.state_dict(), model_name)
      epochsSinceLastImprove = 0
    else:
      epochsSinceLastImprove += 1
      
    if epoch > 400 and epochsSinceLastImprove >= early_stopping_cutoff:
      if should_output:
        print("Stopped Training Early")
      break
    
    # Allow model to change fast to get decent baseline, than adjust slower without waiting for scheduler
    if optimizer.param_groups[0]['lr'] > 0.0015 and best_loss < 5.3:
      if should_output:
        print("Reducing learning rate after intial fast learning period")
      for param_group in optimizer.param_groups:
        param_group['lr'] = 0.0015

  if should_output:
    print(f"Best result at epoch={best_epoch} with loss={best_loss}")

    graphLoss(epoch_counter, train_loss_history[0], test_loss_history[0], start=5, graph_y_range=graph_y_range, title="Total WAR")
    graphLoss(epoch_counter, train_loss_history[1], test_loss_history[1], start=5, title="Peak WAR")
    graphLoss(epoch_counter, train_loss_history[2], test_loss_history[2], start=5, title="Level")
    graphLoss(epoch_counter, train_loss_history[3], test_loss_history[3], start=5, title="PA")
    graphLoss(epoch_counter, train_loss_history[4], test_loss_history[4], start=5, title="Year Stats Prediction")
    graphLoss(epoch_counter, train_loss_history[5], test_loss_history[5], start=5, title="Year Position Prediction")
    graphLoss(epoch_counter, train_loss_history[6], test_loss_history[6], start=5, title="Value Prediction")
  
  if save_last:
    torch.save(network.state_dict(), model_name)
  return best_loss