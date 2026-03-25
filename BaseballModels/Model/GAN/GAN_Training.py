from GAN.Discriminator import Discriminator
from GAN.Generator import Generator
from GAN.KS_Test import KS_Test_TimeSeries
from GAN.GAN_Scheduler import GAN_Scheduler
from DataPrep.Player_Dataset import Player_Dataset
from Utilities import GetModelMaps
from DataPrep.Data_Prep import Data_Prep
from DataPrep.Player_Dataset import Create_Test_Train_Datasets

from torch.utils.data.dataloader import DataLoader
from Constants import device

import torch
import torch.nn as nn

from tqdm import tqdm

_EPOCH_PRINT_INTERVAL = 25

def TrainGAN(
        dataset : Player_Dataset,
        num_epochs : int = 100,
        batch_size : int = 800,
        latent_dim : int = 100,
        generator_hidden_size : int = 100,
        discriminator_hidden_size : int = 100,
        bio_size : int = 0,
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
    
    generator = torch.compile(generator, backend="aot_eager")
    descriminator = torch.compile(descriminator, backend="aot_eager")
    
    scheduler = GAN_Scheduler(generator, descriminator)
    loss_fun = nn.BCELoss()
    
    real_ks_data = torch.zeros((len(dataset), dataset.get_max_length(), dataset.get_input_size())).cpu()
    fake_ks_data = torch.zeros_like(real_ks_data).cpu()
    
    try:
        pbar = tqdm(range(num_epochs))
        for epoch in pbar:
            total_gen_loss = 0
            total_desc_loss = 0
            
            ks_data_idx = 0
            for (data, lengths, _, targets, masks, _) in data_loader:
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
                for _ in range(scheduler.d_trains_per_epoch):
                    # Real Data
                    scheduler.d_optimizer.zero_grad()
                    real_validity = descriminator(data, lengths, targets, masks)
                    real_validity = real_validity.reshape((real_validity.shape[0] * real_validity.shape[1],))
                    real_loss = loss_fun(real_validity, torch.ones(real_validity.shape[0], device=device))
                    
                    # Fake Data
                    z = torch.randn(batch_size, latent_dim, device=device)
                    fake_data = generator(z, generator_targets, generator_masks, time_steps, lengths, bio_size).detach()
                    fake_validity = descriminator(fake_data, lengths, targets, masks)
                    fake_loss = loss_fun(fake_validity, torch.zeros(fake_validity.shape, device=device))
                    
                    # Update Descriminator Weights
                    desc_loss = (real_loss + fake_loss) / 2
                    desc_loss.backward()
                    scheduler.d_optimizer.step()
                
                #### Generator ####
                for _ in range(scheduler.g_trains_per_epoch):
                    scheduler.g_optimizer.zero_grad()
                    z = torch.randn(batch_size, latent_dim, device=device)
                    fake_data = generator(z, generator_targets, generator_masks, time_steps, lengths, bio_size)
                    fake_validity = descriminator(fake_data, lengths, targets, masks)
                    gen_loss = loss_fun(fake_validity, torch.ones(fake_validity.shape, device=device))
                    gen_loss.backward()
                    scheduler.g_optimizer.step()
                
                total_gen_loss += gen_loss.item()
                total_desc_loss += desc_loss.item()
                
                # Update KS_Test
                if epoch % _EPOCH_PRINT_INTERVAL == 0:
                    real_ks_data[ks_data_idx:ks_data_idx+batch_size,:,:] = data
                    fake_ks_data[ks_data_idx:ks_data_idx+batch_size,:,:] = fake_data
                    ks_data_idx += batch_size
                    
                
            total_gen_loss /= len(data_loader)
            total_desc_loss /= len(data_loader)
            
            scheduler.Update(total_gen_loss, total_desc_loss)
            
            pbar.set_description(f"Gen: <{scheduler.g_optimizer.param_groups[0]['lr']}x{scheduler.g_trains_per_epoch}>   Desc: <{scheduler.d_optimizer.param_groups[0]['lr']}x{scheduler.d_trains_per_epoch}>")
            
            if epoch % _EPOCH_PRINT_INTERVAL == 0:
                with torch.no_grad():
                    ks_mean, ks_timestep_mean = KS_Test_TimeSeries(real_ks_data, fake_ks_data) 
                tqdm.write(f"Epoch {epoch} | KS_Feat_Mean: {ks_mean:.4f} | KS_Time_Mean: {ks_timestep_mean:.4f} | D_loss: {total_desc_loss:.4f} | G_loss: {total_gen_loss:.4f}")

    except KeyboardInterrupt:
        print("Keyboard Interrupt, Exiting Training")

    finally:
        return generator

if __name__ == "__main__":
    model_idx = 1
    prep_map, output_map = GetModelMaps(model_idx)
    data_prep = Data_Prep(prep_map, output_map)
    hitter_io_list = data_prep.Generate_IO_Hitters("WHERE lastMLBSeason<? AND signingYear<? AND isHitter=?", (2025,2015,1), use_cutoff=True)
    train_dataset, test_dataset = Create_Test_Train_Datasets(hitter_io_list, 0.10, 0)
    
    generator = TrainGAN(train_dataset, num_epochs=1001, 
                         discriminator_hidden_size=100,
                         generator_hidden_size=100,
                         bio_size = prep_map.bio_size)
    torch.save(generator.state_dict(), f"Models/Generators/Generator_{model_idx}.pt")