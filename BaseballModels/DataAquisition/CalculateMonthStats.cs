using Db;
using Microsoft.EntityFrameworkCore.Storage;
using ShellProgressBar;
using System.Net;
using System.Runtime.CompilerServices;

namespace DataAquisition
{
    internal class CalculateMonthStats
    {
        private static Player_Hitter_MonthStats GetMonthStatsHitter(IEnumerable<Player_Hitter_GameLog> gameLogs, Dictionary<int, (float, float)> adjustedParkFactors, int month)
        {
            int totalAb = 0;
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

                if (gl.Position > 1 && gl.Position <= 9)
                    totalPositions[gl.Position - 2]++;
                else
                    totalPositions[8]++;

                if (adjustedParkFactors.ContainsKey(gl.HomeTeamId))
                {
                    var pf = adjustedParkFactors[gl.HomeTeamId];
                    totalRunFactor += pf.Item1 * gl.AB;
                    totalHRFactor += pf.Item2 * gl.AB;
                }
                else // No enough data on park to create park factor
                {
                    totalRunFactor += gl.AB;
                    totalHRFactor += gl.AB;
                }
            }

            var first = gameLogs.First();
            return new Player_Hitter_MonthStats
            {
                MlbId = first.MlbId,
                Year = first.Year,
                Month = month,
                LevelId = first.LevelId,
                AB = totalAb,
                H = totalH,
                Hit2B = total2B,
                Hit3B = total3B,
                HR = totalHR,
                K = totalK,
                BB = totalBB,
                SB = totalSB,
                CS = totalCS,
                HBP = totalHBP,
                ParkRunFactor = (totalAb > 0) ? totalRunFactor / totalAb : 1.0f,
                ParkHRFactor = (totalAb > 0) ? totalHRFactor / totalAb : 1.0f,
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

        private static Player_Pitcher_MonthStats GetMonthStatsPitcher(IEnumerable<Player_Pitcher_GameLog> gameLogs, Dictionary<int, (float, float)> adjustedParkFactors, int month)
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

                if (adjustedParkFactors.ContainsKey(gl.HomeTeamId))
                {
                    var pf = adjustedParkFactors[gl.HomeTeamId];
                    totalRunFactor += pf.Item1 * gl.BattersFaced;
                    totalHRFactor += pf.Item2 * gl.BattersFaced;
                }
                else // No enough data on park to create park factor
                {
                    totalRunFactor += gl.BattersFaced;
                    totalHRFactor += gl.BattersFaced;
                }
            }

            var first = gameLogs.First();
            return new Player_Pitcher_MonthStats
            {
                MlbId = first.MlbId,
                Year = first.Year,
                Month = month,
                LevelId = first.LevelId,
                H = totalH,
                Hit2B = total2B,
                Hit3B = total3B,
                HR = totalHR,
                K = totalK,
                BB = totalBB,
                HBP = totalHBP,
                BattersFaced = totalBF,
                Outs = totalOuts,
                GO = totalGO,
                AO = totalAO,
                R = totalR,
                ER = totalER,
                ParkRunFactor = (totalBF > 0) ? totalRunFactor / totalBF : 1.0f,
                ParkHRFactor = (totalBF > 0) ? totalHRFactor / totalBF : 1.0f,
            };
        }

        private static Dictionary<int, (float, float)> GetParkFactors(SqliteDbContext db, int year)
        {
            // Get league factor, then adjust park factors by league factors
            var leagueFactors = db.League_Factors.Where(f => f.Year == year);
            var parkFactors = db.Park_Factors.Where(f => f.Year == year);
            Dictionary<int, (float, float)> adjustedParkFactors = [];
            foreach (var pf in parkFactors)
            {
                var lf = leagueFactors.Where(f => f.LeagueId == pf.LeagueId).First();
                float runFactor = pf.RunFactor * lf.RunFactor;
                float hrFactor = pf.HRFactor * lf.HRFactor;
                adjustedParkFactors.Add(pf.TeamId, (runFactor, hrFactor));
            }

            return adjustedParkFactors;
        }

        private static bool CalculateHitterMonthStats(SqliteDbContext db, Dictionary<int, (float, float)> adjustedParkFactors, int year, int month)
        {
            // Remove existing data
            db.Player_Hitter_MonthStats.RemoveRange(db.Player_Hitter_MonthStats.Where(f => f.Year == year && f.Month == month));
            db.Player_Hitter_MonthAdvanced.RemoveRange(db.Player_Hitter_MonthAdvanced.Where(f => f.Year == year && f.Month == month));
            db.SaveChanges();

            // Iterate through player/level combinations
            var idLevels = db.Player_Hitter_GameLog.Where(f => f.Year == year).Select(f => new { f.MlbId, f.LevelId }).Distinct();
            using (ProgressBar progressBar = new ProgressBar(idLevels.Count(), $"Calculating Hitter Month Stats for Month={month} Year={year}"))
            {
                foreach (var idlvl in idLevels)
                {
                    progressBar.Tick(); // Tick before so skips don't mess up count

                    IEnumerable<Player_Hitter_GameLog> gameLogs;
                    if (month == 4) // Group March with April
                        gameLogs = db.Player_Hitter_GameLog.Where(f => f.MlbId == idlvl.MlbId && f.LevelId == idlvl.LevelId && f.Year == year && f.Month <= 4);
                    else if (month == 8 && idlvl.LevelId >= 16) // Group rookie ball September with August
                        gameLogs = db.Player_Hitter_GameLog.Where(f => f.MlbId == idlvl.MlbId && f.LevelId == idlvl.LevelId && f.Year == year && f.Month >= 8);
                    else if (month > 8 && idlvl.LevelId >= 16) // Was done in previous, so skip
                        continue;
                    else if (month == 9) // Group October into September
                        gameLogs = db.Player_Hitter_GameLog.Where(f => f.MlbId == idlvl.MlbId && f.LevelId == idlvl.LevelId && f.Year == year && f.Month >= 9);
                    else // Single month
                        gameLogs = db.Player_Hitter_GameLog.Where(f => f.MlbId == idlvl.MlbId && f.LevelId == idlvl.LevelId && f.Year == year && f.Month == month);

                    if (!gameLogs.Any())
                        continue;

                    var stats = GetMonthStatsHitter(gameLogs, adjustedParkFactors, month);
                    if (stats.AB + stats.BB + stats.HBP + stats.SB + stats.CS == 0)
                        continue;

                    db.Player_Hitter_MonthStats.Add(stats);

                    // Advanced Stats
                    var teamLeagues = gameLogs.Select(f => new { f.TeamId, f.LeagueId }).Distinct();
                    foreach (var a in teamLeagues)
                    {
                        stats = GetMonthStatsHitter(gameLogs.Where(f => f.TeamId == a.TeamId && f.LeagueId == a.LeagueId), adjustedParkFactors, month);
                        int singles = stats.H - stats.Hit2B - stats.Hit3B - stats.HR;
                        int pa = stats.AB + stats.BB + stats.HBP;
                        float avg = stats.AB > 0 ? (float)stats.H / stats.AB : 0.0f;
                        float iso = stats.AB > 0 ? (float)(stats.Hit2B + (2 * stats.Hit3B) + (3 * stats.HR)) / stats.AB : 0;
                        float woba = pa > 0 ?
                            ((0.69f * stats.BB) + (0.72f * stats.HBP) + (0.89f * singles) + (1.27f * stats.Hit2B) + (1.62f * stats.Hit3B) + (2.10f * stats.HR)) / pa
                            : 0;
                        Player_Hitter_MonthAdvanced ma = new()
                        {
                            MlbId = idlvl.MlbId,
                            LevelId = idlvl.LevelId,
                            Year = year,
                            Month = month,
                            TeamId = a.TeamId,
                            LeagueId = a.LeagueId,
                            PA = pa,
                            AVG = avg,
                            OBP = pa > 0 ? (float)(stats.H + stats.BB + stats.HBP) / pa : 0.3f,
                            SLG = avg + iso,
                            ISO = iso,
                            WOBA = woba,
                            WRC = -1.0f, // Fill in later, need league wOBA
                            HRPerc = pa > 0 ? (float)stats.HR / pa : 0,
                            BBPerc = pa > 0 ? (float)stats.BB / pa : 0,
                            KPerc = pa > 0 ? (float)stats.K / pa : 0,
                            SBRate = pa > 0 ?  (float)stats.SB / pa : 0,
                            SBPerc = (stats.SB + stats.CS) > 0 ? (float)stats.SB / (stats.SB + stats.CS) : 0
                        };
                        db.Player_Hitter_MonthAdvanced.Add(ma);
                    }
                }
            }
            
            db.SaveChanges();
            db.ChangeTracker.Clear();

            return true;
        }

        private static bool CalculatePitcherMonthStats(SqliteDbContext db, Dictionary<int, (float, float)> adjustedParkFactors, int year, int month)
        {
            // Remove existing data
            db.Player_Pitcher_MonthStats.RemoveRange(db.Player_Pitcher_MonthStats.Where(f => f.Year == year && f.Month == month));
            db.Player_Pitcher_MonthAdvanced.RemoveRange(db.Player_Pitcher_MonthAdvanced.Where(f => f.Year == year && f.Month == month));
            db.SaveChanges();

            // Iterate through player/level combinations
            var idLevels = db.Player_Pitcher_GameLog.Where(f => f.Year == year).Select(f => new { f.MlbId, f.LevelId }).Distinct();
            using (ProgressBar progressBar = new ProgressBar(idLevels.Count(), $"Calculating Pitcher Month Stats for Month={month} Year={year}"))
            {
                foreach (var idlvl in idLevels)
                {
                    progressBar.Tick(); // Tick before so skips don't mess up count

                    IEnumerable<Player_Pitcher_GameLog> gameLogs;
                    if (month == 4) // Group March with April
                        gameLogs = db.Player_Pitcher_GameLog.Where(f => f.MlbId == idlvl.MlbId && f.LevelId == idlvl.LevelId && f.Year == year && f.Month <= 4);
                    else if (month == 8 && idlvl.LevelId >= 16) // Group rookie ball September with August
                        gameLogs = db.Player_Pitcher_GameLog.Where(f => f.MlbId == idlvl.MlbId && f.LevelId == idlvl.LevelId && f.Year == year && f.Month >= 8);
                    else if (month > 8 && idlvl.LevelId >= 16) // Was done in previous, so skip
                        continue;
                    else if (month == 9) // Group October into September
                        gameLogs = db.Player_Pitcher_GameLog.Where(f => f.MlbId == idlvl.MlbId && f.LevelId == idlvl.LevelId && f.Year == year && f.Month >= 9);
                    else // Single month
                        gameLogs = db.Player_Pitcher_GameLog.Where(f => f.MlbId == idlvl.MlbId && f.LevelId == idlvl.LevelId && f.Year == year && f.Month == month);

                    if (!gameLogs.Any())
                        continue;

                    var stats = GetMonthStatsPitcher(gameLogs, adjustedParkFactors, month);
                    if (stats.BattersFaced == 0)
                        continue;

                    db.Player_Pitcher_MonthStats.Add(stats);

                    // Advanced Stats
                    var teamLeagues = gameLogs.Select(f => new { f.TeamId, f.LeagueId }).Distinct();
                    foreach (var a in teamLeagues)
                    {
                        stats = GetMonthStatsPitcher(gameLogs.Where(f => f.TeamId == a.TeamId && f.LeagueId == a.LeagueId), adjustedParkFactors, month);
                        int singles = stats.H - stats.Hit2B - stats.Hit3B - stats.HR;
                        float woba = stats.BattersFaced > 0 ?
                            ((0.69f * stats.BB) + (0.72f * stats.HBP) + (0.89f * singles) + (1.27f * stats.Hit2B) + (1.62f * stats.Hit3B) + (2.10f * stats.HR)) / stats.BattersFaced
                            : 0;

                        float fipConstant = db.Level_PitcherStats.Where(f => f.Year == year && f.Month == month && f.Level == idlvl.LevelId).Select(f => f.FipConstant).Single();
                        Player_Pitcher_MonthAdvanced ma = new()
                        {
                            MlbId = idlvl.MlbId,
                            LevelId = idlvl.LevelId,
                            Year = year,
                            Month = month,
                            TeamId = a.TeamId,
                            LeagueId = a.LeagueId,
                            BF = stats.BattersFaced,
                            Outs = stats.Outs,
                            WOBA = woba,
                            HRPerc = (float)stats.HR / stats.BattersFaced,
                            BBPerc = (float)stats.BB / stats.BattersFaced,
                            KPerc = (float)stats.K / stats.BattersFaced,
                            ERA = stats.Outs > 0 ? (float)stats.ER * 27 / stats.Outs : stats.ER * 27.0f,
                            FIP = stats.Outs > 0 ? (float)((13 * stats.HR) + 3 * (stats.BB + stats.HBP) - (2 * stats.K)) * 3 / stats.Outs + fipConstant : 99.0f,
                            GBRatio = stats.AO > 0 ? (float)stats.GO / (stats.GO + stats.AO) : 1.0f
                        };
                        db.Player_Pitcher_MonthAdvanced.Add(ma);
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
            Dictionary<int, (float, float)> adjustedParkFactors = GetParkFactors(db, year);

            try {
                if (!CalculateHitterMonthStats(db, adjustedParkFactors, year, month))
                {
                    Console.WriteLine("Error in CalculateHitterMonthStats");
                    return false;
                }
            } catch (Exception e)
            {
                Console.WriteLine("Error in CalculateHitterMonthStats");
                Console.WriteLine(e.Message);
                Console.Write(e.StackTrace);
                return false;
            }

            try {
                if (!CalculatePitcherMonthStats(db, adjustedParkFactors, year, month))
                {
                    Console.WriteLine("Error in CalculatePitcherMonthStats");
                    return false;
                }
            } catch (Exception e)
            {
                Console.WriteLine("Error in CalculatePitcherMonthStats");
                Console.WriteLine(e.Message);
                Console.Write(e.StackTrace);
                return false;
            }

            return true;
        }
    }
}
