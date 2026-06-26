import io
import matplotlib.pyplot as plt
import ipywidgets as widgets
from IPython.display import display
from matplotlib.figure import Figure

def GraphLoss(epoch_counter, train_loss_hist, test_loss_hist, loss_name="Loss", start = 1, graph_y_range=None, title="") -> Figure:
    fig = plt.figure()
    plt.plot(epoch_counter[start:], train_loss_hist[start:], color='blue')
    plt.plot(epoch_counter[start:], test_loss_hist[start:], color='red')
    plt.title(title)
    if graph_y_range is not None:
        plt.ylim(graph_y_range)
    plt.legend(['Train Loss', 'Test Loss'], loc='upper right')
    plt.xlabel('#Epochs')
    plt.ylabel(loss_name)
    return fig
    
def GraphClassCounts(
    epoch_counter : list[int],
    train_pred : list[float],
    train_actual : list[float],
    test_pred : list[float],
    test_actual : list[float],
    start : int = 1,
    title : str = ""
) -> Figure:
    fig = plt.figure()
    plt.plot(epoch_counter[start:], train_pred[start:],   color='blue', linestyle='-')
    plt.plot(epoch_counter[start:], train_actual[start:], color='blue', linestyle='--')
    plt.plot(epoch_counter[start:], test_pred[start:],    color='red',  linestyle='-')
    plt.plot(epoch_counter[start+5:-5], test_actual[start+5:-5],  color='red',  linestyle='--') # Slightly shorter so train_actual is visual if this is too close
    plt.title(title)
    plt.legend(['Train Predicted', 'Train Actual', 'Test Predicted', 'Test Actual'],
               loc='upper right')
    plt.xlabel('#Epochs')
    plt.ylabel('% of Dataset')

    return fig
    
def GraphTimestepLoss(
    timesteps : list[int],
    avg_loss : list[float],
    pct : list[float],
    title : str = ""
) -> Figure:
    fig, ax1 = plt.subplots()
    ax1.plot(timesteps, avg_loss, color='blue')
    ax1.set_xlabel('Timestep')
    ax1.set_ylabel('Avg WAR Loss', color='blue')
    ax1.tick_params(axis='y', labelcolor='blue')

    ax2 = ax1.twinx()
    ax2.plot(timesteps, pct, color='green')
    ax2.set_ylabel('% of Samples', color='green')
    ax2.tick_params(axis='y', labelcolor='green')

    plt.title(title)
    return fig
    
    
def GraphTimestepBSS(
    train_ts : list[int], train_bss : list[float],
    test_ts : list[int],  test_bss : list[float],
    title : str = "", show : bool = True
) -> Figure:
    fig = plt.figure()
    plt.plot(train_ts, train_bss, color='blue')
    plt.plot(test_ts, test_bss, color='red')
    plt.axhline(0, color='gray', linestyle=':')   # 0 = no better than base rate
    plt.title(title)
    plt.legend(['Train BSS', 'Test BSS'], loc='upper right')
    plt.xlabel('Timestep')
    plt.ylabel('Brier Skill Score')
    if show:
        plt.show()
        plt.close(fig)
    return fig

def GraphTimestepDecomposition(
    result : dict, title : str = "", show : bool = True
) -> Figure:
    ts = result['timesteps']
    fig, ax1 = plt.subplots()
    ax1.plot(ts, result['bs_model'],    color='black')
    ax1.plot(ts, result['uncertainty'], color='green')
    ax1.plot(ts, result['resolution'],  color='blue')
    ax1.plot(ts, result['reliability'], color='red')
    ax1.set_xlabel('Timestep')
    ax1.set_ylabel('Brier Components')
    ax1.legend(['Brier (model)', 'Uncertainty (difficulty)', 'Resolution', 'Reliability'],
               loc='upper left')

    ax2 = ax1.twinx()
    ax2.plot(ts, result['pct'], color='gray', linestyle=':')
    ax2.set_ylabel('% of Players', color='gray')
    ax2.tick_params(axis='y', labelcolor='gray')

    plt.title(title)
    if show:
        plt.show()
        plt.close(fig)
    return fig
    
def ShowPlotDropdown(plots : list[tuple[str, "callable"]]) -> None:
    dropdown = widgets.Dropdown(options=[title for title, _ in plots], description='Plot:')
    image = widgets.Image(format='png', layout=widgets.Layout(width='auto', height='auto'))
    plot_map = {title: fn for title, fn in plots}

    def render(title):
        fig = plot_map[title]()            # closures now return the figure (show=False)
        buf = io.BytesIO()
        fig.savefig(buf, format='png', bbox_inches='tight')
        plt.close(fig)
        image.value = buf.getvalue()

    def on_change(change):
        if change['name'] == 'value' and change['type'] == 'change':
            render(change['new'])

    dropdown.observe(on_change)
    display(dropdown, image)
    render(dropdown.value)   # initial plot