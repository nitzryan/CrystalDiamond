using Db;
using DataAquisition.ModelStats;
using DataAquisition.MonthStats;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Reflection;
using UI.Python;
using Python.Runtime;
using UI.Types;

namespace UI
{
    public partial class PlayerViewer : Form
    {
        // Shared caches created on initialization
        private RatioLeagueCache ratioLeagueCache;
        private ModelLeagueCache modelLeagueCache;

        // TODO : have these editable
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

        private Model_Players? dbModelPlayer = null;
        private College_Player? dbCollegePlayer = null;

        private bool isModelRunning = false;

        public PlayerViewer()
        {
            InitializeComponent();

            // Load Python Connections
            TestRunnerPy.LoadPythonResources();
            UpdateModelButtons();
            if (!TestRunnerPy.IsReady)
                TestRunnerPy.Ready += TestRunnerPy_Ready;

            // Load league cache data
            ratioLeagueCache = RatioLeagueCache.Generate();
            modelLeagueCache = ModelLeagueCache.Generate();

            // Load player list
            playerSearchBar.SetPlayerList(Global.db.Player.ToList());
            playerSearchBar.PlayerSelected += PlayerSearchBar_PlayerSelected;
        }

        private void PlayerSearchBar_PlayerSelected(object? sender, Player p)
        {
            currentPlayer = p;
            modelResultsPanel.ClearResults();

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

            hitterMonthAdvanced = Global.db.Player_Hitter_MonthAdvanced.Where(x => x.MlbId == p.MlbId).ToList();
            fielderMonthStats = Global.db.Player_Fielder_MonthStats.Where(x => x.MlbId == p.MlbId).ToList();
            monthlyWar = Global.db.Player_MonthlyWar.Where(x => x.MlbId == p.MlbId).ToList();
            hitterBaserunning = Global.db.Player_Hitter_MonthBaserunning.Where(x => x.MlbId == p.MlbId).ToList();
            pitcherMonthAdvanced = Global.db.Player_Pitcher_MonthAdvanced.Where(x => x.MlbId == p.MlbId).ToList();
            transactionLog = Global.db.Transaction_Log.Where(x => x.MlbId == p.MlbId).ToList();

            dbModelPlayer = Global.db.Model_Players.AsNoTracking()
            .SingleOrDefault(x => x.MlbId == p.MlbId);
            dbCollegePlayer = Global.db.College_Player.AsNoTracking()
                .Where(x => x.MlbId == p.MlbId)
                .SingleOrDefault();

            dbModelHitterStats = Global.db.Model_HitterStats.Where(x => x.MlbId == p.MlbId).ToList();
            dbModelPitcherStats = Global.db.Model_PitcherStats.Where(x => x.MlbId == p.MlbId).ToList();

            UpdateModelButtons();
        }

        private async void btnHitterModelData_Click(object? sender, EventArgs e)
        {
            if (currentPlayer is null || dbModelPlayer is null)
            {
                return;
            }

            if (!tblModelPlayers.TryGetData(out List<Model_Players> mp))
                return;
            Model_Players modelPlayer = mp.Single();

            var sb = new StringBuilder();

            if (!tblHitterMonthStats.TryGetData(out List<Player_Hitter_MonthStats> hitterStats))
                return;

            ModelHitterCache hitters = ModelHitterCache.GenerateForPlayer(
            modelPlayer, currentPlayer,
            hitterStats,
            hitterMonthAdvanced, fielderMonthStats, monthlyWar, hitterBaserunning, transactionLog,
            ratioLeagueCache);

            var ctx = new CalculateHitterStats.HitterModelContext(modelLeagueCache, hitters, END_YEAR, END_MONTH);
            List<Model_HitterStats> hypoHitterStats = CalculateHitterStats.BuildHitterStats(currentPlayer.MlbId, ctx);

            await RunModelAsync(currentPlayer, sb, () => TestRunnerPy.RunHitterVariants(
                currentPlayer.MlbId, dbCollegePlayer?.TBCId, modelId: 1,
                dbModelPlayer, modelPlayer,
                dbModelHitterStats,
                hypoHitterStats.OrderBy(x => x.Year).ThenBy(x => x.Month).ToList()));
        }

        private async void btnPitcherModelData_Click(object? sender, EventArgs e)
        {
            if (currentPlayer is null || dbModelPlayer is null)
            {
                return;
            }

            if (!tblModelPlayers.TryGetData(out List<Model_Players> mp))
                return;
            Model_Players modelPlayer = mp.Single();
            var sb = new StringBuilder();

            if (!tblPitcherMonthStats.TryGetData(out List<Player_Pitcher_MonthStats> pitcherStats))
                return;

            ModelPitcherCache pitchers = ModelPitcherCache.GenerateForPlayer(
                modelPlayer, currentPlayer,
                pitcherStats,
                pitcherMonthAdvanced, transactionLog,
                ratioLeagueCache);

            var ctx = new CalculatePitcherStats.PitcherModelContext(modelLeagueCache, pitchers, END_YEAR, END_MONTH);
            List<Model_PitcherStats> hypoPitcherStats = CalculatePitcherStats.BuildPitcherStats(currentPlayer.MlbId, ctx);

            await RunModelAsync(currentPlayer, sb, () => TestRunnerPy.RunPitcherVariants(
                currentPlayer.MlbId, dbCollegePlayer?.TBCId, modelId: 1,
                dbModelPlayer, modelPlayer,
                dbModelPitcherStats,
                hypoPitcherStats.OrderBy(x => x.Year).ThenBy(x => x.Month).ToList()));
        }

        // Shared run logic for both model buttons: locks the buttons, runs the model, shows results
        private async Task RunModelAsync(Player requestedPlayer, StringBuilder sb, Func<Task<List<ModelResults>>> runVariants)
        {
            isModelRunning = true;
            UpdateModelButtons();
            try
            {
                List<ModelResults> modelResults = await runVariants();
                // Player changed while the model was running
                if (currentPlayer?.MlbId != requestedPlayer.MlbId)
                    return;
                modelResultsPanel.SetResults(modelResults, ["Existing", "Modified"]);
            }
            catch (PythonException ex)
            {
                PyCore.WriteException(ex);
                sb.AppendLine($"Run_Variants failed: {ex.Message}");
            }
            finally
            {
                isModelRunning = false;
                UpdateModelButtons();
            }
        }

        private void TestRunnerPy_Ready(object? sender, EventArgs e)
        {
            TestRunnerPy.Ready -= TestRunnerPy_Ready;
            UpdateModelButtons();
        }

        // Needed so closing before TestRunnerPy is ready doesn't throw an error
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            TestRunnerPy.Ready -= TestRunnerPy_Ready;
            base.OnFormClosed(e);
        }

        private void UpdateModelButtons()
        {
            bool canRun = TestRunnerPy.IsReady && !isModelRunning && currentPlayer is not null;
            bool? isHitter = dbModelPlayer?.IsHitter;
            btnHitterModelData.Enabled = canRun && isHitter == true;
            btnPitcherModelData.Enabled = canRun && isHitter == false;
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
