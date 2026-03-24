from GAN.Discriminator import Discriminator
from GAN.Generator import Generator
from GAN.KS_Test import KS_Test_TimeSeries
from DataPrep.Player_Dataset import Player_Dataset
from Utilities import GetModelMaps
from DataPrep.Data_Prep import Data_Prep
from DataPrep.Player_Dataset import Create_Test_Train_Datasets

from torch.utils.data.dataloader import DataLoader
from Constants import device

import torch
import torch.nn as nn
import torch.optim as optim

from tqdm import tqdm

_EPOCH_PRINT_INTERVAL = 25

def TrainGAN(
        dataset : Player_Dataset,
        num_epochs : int = 100,
        batch_size : int = 800,
        latent_dim : int = 100,
        generator_hidden_size : int = 100,
        discriminator_hidden_size : int = 100,
        g_trains_per_epoch : int = 1,
        d_trains_per_epoch : int = 1
    ) -> tuple[Discriminator, Generator]:
    
    data_loader = DataLoader(dataset, batch_size=batch_size // 2, shuffle=True)
    
    feature_size = dataset.get_input_size()
    output_size = dataset.get_output_size() + dataset.get_mask_size()
    
    generator = Generator(latent_dim=latent_dim, 
                          hidden_size=generator_hidden_size,
                          feature_size=feature_size,
                          output_size = output_size,
                          num_layers=2, max_len=200).to(device)
    descriminator = Discriminator(feature_size=feature_size,
                                  output_size=output_size,
                                  hidden_size=discriminator_hidden_size).to(device)
    
    g_optimizer = optim.Adam(generator.parameters(), lr=0.0002, betas=(0.5, 0.999))
    d_optimizer = optim.Adam(descriminator.parameters(), lr=0.0001, betas=(0.5, 0.999))
    loss_fun = nn.BCELoss()
    
    real_ks_data = torch.zeros((len(dataset), dataset.get_max_length(), dataset.get_input_size())).cpu()
    fake_ks_data = torch.zeros_like(real_ks_data).cpu()
    for epoch in tqdm(range(num_epochs)):
        total_gen_loss = 0
        total_desc_loss = 0
        
        ks_data_idx = 0
        for data, lengths, _, targets, masks in data_loader:
            batch_size = data.size(0)
            time_steps = data.size(1)
            
            targets = (targets[0], targets[1], targets[-1].squeeze(-1).long())
            targets = torch.cat(targets, dim=-1).to(device)
            generator_targets = targets.clone().reshape((batch_size, time_steps, targets.shape[1] // time_steps))
            
            masks = (masks[0].long(),)
            masks = torch.cat(masks, dim=-1).to(device)
            generator_masks = masks.clone().reshape((batch_size, time_steps, masks.shape[1] // time_steps))
            
            data, lengths = data.to(device), lengths.to(device)
            
            #### DESCRIMINATOR ####
            for _ in range(d_trains_per_epoch):
                # Real Data
                d_optimizer.zero_grad()
                real_validity = descriminator(data, lengths, targets, masks)
                real_validity = real_validity.reshape((real_validity.shape[0] * real_validity.shape[1],))
                real_loss = loss_fun(real_validity, torch.ones(real_validity.shape[0], device=device))
                
                # Fake Data
                z = torch.randn(batch_size, latent_dim, device=device)
                fake_data = generator(z, generator_targets, generator_masks, time_steps, lengths).detach()
                fake_validity = descriminator(fake_data, lengths, targets, masks)
                fake_loss = loss_fun(fake_validity, torch.zeros(fake_validity.shape, device=device))
                
                # Update Descriminator Weights
                desc_loss = (real_loss + fake_loss) / 2
                desc_loss.backward()
                d_optimizer.step()
            
            #### Generator ####
            for _ in range(g_trains_per_epoch):
                g_optimizer.zero_grad()
                z = torch.randn(batch_size, latent_dim, device=device)
                fake_data = generator(z, generator_targets, generator_masks, time_steps, lengths)
                fake_validity = descriminator(fake_data, lengths, targets, masks)
                gen_loss = loss_fun(fake_validity, torch.ones(fake_validity.shape, device=device))
                gen_loss.backward()
                g_optimizer.step()
            
            total_gen_loss += gen_loss.item()
            total_desc_loss += desc_loss.item()
            
            # Update KS_Test
            if epoch % _EPOCH_PRINT_INTERVAL == 0:
                real_ks_data[ks_data_idx:ks_data_idx+batch_size,:,:] = data
                fake_ks_data[ks_data_idx:ks_data_idx+batch_size,:,:] = fake_data
                ks_data_idx += batch_size
                
            
        total_gen_loss /= len(data_loader)
        total_desc_loss /= len(data_loader)
        
        if epoch % _EPOCH_PRINT_INTERVAL == 0:
            with torch.no_grad():
                ks_mean = KS_Test_TimeSeries(real_ks_data, fake_ks_data) 
            tqdm.write(f"Epoch {epoch} | KS_Mean: {ks_mean:.4f} | D_loss: {total_desc_loss:.4f} | G_loss: {total_gen_loss:.4f}")

    return generator

if __name__ == "__main__":
    model_idx = 1
    prep_map, output_map = GetModelMaps(model_idx)
    data_prep = Data_Prep(prep_map, output_map)
    hitter_io_list = data_prep.Generate_IO_Hitters("WHERE lastMLBSeason<? AND signingYear<? AND isHitter=?", (2025,2015,1), use_cutoff=True)
    train_dataset, test_dataset = Create_Test_Train_Datasets(hitter_io_list, 0.10, 0)
    
    generator = TrainGAN(train_dataset, num_epochs=1001, g_trains_per_epoch=3)
    torch.save(generator.state_dict(), f"Models/Generators/Generator_{model_idx}.pt")