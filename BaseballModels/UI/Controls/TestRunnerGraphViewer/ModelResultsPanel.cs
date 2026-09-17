using UI.Types;

namespace UI.Controls.TestRunnerGraphViewer
{
    public partial class ModelResultsPanel : UserControl
    {
        public PlottedPoint? SelectedPoint => graph.SelectedPoint;
        public event EventHandler<PlottedPoint?>? PointSelected;
        
        public ModelResultsPanel()
        {
            InitializeComponent();

            graph.PointSelected += Graph_PointSelected;
        }

        public void SetResults(IReadOnlyList<ModelResults> results, IReadOnlyList<string> seriesNames)
        {
            graph.SetResults(results, seriesNames);
            legend.SetSeries(graph.Series);
        }

        public void ClearResults()
        {
            graph.ClearResults();
            legend.SetSeries([]);
        }

        private void Graph_PointSelected(object? sender, PlottedPoint? point)
        {
            detail.SetPoint(point);
            PointSelected?.Invoke(this, point);
        }
    }
}

