using Db;
using UI.Controls;

namespace UI
{
    public partial class PlayerViewer : Form
    {
        public PlayerViewer()
        {
            InitializeComponent();

            playerSearchBar.SetPlayerList(Global.db.Player.ToList());
            playerSearchBar.PlayerSelected += PlayerSearchBar_PlayerSelected;
        }

        private void PlayerSearchBar_PlayerSelected(object? sender, Player p)
        {
            lblTitle.Text = $"{p.UseFirstName} {p.UseLastName}";
            tblModelPlayers.SetData("Model_Players",
                Global.db.Model_Players
                    .Where(x => x.MlbId == p.MlbId)
                    .ToList());
            tblHitterMonthStats.SetData("Player_Hitter_MonthStats",
                Global.db.Player_Hitter_MonthStats
                    .Where(x => x.MlbId == p.MlbId)
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToList());
            tblPitcherMonthStats.SetData("Player_Pitcher_MonthStats",
                Global.db.Player_Pitcher_MonthStats
                    .Where(x => x.MlbId == p.MlbId)
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToList());
        }
    }
}
