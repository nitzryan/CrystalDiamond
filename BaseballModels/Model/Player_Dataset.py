import torch

class Player_Dataset(torch.utils.data.Dataset):
    def __init__(self, data, lengths, labels, mask_labels, mask_stats, year_mask, year_stats, year_positions, mlb_value_mask, mlb_value_stats):
        self.data = data
        self.lengths = lengths
        
        self.twar_buckets = labels[:,:,0]
        self.value_buckets = labels[:,:,1]
        self.pwar_buckets = labels[:,:,2]
        self.level_buckets = labels[:,:,3]
        self.pa_buckets = labels[:,:,4]
        
        self.mask_labels = mask_labels
        self.mask_stats = mask_stats
        
        self.year_mask = year_mask
        self.year_stats = year_stats
        self.year_positions = year_positions
        
        self.mlb_value_mask = mlb_value_mask
        self.mlb_value_stats = mlb_value_stats
    
    def __len__(self):
        return self.data.size(dim=1)
    
    def should_augment_data(self, should_augment):
        self.should_augment = should_augment
        
    def __getitem__(self, idx):
        return self.data[:,idx], self.lengths[idx], self.twar_buckets[:,idx], self.value_buckets[:,idx], self.pwar_buckets[:,idx], self.level_buckets[:,idx], self.pa_buckets[:,idx], self.mask_labels[:,idx], self.mask_stats[:,idx], self.year_mask[:,idx], self.year_stats[:,idx], self.year_positions[:,idx], self.mlb_value_mask[:,idx], self.mlb_value_stats[:,idx]