using ModelDb;

namespace UI.Controls.TestRunnerGraphViewer
{
    public partial class SelectionDetailTable : UserControl
    {
        public SelectionDetailTable()
        {
            InitializeComponent();
        
            Visible = false;
        }

        public void SetPoint(PlottedPoint? point)
        {
            Visible = point is not null;
            if (point is null)
                return;

            Output_PlayerWarAggregation w = point.Row;
            if (w.Year == 0)
                labelDate.Text = "Init";
            else
                labelDate.Text = $"{w.Month:D2}-{w.Year}";
            labelWar.Text = w.War.ToString("F2");
            labelBust.Text = w.War0.ToString("P1");
            labelBackup.Text = (w.War1 + w.War2).ToString("P1");
            labelStarter.Text = (w.War3 + w.War4).ToString("P1");
            labelStar.Text = (w.War5 + w.War6).ToString("P1");
        }
    }
}
