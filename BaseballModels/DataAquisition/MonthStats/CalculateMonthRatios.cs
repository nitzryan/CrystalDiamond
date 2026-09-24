using DataAquisition.ModelStats;
using Db;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace DataAquisition.MonthStats
{
    public record RatioLeagueCache(
        Dictionary<LeagueMonthKey, League_HitterStats> HitterMonthStats,
        Dictionary<LeagueMonthKey, League_PitcherStats> PitcherMonthStats,
        Dictionary<(int LeagueId, int Year), LeagueStats> LeagueStats)
    {
        public static RatioLeagueCache Generate()
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            return new RatioLeagueCache(
                db.League_HitterStats.AsNoTracking().ToDictionary(f => new LeagueMonthKey(f.Year, f.Month, f.LeagueId)),
                db.League_PitcherStats.AsNoTracking().ToDictionary(f => new LeagueMonthKey(f.Year, f.Month, f.LeagueId)),
                db.LeagueStats.AsNoTracking().ToDictionary(f => (f.LeagueId, f.Year)));
        }
    }

    public static class HitterMonthRatios
    {
        private static Player_Hitter_MonthlyRatios? BuildRatioRow(
            Player_Hitter_MonthStats stat, 
            League_HitterStats league, 
            LeagueStats leagueStats)
        {
            Player_Hitter_MonthAdvanced advStat = 
                Utilities.HitterNormalToAdvanced(
                    stat, 
                    leagueStats, 
                    0, // This, and below, are 0 since they are only necessary
                    0, // To calculate crWAR, which this doesn't use
                    -1); // TeamId not needed

            int totalGames = stat.GamesC + stat.Games1B + stat.Games2B + stat.GamesSS + stat.Games3B
                + stat.GamesLF + stat.GamesCF + stat.GamesRF + stat.GamesDH;

            if (totalGames == 0)
            {
                return null;
            }

            var row = new Player_Hitter_MonthlyRatios
            {
                MlbId = stat.MlbId,
                Year = stat.Year,
                Month = stat.Month,
                LevelId = stat.LevelId,
                LeagueId = stat.LeagueId,
                AVGRatio = Utilities.SafeDivide(advStat.AVG, league.AVG),
                OBPRatio = Utilities.SafeDivide(advStat.OBP, league.OBP),
                ISORatio = Utilities.SafeDivide(advStat.ISO, league.ISO),
                WRC = advStat.WRC, // Already a normalized stat
                SBRateRatio = Utilities.SafeDivide(advStat.SBRate, league.SBRate),
                SBPercRatio = Utilities.SafeDivide(advStat.SBPerc, league.SBPerc),
                HRPercRatio = Utilities.SafeDivide(advStat.HRPerc, league.HRPerc),
                BBPercRatio = Utilities.SafeDivide(advStat.BBPerc, league.BBPerc),
                KPercRatio = Utilities.SafeDivide(advStat.KPerc, league.KPerc),
                PercC = Utilities.SafeDivide(stat.GamesC, totalGames),
                Perc1B = Utilities.SafeDivide(stat.Games1B, totalGames),
                Perc2B = Utilities.SafeDivide(stat.Games2B, totalGames),
                Perc3B = Utilities.SafeDivide(stat.Games3B, totalGames),
                PercSS = Utilities.SafeDivide(stat.GamesSS, totalGames),
                PercLF = Utilities.SafeDivide(stat.GamesLF, totalGames),
                PercCF = Utilities.SafeDivide(stat.GamesCF, totalGames),
                PercRF = Utilities.SafeDivide(stat.GamesRF, totalGames),
                PercDH = Utilities.SafeDivide(stat.GamesDH, totalGames),
            };
            return row;
        }

        internal static void UpdateHitterRatios(SqliteDbContext db, int year, int month)
        {
            db.Player_Hitter_MonthlyRatios.Where(f => f.Year == year && f.Month == month).ExecuteDelete();

            List<Player_Hitter_MonthStats> stats = db.Player_Hitter_MonthStats
                .Where(f => f.Year == year && f.Month == month)
                .ToList();
            
            // Caches    
            Dictionary<int, League_HitterStats> leagueStats = db.League_HitterStats
                .Where(f => f.Year == year && f.Month == month)
                .ToDictionary(f => f.LeagueId);
            Dictionary<int, LeagueStats> leagueStatsDict =
                db.LeagueStats
                    .Where(f => f.Year == year)
                    .ToDictionary(f => f.LeagueId);

            foreach (Player_Hitter_MonthStats stat in stats)
            {
                var row = BuildRatioRow(
                                stat, 
                                leagueStats[stat.LeagueId], 
                                leagueStatsDict[stat.LeagueId]
                                );
                if (row is not null)
                {
                    db.Player_Hitter_MonthlyRatios.Add(row);
                }
            }
            db.SaveChanges();
        }

        public static List<Player_Hitter_MonthlyRatios> ConvertMonthStats(
            List<Player_Hitter_MonthStats> stats, RatioLeagueCache league)
        {
            var output = new List<Player_Hitter_MonthlyRatios>(stats.Count);

            foreach (Player_Hitter_MonthStats stat in stats)
            {
                LeagueStats ls = league.LeagueStats[(stat.LeagueId, stat.Year)];
                var row = BuildRatioRow(stat, 
                                        league.HitterMonthStats[new LeagueMonthKey(stat.Year, stat.Month, stat.LeagueId)], 
                                        ls);
                if (row is not null)
                {
                    output.Add(row);
                }
            }

            return output;
        }
    }

    public static class PitcherMonthRatios
    {
        private static Player_Pitcher_MonthlyRatios BuildRatioRow(
            Player_Pitcher_MonthStats stat, League_PitcherStats league, LeagueStats leagueStats)
        {
            Player_Pitcher_MonthAdvanced advStat = Utilities.PitcherNormalToAdvanced(
                stat, 
                leagueStats, 
                null, // Only used for WAR calcultion, which is not used here.
                -1); // TeamId not needed

            return new Player_Pitcher_MonthlyRatios
            {
                MlbId = stat.MlbId,
                Year = stat.Year,
                Month = stat.Month,
                LevelId = stat.LevelId,
                LeagueId = stat.LeagueId,
                SPPerc = stat.SPPerc,
                WOBARatio = Utilities.SafeDivide(advStat.WOBA, league.WOBA),
                HRPercRatio = Utilities.SafeDivide(advStat.HRPerc, league.HRPerc),
                BBPercRatio = Utilities.SafeDivide(advStat.BBPerc, league.BBPerc),
                KPercRatio = Utilities.SafeDivide(advStat.KPerc, league.KPerc),
                FIPRatio = Utilities.SafeDivide(advStat.FIP, league.FipConstant + league.ERA),
                ERARatio = Utilities.SafeDivide(advStat.ERA, league.ERA),
                GBPercRatio = Utilities.SafeDivide(advStat.GBRatio, league.GOPerc),
            };
        }

        internal static void UpdatePitcherRatios(SqliteDbContext db, int year, int month)
        {
            db.Player_Pitcher_MonthlyRatios.Where(f => f.Year == year && f.Month == month).ExecuteDelete();

            List<Player_Pitcher_MonthStats> stats = db.Player_Pitcher_MonthStats
                .Where(f => f.Year == year && f.Month == month)
                .ToList();
            Dictionary<int, League_PitcherStats> leaguePitcherStats = db.League_PitcherStats
                .Where(f => f.Year == year && f.Month == month)
                .ToDictionary(f => f.LeagueId);
            Dictionary<int, LeagueStats> leagueStats = db.LeagueStats
                .Where(f => f.Year == year)
                .ToDictionary(f => f.LeagueId);

            foreach (Player_Pitcher_MonthStats stat in stats)
            {
                db.Player_Pitcher_MonthlyRatios.Add(BuildRatioRow(stat, leaguePitcherStats[stat.LeagueId], leagueStats[stat.LeagueId]));
            }

            db.SaveChanges();
        }

        public static List<Player_Pitcher_MonthlyRatios> ConvertMonthStats(
            List<Player_Pitcher_MonthStats> stats, RatioLeagueCache league)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            var output = new List<Player_Pitcher_MonthlyRatios>(stats.Count);

            foreach (Player_Pitcher_MonthStats stat in stats)
            {
                output.Add(BuildRatioRow(
                    stat,
                    league.PitcherMonthStats[new LeagueMonthKey(stat.Year, stat.Month, stat.LeagueId)],
                    league.LeagueStats[(stat.LeagueId, stat.Year)]
                    ));
            }

            return output;
        }
    }

    internal class CalculateMonthRatios
    {
        public static void Update(int year, int month)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            try
            {
                HitterMonthRatios.UpdateHitterRatios(db, year, month);
                PitcherMonthRatios.UpdatePitcherRatios(db, year, month);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in CalculateMonthRatios");
                Utilities.LogException(e);
                throw;
            }
        }
    }
}
