import os, warnings
import torch

from Model.Pro.Model.Player_Model import Recurrent_Model as ProModel
from Model.College.Model.College_Model import RNN_Model as ColModel
from Model.ModelDBTypes import DB_ModelId, DB_WarBucketAverages, DB_Model_TrainingHistory
from Model.Constants import model_db


class ModelCache:
    """One resident, eval()-mode, weights-loaded network per
    (modelId, is_hitter, is_pro, run). Everything is built lazily."""

    def __init__(self, data_prep, model_dir: str, device):
        self.data_prep = data_prep
        self.model_dir = model_dir
        self.device = torch.device(device)
        self._names: dict[int, str] = {}                 # modelId -> modelName
        self._nets: dict[tuple, torch.nn.Module] = {}    # (name, is_hitter, is_pro, run) -> module
        self._wba: dict[bool, torch.Tensor] = {}         # is_hitter -> tensor

    def model_name(self, modelId: int) -> str:
        if modelId not in self._names:
            cur = model_db.cursor()
            self._names[modelId] = DB_ModelId.Select_From_DB(cur, "WHERE id=?", (modelId,))[0].modelName
        return self._names[modelId]

    def war_bucket_averages(self, is_hitter: bool) -> torch.Tensor:
        if is_hitter not in self._wba:
            cur = model_db.cursor()
            w = DB_WarBucketAverages.Select_From_DB(cur, "WHERE isHitter=?", (1 if is_hitter else 0,))[0]
            self._wba[is_hitter] = torch.tensor(
                [0, w.war1, w.war2, w.war3, w.war4, w.war5, w.war6]).to(self.device)
        return self._wba[is_hitter]

    def network(self, modelId: int, is_hitter: bool, is_pro: bool, run: int) -> torch.nn.Module:
        name = self.model_name(modelId)
        pos_str = "hit" if is_hitter else "pit"
        kind_str = "pro" if is_pro else "col"
        
        key = (name, pos_str, kind_str, run)
        net = self._nets.get(key)
        if net is None:
            if is_pro:
                net = ProModel.LoadFromFile(
                    f"{self.model_dir}{name}_{pos_str}_pro.json", self.data_prep.pro_data_prep)
            else:
                net = ColModel.LoadFromFile(
                    f"{self.model_dir}{name}_{pos_str}_col.json", self.data_prep.college_data_prep)
            net = net.to(self.device)
            with warnings.catch_warnings():
                warnings.simplefilter("ignore", FutureWarning)
                sd = torch.load(f"{self.model_dir}{kind_str}_{name}_{run}_{pos_str}.pt",
                                map_location=self.device)
            net.load_state_dict(sd)
            net.eval()
            self._nets[key] = net
        return net

    def preload(self) -> None:
        """Build every network and both WAR tables."""
        cur = model_db.cursor()
        self.war_bucket_averages(True)
        self.war_bucket_averages(False)
        for m in DB_ModelId.Select_From_DB(cur, "", ()):
            self._names[m.id] = m.modelName
            for is_hitter in (True, False):
                runs = DB_Model_TrainingHistory.Select_From_DB(
                    cur, "WHERE ModelName=? AND IsHitter=?",
                    (m.modelName, 1 if is_hitter else 0))
                for th in runs:
                    for is_pro in (True, False):
                        self.network(m.id, is_hitter, is_pro, th.ModelRun)