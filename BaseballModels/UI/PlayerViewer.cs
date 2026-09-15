using Db;
using DataAquisition.ModelStats;
using UI.Controls;
using DataAquisition.MonthStats;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Reflection;

namespace UI
{
    public partial class PlayerViewer : Form
    {
        // Shared caches created on initialization
        private RatioLeagueCache ratioLeagueCache;
        private ModelLeagueCache modelLeagueCache;

        // TODO : Get these from DB somehow
        private int END_YEAR = 2026;
        private int END_MONTH = 9;

        // Player caches
        private Player? currentPlayer = null;
        private List<Player_Hitter_MonthAdvanced> hitterMonthAdvanced = [];
        private List<Player_Fielder_MonthStats> fielderMonthStats = [];
        private List<Player_MonthlyWar> monthlyWar = [];
        private List<Player_Hitter_MonthBaserunning> hitterBaserunning = [];
        private List<Player_Pitcher_MonthAdvanced> pitcherMonthAdvanced = [];
        private List<Transaction_Log> transactionLog = [];


        private List<Model_HitterStats> dbModelHitterStats = [];
        private List<Model_PitcherStats> dbModelPitcherStats = [];


        public PlayerViewer()
        {
            InitializeComponent();

            ratioLeagueCache = RatioLeagueCache.Generate();
            modelLeagueCache = ModelLeagueCache.Generate();

            playerSearchBar.SetPlayerList(Global.db.Player.ToList());
            playerSearchBar.PlayerSelected += PlayerSearchBar_PlayerSelected;
        }

        private void PlayerSearchBar_PlayerSelected(object? sender, Player p)
        {
            currentPlayer = p;

            lblTitle.Text = $"{p.UseFirstName} {p.UseLastName}";
            tblModelPlayers.SetData("Model_Players",
                Global.db.Model_Players
                    .Where(x => x.MlbId == p.MlbId)
                    .ToList());
            tblHitterMonthStats.SetData("Player_Hitter_MonthStats",
                Global.db.Player_Hitter_MonthStats
                    .Where(x => x.MlbId == p.MlbId)
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToList(),
                    nameof(Player_Hitter_MonthStats.MlbId),
                    nameof(Player_Hitter_MonthStats.Year),
                    nameof(Player_Hitter_MonthStats.Month)
                );
            tblPitcherMonthStats.SetData("Player_Pitcher_MonthStats",
                Global.db.Player_Pitcher_MonthStats
                    .Where(x => x.MlbId == p.MlbId)
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToList());

            hitterMonthAdvanced = Global.db.Player_Hitter_MonthAdvanced.AsNoTracking().Where(x => x.MlbId == p.MlbId).ToList();
            fielderMonthStats = Global.db.Player_Fielder_MonthStats.AsNoTracking().Where(x => x.MlbId == p.MlbId).ToList();
            monthlyWar = Global.db.Player_MonthlyWar.AsNoTracking().Where(x => x.MlbId == p.MlbId).ToList();
            hitterBaserunning = Global.db.Player_Hitter_MonthBaserunning.AsNoTracking().Where(x => x.MlbId == p.MlbId).ToList();
            pitcherMonthAdvanced = Global.db.Player_Pitcher_MonthAdvanced.AsNoTracking().Where(x => x.MlbId == p.MlbId).ToList();
            transactionLog = Global.db.Transaction_Log.AsNoTracking().Where(x => x.MlbId == p.MlbId).ToList();

            dbModelHitterStats = Global.db.Model_HitterStats.AsNoTracking().Where(x => x.MlbId == p.MlbId).ToList();
            dbModelPitcherStats = Global.db.Model_PitcherStats.AsNoTracking().Where(x => x.MlbId == p.MlbId).ToList();
        }

        private void btnModelData_Click(object? sender, EventArgs e)
        {
            if (currentPlayer is null)
            {
                return;
            }

            Model_Players modelPlayer = tblModelPlayers.GetData<Model_Players>().Single();
            var sb = new StringBuilder();

            if (modelPlayer.IsHitter)
            {
                ModelHitterCache hitters = ModelHitterCache.GenerateForPlayer(
                    modelPlayer, currentPlayer,
                    tblHitterMonthStats.GetData<Player_Hitter_MonthStats>(),
                    hitterMonthAdvanced, fielderMonthStats, monthlyWar, hitterBaserunning, transactionLog,
                    ratioLeagueCache);
                var ctx = new CalculateHitterStats.HitterModelContext(modelLeagueCache, hitters, END_YEAR, END_MONTH);

                List<Model_HitterStats> hypo = CalculateHitterStats.BuildHitterStats(currentPlayer.MlbId, ctx);
                sb.AppendLine(ModelRowComparer.Compare("Hitter", dbModelHitterStats, hypo, s => (s.Year, s.Month)));
            }

            if (modelPlayer.IsPitcher)
            {
                ModelPitcherCache pitchers = ModelPitcherCache.GenerateForPlayer(
                    modelPlayer, currentPlayer,
                    tblPitcherMonthStats.GetData<Player_Pitcher_MonthStats>(),
                    pitcherMonthAdvanced, transactionLog,
                    ratioLeagueCache);
                var ctx = new CalculatePitcherStats.PitcherModelContext(modelLeagueCache, pitchers, END_YEAR, END_MONTH);

                List<Model_PitcherStats> hypo = CalculatePitcherStats.BuildPitcherStats(currentPlayer.MlbId, ctx);
                sb.AppendLine(ModelRowComparer.Compare("Pitcher", dbModelPitcherStats, hypo, s => (s.Year, s.Month)));
            }

            MessageBox.Show(sb.ToString(), $"Model data: {currentPlayer.UseFirstName} {currentPlayer.UseLastName}");
        }
    }

    public static class ModelRowComparer
    {
        private const float FLOAT_TOLERANCE = 1e-4f;

        public static string Compare<T>(
            string label, List<T> db, List<T> hypothetical, Func<T, (int Year, int Month)> key)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== {label} (DB vs hypothetical) ===");

            if (db.Count != hypothetical.Count)
            {
                sb.AppendLine($"Row count differs: {db.Count} vs {hypothetical.Count}");
            }

            Dictionary<(int Year, int Month), T> dbByKey = db.ToDictionary(key);
            Dictionary<(int Year, int Month), T> hypoByKey = hypothetical.ToDictionary(key);
            PropertyInfo[] properties = typeof(T).GetProperties()
                .ToArray();
            bool anyDiff = false;

            foreach (var k in dbByKey.Keys.Union(hypoByKey.Keys).OrderBy(k => k.Year).ThenBy(k => k.Month))
            {
                bool inDb = dbByKey.TryGetValue(k, out T? dbRow);
                bool inHypo = hypoByKey.TryGetValue(k, out T? hypoRow);

                if (!inDb || !inHypo)
                {
                    sb.AppendLine($"{k.Year}-{k.Month}: {(inDb ? "DB only" : "hypothetical only")}");
                    anyDiff = true;
                    continue;
                }

                foreach (PropertyInfo p in properties)
                {
                    object? dbValue = p.GetValue(dbRow);
                    object? hypoValue = p.GetValue(hypoRow);
                    if (!ValuesEqual(dbValue, hypoValue))
                    {
                        sb.AppendLine($"{k.Year}-{k.Month} {p.Name}: {dbValue} vs {hypoValue}");
                        anyDiff = true;
                    }
                }
            }

            if (!anyDiff && db.Count == hypothetical.Count)
            {
                sb.AppendLine("All rows are the same");
            }
            return sb.ToString();
        }

        private static PropertyInfo[] ComparableProperties<T>()
        {
            return typeof(T).GetProperties()
                .Where(p => p.CanRead
                    && p.Name != "Id"
                    && (p.PropertyType.IsPrimitive || p.PropertyType == typeof(string) || p.PropertyType == typeof(decimal)))
                .ToArray();
        }

        private static bool ValuesEqual(object? a, object? b)
        {
            if (a is float fa && b is float fb)
            {
                return Math.Abs(fa - fb) <= FLOAT_TOLERANCE + FLOAT_TOLERANCE * Math.Max(Math.Abs(fa), Math.Abs(fb));
            }
            if (a is double da && b is double dbv)
            {
                return Math.Abs(da - dbv) <= FLOAT_TOLERANCE + FLOAT_TOLERANCE * Math.Max(Math.Abs(da), Math.Abs(dbv));
            }
            return Equals(a, b);
        }
    }
}
