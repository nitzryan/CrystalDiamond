using Db;
using ShellProgressBar;

namespace DataAquisition
{
    internal class CalculateMonthRatios
    {
        private static bool CalculateHitterMonthRatios(SqliteDbContext db, int year, int month)
        {
            db.Player_Hitter_MonthlyRatios.RemoveRange(
                db.Player_Hitter_MonthlyRatios.Where(f => f.Year == year && f.Month == month)
            );
            db.SaveChanges();

            var stats = db.Player_Hitter_MonthStats.Where(f => f.Year == year && f.Month == month);//.OrderByDescending(f => f.MlbId).OrderByDescending(f => f.LevelId);
            var leagueStats = db.League_HitterStats.Where(f => f.Year == year && f.Month == month);
            foreach (var stat in stats)
            {
                League_HitterStats thisLevelStats = leagueStats.Where(f => f.LeagueId == stat.LeagueId).First();
                var advStat = Utilities.HitterNormalToAdvanced(stat, db.LeagueStats.First()); // Don't need parts of advStats that are affected by LeagueStats
                int totalGames = stat.GamesC + stat.Games1B + stat.Games2B + stat.GamesSS + stat.Games3B + stat.GamesLF + stat.GamesCF + stat.GamesRF + stat.GamesDH;

                if (totalGames != 0)
                    db.Player_Hitter_MonthlyRatios.Add(new Player_Hitter_MonthlyRatios
                    {
                        MlbId = stat.MlbId,
                        Year = stat.Year,
                        Month = stat.Month,
                        LevelId = stat.LevelId,
                        LeagueId = stat.LeagueId,
                        AVGRatio = Utilities.SafeDivide(advStat.AVG, thisLevelStats.AVG),
                        OBPRatio = Utilities.SafeDivide(advStat.OBP, thisLevelStats.OBP),
                        ISORatio = Utilities.SafeDivide(advStat.ISO, thisLevelStats.ISO),
                        WRC = -1, // wRC+ is not calculated yet
                        SBRateRatio = Utilities.SafeDivide(advStat.SBRate, thisLevelStats.SBRate),
                        SBPercRatio = Utilities.SafeDivide(advStat.SBPerc, thisLevelStats.SBPerc),
                        HRPercRatio = Utilities.SafeDivide(advStat.HRPerc, thisLevelStats.HRPerc),
                        BBPercRatio = Utilities.SafeDivide(advStat.BBPerc, thisLevelStats.BBPerc),
                        KPercRatio = Utilities.SafeDivide(advStat.KPerc, thisLevelStats.KPerc),
                        PercC = Utilities.SafeDivide((float)stat.GamesC, totalGames),
                        Perc1B = Utilities.SafeDivide((float)stat.Games1B, totalGames),
                        Perc2B = Utilities.SafeDivide((float)stat.Games2B, totalGames),
                        Perc3B = Utilities.SafeDivide((float)stat.Games3B, totalGames),
                        PercSS = Utilities.SafeDivide((float)stat.GamesSS, totalGames),
                        PercLF = Utilities.SafeDivide((float)stat.GamesLF, totalGames),
                        PercCF = Utilities.SafeDivide((float)stat.GamesCF, totalGames),
                        PercRF = Utilities.SafeDivide((float)stat.GamesRF, totalGames),
                        PercDH = Utilities.SafeDivide((float)stat.GamesDH, totalGames),
                    });
            }

            db.SaveChanges();
            return true;
        }

        private static bool CalculatePitcherMonthRatios(SqliteDbContext db, int year, int month)
        {
            db.Player_Pitcher_MonthlyRatios.RemoveRange(
                db.Player_Pitcher_MonthlyRatios.Where(f => f.Year == year && f.Month == month)
            );
            db.SaveChanges();

            var stats = db.Player_Pitcher_MonthStats.Where(f => f.Year == year && f.Month == month);
            var leagueStats = db.League_PitcherStats.Where(f => f.Year == year && f.Month == month);
            foreach (var stat in stats)
            {
                League_PitcherStats thisLeagueStats = leagueStats.Where(f => f.LeagueId == stat.LeagueId).First();
                var advStat = Utilities.PitcherNormalToAdvanced(stat, db.LeagueStats.First(), db); 

                db.Player_Pitcher_MonthlyRatios.Add(new Player_Pitcher_MonthlyRatios
                {
                    MlbId = stat.MlbId,
                    Year = stat.Year,
                    Month = stat.Month,
                    LevelId = stat.LevelId,
                    LeagueId = stat.LeagueId,
                    SPPerc = stat.SPPerc,
                    WOBARatio = Utilities.SafeDivide(advStat.WOBA, thisLeagueStats.WOBA),
                    HRPercRatio = Utilities.SafeDivide(advStat.HRPerc, thisLeagueStats.HRPerc),
                    BBPercRatio = Utilities.SafeDivide(advStat.BBPerc, thisLeagueStats.BBPerc),
                    KPercRatio = Utilities.SafeDivide(advStat.KPerc, thisLeagueStats.KPerc),
                    FIPRatio = Utilities.SafeDivide(advStat.FIP, (thisLeagueStats.FipConstant + thisLeagueStats.ERA)),
                    ERARatio = Utilities.SafeDivide(advStat.ERA, thisLeagueStats.ERA),
                    GBPercRatio = Utilities.SafeDivide(advStat.GBRatio, thisLeagueStats.GOPerc)
                });
            }

            db.SaveChanges();
            return true;
        }

        public static bool Main(int year, int month)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            try
            {
                if (!CalculateHitterMonthRatios(db, year, month))
                {
                    Console.WriteLine("Error in CalculateHitterMonthRatios");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in CalculateHitterMonthRatios");
                Utilities.LogException(e);
                return false;
            }

            try
            {
                if (!CalculatePitcherMonthRatios(db, year, month))
                {
                    Console.WriteLine("Error in CalculatePitcherMonthRatios");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in CalculatePitcherMonthRatios");
                Utilities.LogException(e);
                return false;
            }

            return true;

        }
    }
}
