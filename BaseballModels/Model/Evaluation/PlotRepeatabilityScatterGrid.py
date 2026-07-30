from matplotlib import pyplot as plt
from matplotlib.colors import LogNorm
import math
import numpy as np

from Model.Evaluation.Classes import FoldRepeatabilityResult

def PlotRepeatabilityScatterGrid(result : FoldRepeatabilityResult, n_cols : int = 4,
                                 gridsize : int = 60, shared_color_scale : bool = True,
                                 log_scale : bool = False, linear_threshold : float = 0.1,
                                 save_path : str | None = None) -> None:
    ks = result.num_folds
    if not ks:
        raise ValueError("FoldRepeatabilityResult contains no fold counts to plot.")
    
    n_cols = max(1, min(n_cols, len(ks)))
    n_rows = math.ceil(len(ks) / n_cols)
    
    if log_scale:
        lt = max(linear_threshold, 1e-9)
        fwd = lambda v: np.log10(np.asarray(v, dtype=float))
        
        all_filtered = []
        for k in ks:
            pairs = result.paired_war[k]
            mask = (pairs[:, 0] >= lt) & (pairs[:, 1] >= lt)
            all_filtered.append(pairs[mask])
        all_filt_vals = np.concatenate([p.reshape(-1) for p in all_filtered])
        
        log_lo = float(np.log10(lt))
        log_hi = float(np.log10(all_filt_vals.max()))
        log_span = log_hi - log_lo if log_hi > log_lo else 1.0
        lo = log_lo - 0.03 * log_span
        hi = log_hi + 0.03 * log_span
        
        tick_values = _LogTicks(lt, float(all_filt_vals.max()))
        tick_labels = [f"{v:g}" for v in tick_values]
        tick_positions = fwd(np.array(tick_values))
    else:
        fwd = lambda v: np.asarray(v, dtype=float)
        tick_values = None
        
        all_vals = np.concatenate([result.paired_war[k].reshape(-1) for k in ks])
        raw_lo = min(0.0, float(all_vals.min()))
        raw_hi = float(all_vals.max())
        span = raw_hi - raw_lo if raw_hi > raw_lo else 1.0
        lo, hi = raw_lo, raw_hi + 0.03 * span
    
    fig, axes = plt.subplots(n_rows, n_cols, figsize=(3.2 * n_cols, 3.2 * n_rows),
                             squeeze=False)
    
    meshes = []
    for idx, k in enumerate(ks):
        ax = axes[idx // n_cols][idx % n_cols]
        pairs = result.paired_war[k]
        
        if log_scale:
            mask = (pairs[:, 0] >= lt) & (pairs[:, 1] >= lt)
            excluded = pairs[~mask]
            max_excluded = float(excluded.max()) if excluded.size > 0 else 0.0
            pairs = pairs[mask]
        
        plot_data = fwd(pairs)
        
        mesh = ax.hexbin(plot_data[:, 0], plot_data[:, 1], gridsize=gridsize,
                         extent=(lo, hi, lo, hi), cmap="viridis", mincnt=1)
        meshes.append(mesh)
        
        ax.plot([lo, hi], [lo, hi], color="red", lw=1, ls="--", zorder=3)
        ax.set_xlim(lo, hi)
        ax.set_ylim(lo, hi)
        ax.set_aspect("equal")
        ax.set_title(f"K = {k}", fontsize=10)
        
        annotation = f"r = {result.corr[idx]:.3f}\nMAE = {result.mae[idx]:.3f}"
        if log_scale:
            annotation += f"\nmax cut: {max_excluded:.3f}"
        
        ax.text(0.04, 0.95, annotation,
                transform=ax.transAxes, va="top", fontsize=8,
                bbox=dict(boxstyle="round", fc="white", alpha=0.75))
        
        if tick_values is not None:
            ax.set_xticks(tick_positions)
            ax.set_xticklabels(tick_labels, fontsize=6)
            ax.set_yticks(tick_positions)
            ax.set_yticklabels(tick_labels, fontsize=6)
        
        if idx // n_cols != n_rows - 1:
            ax.set_xticklabels([])
        if idx % n_cols != 0:
            ax.set_yticklabels([])
    
    if shared_color_scale:
        vmax = max(float(m.get_array().max()) for m in meshes)
        for m in meshes:
            m.set_norm(LogNorm(vmin=1, vmax=max(vmax, 2)))
    else:
        for m in meshes:
            m.set_norm(LogNorm(vmin=1, vmax=max(float(m.get_array().max()), 2)))
    
    for idx in range(len(ks), n_rows * n_cols):
        axes[idx // n_cols][idx % n_cols].axis("off")
    
    fig.colorbar(meshes[-1], ax=axes, shrink=0.6, label="count per hex")
    scale_label = f" (log, cut below {linear_threshold:g})" if log_scale else ""
    fig.supxlabel(f"Expected WAR, fold group A{scale_label}")
    fig.supylabel(f"Expected WAR, fold group B{scale_label}")
    fig.suptitle(f"Disjoint K-ensemble agreement ({result.num_observations} obs, "
                 f"{result.num_players} players)")
    
    if save_path:
        fig.savefig(save_path, dpi=150, bbox_inches="tight")
    plt.show()
    
def _LogTicks(lo : float, hi : float) -> list[float]:
    candidates = []
    for exp in range(-2, 4):
        for mult in (1, 2, 5):
            candidates.append(mult * 10.0 ** exp)
    return sorted(v for v in candidates if lo <= v <= hi)