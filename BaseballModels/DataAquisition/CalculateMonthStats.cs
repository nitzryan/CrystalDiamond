using Db;
using ShellProgressBar;

namespace DataAquisition
{
    internal class CalculateMonthStats
    {
        private static Player_Hitter_MonthStats GetMonthStatsHitter(IEnumerable<Player_Hitter_GameLog> gameLogs, SqliteDbContext db, int month)
        {
            int totalAb = 0;
            int totalPA = 0;
            int totalH = 0;
            int total2B = 0;
            int total3B = 0;
            int totalHR = 0;
            int totalK = 0;
            int totalBB = 0;
            int totalHBP = 0;
            int totalSB = 0;
            int totalCS = 0;
            List<int> totalPositions = [0,0,0,0,0,0,0,0,0];
            float totalRunFactor = 0;
            float totalHRFactor = 0;

            foreach (var gl in gameLogs)
            {
                totalAb += gl.AB;
                totalH += gl.H;
                total2B += gl.Hit2B;
                total3B += gl.Hit3B;
                totalHR += gl.HR;
                totalK += gl.K;
                totalBB += gl.BB;
                totalHBP += gl.HBP;
                totalSB += gl.SB;
                totalCS += gl.CS;
                totalPA += gl.PA;

                if (gl.Position > 1 && gl.Position <= 9)
                    totalPositions[gl.Position - 2]++;
                else
                    totalPositions[8]++;

                Park_Factors pf = db.Park_Factors.Where(f => f.Year == gl.Year && f.StadiumId == gl.StadiumId).Single();
                totalRunFactor += gl.PA * pf.RunFactor;
                totalHRFactor += gl.PA * pf.HRFactor;

            }

            var first = gameLogs.First();
            return new Player_Hitter_MonthStats
            {
                MlbId = first.MlbId,
                Year = first.Year,
                Month = month,
                LevelId = first.LevelId,
                LeagueId = first.LeagueId,
                AB = totalAb,
                PA = totalPA,
                H = totalH,
                Hit2B = total2B,
                Hit3B = total3B,
                HR = totalHR,
                K = totalK,
                BB = totalBB,
                SB = totalSB,
                CS = totalCS,
                HBP = totalHBP,
                ParkRunFactor = (totalAb > 0) ? totalRunFactor / totalPA : 1.0f,
                ParkHRFactor = (totalAb > 0) ? totalHRFactor / totalPA : 1.0f,
                GamesC = totalPositions[0],
                Games1B = totalPositions[1],
                Games2B = totalPositions[2],
                Games3B = totalPositions[3],
                GamesSS = totalPositions[4],
                GamesLF = totalPositions[5],
                GamesCF = totalPositions[6],
                GamesRF = totalPositions[7],
                GamesDH = totalPositions[8]
            };
        }

        private static Player_Pitcher_MonthStats GetMonthStatsPitcher(IEnumerable<Player_Pitcher_GameLog> gameLogs, SqliteDbContext db, int month)
        {
            int totalBF = 0;
            int totalOuts = 0;
            int totalH = 0;
            int total2B = 0;
            int total3B = 0;
            int totalHR = 0;
            int totalK = 0;
            int totalBB = 0;
            int totalHBP = 0;
            int totalGO = 0;
            int totalAO = 0;
            int totalR = 0;
            int totalER = 0;
            float totalRunFactor = 0;
            float totalHRFactor = 0;
            int outsSP = 0;

            foreach (var gl in gameLogs)
            {
                totalBF += gl.BattersFaced;
                totalH += gl.H;
                total2B += gl.Hit2B;
                total3B += gl.Hit3B;
                totalHR += gl.HR;
                totalK += gl.K;
                totalBB += gl.BB;
                totalHBP += gl.HBP;
                totalOuts += gl.Outs;
                totalGO += gl.GO;
                totalAO += gl.AO;
                totalR += gl.R;
                totalER += gl.ER;
                if (gl.Started == 1)
                    outsSP += gl.Outs;

                Park_Factors pf = db.Park_Factors.Where(f => f.Year == gl.Year && f.StadiumId == gl.StadiumId).Single();
                totalRunFactor += gl.BattersFaced * pf.RunFactor;
                totalHRFactor += gl.BattersFaced * pf.HRFactor;
            }

            var first = gameLogs.First();
            return new Player_Pitcher_MonthStats
            {
                MlbId = first.MlbId,
                Year = first.Year,
                Month = month,
                LevelId = first.LevelId,
                LeagueId = first.LeagueId,
                H = totalH,
                Hit2B = total2B,
                Hit3B = total3B,
                HR = totalHR,
                K = totalK,
                BB = totalBB,
                HBP = totalHBP,
                BattersFaced = totalBF,
                Outs = totalOuts,
                SPPerc = totalOuts > 0 ? (float)(outsSP) / totalOuts : 0.5f,
                GO = totalGO,
                AO = totalAO,
                R = totalR,
                ER = totalER,
                ParkRunFactor = (totalBF > 0) ? totalRunFactor / totalBF : 1.0f,
                ParkHRFactor = (totalBF > 0) ? totalHRFactor / totalBF : 1.0f,
            };
        }

        private static bool CalculateHitterMonthStats(SqliteDbContext db, int year, int month)
        {
            // Remove existing data
            db.Player_Hitter_MonthStats.RemoveRange(db.Player_Hitter_MonthStats.Where(f => f.Year == year && f.Month == month));
            db.Player_Hitter_MonthAdvanced.RemoveRange(db.Player_Hitter_MonthAdvanced.Where(f => f.Year == year && f.Month == month));
            db.SaveChanges();

            // Iterate through player/level combinations
            var monthGames = month == 4 ? 
                db.Player_Hitter_GameLog.Where(f => f.Year == year && f.Month <= month) :
                month == 9 ?
                    db.Player_Hitter_GameLog.Where(f => f.Year == year && f.Month >= month) :
                    db.Player_Hitter_GameLog.Where(f => f.Year == year && f.Month == month);
            var ids = monthGames.Select(f => f.MlbId).Distinct();
            using (ProgressBar progressBar = new ProgressBar(ids.Count(), $"Calculating Hitter Month Stats for Month={month} Year={year}"))
            {
                foreach (int mlbId in ids)
                {
                    progressBar.Tick(); // Tick before so skips don't mess up count

                    var playerGames = monthGames.Where(f => f.MlbId == mlbId);
                    var leagues = playerGames.Select(f => f.LeagueId).Distinct();
                    foreach (int leagueId in leagues)
                    {
                        var gameLogs = playerGames.Where(f => f.LeagueId == leagueId);

                        var stats = GetMonthStatsHitter(gameLogs, db, month);
                        if (stats.AB + stats.BB + stats.HBP + stats.SB + stats.CS == 0)
                            continue;

                        db.Player_Hitter_MonthStats.Add(stats);

                        // Advanced Stats
                        var teamLeagues = gameLogs.Select(f => new { f.TeamId, f.LeagueId }).Distinct();
                        foreach (var a in teamLeagues)
                        {
                            stats = GetMonthStatsHitter(gameLogs.Where(f => f.TeamId == a.TeamId && f.LeagueId == a.LeagueId), db, month);
                            Player_Hitter_MonthAdvanced ma = Utilities.HitterNormalToAdvanced(stats, db.LeagueStats.Where(f => f.LeagueId == a.LeagueId && f.Year == year).Single());
                            ma.TeamId = a.TeamId;
                            ma.LeagueId = a.LeagueId;

                            if (ma.LevelId == 1)
                            {
                                // Use Fangraphs WAR
                                Player_MonthlyWar? pwm = db.Player_MonthlyWar.Where(f => f.MlbId == mlbId && f.Year == year && f.Month == month).SingleOrDefault();
                                if (pwm == null)
                                {
                                    ma.CrOFF = 0;
                                    ma.CrBSR = 0;
                                    ma.CrDEF = 0;
                                    ma.CrWAR = 0;
                                }
                                else
                                {
                                    ma.CrOFF = pwm.OFF;
                                    ma.CrBSR = pwm.BSR;
                                    ma.CrDEF = pwm.DEF;
                                    ma.CrWAR = pwm.WAR_h;
                                }
                            }
                            else
                            {
                                // Calculate crWAR
                                LeagueStats ls = db.LeagueStats.Where(f => f.Year == year && f.LeagueId == a.LeagueId).Single();
                                float wRAA = ((ma.WOBA - ls.AvgWOBA) / ls.WOBAScale) * ma.PA;
                                float battingRuns = wRAA + ((ls.RPerPA * (1.0f - ma.ParkFactor)) * ma.PA); // Ignored pitcher adjustment for now
                                float baserunningRuns = (ma.SB * ls.RunSB) + (ma.CS * ls.RunCS);
                                float defensiveRuns =
                                    (Constants.POSITIONAL_ADJUSTMENT_C * stats.GamesC) +
                                    (Constants.POSITIONAL_ADJUSTMENT_1B * stats.Games1B) +
                                    (Constants.POSITIONAL_ADJUSTMENT_2B * stats.Games2B) +
                                    (Constants.POSITIONAL_ADJUSTMENT_3B * stats.Games3B) +
                                    (Constants.POSITIONAL_ADJUSTMENT_SS * stats.GamesSS) +
                                    (Constants.POSITIONAL_ADJUSTMENT_LF * stats.GamesLF) +
                                    (Constants.POSITIONAL_ADJUSTMENT_CF * stats.GamesCF) +
                                    (Constants.POSITIONAL_ADJUSTMENT_RF * stats.GamesRF) +
                                    (Constants.POSITIONAL_ADJUSTMENT_DH * stats.GamesDH);

                                ma.CrOFF = battingRuns;
                                ma.CrBSR = baserunningRuns;
                                ma.CrDEF = defensiveRuns;

                                float replacementRuns =
                                    Constants.REPLACEMENT_LEVEL_WIN_PERCENTAGE * Constants.HITTER_WAR_PERCENTAGE *
                                    ls.LeagueGames * ls.RPerWin / ls.LeaguePA * ma.PA;

                                float runsAboveReplacement = battingRuns + baserunningRuns + defensiveRuns + replacementRuns;
                                ma.CrWAR = runsAboveReplacement / ls.RPerWin;
                            }

                            db.Player_Hitter_MonthAdvanced.Add(ma);
                        }

                    
                    }
                }
            }
            
            db.SaveChanges();
            db.ChangeTracker.Clear();

            return true;
        }

        private static bool CalculatePitcherMonthStats(SqliteDbContext db, int year, int month)
        {
            // Remove existing data
            db.Player_Pitcher_MonthStats.RemoveRange(db.Player_Pitcher_MonthStats.Where(f => f.Year == year && f.Month == month));
            db.Player_Pitcher_MonthAdvanced.RemoveRange(db.Player_Pitcher_MonthAdvanced.Where(f => f.Year == year && f.Month == month));
            db.SaveChanges();

            // Iterate through player/level combinations
            // Iterate through player/level combinations
            var monthGames = month == 4 ?
                db.Player_Pitcher_GameLog.Where(f => f.Year == year && f.Month <= month) :
                month == 9 ?
                    db.Player_Pitcher_GameLog.Where(f => f.Year == year && f.Month >= month) :
                    db.Player_Pitcher_GameLog.Where(f => f.Year == year && f.Month == month);
            var ids = monthGames.Select(f => f.MlbId).Distinct();
            using (ProgressBar progressBar = new ProgressBar(ids.Count(), $"Calculating Pitcher Month Stats for Month={month} Year={year}"))
            {
                foreach (int mlbId in ids)
                {
                    progressBar.Tick(); // Tick before so skips don't mess up count

                    var playerGames = monthGames.Where(f => f.MlbId == mlbId);
                    var leagues = playerGames.Select(f => f.LeagueId).Distinct();
                    foreach (int leagueId in leagues)
                    {
                        var gameLogs = playerGames.Where(f => f.LeagueId == leagueId);

                        var stats = GetMonthStatsPitcher(gameLogs, db, month);
                        if (stats.BattersFaced == 0)
                            continue;

                        db.Player_Pitcher_MonthStats.Add(stats);

                        // Advanced Stats
                        var teamLeagues = gameLogs.Select(f => new { f.TeamId, f.LeagueId }).Distinct();
                        foreach (var a in teamLeagues)
                        {
                            var games = gameLogs.Where(f => f.TeamId == a.TeamId && f.LeagueId == a.LeagueId);
                            stats = GetMonthStatsPitcher(games, db, month);
                            Player_Pitcher_MonthAdvanced ma = Utilities.PitcherNormalToAdvanced(stats, db.LeagueStats.Where(f => f.LeagueId == a.LeagueId && f.Year == year).Single(), db);
                            ma.TeamId = a.TeamId;
                            ma.LeagueId = a.LeagueId;

                            if (ma.LevelId == 1)
                            {
                                // Use Fangraphs WAR
                                Player_MonthlyWar? pwm = db.Player_MonthlyWar.Where(f => f.MlbId == mlbId && f.Year == year && f.Month == month).SingleOrDefault();

                                if (pwm == null)
                                {
                                    ma.CrWAR = 0;
                                }
                                else
                                {
                                    ma.CrWAR = pwm.WAR_r + pwm.WAR_s;
                                }
                            }
                            else
                            {
                                // crWAR
                                // https://library.fangraphs.com/war/calculating-war-pitchers/
                                LeagueStats ls = db.LeagueStats.Where(f => f.LeagueId == a.LeagueId && f.Year == year).Single();
                                float fip = Utilities.CalculateFip(ls.CFIP, stats.HR, stats.K, stats.BB + stats.HBP, stats.Outs);
                                float fipr9 = fip + ls.FIPR9Adjustment;
                                float pFIPR9 = fipr9 / stats.ParkRunFactor;
                                float leagueFIPR9 = ls.LeagueERA + ls.FIPR9Adjustment;
                                float raap9 = leagueFIPR9 - pFIPR9;

                                int numGames = games.Count();
                                float inningsPerGame = (stats.Outs / 3.0f) / numGames;

                                // calculate dynamic runs per win
                                float nonPitcherRunEnvironent = (18 - inningsPerGame) * leagueFIPR9;
                                float pitcherRunEnvironment = (inningsPerGame * pFIPR9);
                                float runEnvironment = (nonPitcherRunEnvironent + pitcherRunEnvironment) / 18;
                                float dRPW = (runEnvironment + 2) * 1.5f;

                                // Wins per game above average
                                float wpgaa = raap9 / dRPW;

                                int gamesStarted = games.Where(f => f.Started == 1).Count();
                                float startPercentage = (float)gamesStarted / numGames;
                                float replacementLevel = (0.03f * (1 - startPercentage)) + (0.12f * startPercentage);
                                float wpgar = wpgaa + replacementLevel;

                                ma.CrWAR = wpgar * stats.Outs / 27.0f;
                            }

                            db.Player_Pitcher_MonthAdvanced.Add(ma);
                        }
                    }
                }
            }

            db.SaveChanges();
            db.ChangeTracker.Clear();

            return true;
        }

        public static bool Main(int year, int month)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            try {
                if (!CalculateHitterMonthStats(db, year, month))
                {
                    Console.WriteLine("Error in CalculateHitterMonthStats");
                    return false;
                }
            } catch (Exception e)
            {
                Console.WriteLine("Error in CalculateHitterMonthStats");
                Utilities.LogException(e);
                return false;
            }

            try {
                if (!CalculatePitcherMonthStats(db, year, month))
                {
                    Console.WriteLine("Error in CalculatePitcherMonthStats");
                    return false;
                }
            } catch (Exception e)
            {
                Console.WriteLine("Error in CalculatePitcherMonthStats");
                Utilities.LogException(e);
                return false;
            }

            return true;
        }
    }
}
