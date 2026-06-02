using Db;

namespace UI.Controls
{
    public partial class PitchBucketViewer : UserControl
    {
        public PitchBucketViewer()
        {
            InitializeComponent();

            pitchPanel.PitchStatsUpdate += PitchStatsUpdate;
        }

        public void HidePitches()
        {
            pitchPanel.HidePitches();
        }

        public void ShowPitches(
            IEnumerable<PitchStatcast> pitches,
            int modelId,
            PitchValueType pvt,
            PitchGridType pgt,
            decimal scale,
            int minPitches)
        {
            pitchPanel.ShowPitches(
                pitches,
                modelId,
                pvt,
                pgt,
                scale,
                minPitches
            );
        }

        private void PitchStatsUpdate(object sender, PitchStats? pitchStats)
        {
            if (pitchStats == null)
            {
                panelPitchStats.Hide();
                return;
            }

            panelPitchStats.Show();
            labelABPA.Text = $"{pitchStats.AB}/{pitchStats.PA}";
            labelAVG.Text = $"{pitchStats.AVG.ToString("F3")}";
            labelOBP.Text = $"{pitchStats.OBP.ToString("F3")}";
            labelSLG.Text = $"{pitchStats.SLG.ToString("F3")}";

            labelWhiff.Text = $"{Math.Round(100 * pitchStats.WhiffRate, 1)}%";
            labelCSW.Text = $"{Math.Round(100 * pitchStats.CSWRate, 1)}%";
            labelFoul.Text = $"{Math.Round(100 * pitchStats.FoulRate, 1)}%";
            labelInPlay.Text = $"{Math.Round(100 * pitchStats.IPRate, 1)}%";

            labelVel.Text = $"{Math.Round(pitchStats.Vel, 1)}";
            labelMoveX.Text = $"{Math.Round(pitchStats.BreakHoriz, 1)}";
            labelMoveZ.Text = $"{Math.Round(pitchStats.BreakVert, 1)}";

            labelLocation.Text = $"{Math.Round(pitchStats.LocPlus, 1)}";
            labelStuff.Text = $"{Math.Round(pitchStats.StuffPlus, 1)}";
            labelPitch.Text = $"{Math.Round(pitchStats.PitchPlus, 1)}";
            labelActual.Text = $"{Math.Round(pitchStats.ActualPlus, 1)}";
        }
    }
}
