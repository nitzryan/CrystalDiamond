using UI.Types;

namespace UI.Controls.TestRunnerGraphViewer
{
    public partial class ModelResultsPanel : UserControl
    {
        private const int SIDE_WIDTH = 150;
        private readonly ModelGraphViewer graph = new() { Dock = DockStyle.Fill };
        private readonly SeriesLegend legend = new() { Anchor = AnchorStyles.Top | AnchorStyles.Left };
        
        private readonly SelectionDetailTable detail = new() { Anchor = AnchorStyles.Top | AnchorStyles.Left };
        public PlottedPoint? SelectedPoint => graph.SelectedPoint;
        public event EventHandler<PlottedPoint?>? PointSelected;
        
        public ModelResultsPanel()
        {
            var tlpSide = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(4, 30, 0, 0)
            };

            tlpSide.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpSide.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpSide.Controls.Add(legend, 0, 0);
            tlpSide.Controls.Add(detail, 0, 1);

            var tlpMain = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SIDE_WIDTH));
            tlpMain.Controls.Add(graph, 0, 0);
            tlpMain.Controls.Add(tlpSide, 1, 0);
            Controls.Add(tlpMain);

            graph.PointSelected += Graph_PointSelected;
            Size = new Size(750, 300);
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

