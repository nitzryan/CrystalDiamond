# import sys
# from Combined.DataPrep.Data_Prep import Combined_Data_Prep, Combined_IO
# from Combined.DataPrep.Player_Dataset import Create_Test_Train_Datasets
# from Combined.Model.Model_Train import TrainAndGraph
# from tqdm import tqdm
# from Constants import device, model_db
# from Utilities import GetModelMaps, GetCollegeModelMaps

# if __name__ == "__main__":
#     num_models = int(sys.argv[1])
#     if num_models < 0:
#         exit(1)
        
#     model_cursor = model_db.cursor()
#     model_idxs = [("test", 1)]
    
#     for model_name, model_id in tqdm(model_idxs, desc="Training Architectures")