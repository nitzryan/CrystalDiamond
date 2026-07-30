from Model.ModelDBTypes import DB_WarBucketAverages
import torch
from Model.Constants import model_db

def GetWarBucketAverages(is_hitter : bool) -> torch.Tensor:
    cursor = model_db.cursor()
    rows = DB_WarBucketAverages.Select_From_DB(cursor, "WHERE isHitter = ?", (int(is_hitter),))
    assert len(rows) == 1, f"Expected 1 row for isHitter={is_hitter}, got {len(rows)}"
    row = rows[0]
    return torch.tensor(
        [0, row.war1, row.war2, row.war3, row.war4, row.war5, row.war6],
        dtype=torch.float32,
    )