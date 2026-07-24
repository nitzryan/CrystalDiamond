import Model.DraftPickSlot.Config as config
from Model.DBTypes import *
from Model.ModelDBTypes import *
import sqlite3
import torch

FEATURE_NAMES: list[str] = ["isHitter", "ageAtSigningYear",
                            "draftPick", "draftSignRank"]
NUM_FEATURES: int = len(FEATURE_NAMES)
NUM_CLASSES: int = 7

def _load_war_rows(model_db: sqlite3.Connection
            ) -> dict[tuple[int, int], list[DB_Output_PlayerWarAggregation]]:
    
    rows = DB_Output_PlayerWarAggregation.Select_From_DB(
        model_db.cursor(), "WHERE ModelId = ?", (config.MODEL_ID,))

    by_player: dict[tuple[int, int], list[DB_Output_PlayerWarAggregation]] = {}
    for r in rows:
        by_player.setdefault((r.mlbId, r.isHitter), []).append(r)
    return by_player

def _resolve_target(
        player: DB_Model_Players,
        war_rows: dict[tuple[int, int], list[DB_Output_PlayerWarAggregation]]
        ) -> list[float]:
    
    key = (player.mlbId, player.isHitter)
    cutoff = player.signingYear + config.TIMEFRAME_M

    candidates = [r for r in war_rows.get(key, []) if r.year <= cutoff]
    if not candidates:
        raise ValueError(
            f"No OPWA row for mlbId={player.mlbId} "
            f"isHitter={player.isHitter} with year <= {cutoff} "
            f"(ModelId={config.MODEL_ID}).")

    best = max(candidates, key=lambda r: (r.year, r.month))
    return [best.war0, best.war1, best.war2, best.war3,
            best.war4, best.war5, best.war6]
    
def _build_dataset(
        players: list[DB_Model_Players],
        war_rows: dict[tuple[int, int], list[DB_Output_PlayerWarAggregation]]
        ) -> tuple[torch.Tensor, torch.Tensor]:
    
    x_rows: list[list[float]] = []
    y_rows: list[list[float]] = []
    for p in players:
        y_rows.append(_resolve_target(p, war_rows))
        x_rows.append([
            float(p.isHitter),
            float(p.draftPick),
            float(p.prospectType),
            float(p.draftSignRank),
        ])

    X = torch.tensor(x_rows, dtype=torch.float32)
    Y = torch.tensor(y_rows, dtype=torch.float32)
    return X, Y

def normalize(
        X: torch.Tensor
        ) -> tuple[torch.Tensor, torch.Tensor, torch.Tensor]:
    
    mean = X.mean(dim=0)
    std = X.std(dim=0)
    std[std == 0] = 1.0
    return (X - mean) / std, mean, std

def apply_normalize(
        X: torch.Tensor, mean: torch.Tensor, std: torch.Tensor
        ) -> torch.Tensor:
    
    return (X - mean) / std

def train_test_split(
        X: torch.Tensor, Y: torch.Tensor,
        test_frac: float = config.TEST_FRAC,
        seed: int = config.RANDOM_SEED
        ) -> tuple[tuple[torch.Tensor, torch.Tensor],
           tuple[torch.Tensor, torch.Tensor]]:
            
    n = X.shape[0]
    g = torch.Generator().manual_seed(seed)
    perm = torch.randperm(n, generator=g)
    n_test = int(n * test_frac)
    test_idx, train_idx = perm[:n_test], perm[n_test:]
    return (X[train_idx], Y[train_idx]), (X[test_idx], Y[test_idx])

def load_dataset(
        db: sqlite3.Connection, model_db: sqlite3.Connection
        ) -> tuple[torch.Tensor, torch.Tensor, list[DB_Model_Players]]:
    
    PLAYER_CONDITIONAL: str = ("WHERE IsEligible = 0 "
                            "AND draftPick <= 600 "
                            "AND NOT (isHitter = 1 AND isPitcher = 1) "
                            "AND signingYear <= ?")
    
    players = DB_Model_Players.Select_From_DB(
        db.cursor(), PLAYER_CONDITIONAL, (config.MAX_SIGNING_YEAR,))
    war_rows = _load_war_rows(model_db)
    X, Y = _build_dataset(players, war_rows)
    return X, Y, players