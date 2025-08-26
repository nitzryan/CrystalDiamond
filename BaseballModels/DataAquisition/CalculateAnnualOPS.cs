using Db;
using ShellProgressBar;

namespace DataAquisition
{
    internal class CalculateAnnualOPS
    {
        public static bool Main(int year)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                var leagues = db.Player_Hitter_MonthAdvanced.Where(f => f.Year == year)
                    .Select(f => f.LeagueId).Distinct();

                using (ProgressBar progressBar = new(leagues.Count(), $"Generating Hitter OPS+ for {year}"))
                {
                    foreach (int league in leagues)
                    {
                        Player_Hitter_GameLog stats = db.Player_Hitter_GameLog.Where(f => f.Year == year && f.LeagueId == league && f.Position != 1)
                            .Aggregate(Utilities.HitterGameLogAggregation);

                        Player_Hitter_MonthAdvanced advanced = Utilities.HitterNormalToAdvanced(stats);

                        var monthAdvanced = db.Player_Hitter_MonthAdvanced.Where(f => f.Year == year && f.LeagueId == league);
                        foreach (var ma in monthAdvanced)
                        {
                            ma.WRC = 100 * ((ma.OBP / advanced.OBP) + (ma.SLG + advanced.SLG) - 1); // OPS+ for now, simpler
                        }

                        var yearAdvanced = db.Player_Hitter_YearAdvanced.Where(f => f.Year == year && f.LeagueId == league);
                        foreach (var ya in yearAdvanced)
                        {
                            ya.WRC = 100 * ((ya.OBP / advanced.OBP) + (ya.SLG + advanced.SLG) - 1);
                        }
                        db.SaveChanges();

                        progressBar.Tick();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in CalculateAnnualWRC");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
