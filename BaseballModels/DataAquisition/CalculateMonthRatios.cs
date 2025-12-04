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
            var levelStats = db.Level_HitterStats.Where(f => f.Year == year && f.Month == month);
            foreach (var stat in stats)
            {
                Level_HitterStats thisLevelStats = levelStats.Where(f => f.LevelId == stat.LevelId).First();
                var advStat = Utilities.HitterNormalToAdvanced(stat, db.LeagueStats.First()); // Don't need parts of advStats that are affected by LeagueStats
                int totalGames = stat.GamesC + stat.Games1B + stat.Games2B + stat.GamesSS + stat.Games3B + stat.GamesLF + stat.GamesCF + stat.GamesRF + stat.GamesDH;

                db.Player_Hitter_MonthlyRatios.Add(new Player_Hitter_MonthlyRatios
                {
                    MlbId = stat.MlbId,
                    Year = stat.Year,
                    Month = stat.Month,
                    LevelId = stat.LevelId,
                    AVGRatio = advStat.AVG / thisLevelStats.AVG,
                    OBPRatio = advStat.OBP / thisLevelStats.OBP,
                    ISORatio = advStat.ISO / thisLevelStats.ISO,
                    WOBARatio = advStat.WOBA / thisLevelStats.WOBA,
                    SBRateRatio = advStat.SBRate / thisLevelStats.SBRate,
                    SBPercRatio = advStat.SBPerc / thisLevelStats.SBPerc,
                    HRPercRatio = advStat.HRPerc / thisLevelStats.HRPerc,
                    BBPercRatio = advStat.BBPerc / thisLevelStats.BBPerc,
                    KPercRatio = advStat.KPerc / thisLevelStats.KPerc,
                    PercC = (float)stat.GamesC / totalGames,
                    Perc1B = (float)stat.Games1B / totalGames,
                    Perc2B = (float)stat.Games2B / totalGames,
                    Perc3B = (float)stat.Games3B / totalGames,
                    PercSS = (float)stat.GamesSS / totalGames,
                    PercLF = (float)stat.GamesLF / totalGames,
                    PercCF = (float)stat.GamesCF / totalGames,
                    PercRF = (float)stat.GamesRF / totalGames,
                    PercDH = (float)stat.GamesDH / totalGames,
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
            var levelStats = db.Level_PitcherStats.Where(f => f.Year == year && f.Month == month);
            foreach (var stat in stats)
            {
                Level_PitcherStats thisLevelStats = levelStats.Where(f => f.LevelId == stat.LevelId).First();
                var advStat = Utilities.PitcherNormalToAdvanced(stat, db.LeagueStats.First(), db); 

                db.Player_Pitcher_MonthlyRatios.Add(new Player_Pitcher_MonthlyRatios
                {
                    MlbId = stat.MlbId,
                    Year = stat.Year,
                    Month = stat.Month,
                    LevelId = stat.LevelId,
                    SPPerc = stat.SPPerc,
                    WOBARatio = advStat.WOBA / thisLevelStats.WOBA,
                    HRPercRatio = advStat.HRPerc / thisLevelStats.HRPerc,
                    BBPercRatio = advStat.BBPerc / thisLevelStats.BBPerc,
                    KPercRatio = advStat.KPerc / thisLevelStats.KPerc,
                    FIPRatio = advStat.FIP / (thisLevelStats.FipConstant + thisLevelStats.ERA),
                    ERARatio = advStat.ERA / thisLevelStats.ERA,
                    GBPercRatio = advStat.GBRatio / thisLevelStats.GOPerc
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
