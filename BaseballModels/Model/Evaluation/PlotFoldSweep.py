from matplotlib import pyplot as plt

from Model.Evaluation.Classes import FoldSweepResult

def PlotFoldSweep(result : FoldSweepResult, is_hitter : bool):
    player_type = "Hitter" if is_hitter else "Pitcher"
    folds = result.num_folds
    
    fig, (ax_loss, ax_brier) = plt.subplots(1, 2, figsize=(11, 4))
    
    ax_loss.plot(folds, result.loss_war, marker='o')
    ax_loss.set_xlabel("Folds Averaged")
    ax_loss.set_ylabel("WAR Classification Loss (per sample)")
    ax_loss.set_xticks(folds)
    ax_loss.grid(alpha=0.3)
    
    ax_brier.plot(folds, result.brier, marker='o', color='tab:orange')
    ax_brier.set_xlabel("Folds Averaged")
    ax_brier.set_ylabel("Brier Score (per sample)")
    ax_brier.set_xticks(folds)
    ax_brier.grid(alpha=0.3)
    
    fig.suptitle(f"{player_type} — Fold Averaging Returns ({result.num_observations:,} observations, {result.num_players:,} players)")
    fig.tight_layout()
    