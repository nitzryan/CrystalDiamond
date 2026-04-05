from Constants import device
import matplotlib.pyplot as plt
import torch
from tqdm import tqdm
from College.Model.College_Model import Classification_Loss, Position_Loss

ELEMENT_LIST = ["DraftPos", "ProPosition"]
NUM_ELEMENTS = len(ELEMENT_LIST)

def GetLosses(network, data, length, targets, masks, shouldBackprop : bool):
    # Get Output
    data = data.to(device)
    length = length.to(device)
    output_draft, output_pos = network(data, length)
    
    max_len = output_draft.size(1)
    mask_length = (torch.arange(max_len, device=length.device)
                   .unsqueeze(0) < length.unsqueeze(1))
    
    # Get losses
    target_draft, target_pos = targets
    mask_pos = masks
    
    target_draft = target_draft.to(device)
    loss_draft = Classification_Loss(output_draft, target_draft, mask_length)
    
    target_pos = target_pos.to(device)
    mask_pos = mask_pos.to(device)
    loss_pos = Position_Loss(output_pos, target_pos, mask_length * mask_pos.unsqueeze(-1))
    
    if shouldBackprop:
      loss_draft.backward(retain_graph=True)
      loss_pos.backward()
    
    return loss_draft, loss_pos

def train(network, data_generator, num_elements, optimizer):
    network.train()
    avg_loss = [0] * NUM_ELEMENTS
    
    for data, length, targets, masks in data_generator:
        optimizer.zero_grad()
        loss_draft, loss_pos = GetLosses(network, data, length, targets, masks, shouldBackprop=True)
        torch.nn.utils.clip_grad_norm_(network.parameters(), max_norm=0.05)
        optimizer.step()
        
        avg_loss[0] += loss_draft.item()
        avg_loss[1] += loss_pos.item()
        
    for n in range(NUM_ELEMENTS):
        avg_loss[n] /= num_elements
    return avg_loss

def test(network, test_loader, num_elements):
    network.eval()
    avg_loss = [0] * NUM_ELEMENTS
    
    with torch.no_grad():
        for data, length, targets, masks in test_loader:
            loss_draft, loss_pos = GetLosses(network, data, length, targets, masks, shouldBackprop=False)
            avg_loss[0] += loss_draft.item()
            avg_loss[1] += loss_pos.item()
        
    for n in range(NUM_ELEMENTS):
        avg_loss[n] /= num_elements
    return avg_loss

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
                  scheduler,
                  optimizer,
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
  
  iterable = range(num_epochs)
  if not should_output:
    iterable = tqdm(iterable, leave=False, desc="Training")
  for epoch in iterable:
    avg_loss = train(network, train_generator, len(training_dataset), optimizer)
    test_loss = test(network, test_generator, len(testing_dataset))

    scheduler.step(test_loss[0])
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