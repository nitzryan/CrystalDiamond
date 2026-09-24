using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace DataAquisition.MonthStats
{
    internal class CalculateMonthStats
    {
        private static Player_Hitter_MonthStats GetMonthStatsHitter(IEnumerable<Player_Hitter_GameLog> gameLogs, Dictionary<int, Park_Factors> parkFactorDict, int month)
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

                Park_Factors pf = parkFactorDict[gl.StadiumId];
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
                ParkRunFactor = totalAb > 0 ? totalRunFactor / totalPA : 1.0f,
                ParkHRFactor = totalAb > 0 ? totalHRFactor / totalPA : 1.0f,
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

        private static Player_Pitcher_MonthStats GetMonthStatsPitcher(IEnumerable<Player_Pitcher_GameLog> gameLogs, Dictionary<int, Park_Factors> parkFactorDict, int month)
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

                Park_Factors pf = parkFactorDict[gl.StadiumId];
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
                G = gameLogs.Count(),
                H = totalH,
                Hit2B = total2B,
                Hit3B = total3B,
                HR = totalHR,
                K = totalK,
                BB = totalBB,
                HBP = totalHBP,
                BattersFaced = totalBF,
                Outs = totalOuts,
                SPPerc = totalOuts > 0 ? (float)outsSP / totalOuts : 0.5f,
                GO = totalGO,
                AO = totalAO,
                R = totalR,
                ER = totalER,
                ParkRunFactor = totalBF > 0 ? totalRunFactor / totalBF : 1.0f,
                ParkHRFactor = totalBF > 0 ? totalHRFactor / totalBF : 1.0f,
            };
        }

        private static void CalculateHitterMonthStats(SqliteDbContext db, int year, int month)
        {
            db.Player_Hitter_MonthStats.Where(f => f.Year == year && f.Month == month).ExecuteDelete();

            // Store all park factor values to reduce db queries
            Dictionary<int, Park_Factors> ParkFactorDict = new();
            var parkFactors = db.Park_Factors.Where(f => f.Year == year);
            foreach (var pf in parkFactors)
                ParkFactorDict[pf.StadiumId] = pf;

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

                    var playerGames = monthGames.Where(f => f.MlbId == mlbId).ToArray();
                    var leagues = playerGames.Select(f => f.LeagueId).Distinct();
                    foreach (int leagueId in leagues)
                    {
                        var gameLogs = playerGames.Where(f => f.LeagueId == leagueId);

                        var stats = GetMonthStatsHitter(gameLogs, ParkFactorDict, month);
                        if (stats.AB + stats.BB + stats.HBP + stats.SB + stats.CS == 0)
                            continue;

                        db.Player_Hitter_MonthStats.Add(stats);
                    }
                }
            }
            
            db.SaveChanges();
            db.ChangeTracker.Clear();
        }

        private static void CalculateHitterMonthStatsAdvanced(SqliteDbContext db, int year, int month)
        {
            // Remove existing data
            db.Player_Hitter_MonthAdvanced.Where(f => f.Year == year && f.Month == month).ExecuteDelete();

            // Store DB Values
            Dictionary<int, Park_Factors> parkFactorDict =
                db.Park_Factors
                    .Where(f => f.Year == year)
                    .ToDictionary(f => f.StadiumId);
            Dictionary<(int MlbId, int TeamId), IEnumerable<Player_Fielder_MonthStats>> fielderDict =
                db.Player_Fielder_MonthStats
                    .Where(f => f.Year == year && f.Month == month)
                    .AsEnumerable()
                    .GroupBy(f => (f.MlbId, f.TeamId))
                    .ToDictionary(f => f.Key, f => f.AsEnumerable());
            Dictionary<(int MlbId, int TeamId), Player_Hitter_MonthBaserunning> bsrDict =
                db.Player_Hitter_MonthBaserunning
                    .Where(f => f.Year == year && f.Month == month)
                    .ToDictionary(f => (f.MlbId, f.TeamId));
            Dictionary<int, LeagueStats> leagueDict =
                db.LeagueStats
                    .Where(f => f.Year == year)
                    .ToDictionary(f => f.LeagueId);

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

                    var playerGames = monthGames.Where(f => f.MlbId == mlbId).ToArray();
                    var leagues = playerGames.Select(f => f.LeagueId).Distinct();
                    foreach (int leagueId in leagues)
                    {
                        var gameLogs = playerGames.Where(f => f.LeagueId == leagueId);

                        // Advanced Stats
                        var teamLeagues = gameLogs.Select(f => new { f.TeamId, f.LeagueId }).Distinct();
                        foreach (var a in teamLeagues)
                        {
                            var stats = GetMonthStatsHitter(gameLogs.Where(f => f.TeamId == a.TeamId && f.LeagueId == a.LeagueId), parkFactorDict, month);
                            if (stats.AB + stats.BB + stats.HBP + stats.SB + stats.CS == 0)
                                continue;

                            var defStats = fielderDict.GetValueOrDefault((mlbId, a.TeamId), []);

                            Player_Hitter_MonthAdvanced ma = Utilities.HitterNormalToAdvanced(
                                stats,
                                leagueDict[a.LeagueId],
                                bsrDict.GetValueOrDefault((mlbId, a.TeamId))?.RBSR ?? 0,
                                defStats.Sum(f => f.ScaledDRAA + f.PosAdjust),
                                a.TeamId);

                            db.Player_Hitter_MonthAdvanced.Add(ma);
                        }
                    }
                }
            }

            db.SaveChanges();
            db.ChangeTracker.Clear();
        }

        private static void CalculatePitcherMonthStats(SqliteDbContext db, int year, int month)
        {
            // Remove existing data
            db.Player_Pitcher_MonthStats.Where(f => f.Year == year && f.Month == month).ExecuteDelete();

            // Store all park factor values to reduce db queries
            Dictionary<int, Park_Factors> ParkFactorDict = new();
            var parkFactors = db.Park_Factors.Where(f => f.Year == year);
            foreach (var pf in parkFactors)
                ParkFactorDict[pf.StadiumId] = pf;

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

                    var playerGames = monthGames.Where(f => f.MlbId == mlbId).ToArray();
                    var leagues = playerGames.Select(f => f.LeagueId).Distinct();
                    foreach (int leagueId in leagues)
                    {
                        var gameLogs = playerGames.Where(f => f.LeagueId == leagueId);

                        var stats = GetMonthStatsPitcher(gameLogs, ParkFactorDict, month);
                        if (stats.BattersFaced == 0)
                            continue;

                        db.Player_Pitcher_MonthStats.Add(stats);
                    }
                }
            }

            db.SaveChanges();
            db.ChangeTracker.Clear();
        }

        private static void CalculatePitcherMonthStatsAdvanced(SqliteDbContext db, int year, int month)
        {
            // Remove existing data
            db.Player_Pitcher_MonthAdvanced.Where(f => f.Year == year && f.Month == month).ExecuteDelete();

            // Store all park factor values to reduce db queries
            Dictionary<int, Park_Factors> ParkFactorDict = new();
            var parkFactors = db.Park_Factors.Where(f => f.Year == year);
            foreach (var pf in parkFactors)
                ParkFactorDict[pf.StadiumId] = pf;

            Dictionary<int, Player_MonthlyWar> pmwDict =
                db.Player_MonthlyWar
                    .Where(f => f.Year == year && f.Month == month)
                    .ToDictionary(f => f.MlbId);

            Dictionary<int, LeagueStats> lsDict =
                db.LeagueStats
                    .Where(f => f.Year == year)
                    .ToDictionary(f => f.LeagueId);

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

                    var playerGames = monthGames.Where(f => f.MlbId == mlbId).ToArray();
                    var leagues = playerGames.Select(f => f.LeagueId).Distinct();
                    foreach (int leagueId in leagues)
                    {
                        var gameLogs = playerGames.Where(f => f.LeagueId == leagueId);

                        // Advanced Stats
                        var teamLeagues = gameLogs.Select(f => new { f.TeamId, f.LeagueId }).Distinct();
                        foreach (var a in teamLeagues)
                        {
                            var games = gameLogs.Where(f => f.TeamId == a.TeamId && f.LeagueId == a.LeagueId);
                            var stats = GetMonthStatsPitcher(games, ParkFactorDict, month);

                            if (stats.BattersFaced == 0)
                                continue;

                            Player_Pitcher_MonthAdvanced ma = Utilities.PitcherNormalToAdvanced(
                                    stats,
                                    lsDict[a.LeagueId],
                                    pmwDict.GetValueOrDefault(mlbId),
                                    a.TeamId);

                            db.Player_Pitcher_MonthAdvanced.Add(ma);
                        }
                    }
                }
            }

            db.SaveChanges();
            db.ChangeTracker.Clear();
        }

        public static void Update(int year, int month)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            try {
                CalculateHitterMonthStats(db, year, month);
                CalculatePitcherMonthStats(db, year, month);
            } catch (Exception e)
            {
                Console.WriteLine("Error in CalculateMonthStats");
                Utilities.LogException(e);
                throw;
            }
        }

        public static void UpdateAdvanced(int year, int month)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            try
            {
                CalculateHitterMonthStatsAdvanced(db, year, month);
                CalculatePitcherMonthStatsAdvanced(db, year, month);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in CalculateMonthStats Advanced");
                Utilities.LogException(e);
                throw;
            }
        }
    }
}
