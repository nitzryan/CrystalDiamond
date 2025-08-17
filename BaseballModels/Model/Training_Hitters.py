import sys
from Data_Prep import Data_Prep
from sklearn.model_selection import train_test_split # type: ignore
import torch
from Hitter_Dataset import Hitter_Dataset
from Hitter_Model import LSTM_Model, Classification_Loss
from torch.optim import lr_scheduler
import Model_Train
from tqdm import tqdm
from Constants import device, db

if __name__ == "__main__":
    num_models = int(sys.argv[1])
    if num_models < 0:
        exit(1)
        
    data_prep = Data_Prep()
    hitters, inputs, outputs, max_input_size = data_prep.Generate_IO_Hitters("WHERE (lastProspectYear<? OR lastMLBSeason<?) AND isHitter=?", (10000,10000,1))
    
    batch_size = 1000
    hitting_mutators = data_prep.Generate_Hitting_Mutators(batch_size, max_input_size)
    
    cursor = db.cursor()
    cursor.execute("DELETE FROM Model_TrainingHistory WHERE ModelName='Hitter'")
    db.commit()
    for i in tqdm(range(num_models), desc="Training Hitter Models"):
        x_train, x_test, y_train, y_test = train_test_split(inputs, outputs, test_size=0.25, random_state=i)

        train_lengths = torch.tensor([len(seq) for seq in x_train])
        test_lengths = torch.tensor([len(seq) for seq in x_test])

        x_train_padded = torch.nn.utils.rnn.pad_sequence(x_train)
        x_test_padded = torch.nn.utils.rnn.pad_sequence(x_test)
        y_train_padded = torch.nn.utils.rnn.pad_sequence(y_train)
        y_test_padded = torch.nn.utils.rnn.pad_sequence(y_test)
        train_hitters_dataset = Hitter_Dataset(x_train_padded, train_lengths, y_train_padded)
        test_hitters_dataset = Hitter_Dataset(x_test_padded, test_lengths, y_test_padded)
        
        # Setup Model
        num_layers = 3
        hidden_size = 30
        network = LSTM_Model(x_train_padded[0].shape[1], num_layers, hidden_size, hitting_mutators)
        network = network.to(device)
        
        optimizer = torch.optim.Adam(network.parameters(), lr=0.003)
        scheduler = lr_scheduler.ReduceLROnPlateau(optimizer, factor=0.5, patience=20, cooldown=5)
        loss_function = Classification_Loss
        
        num_epochs = 1000
        training_generator = torch.utils.data.DataLoader(train_hitters_dataset, batch_size=batch_size, shuffle=True)
        testing_generator = torch.utils.data.DataLoader(test_hitters_dataset, batch_size=batch_size, shuffle=False)
        
        model_name = f"Hitter_{i}"
        best_loss = Model_Train.trainAndGraph(network, training_generator, testing_generator, loss_function, optimizer, scheduler, num_epochs, logging_interval=10000, early_stopping_cutoff=40, should_output=False, model_name=f"Models/{model_name}.pt")
        
        cursor = db.cursor()
        cursor.execute("INSERT INTO Model_TrainingHistory VALUES (?,?,?,?)", ("Hitter", 1, best_loss, i))
        db.commit()