namespace UI.Controls.TestRunnerGraphViewer
{
    public partial class SeriesLegend : UserControl
    {
        private const int SWATCH_SIZE = 12;

        public SeriesLegend()
        {
            InitializeComponent();
        }


        public void SetSeries(IReadOnlyList<SeriesInfo> series)
        {
            SuspendLayout();
            while (flpEntries.Controls.Count > 0)
                flpEntries.Controls[0].Dispose();
            foreach (SeriesInfo s in series)
                flpEntries.Controls.Add(BuildRow(s));
            ResumeLayout();
        }

        private static Control BuildRow(SeriesInfo s)
        {
            var row = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 2, 0, 2)
            };
            row.Controls.Add(new Panel
            {
                BackColor = Color.FromArgb(s.Color.A, s.Color.R, s.Color.G, s.Color.B),
                Size = new Size(SWATCH_SIZE, SWATCH_SIZE),
                Margin = new Padding(3, 3, 6, 3)
            });
            row.Controls.Add(new Label
            {
                Text = s.Name,
                AutoSize = true,
                Margin = new Padding(0, 2, 3, 2)
            });
            return row;
        }
    }
}
