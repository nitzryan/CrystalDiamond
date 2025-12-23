using Db;
using ShellProgressBar;
using SiteDb;
using EFCore.BulkExtensions;

namespace SitePrep
{
    internal class GeneratePredictions
    {
        private static bool GenerateHitterPredictions()
        {
            using (SiteDbContext sitedb = new(Constants.SITEDB_OPTIONS))
            {
                sitedb.Truncate<Prediction_HitterStats>();
            }

            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
            var models = db.Output_HitterStatsAggregation.Select(f => f.Model).Distinct().OrderBy(f => f).ToList();
            var dates = db.Output_HitterStatsAggregation.Where(f => f.Year != 0).Select(f => new { f.Year, f.Month }).Distinct().OrderBy(f => f.Year).ThenBy(f => f.Month).ToList();
            List<int> levels = [0, 1, 2, 3, 4, 5, 6, 7];

            List<Prediction_HitterStats> results = new();
            using (ProgressBar progressBar = new ProgressBar(models.Count * dates.Count * levels.Count, "Generating Hitter Predictions"))
            {
                foreach(var date in dates)
                {
                    foreach(var level in levels)
                    {
                        if (level == 5 && date.Year >= 2020) // Short season A was discontinued
                        {
                            foreach(var model in models)
                            {
                                progressBar.Tick();
                            }
                            continue;
                        }

                        int year = date.Year;
                        int month = date.Month;
                        // Get baseline for all leagues at this level
                        // If data doesn't exist, go back month by month until it does
                        List<int> leagues = new();
                        do
                        {
                            leagues = db.Player_Hitter_MonthStats.Where(f => f.Year == year && f.LevelId == Constants.ModelLevelToMlbLevel[level]).Select(f => f.LeagueId).Distinct().ToList();
                                
                            if (leagues.Count == 0)
                            {
                                month--;
                                if (month <= 3)
                                {
                                    month = 9;
                                    year--;
                                }
                            }
                        } while (leagues.Count == 0);

                        // Get all league baselines for that level
                        var leagueStats = db.League_HitterYearStats.Where(f => f.Year == year && f.Month == month && leagues.Contains(f.LeagueId)).ToList();
                        var leagueBaselines = db.LeagueStats.Where(f => f.Year == year && leagues.Contains(f.LeagueId)).ToList();

                        if (leagueStats.Count == 0)
                        {
                            throw new Exception($"No League_HitterStats found for {date.Year}-{date.Month}-({year}-{month})-{level}");
                        }
                        if (leagueBaselines.Count == 0)
                        {
                            throw new Exception($"No LeagueStats found for {date.Year}-{date.Month}-({year}-{month})-{level}");
                        }

                        // Take average of each league
                        League_HitterYearStats leagueStatsAvg = Utilities.MergeLeagueHitterYearStats(leagueStats);
                        LeagueStats leagueBaselineAvg = Utilities.MergeLeagueStats(leagueBaselines);

                        float leaguewRCperPA = (((leagueBaselineAvg.AvgHitterWOBA - leagueBaselineAvg.AvgWOBA) / leagueBaselineAvg.WOBAScale) + leagueBaselineAvg.RPerPA);

                        // Get all predictions for this level
                        foreach (var model in models) // Allows for better indexing to include
                        {
                            var players = db.Output_HitterStatsAggregation.Where(f => f.Model == model && f.Year == date.Year && f.Month == date.Month && f.LevelId == level).ToList();
                            foreach (var player in players)
                            {
                                
                                // Convert player rates and stat rates to raw numbers
                                float hit1B = player.Hit1B * leagueStatsAvg.Hit1B * player.Pa;
                                float hit2B = player.Hit2B * leagueStatsAvg.Hit2B * player.Pa;
                                float hit3B = player.Hit3B * leagueStatsAvg.Hit3B * player.Pa;
                                float hitHR = player.HitHR * leagueStatsAvg.HitHR * player.Pa;
                                float BB = player.BB * leagueStatsAvg.BB * player.Pa;
                                float HBP = player.HBP * leagueStatsAvg.HBP * player.Pa;
                                float K = player.K * leagueStatsAvg.K * player.Pa;
                                float SB = player.SB * leagueStatsAvg.SB * player.Pa;
                                float CS = player.CS * leagueStatsAvg.CS * player.Pa;

                                // Do some stat calculations

                                float wOBA = Utilities.CalculateWOBA(leagueBaselineAvg, HBP, BB, hit1B, hit2B, hit3B, hitHR, player.Pa);
                                float ab = (player.Pa - BB - HBP);
                                float avg = (hit1B + hit2B + hit3B + hitHR) / ab;
                                float slg = (hit1B + (2 * hit2B) + (3 * hit3B) + (4 * hitHR)) / ab;
                                float iso = avg - slg;
                                float obp = (hit1B + hit2B + hit3B + hitHR + BB + HBP) / player.Pa;

                                // Calculate wRC and OFF
                                float wRAAPerPA = (wOBA - leagueBaselineAvg.AvgWOBA) / leagueBaselineAvg.WOBAScale;
                                // wRC+ = (a + (b - c)) / d * 100
                                float a = wRAAPerPA + leagueBaselineAvg.RPerPA;
                                float b = leagueBaselineAvg.RPerPA;
                                float c = (player.ParkRunFactor * leagueBaselineAvg.RPerPA);
                                float d = leaguewRCperPA;
                                float wrc = 100 * (a + (b - c)) / d;

                                // Calculate value
                                float bsr = (leagueBaselineAvg.RunSB * SB) + (leagueBaselineAvg.RunCS * CS);
                                float def = Utilities.CalculateDef(player.Pa, player.PercC, player.Perc1B, player.Perc2B, player.Perc3B, player.PercSS, player.PercLF, player.PercCF, player.PercRF, player.PercDH);
                                float off = wRAAPerPA * player.Pa;
                                float replacementRuns =
                                    Constants.REPLACEMENT_LEVEL_WIN_PERCENTAGE * Constants.HITTER_WAR_PERCENTAGE *
                                    leagueBaselineAvg.LeagueGames * leagueBaselineAvg.RPerWin / leagueBaselineAvg.LeaguePA * player.Pa;
                                float runsAboveRep = replacementRuns + bsr + def + off;
                                float war = runsAboveRep / leagueBaselineAvg.RPerWin;

                                if (level == 0 && date.Year == 2025 && date.Month == 9 && player.MlbId == 691406)
                                {
                                    leagueBaselineAvg.Year += 0;
                                }

                                results.Add(new Prediction_HitterStats{
                                    MlbId = player.MlbId,
                                    Month = date.Month,
                                    Year = date.Year,
                                    Model = model,
                                    LevelId = level,
                                    Pa = player.Pa,
                                    Hit1B = hit1B,
                                    Hit2B = hit2B,
                                    Hit3B = hit3B,
                                    HitHR = hitHR,
                                    BB = BB,
                                    HBP = HBP,
                                    K = K,
                                    SB = SB,
                                    CS = CS,
                                    ParkRunFactor = player.ParkRunFactor,
                                    AVG = avg,
                                    OBP = obp,
                                    SLG = slg,
                                    ISO = iso,
                                    WRC = wrc,
                                    CrOFF = off,
                                    CrDEF = def,
                                    CrBSR = bsr,
                                    CrWAR = war,
                                    PercC = player.PercC,
                                    Perc1B = player.Perc1B,
                                    Perc2B = player.Perc2B,
                                    Perc3B = player.Perc3B,
                                    PercSS = player.PercSS,
                                    PercLF = player.PercLF,
                                    PercCF = player.PercCF,
                                    PercRF = player.PercRF,
                                    PercDH = player.PercDH,
                                });
                            }
                            progressBar.Tick();
                        }
                    }
                }
            }

            siteDb.BulkInsert(results);
            return true;
        }

        private static bool GeneratePitcherPredictions()
        {
            using (SiteDbContext sitedb = new(Constants.SITEDB_OPTIONS))
            {
                sitedb.Truncate<Prediction_PitcherStats>();
            }

            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
            var models = db.Output_PitcherStatsAggregation.Select(f => f.Model).Distinct().OrderBy(f => f).ToList();
            var dates = db.Output_PitcherStatsAggregation.Where(f => f.Year != 0).Select(f => new { f.Year, f.Month }).Distinct().OrderBy(f => f.Year).ThenBy(f => f.Month).ToList();
            List<int> levels = [0, 1, 2, 3, 4, 5, 6, 7];
            List<Prediction_PitcherStats> results = new();

            using (ProgressBar progressBar = new ProgressBar(models.Count * dates.Count * levels.Count, "Generating Pitcher Predictions"))
            {
                foreach (var date in dates)
                {
                    foreach (var level in levels)
                    {
                        if (level == 5 && date.Year >= 2020) // Short season A was discontinued
                        {
                            foreach (var model in models)
                            {
                                progressBar.Tick();
                            }
                            continue;
                        }

                        int year = date.Year;
                        int month = date.Month;
                        // Get baseline for all leagues at this level
                        // If data doesn't exist, go back month by month until it does
                        List<int> leagues = new();
                        do
                        {
                            leagues = db.Player_Hitter_MonthStats.Where(f => f.Year == year && f.LevelId == Constants.ModelLevelToMlbLevel[level]).Select(f => f.LeagueId).Distinct().ToList();

                            if (leagues.Count == 0)
                            {
                                month--;
                                if (month <= 3)
                                {
                                    month = 9;
                                    year--;
                                }
                            }
                        } while (leagues.Count == 0);

                        // Get all league baselines for that level
                        var leagueStats = db.League_PitcherYearStats.Where(f => f.Year == year && f.Month == month && leagues.Contains(f.LeagueId)).OrderBy(f => f.LeagueId).ToList();
                        var leagueBaselines = db.LeagueStats.Where(f => f.Year == year && leagues.Contains(f.LeagueId)).OrderBy(f => f.LeagueId).ToList();

                        if (leagueStats.Count == 0)
                        {
                            throw new Exception($"No League_PitcherStats found for {date.Year}-{date.Month}-({year}-{month})-{level}");
                        }
                        if (leagueBaselines.Count == 0)
                        {
                            throw new Exception($"No LeagueStats found for {date.Year}-{date.Month}-({year}-{month})-{level}");
                        }

                        // Take average of each league
                        League_PitcherYearStats leagueStatsAvg = Utilities.MergeLeaguePitcherYearStats(leagueStats, leagueBaselines.Select(f => f.LeaguePA));
                        LeagueStats lbs = Utilities.MergeLeagueStats(leagueBaselines);
                        float leagueFIPR9 = lbs.LeagueERA + lbs.FIPR9Adjustment;

                        // Get all predictions for this level
                        foreach (var model in models) // Allows for better indexing to include
                        {
                            var players = db.Output_PitcherStatsAggregation.Where(f => f.Model == model && f.Year == date.Year && f.Month == date.Month && f.LevelId == level).ToList();
                            foreach (var player in players)
                            {
                                // Convert player rates and stat rates to raw numbers
                                float pa = (player.Outs_RP + player.Outs_SP) / 0.7f; // Need to get this better
                                float hitHR = player.HR * leagueStatsAvg.HRPerc * pa;
                                float BB = player.BB * leagueStatsAvg.BBPerc * pa;
                                float HBP = player.HBP * leagueStatsAvg.BBPerc * pa * .125f; // Need to add HBP to leaguePitcherStats
                                float K = player.K * leagueStatsAvg.KPerc * pa;
                                float era = player.ERA * leagueStatsAvg.ERA;

                                // Calculate value
                                float fip = Utilities.CalculateFip(lbs.CFIP, player.HR, player.K, player.BB + player.HBP, player.Outs_SP + player.Outs_RP);
                                float fipr9 = fip + lbs.FIPR9Adjustment;
                                float pFIPR9 = fipr9 / player.ParkRunFactor;
                                float raap9 = leagueFIPR9 - pFIPR9;
                                float crRAA = raap9 * (player.Outs_SP + player.Outs_RP) / 27.0f;

                                float numGames = player.GS + player.GR;
                                float inningsPerGame = ((player.Outs_RP + player.Outs_SP) / 3.0f) / numGames;

                                // calculate dynamic runs per win
                                float nonPitcherRunEnvironent = (18 - inningsPerGame) * leagueFIPR9;
                                float pitcherRunEnvironment = (inningsPerGame * pFIPR9);
                                float runEnvironment = (nonPitcherRunEnvironent + pitcherRunEnvironment) / 18;
                                float dRPW = (runEnvironment + 2) * 1.5f;

                                // Wins per game above average
                                float wpgaa = raap9 / dRPW;

                                float replacementLevel = (0.03f * player.RP_Perc) + (0.12f * player.SP_Perc);
                                float wpgar = wpgaa + replacementLevel;
                                float war = wpgar * (player.Outs_RP + player.Outs_SP) / 27.0f;

                                results.Add(new Prediction_PitcherStats
                                {
                                    MlbId = player.MlbId,
                                    Month = date.Month,
                                    Year = date.Year,
                                    Model = model,
                                    LevelId = level,
                                    Outs_SP = player.Outs_SP,
                                    Outs_RP = player.Outs_RP,
                                    GS = player.GS,
                                    GR = player.GR,
                                    BB = BB,
                                    HBP = HBP,
                                    K = K,
                                    HR = hitHR,
                                    ERA = era,
                                    FIP = fip,
                                    ParkRunFactor = player.ParkRunFactor,
                                    CrRAA = crRAA,
                                    CrWAR = war,
                                    SP_Perc = player.SP_Perc,
                                    RP_Perc = player.RP_Perc
                                });
                            }

                            progressBar.Tick();
                        }
                    }
                }
            }

            siteDb.BulkInsert(results);
            return true;
        }

        public static bool Update()
        {
            try {
                if (!GenerateHitterPredictions())
                    return false;

                if (!GeneratePitcherPredictions())
                    return false;

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in GeneratePredictions");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
