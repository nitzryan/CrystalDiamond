using ModelDb;
using SiteDb;

namespace UI.Controls.TestRunnerGraphViewer
{
    public record DetailRow(string Label, string Value);
    public static class DetailViews
    {
        public static IReadOnlyList<DetailRow> War(ModelGraphViewerPoint point)
        {
            Output_PlayerWarAggregation w = point.Opwa!;
            return [
                new("War", w.War.ToString("F2")),
                new("Bust", w.War0.ToString("P1")),
                new("Backup", (w.War1 + w.War2).ToString("P1")),
                new("Starter", (w.War3 + w.War4).ToString("P1")),
                new("Star", (w.War5 + w.War6).ToString("P1")),
            ];
        }

        public static IReadOnlyList<DetailRow> Hitter(ModelGraphViewerPoint point)
        {
            Prediction_HitterStats h = point.Phs![0];
            return [
                new("PA", h.Pa.ToString("F0")),
                new("AVG", h.AVG.ToString("F3")),
                new("OBP", h.OBP.ToString("F3")),
                new("SLG", h.SLG.ToString("F3")),
                new("wRC+", h.WRC.ToString("F0")),
                new("HR", h.HitHR.ToString("F1")),
                new("SB", h.SB.ToString("F1")),
                new("WAR", h.CrWAR.ToString("F1")),
            ];
        }

        public static IReadOnlyList<DetailRow> Pitcher(ModelGraphViewerPoint point)
        {
            Prediction_PitcherStats p = point.Pps![0];
            return [
                new("IP", ((p.Outs_SP + p.Outs_RP) / 3).ToString("F0")),
                new("GS", p.GS.ToString("F0")),
                new("ERA", p.ERA.ToString("F2")),
                new("FIP", p.FIP.ToString("F2")),
                new("K%", $"{p.KPerc:F1}%"),
                new("BB%", $"{p.BBPerc:F1}%"),
                new("HR/9", p.HR9.ToString("F2")),
                new("WAR", p.CrWAR.ToString("F1")),
            ];
        }
    }

    public partial class SelectionDetailTable : UserControl
    {
        private static readonly Font LabelFont = new("Segoe UI", 12F, FontStyle.Bold);
        private static readonly Font ValueFont = new("Segoe UI", 12F);

        public SelectionDetailTable()
        {
            InitializeComponent();
        
            Visible = false;
        }

        public void SetPoint(ModelGraphViewerPoint? point, PlotArgs? args)
        {
            if (point is null || args is null || !args.ResultValid(point))
            {
                Visible = false;
                return;
            }

            List<DetailRow> rows = [new DetailRow("Date", point.Year == 0 ? "Init" : $"{point.Month:D2}-{point.Year}")];
            rows.AddRange(args.DetailRows(point));

            tlpMain.SuspendLayout();
            while (tlpMain.Controls.Count > 0)
                tlpMain.Controls[0].Dispose();

            tlpMain.RowStyles.Clear();
            tlpMain.RowCount = rows.Count + 1;

            for (int i = 0; i < rows.Count; i++)
            {
                tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tlpMain.Controls.Add(BuildLabel(rows[i].Label, LabelFont, AnchorStyles.Right), 0, i);
                tlpMain.Controls.Add(BuildLabel(rows[i].Value, ValueFont, AnchorStyles.Left), 1, i);
            }

            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // filler
            tlpMain.ResumeLayout();

            Visible = true;
        }

        private static Label BuildLabel(string text, Font font, AnchorStyles anchor)
        {
            return new Label
            {
                Text = text,
                Font = font,
                Anchor = anchor,
                AutoSize = true,
            };
        }
    }
}
