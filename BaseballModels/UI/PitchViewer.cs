using Db;
using PitchDb;
using static Db.DbEnums;

namespace UI
{
    public partial class PitchViewer : Form
    {
        private class ComboBoxItem<T>
        {
            public required string Text { get; set; }
            public required T Value { get; set; }

            public override string ToString() => Text;
        }

        private SqliteDbContext? db = null;
        private PitchDbContext? pitchDb = null;
        private int PlayerId = 0;
        private List<PitchStatcast> PlayerPitches = [];

        public PitchViewer()
        {
            InitializeComponent();

            yearSelector.Update(2010, 2025, "Year");

            // Situations
            foreach (var situation in Enum.GetValues<PitchScenario>())
            {
                cbSituations.Items.Add(situation);
            }
            cbSituations.SelectedIndex = 0;

            List<ComboBoxItem<PitchGridType>> gridTypes = [
                new ComboBoxItem<PitchGridType>{
                    Text = "3x3 Heart",
                    Value = PitchGridType._3x3_Shadow
                },
                new ComboBoxItem<PitchGridType>{
                    Text = "5x5",
                    Value = PitchGridType._5x5
                },
                new ComboBoxItem<PitchGridType>{
                    Text = "3x3",
                    Value = PitchGridType._3x3
                },
            ];
            foreach (var gt in gridTypes)
                cbBinSizes.Items.Add(gt);
            cbBinSizes.SelectedIndex = 0;

            pitchPanel.PitchStatsUpdate += PitchStatsUpdate;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            db = new(Db.Connection.DB_READONLY_OPTIONS);
            pitchDb = new(PitchDb.Connection.PITCHDB_READONLY_OPTIONS);

            // Models
            cbModel.Items.Clear();
            var pitchModels = pitchDb.Models_PitchValue.OrderBy(f => f.Id).ToList();
            foreach (var pm in pitchModels)
            {
                cbModel.Items.Add(new ComboBoxItem<int>
                {
                    Text = $"{pm.Id} {pm.Name}",
                    Value = pm.Id
                });
            }
            cbModel.SelectedIndex = 0;

            // Output type
            List<(PitchValueType, string)> outputTypes = [
                (PitchValueType.Actual, "Actual"),
                (PitchValueType.Stuff, "Stuff Only"),
                (PitchValueType.Location, "Location Only"),
                (PitchValueType.Exp, "Pitch Model")
            ];
            cbOutput.Items.Clear();
            foreach (var ot in outputTypes)
            {
                cbOutput.Items.Add(new ComboBoxItem<PitchValueType>
                {
                    Text = ot.Item2,
                    Value = ot.Item1
                });
            }
            cbOutput.SelectedIndex = 0;
        }

        private void pbSearchPlayer_Click(object sender, EventArgs e)
        {
            if (db == null || pitchDb == null)
                throw new Exception("Failed to load db");

            PlayerId = (int)playerIdEntry.Value;

            // Player Name
            Player? player = db.Player.Where(f => f.MlbId == PlayerId).SingleOrDefault();
            if (player == null)
            {
                playerLinkLabel.Text = "No Player";
                return;
            }

            playerLinkLabel.Text = player.UseFirstName + " " + player.UseLastName;
            labelName.Text = playerLinkLabel.Text;

            PlayerPitches = db.PitchStatcast.Where(f => f.PitcherId == PlayerId && f.ModelOutput != "").ToList();

            // Player Years
            List<int> years = PlayerPitches
                .Where(f => f.PitcherId == player.MlbId)
                .Select(f => f.Year)
                .Distinct()
                .OrderDescending()
                .ToList();

            pitchPanel.HidePitches();
            if (!years.Any())
            {
                groupBoxFilters.Hide();
                this.PlayerPitches = [];
                return;
            }
            else
            {
                groupBoxFilters.Show();
            }

            yearSelector.Update(years.Min(), years.Max(), "Year");

            // Pitch Types
            List<PitchType> pitchTypes = PlayerPitches
                .Where(f => f.PitcherId == player.MlbId)
                .GroupBy(f => f.PitchType)
                .OrderByDescending(f => f.Count())
                .Select(f => f.Key)
                .ToList();
            cbPitchSelector.Items.Clear();
            foreach (var pt in pitchTypes)
                cbPitchSelector.Items.Add(pt);
            cbPitchSelector.SelectedIndex = 0;



            Invalidate();
        }

        private void pbPitchSearch_Click(object sender, EventArgs e)
        {
            int minYear = (int)yearSelector.GetMinimum();
            int maxYear = (int)yearSelector.GetMaximum();
            decimal scale = nudMaxScale.Value;

            if (cbPitchSelector.SelectedItem is PitchType pitchType &&
                cbSituations.SelectedItem is PitchScenario scenario &&
                cbModel.SelectedItem is ComboBoxItem<int> modelComboBox &&
                cbOutput.SelectedItem is ComboBoxItem<PitchValueType> pitchValueComboBox &&
                cbBinSizes.SelectedItem is ComboBoxItem<PitchGridType> pitchGridTypeComboBox)
            {
                var pitches = PlayerPitches
                    .Where(f => f.Year >= minYear
                        && f.Year <= maxYear
                        && f.PitchType == pitchType);

                foreach (PitchScenario ps in Enum.GetValues(typeof(PitchScenario)))
                {
                    if (ps != PitchScenario.All && scenario.HasFlag(ps))
                    {
                        pitches = pitches.Where(f => f.Scenario.HasFlag(ps));
                    }
                }

                pitchPanel.ShowPitches(
                    pitches,
                    modelComboBox.Value,
                    pitchValueComboBox.Value,
                    pitchGridTypeComboBox.Value,
                    scale,
                    (int)nudMinPitches.Value);
            }
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

            labelVel.Text = $"{Math.Round(pitchStats.Vel, 1)}";
            labelMoveX.Text = $"{Math.Round(pitchStats.BreakHoriz, 1)}";
            labelMoveZ.Text = $"{Math.Round(pitchStats.BreakVert, 1)}";

            labelLocation.Text = $"{Math.Round(pitchStats.LocPlus, 1)}";
            labelStuff.Text = $"{Math.Round(pitchStats.StuffPlus, 1)}";
            labelPitch.Text = $"{Math.Round(pitchStats.PitchPlus, 1)}";
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }
    }
}
