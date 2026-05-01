using Db;
using static Db.DbEnums;
using static PitchDb.DbEnums;

namespace UI
{
    public partial class PitchViewer : Form
    {
        private SqliteDbContext? db = null;
        private int PlayerId = 0;

        public PitchViewer()
        {
            InitializeComponent();

            yearSelector.Update(2010, 2025, "Year");

            // Situations
            foreach (var situation in Enum.GetValues<Scenario>())
            {
                cbSituations.Items.Add(situation);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            db = new(Connection.DB_READONLY_OPTIONS);
        }

        private void pbSearchPlayer_Click(object sender, EventArgs e)
        {
            if (db == null)
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
            
            // Player Years
            List<int> years = db.PitchStatcast
                .Where(f => f.PitcherId == player.MlbId)
                .Select(f => f.Year)
                .Distinct()
                .OrderDescending()
                .ToList();

            pitchPanel.HidePitches();
            if (!years.Any())
            {
                groupBoxFilters.Hide();
                return;
            } else
            {
                groupBoxFilters.Show();
            }

            yearSelector.Update(years.Min(), years.Max(), "Year");

            // Pitch Types
            List<PitchType> pitchTypes = db.PitchStatcast
                .Where(f => f.PitcherId == player.MlbId)
                .Select(f => f.PitchType)
                .Distinct()
                .Order()
                .ToList();
            cbPitchSelector.Items.Clear();
            foreach (var pt in pitchTypes)
                cbPitchSelector.Items.Add(pt);

            Invalidate();
        }

        private void pbPitchSearch_Click(object sender, EventArgs e)
        {
            if (db == null)
                throw new Exception("Failed to load db");

            int minYear = (int)yearSelector.GetMinimum();
            int maxYear = (int)yearSelector.GetMaximum();

            if (cbPitchSelector.SelectedItem is PitchType pitchType)
            {
                if (cbSituations.SelectedItem is Scenario scenario)
                {
                    var pitches = db.PitchStatcast
                        .Where(f => f.PitcherId == PlayerId
                            && f.Year >= minYear
                            && f.Year <= maxYear
                            && f.PitchType == pitchType);

                    pitchPanel.ShowPitches(pitches);
                }
            }
        }
    }
}
