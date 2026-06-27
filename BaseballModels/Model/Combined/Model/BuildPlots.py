from Combined.Utilities.Types import *
from Combined.Model.GetTimestepWarBrier import GetTimestepWarBrier, GetTimestepWarBrierCollege
from Combined.Model.GetTimestepWarLoss import GetTimestepWarLoss
from Combined.Model.Graphing import *
from Pro.Model.Player_Model import RNN_Model as Pro_Model
from College.Model.College_Model import RNN_Model as Col_Model
from Combined.DataPrep.Player_Dataset import Combined_Player_Dataset

def BuildPlots(
    epoch_counter: list[int],
    train_history: list[EpochResult],
    test_history: list[EpochResult],
    element_list: list[str],
    pro_network : Pro_Model,
    col_network : Col_Model,
    train_dataset : Combined_Player_Dataset,
    test_dataset : Combined_Player_Dataset,
    is_hitter : bool,
    batch_size : int,
    timestep_pct_cutoff : float,
) -> list[tuple[str, "callable"]]:
    num_war_classes = len(train_history[0].pro_war_dist.pred_pct)
    col_num_war_classes = len(train_history[0].col_war_dist.pred_pct)
    
    tr_ts, tr_avg, tr_pct = GetTimestepWarLoss(
        pro_network, col_network, train_dataset, is_hitter, batch_size, timestep_pct_cutoff)
    te_ts, te_avg, te_pct = GetTimestepWarLoss(
        pro_network, col_network, test_dataset, is_hitter, batch_size, timestep_pct_cutoff)
    
    plots : list[tuple[str, "callable"]] = []
    # Output Losses
    num_elements = len(element_list)
    for n in range(num_elements):
        plots.append((
            element_list[n],
            lambda n=n: GraphLoss(
                epoch_counter,
                [r.avg_loss[n] for r in train_history],
                [r.avg_loss[n] for r in test_history],
                title=element_list[n])
        ))

    # Brier Score
    plots.append((
        "PRO WAR Brier Score",
        lambda: GraphLoss(
            epoch_counter,
            [r.pro_brier_total for r in train_history],
            [r.pro_brier_total for r in test_history],
            loss_name="Brier Score", title="PRO WAR Brier Score")
    ))
    plots.append((
        "COL WAR Brier Score",
        lambda: GraphLoss(
            epoch_counter,
            [r.col_brier_total for r in train_history],
            [r.col_brier_total for r in test_history],
            loss_name="Brier Score", title="COL WAR Brier Score")
    ))

    # WAR Distribution
    for c in range(num_war_classes):
        plots.append((
            f"PRO WAR Class {c} Distribution",
            lambda c=c: GraphClassCounts(
                epoch_counter,
                [r.pro_war_dist.pred_pct[c] for r in train_history],
                [r.pro_war_dist.actual_pct[c] for r in train_history],
                [r.pro_war_dist.pred_pct[c] for r in test_history],
                [r.pro_war_dist.actual_pct[c] for r in test_history],
                title=f"PRO WAR Class {c} Distribution")
        ))
    for c in range(col_num_war_classes):
        plots.append((
            f"COL WAR Class {c} Distribution",
            lambda c=c: GraphClassCounts(
                epoch_counter,
                [r.col_war_dist.pred_pct[c] for r in train_history],
                [r.col_war_dist.actual_pct[c] for r in train_history],
                [r.col_war_dist.pred_pct[c] for r in test_history],
                [r.col_war_dist.actual_pct[c] for r in test_history],
                title=f"COL WAR Class {c} Distribution")
        ))
    
    # Loss Per Timestep
    plots.append((
        "PRO WAR Loss per Timestep (Train)",
        lambda: GraphTimestepLoss(tr_ts, tr_avg, tr_pct,
                                    title="PRO WAR Loss per Timestep (Train)")
    ))
    plots.append((
        "PRO WAR Loss per Timestep (Test)",
        lambda: GraphTimestepLoss(te_ts, te_avg, te_pct,
                                    title="PRO WAR Loss per Timestep (Test)")
    ))

    # Brier Skill Score and Murphy Decomposition
    train_brier_ts = GetTimestepWarBrier(
        pro_network, col_network, train_dataset, is_hitter, batch_size)
    test_brier_ts = GetTimestepWarBrier(
        pro_network, col_network, test_dataset, is_hitter, batch_size)
    plots.append((
        "PRO WAR BSS per Timestep",
        lambda: GraphTimestepBSS(
            train_brier_ts.timesteps, train_brier_ts.bss,
            test_brier_ts.timesteps,  test_brier_ts.bss,
            title="PRO WAR Brier Skill Score per Timestep", show=False)
    ))
    plots.append((
        "PRO WAR Brier Decomposition per Timestep (Train)",
        lambda: GraphTimestepDecomposition(
            train_brier_ts, title="PRO WAR Brier Decomposition per Timestep (Train)", show=False)
    ))
    plots.append((
        "PRO WAR Brier Decomposition per Timestep (Test)",
        lambda: GraphTimestepDecomposition(
            test_brier_ts, title="PRO WAR Brier Decomposition per Timestep (Test)", show=False)
    ))

    train_col_brier_ts = GetTimestepWarBrierCollege(
        col_network, train_dataset, batch_size)
    test_col_brier_ts = GetTimestepWarBrierCollege(
        col_network, test_dataset, batch_size)
    plots.append((
        "COL WAR BSS per Timestep",
        lambda: GraphTimestepBSS(
            train_col_brier_ts.timesteps, train_col_brier_ts.bss,
            test_col_brier_ts.timesteps,  test_col_brier_ts.bss,
            title="COL WAR Brier Skill Score per Timestep", show=False)
    ))
    plots.append((
        "COL WAR Brier Decomposition per Timestep (Train)",
        lambda: GraphTimestepDecomposition(
            train_col_brier_ts, title="COL WAR Brier Decomposition per Timestep (Train)", show=False)
    ))
    plots.append((
        "COL WAR Brier Decomposition per Timestep (Test)",
        lambda: GraphTimestepDecomposition(
            test_col_brier_ts, title="COL WAR Brier Decomposition per Timestep (Test)", show=False)
    ))

    ShowPlotDropdown(plots)