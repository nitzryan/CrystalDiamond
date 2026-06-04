using Db;
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

        private Player? player = null;
        private List<PitchStatcast> PlayerPitches = [];

        public PitchViewer()
        {
            InitializeComponent();

            playerSearchBar.PlayerSelected += PlayerSelected;
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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            playerSearchBar.SetPlayerList(Global.db.Player.Where(f => f.Position != "H").ToList());
            Global.YldDict = Global.pitchDb.YearLeagueDeviations
                .ToDictionary(
                    f => new Global.YearLeagueDevKey(f.ModelId, f.Year, f.Balls, f.Strikes),
                    f => f
                );

            // Models
            cbModel.Items.Clear();
            var pitchModels = Global.pitchDb.Models_PitchValue.OrderBy(f => f.Id).ToList();
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

        public void PlayerSelected(object senser, Player p)
        {
            if (Global.db == null || Global.pitchDb == null)
                throw new Exception("Failed to load db");

            player = p;
            PlayerPitches = Global.db.PitchStatcast
                .Where(
                    f => f.PitcherId == player.MlbId && 
                    f.ModelStuff != null &&
                    f.PitchType != PitchType.Unknown)
                .ToList();

            // Player Years
            List<int> years = PlayerPitches
                .Select(f => f.Year)
                .Distinct()
                .OrderDescending()
                .ToList();

            pitchBucketViewer.HidePitches();
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

            // Levels
            List<int> levelIds = PlayerPitches
                .Select(f => f.LevelId)
                .Distinct()
                .Order()
                .ToList();
            cbLevel.Items.Clear();
            foreach (int level in levelIds)
                cbLevel.Items.Add(level);
            cbLevel.SelectedIndex = 0;

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

            // Pitch Arsenal Stats
            pitcherArsenal.SetPitches(PlayerPitches);

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
                cbBinSizes.SelectedItem is ComboBoxItem<PitchGridType> pitchGridTypeComboBox &&
                cbLevel.SelectedItem is int levelId)
            {
                var pitches = PlayerPitches
                    .Where(f => f.Year >= minYear
                        && f.Year <= maxYear
                        && f.PitchType == pitchType
                        && f.LevelId == levelId);

                foreach (PitchScenario ps in Enum.GetValues(typeof(PitchScenario)))
                {
                    if (ps != PitchScenario.All && scenario.HasFlag(ps))
                    {
                        pitches = pitches.Where(f => f.Scenario.HasFlag(ps));
                    }
                }

                pitchBucketViewer.ShowPitches(
                    pitches,
                    modelComboBox.Value,
                    pitchValueComboBox.Value,
                    pitchGridTypeComboBox.Value,
                    scale,
                    (int)nudMinPitches.Value);

                stuffModelViewer.SetPitches(pitches.ToList());
            }
        }

        
    }
}
