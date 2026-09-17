using ModelDb;

namespace UI.Controls.TestRunnerGraphViewer
{
    public partial class SelectionDetailTable : UserControl
    {
        private static readonly string[] ROW_NAMES = ["Date", "WAR", "Bust", "Backup", "Starter", "Star"];
        private readonly Label[] values = new Label[ROW_NAMES.Length];

        public SelectionDetailTable()
        {
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = ROW_NAMES.Length
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            for (int i = 0; i < ROW_NAMES.Length; i++)
            {
                tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tlp.Controls.Add(new Label
                {
                    Text = ROW_NAMES[i],
                    AutoSize = true,
                    Font = new Font(Font, FontStyle.Bold),
                    Margin = new Padding(3, 3, 8, 3)
                }, 0, i);
                values[i] = new Label { AutoSize = true, TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(3) };
                tlp.Controls.Add(values[i], 1, i);
            }
            Controls.Add(tlp);
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Visible = false;
        }

        public void SetPoint(PlottedPoint? point)
        {
            Visible = point is not null;
            if (point is null)
                return;

            Output_PlayerWarAggregation w = point.Row;
            values[0].Text = $"{w.Month:D2}-{w.Year}";
            values[1].Text = w.War.ToString("F2");
            values[2].Text = w.War0.ToString("P1");
            values[3].Text = (w.War1 + w.War2).ToString("P1");
            values[4].Text = (w.War3 + w.War4).ToString("P1");
            values[5].Text = (w.War5 + w.War6).ToString("P1");
        }
    }
}
