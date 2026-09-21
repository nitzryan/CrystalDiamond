using UI.Types;

namespace UI.Controls.TestRunnerGraphViewer
{
    public partial class ModelResultsPanel : UserControl
    {
        public ModelGraphViewerPoint? SelectedPoint => graph.SelectedPoint;
        public event EventHandler<(ModelGraphViewerPoint?, PlotArgs?)>? PointSelected;
        
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

        private void Graph_PointSelected(object? sender, (ModelGraphViewerPoint?, PlotArgs?)p)
        {
            var point = p.Item1;
            var args = p.Item2;
            detail.SetPoint(point, args);
            PointSelected?.Invoke(this, p);
        }
    }
}

