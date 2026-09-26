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
                new("K%", (h.K / h.Pa).ToString("P1")),
                new("BB%", (h.BB / h.Pa).ToString("P1")),
                new("wRC+", h.WRC.ToString("F0")),
                new("DEF", h.CrDEF.ToString("F1")),
                new("BSR", h.CrBSR.ToString("F1")),
                new("OFF", h.CrOFF.ToString("F1")),
                new("WAR", h.CrWAR.ToString("F1")),
            ];
        }

        public static IReadOnlyList<DetailRow> Pitcher(ModelGraphViewerPoint point)
        {
            Prediction_PitcherStats p = point.Pps![0];
            return [
                new("IP", ((p.Outs_SP + p.Outs_RP) / 3).ToString("F0")),
                new("GS", p.GS.ToString("F0")),
                new("ERA-", p.FIPMinus.ToString("F0")),
                new("FIP-", p.FIPMinus.ToString("F0")),
                new("K%", $"{p.KPerc:F1}%"),
                new("BB%", $"{p.BBPerc:F1}%"),
                new("HR/9", p.HR9.ToString("F2")),
                new("WAR", p.CrWAR.ToString("F1")),
            ];
        }
    }

    public static class PlotMetrics
    {
        public static readonly IReadOnlyList<PlotMetric> War = [
            new("WAR", f => f.Opwa!.War, 0, 20),
            new("WAR0", f => f.Opwa!.War0, 0, 1),
            new("WAR1", f => f.Opwa!.War1, 0, 0.3),
            new("WAR2", f => f.Opwa!.War2, 0, 0.3),
            new("WAR3", f => f.Opwa!.War3, 0, 0.3),
            new("WAR4", f => f.Opwa!.War4, 0, 0.3),
            new("WAR5", f => f.Opwa!.War5, 0, 0.3),
            new("WAR6", f => f.Opwa!.War6, 0, 0.3),
        ];

        public static readonly IReadOnlyList<PlotMetric> Hitter = [
            new("PA", f => Math.Round(f.Phs![0].Pa), 0, 600),
            new("K%", f => 100 * f.Phs![0].K / f.Phs![0].Pa, 10, 33),
            new("BB%", f => 100 * f.Phs![0].BB / f.Phs![0].Pa, 5, 15),
            new("WRC+", f => Math.Round(f.Phs![0].WRC), 75, 150),
            new("DEF", f => f.Phs![0].CrDEF, -10, 10),
            new("BSR", f => f.Phs![0].CrBSR, -5, 10),
            new("WAR", f => f.Phs![0].CrWAR, 0, 6),
        ];

        public static readonly IReadOnlyList<PlotMetric> Pitcher = [
            new("IP", f => Math.Round((f.Pps![0].Outs_SP + f.Pps![0].Outs_RP) / 3), 0, 200),
            new("ERA-", f => f.Pps![0].ERAMinus, 60, 130),
            new("FIP-", f => f.Pps![0].FIPMinus, 60, 130),
            new("K%", f => f.Pps![0].KPerc, 10, 35),
            new("BB%", f => f.Pps![0].BBPerc, 5, 15),
            new("HR/9", f => f.Pps![0].HR9, 0.5, 2),
            new("WAR", f => f.Pps![0].CrWAR, 0, 6),
        ];
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
