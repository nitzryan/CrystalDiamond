from GAN.Generator import Generator
from GAN.Discriminator import Discriminator

import torch.optim as optim

class GAN_Scheduler:
    def __init__(self, g : Generator, d : Discriminator):
        self.g_optimizer = optim.Adam(g.parameters(), lr=0.0002, betas=(0.5, 0.999))
        self.d_optimizer = optim.Adam(d.parameters(), lr=0.0002, betas=(0.5, 0.999))
        self.g_trains_per_epoch = 1
        self.d_trains_per_epoch = 1
        
    def Update(self, g_loss : float, d_loss : float):
        # Generator
        self.g_trains_per_epoch = 1
        if g_loss < 0.6:
            g_lr = 0.00005
        elif g_loss < 0.8:
            g_lr = 0.0001
        elif g_loss < 1:
            g_lr = 0.0002
            self.g_trains_per_epoch = 2
        elif g_loss < 1.1:
            g_lr = 0.0003
            self.g_trains_per_epoch = 3
        elif g_loss < 1.2:
            g_lr = 0.0004
            self.g_trains_per_epoch = 4
        else:
            g_lr = 0.0006
            self.g_trains_per_epoch = 6
            
        self.g_optimizer.param_groups[0]['lr'] = g_lr
        
        # Descriminator
        self.d_trains_per_epoch = 1
        if d_loss < 0.54:
            d_lr = 0
        elif d_loss < 0.55:
            d_lr = 0.00001
        elif d_loss < 0.57:
            d_lr = 0.00002
        elif d_loss < 0.59:
            d_lr = 0.00005
        elif d_loss < 0.63:
            d_lr = 0.0001
        elif d_loss < 0.7:
            d_lr = 0.00015
        elif d_loss < 0.8:
            d_lr = 0.0001
        elif d_loss < 1:
            d_lr = 0.0001
            self.d_trains_per_epoch = 2
        elif d_loss < 1.25:
            d_lr = 0.0001
            self.d_trains_per_epoch = 3
        elif d_loss < 1.50:
            d_lr = 0.0001
            self.d_trains_per_epoch = 4
        else:
            d_lr = 0.0001
            self.d_trains_per_epoch = 6
            
        self.d_optimizer.param_groups[0]['lr'] = d_lr