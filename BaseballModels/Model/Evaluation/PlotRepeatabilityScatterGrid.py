from matplotlib import pyplot as plt
from matplotlib.colors import LogNorm
import math
import numpy as np
import io
from PIL import Image

from Model.Evaluation.Classes import FoldRepeatabilityResult

def PlotRepeatabilityScatterGrid(result: FoldRepeatabilityResult, n_cols: int = 4,
                                 gridsize: int = 60, shared_color_scale: bool = True,
                                 log_scale: bool = False, linear_threshold: float = 0.1,
                                 save_path: str | None = None,
                                 as_gif: bool = False,
                                 gif_duration_ms: int = 500) -> None:
    ks = result.num_folds
    if not ks:
        raise ValueError("FoldRepeatabilityResult contains no fold counts to plot.")

    if as_gif and not save_path:
        raise ValueError("save_path must be provided when as_gif=True")

    # ------------------------------------------------------------------
    # Shared axis limits / tick setup (identical for grid and GIF)
    # ------------------------------------------------------------------
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

    # ------------------------------------------------------------------
    # Pre-compute global vmax when a shared colour scale is requested
    # ------------------------------------------------------------------
    if shared_color_scale:
        vmax = 1.0
        for k in ks:
            pairs = result.paired_war[k]
            if log_scale:
                mask = (pairs[:, 0] >= lt) & (pairs[:, 1] >= lt)
                pairs = pairs[mask]
            plot_data = fwd(pairs)
            # temporary hexbin solely to obtain the count array
            fig_tmp, ax_tmp = plt.subplots()
            mesh_tmp = ax_tmp.hexbin(plot_data[:, 0], plot_data[:, 1],
                                     gridsize=gridsize, extent=(lo, hi, lo, hi),
                                     mincnt=1)
            vmax = max(vmax, float(mesh_tmp.get_array().max()) if mesh_tmp.get_array().size else 1.0)
            plt.close(fig_tmp)
        shared_norm = LogNorm(vmin=1, vmax=max(vmax, 2))
    else:
        shared_norm = None

    # ------------------------------------------------------------------
    # Helper that draws one complete panel (used by both modes)
    # ------------------------------------------------------------------
    def _draw_one(ax, k, idx):
        pairs = result.paired_war[k]

        if log_scale:
            mask = (pairs[:, 0] >= lt) & (pairs[:, 1] >= lt)
            excluded = pairs[~mask]
            max_excluded = float(excluded.max()) if excluded.size > 0 else 0.0
            pairs = pairs[mask]
        else:
            max_excluded = 0.0

        plot_data = fwd(pairs)

        mesh = ax.hexbin(plot_data[:, 0], plot_data[:, 1], gridsize=gridsize,
                         extent=(lo, hi, lo, hi), cmap="viridis", mincnt=1)

        if shared_norm is not None:
            mesh.set_norm(shared_norm)
        else:
            mesh.set_norm(LogNorm(vmin=1, vmax=max(float(mesh.get_array().max()), 2)))

        ax.plot([lo, hi], [lo, hi], color="red", lw=1, ls="--", zorder=3)
        ax.set_xlim(lo, hi)
        ax.set_ylim(lo, hi)
        ax.set_aspect("equal")
        ax.set_title(f"K = {k}", fontsize=14)

        annotation = f"r = {result.corr[idx]:.3f}\nMAE = {result.mae[idx]:.3f}"
        if log_scale:
            annotation += f"\nmax cut: {max_excluded:.2f}"

        ax.text(0.04, 0.95, annotation,
                transform=ax.transAxes, va="top", fontsize=12,
                bbox=dict(boxstyle="round", fc="white", alpha=0.75))

        if tick_values is not None:
            ax.set_xticks(tick_positions)
            ax.set_xticklabels(tick_labels, fontsize=10)
            ax.tick_params(axis='x', labelrotation=90)
            ax.set_yticks(tick_positions)
            ax.set_yticklabels(tick_labels, fontsize=10)

        return mesh

    # ------------------------------------------------------------------
    # GIF mode – one frame per K
    # ------------------------------------------------------------------
    if as_gif:
        frames = []
        for idx, k in enumerate(ks):
            fig, ax = plt.subplots(figsize=(6, 5))
            mesh = _draw_one(ax, k, idx)

            scale_label = f" (log, cut below {linear_threshold:g})" if log_scale else ""
            ax.set_xlabel(f"Expected WAR, fold group A{scale_label}")
            ax.set_ylabel(f"Expected WAR, fold group B{scale_label}")
            fig.suptitle(f"Disjoint K-ensemble agreement", fontsize=12)

            fig.colorbar(mesh, ax=ax, shrink=0.8, label="count per hex")
            fig.tight_layout()

            buf = io.BytesIO()
            fig.savefig(buf, format="png", dpi=150, bbox_inches="tight")
            buf.seek(0)
            frames.append(Image.open(buf).convert("RGB"))
            plt.close(fig)

        frames[0].save(
            save_path,
            save_all=True,
            append_images=frames[1:],
            duration=gif_duration_ms,
            loop=0,
            optimize=True,
        )
        return

    # ------------------------------------------------------------------
    # Original static grid mode
    # ------------------------------------------------------------------
    n_cols = max(1, min(n_cols, len(ks)))
    n_rows = math.ceil(len(ks) / n_cols)

    fig, axes = plt.subplots(n_rows, n_cols, figsize=(5 * n_cols, 5 * n_rows),
                             squeeze=False)

    meshes = []
    for idx, k in enumerate(ks):
        ax = axes[idx // n_cols][idx % n_cols]
        mesh = _draw_one(ax, k, idx)
        meshes.append(mesh)

        # hide tick labels on interior axes (original behaviour)
        if idx // n_cols != n_rows - 1:
            ax.set_xticklabels([])
        if idx % n_cols != 0:
            ax.set_yticklabels([])

    for idx in range(len(ks), n_rows * n_cols):
        axes[idx // n_cols][idx % n_cols].axis("off")

    fig.colorbar(meshes[-1], ax=axes, shrink=0.6, label="count per hex")
    scale_label = f" (log, cut below {linear_threshold:g})" if log_scale else ""
    fig.supxlabel(f"Expected WAR, fold group A{scale_label}")
    fig.supylabel(f"Expected WAR, fold group B{scale_label}")
    fig.suptitle(f"Disjoint K-ensemble agreement")

    if save_path:
        fig.savefig(save_path, dpi=150, bbox_inches="tight")
    plt.show()
    

    
def _LogTicks(lo : float, hi : float) -> list[float]:
    candidates = []
    for exp in range(-2, 4):
        for mult in (1, 2, 5):
            candidates.append(mult * 10.0 ** exp)
    return sorted(v for v in candidates if lo <= v <= hi)