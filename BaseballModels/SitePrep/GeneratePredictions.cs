using ModelDb;
using Db;
using ShellProgressBar;
using SiteDb;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace SitePrep
{
    public static class PredictionConverter
    {
        public static bool IsLevelDiscontinued(int level, int year)
        {
            return level == 5 && year >= 2020; // Short season A was discontinued
        }

        public static Prediction_HitterStats? ConvertHitter(Output_HitterStatsAggregation output, LeagueBaselineCache cache)
        {
            if (output.Year == 0 || IsLevelDiscontinued(output.LevelId, output.Year))
                return null;

            HitterBaseline baseline = cache.GetHitterBaseline(output.Year, output.Month, output.LevelId);
            League_HitterYearStats leagueStatsAvg = baseline.StatsAvg;
            LeagueStats ls = baseline.BaselineAvg;
            float pa = output.Pa;

            // Convert player rates and stat rates to raw numbers
            float hit1B = output.Hit1B * leagueStatsAvg.Hit1B * pa;
            float hit2B = output.Hit2B * leagueStatsAvg.Hit2B * pa;
            float hit3B = output.Hit3B * leagueStatsAvg.Hit3B * pa;
            float hitHR = output.HitHR * leagueStatsAvg.HitHR * pa;
            float BB = output.BB * leagueStatsAvg.BB * pa;
            float HBP = output.HBP * leagueStatsAvg.HBP * pa;
            float K = output.K * leagueStatsAvg.K * pa;
            float SB = output.SB * leagueStatsAvg.SB * pa;
            float CS = output.CS * leagueStatsAvg.CS * pa;

            // Do some stat calculations
            float ab = pa - BB - HBP; // TODO : This does not track sac flies/bunts
            float avg = (hit1B + hit2B + hit3B + hitHR) / ab;
            float slg = (hit1B + (2 * hit2B) + (3 * hit3B) + (4 * hitHR)) / ab;
            float iso = slg - avg;
            float obp = (hit1B + hit2B + hit3B + hitHR + BB + HBP) / pa;
            

            // Calculate value
            float bsr = output.BSR * pa / 100.0f;
            float def_pos = Utilities.CalculateDef(pa, output.PercC, output.Perc1B, output.Perc2B, output.Perc3B, output.PercSS, output.PercLF, output.PercCF, output.PercRF, output.PercDH);
            float draa = output.DRAA * pa / 100.0f;
            float def = def_pos + draa;

            var warValues = DataAquisition.Utilities.GetHitterWarValues(ls, HBP, BB, hit1B, hit2B, hit3B, hitHR, pa,
                output.ParkRunFactor, bsr, def);
            float wrc = DataAquisition.Utilities.CalculateWrcPlus(warValues.Woba, output.ParkRunFactor, ls);


            return new Prediction_HitterStats
            {
                MlbId = output.MlbId,
                Month = output.Month,
                Year = output.Year,
                Model = output.ModelId,
                LevelId = output.LevelId,
                Pa = pa,
                Hit1B = hit1B,
                Hit2B = hit2B,
                Hit3B = hit3B,
                HitHR = hitHR,
                BB = BB,
                HBP = HBP,
                K = K,
                SB = SB,
                CS = CS,
                ParkRunFactor = output.ParkRunFactor,
                AVG = avg,
                OBP = obp,
                SLG = slg,
                ISO = iso,
                WRC = wrc,
                CrOFF = warValues.CrOFF,
                CrDEF = def,
                CrDPOS = def_pos,
                CrDRAA = draa,
                CrBSR = bsr,
                CrWAR = warValues.CrWAR,
                PercC = output.PercC,
                Perc1B = output.Perc1B,
                Perc2B = output.Perc2B,
                Perc3B = output.Perc3B,
                PercSS = output.PercSS,
                PercLF = output.PercLF,
                PercCF = output.PercCF,
                PercRF = output.PercRF,
                PercDH = output.PercDH,
            };
        }

        public static Prediction_PitcherStats? ConvertPitcher(Output_PitcherStatsAggregation output, LeagueBaselineCache cache)
        {
            // The pipeline never produced rows for these, so the shared function doesn't either
            if (output.Year == 0 || IsLevelDiscontinued(output.LevelId, output.Year))
                return null;

            PitcherBaseline baseline = cache.GetPitcherBaseline(output.Year, output.Month, output.LevelId);
            League_PitcherYearStats leagueStatsAvg = baseline.StatsAvg;
            LeagueStats ls = baseline.BaselineAvg;

            // Convert player rates and stat rates to raw numbers
            float outs = output.Outs_SP + output.Outs_RP;
            float pa = outs / 0.7f; // TODO : Need to get this better
            float hitHR = output.HR * leagueStatsAvg.HRPerc * pa;
            float BB = output.BB * leagueStatsAvg.BBPerc * pa;
            float HBP = output.HBP * leagueStatsAvg.BBPerc * pa * .125f; // TODO : Need to add HBP to leaguePitcherStats
            float K = output.K * leagueStatsAvg.KPerc * pa;
            float era = output.ERA * leagueStatsAvg.ERA;
            float hr9 = hitHR * 27.0f / outs;

            // Calculate value
            var warValues = DataAquisition.Utilities.GetPitcherWarValues(ls, hitHR, K, BB + HBP, outs, era,
                games: output.GS + output.GR, spPerc: output.SP_Perc, parkRunFactor: output.ParkRunFactor);

            return new Prediction_PitcherStats
            {
                MlbId = output.MlbId,
                Month = output.Month,
                Year = output.Year,
                Model = output.ModelId,
                LevelId = output.LevelId,
                Outs_SP = output.Outs_SP,
                Outs_RP = output.Outs_RP,
                GS = output.GS,
                GR = output.GR,
                BB = BB,
                HBP = HBP,
                K = K,
                HR = hitHR,
                ERA = era,
                FIP = warValues.FIP,
                ERAMinus = warValues.ERAMinus,
                FIPMinus = warValues.FIPMinus,
                ParkRunFactor = output.ParkRunFactor,
                CrRAA = warValues.CrRAA,
                CrWAR = warValues.CrWAR,
                SP_Perc = output.SP_Perc,
                RP_Perc = output.RP_Perc,
                BBPerc = (float)Math.Round(output.BB * leagueStatsAvg.BBPerc * 100, 1),
                KPerc = (float)Math.Round(output.K * leagueStatsAvg.KPerc * 100, 1),
                HR9 = hr9,
            };
        }
    }

    internal record HitterBaseline(League_HitterYearStats StatsAvg, LeagueStats BaselineAvg);
    internal record PitcherBaseline(League_PitcherYearStats StatsAvg, LeagueStats BaselineAvg);
    public class LeagueBaselineCache
    {
        private readonly Dictionary<(int Year, int LevelId), List<int>> leaguesByYearLevel;
        private readonly ILookup<(int Year, int Month), League_HitterYearStats> hitterStatsByDate;
        private readonly ILookup<(int Year, int Month), League_PitcherYearStats> pitcherStatsByDate;
        private readonly ILookup<int, LeagueStats> leagueBaselinesByYear;
        
        private readonly int minYear;

        private readonly Dictionary<(int Year, int Month, int Level), HitterBaseline> hitterBaselines = new();
        private readonly Dictionary<(int Year, int Month, int Level), PitcherBaseline> pitcherBaselines = new();

        public LeagueBaselineCache(
            IEnumerable<(int Year, int LevelId, int LeagueId)> levelLeagues,
            IEnumerable<League_HitterYearStats> hitterLeagueStats,
            IEnumerable<League_PitcherYearStats> pitcherLeagueStats,
            IEnumerable<LeagueStats> leagueBaselines)
        {
            leaguesByYearLevel = levelLeagues
                .GroupBy(f => (f.Year, f.LevelId))
                .ToDictionary(g => g.Key, g => g.Select(f => f.LeagueId).Distinct().ToList());
            hitterStatsByDate = hitterLeagueStats.ToLookup(f => (f.Year, f.Month));
            pitcherStatsByDate = pitcherLeagueStats.ToLookup(f => (f.Year, f.Month));
            leagueBaselinesByYear = leagueBaselines.ToLookup(f => f.Year);
            minYear = leaguesByYearLevel.Count > 0 ? leaguesByYearLevel.Keys.Min(k => k.Year) : int.MaxValue;
        }

        public static LeagueBaselineCache Load(SqliteDbContext db)
        {
            var levelLeagues = db.Player_Hitter_MonthStats
                .Select(f => new { f.Year, f.LevelId, f.LeagueId })
                .Distinct()
                .ToList()
                .Select(f => (f.Year, f.LevelId, f.LeagueId));

            return new LeagueBaselineCache(
                levelLeagues,
                db.League_HitterYearStats.AsNoTracking().ToList(),
                db.League_PitcherYearStats.AsNoTracking().ToList(),
                db.LeagueStats.AsNoTracking().ToList());
        }

        internal HitterBaseline GetHitterBaseline(int dateYear, int dateMonth, int level)
        {
            var key = (dateYear, dateMonth, level);
            if (hitterBaselines.TryGetValue(key, out HitterBaseline? cached))
                return cached;

            var (year, month, leagues, leagueBaselines) = ResolveLeagues(dateYear, dateMonth, level);

            var leagueStats = hitterStatsByDate[(year, month)]
                .Where(f => leagues.Contains(f.LeagueId))
                .OrderBy(f => f.LeagueId)
                .ToList();

            if (leagueStats.Count == 0)
                throw new Exception($"No League_HitterStats found for {dateYear}-{dateMonth}-({year}-{month})-{level}");
            if (leagueBaselines.Count == 0)
                throw new Exception($"No LeagueStats found for {dateYear}-{dateMonth}-({year}-{month})-{level}");

            HitterBaseline baseline = new(
                Utilities.MergeLeagueHitterYearStats(leagueStats),
                Utilities.MergeLeagueStats(leagueBaselines));
            hitterBaselines[key] = baseline;
            return baseline;
        }

        internal PitcherBaseline GetPitcherBaseline(int dateYear, int dateMonth, int level)
        {
            var key = (dateYear, dateMonth, level);
            if (pitcherBaselines.TryGetValue(key, out PitcherBaseline? cached))
                return cached;

            var (year, month, leagues, leagueBaselines) = ResolveLeagues(dateYear, dateMonth, level);

            // Ordering matters here: the merge pairs each league's stats with its PA weight by position
            var leagueStats = pitcherStatsByDate[(year, month)]
                .Where(f => leagues.Contains(f.LeagueId))
                .OrderBy(f => f.LeagueId)
                .ToList();

            if (leagueStats.Count == 0)
                throw new Exception($"No League_PitcherStats found for {dateYear}-{dateMonth}-({year}-{month})-{level}");
            if (leagueBaselines.Count == 0)
                throw new Exception($"No LeagueStats found for {dateYear}-{dateMonth}-({year}-{month})-{level}");

            PitcherBaseline baseline = new(
                Utilities.MergeLeaguePitcherYearStats(leagueStats, leagueBaselines.Select(f => f.LeaguePA)),
                Utilities.MergeLeagueStats(leagueBaselines));
            pitcherBaselines[key] = baseline;
            return baseline;
        }

        private (int Year, int Month, List<int> Leagues, List<LeagueStats> LeagueBaselines) ResolveLeagues(int dateYear, int dateMonth, int level)
        {
            int year = dateYear;
            int month = dateMonth;
            int mlbLevelId = Constants.ModelLevelToMlbLevel[level];

            // If data doesn't exist, go back month by month until it does
            List<int>? leagues;
            while (!leaguesByYearLevel.TryGetValue((year, mlbLevelId), out leagues))
            {
                month--;
                if (month <= 3)
                {
                    month = 9;
                    year--;
                }
                
                if (year < minYear)
                    throw new Exception($"No leagues found for {dateYear}-{dateMonth}-{level}");
            }

            List<LeagueStats> leagueBaselines = leagueBaselinesByYear[year]
                .Where(f => leagues.Contains(f.LeagueId))
                .OrderBy(f => f.LeagueId)
                .ToList();

            return (year, month, leagues, leagueBaselines);
        }
    }

    internal class GeneratePredictions
    {
        private static void GenerateHitterPredictions()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            siteDb.Prediction_HitterStats.ExecuteDelete();

            LeagueBaselineCache cache = LeagueBaselineCache.Load(db);

            var models = modelDb.Output_HitterStatsAggregation.Select(f => f.ModelId).Distinct().OrderBy(f => f).ToList();
            var dates = modelDb.Output_HitterStatsAggregation.Where(f => f.Year != 0).Select(f => new { f.Year, f.Month }).Distinct().OrderBy(f => f.Year).ThenBy(f => f.Month).ToList();
            List<int> levels = [0, 1, 2, 3, 4, 5, 6, 7];

            List<Prediction_HitterStats> results = new();
            using (ProgressBar progressBar = new ProgressBar(models.Count * dates.Count * levels.Count, "Generating Hitter Predictions"))
            {
                foreach(var date in dates)
                {
                    foreach(var level in levels)
                    {
                        if (PredictionConverter.IsLevelDiscontinued(level, date.Year)) // Short season A was discontinued
                        {
                            foreach(var model in models)
                            {
                                progressBar.Tick();
                            }
                            continue;
                        }

                        cache.GetHitterBaseline(date.Year, date.Month, level);

                        // Get all predictions for this level
                        foreach (var model in models) // Allows for better indexing to include
                        {
                            var players = modelDb.Output_HitterStatsAggregation.Where(f => f.ModelId == model && f.Year == date.Year && f.Month == date.Month && f.LevelId == level).ToList();
                            foreach (var player in players)
                            {
                                Prediction_HitterStats? prediction = PredictionConverter.ConvertHitter(player, cache);
                                if (prediction != null)
                                    results.Add(prediction);
                            }
                            progressBar.Tick();
                        }
                    }
                }
            }

            siteDb.BulkInsert(results);
        }

        private static void GeneratePitcherPredictions()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
            using ModelDbContext modelDb = new(Constants.MODELDB_OPTIONS);

            siteDb.Prediction_PitcherStats.ExecuteDelete();

            LeagueBaselineCache cache = LeagueBaselineCache.Load(db);

            var models = modelDb.Output_PitcherStatsAggregation.Select(f => f.ModelId).Distinct().OrderBy(f => f).ToList();
            var dates = modelDb.Output_PitcherStatsAggregation.Where(f => f.Year != 0).Select(f => new { f.Year, f.Month }).Distinct().OrderBy(f => f.Year).ThenBy(f => f.Month).ToList();
            List<int> levels = [0, 1, 2, 3, 4, 5, 6, 7];
            List<Prediction_PitcherStats> results = new();

            using (ProgressBar progressBar = new ProgressBar(models.Count * dates.Count * levels.Count, "Generating Pitcher Predictions"))
            {
                foreach (var date in dates)
                {
                    foreach (var level in levels)
                    {
                        if (PredictionConverter.IsLevelDiscontinued(level, date.Year))
                        {
                            foreach (var model in models)
                            {
                                progressBar.Tick();
                            }

                            continue;
                        }
                        

                        cache.GetPitcherBaseline(date.Year, date.Month, level);

                        // Get all predictions for this level
                        foreach (var model in models)
                        {
                            var players = modelDb.Output_PitcherStatsAggregation.Where(f => f.ModelId == model && f.Year == date.Year && f.Month == date.Month && f.LevelId == level).ToList();
                            foreach (var player in players)
                            {
                                Prediction_PitcherStats? prediction = PredictionConverter.ConvertPitcher(player, cache);
                                if (prediction != null)
                                    results.Add(prediction);
                            }

                            progressBar.Tick();
                        }
                    }
                }
            }

            siteDb.BulkInsert(results);
        }

        public static void Update()
        {
            try {
                GenerateHitterPredictions();
                GeneratePitcherPredictions();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in GeneratePredictions");
                Utilities.LogException(e);
                throw;
            }
        }
    }
}
