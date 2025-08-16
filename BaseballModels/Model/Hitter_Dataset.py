import torch
import warnings

from Constants import HITTER_TOTAL_WAR_BUCKETS, HITTER_LEVEL_BUCKETS, HITTER_PA_BUCKETS

class Hitter_Dataset(torch.utils.data.Dataset):
    def __init__(self, data, lengths, labels):
        self.data = data
        self.lengths = lengths
        with warnings.catch_warnings(): # Get warning for data copy, which is okay since this is only run once
            warnings.filterwarnings("ignore", category=UserWarning, message='.*non-contiguous.*')
            self.twar_buckets = labels[:,:,0]
            self.pwar_buckets = labels[:,:,1]
            self.level_buckets = labels[:,:,2]
            self.pa_buckets = labels[:,:,3]
            
    def __len__(self):
        return self.data.size(dim=1)
    
    def should_augment_data(self, should_augment):
        self.should_augment = should_augment
        
    def __getitem__(self, idx):
        return self.data[:,idx], self.lengths[idx], self.twar_buckets[:,idx], self.pwar_buckets[:,idx], self.level_buckets[:,idx], self.pa_buckets[:,idx]