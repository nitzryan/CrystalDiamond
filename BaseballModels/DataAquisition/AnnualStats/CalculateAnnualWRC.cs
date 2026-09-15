using Db;
using ShellProgressBar;

namespace DataAquisition.AnnualStats
{
    internal class CalculateAnnualWRC
    {
        public static void Update(int year)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                var leagues = db.Player_Hitter_MonthAdvanced.Where(f => f.Year == year)
                    .Select(f => f.LeagueId).Distinct();

                using (ProgressBar progressBar = new(leagues.Count(), $"Generating Hitter WRC+ for {year}"))
                {
                    foreach (int league in leagues)
                    {
                        LeagueStats ls = db.LeagueStats.Where(f => f.LeagueId == league && f.Year == year).Single();

                        float leaguewRCperPA = (ls.AvgHitterWOBA - ls.AvgWOBA) / ls.WOBAScale + ls.RPerPA;

                        // Iterate through player month stats
                        var monthsAdvanced = db.Player_Hitter_MonthAdvanced.Where(f => f.Year == year && f.LeagueId == league);
                        foreach (var ma in monthsAdvanced)
                        {
                            ma.WRC = Utilities.CalculateWrcPlus(ma.WOBA, ma.ParkFactor, ls);
                        }

                        var yearAdvanced = db.Player_Hitter_YearAdvanced.Where(f => f.Year == year && f.LeagueId == league);
                        foreach (var ya in yearAdvanced)
                        {
                            ya.WRC = Utilities.CalculateWrcPlus(ya.WOBA, ya.ParkFactor, ls);
                        }

                        progressBar.Tick();
                    }
                }

                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in CalculateAnnualWRC");
                Utilities.LogException(e);
                throw;
            }
        }

        public static void UpdateMonthRatiosWRC(int year, int month)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                var phma = db.Player_Hitter_MonthAdvanced.Where(f => f.Year == year && f.Month == month)
                    .GroupBy(f => new { f.MlbId, f.LeagueId });
                using (ProgressBar progressBar = new(phma.Count(), $"Generating Hitter Monthly Ratios WRC+ for {year}-{month}"))
                {
                    foreach (var grouping in phma)
                    {
                        int totalPa = 0;
                        float wRCPlus = 100.0f;
                        foreach (var ma in grouping)
                        {
                            if (ma.PA == 0)
                                continue;

                            float statFrac = ma.PA / (ma.PA + totalPa);
                            wRCPlus = (statFrac * ma.WRC) + ((1 - statFrac) * wRCPlus);
                        }

                        Player_Hitter_MonthlyRatios ratio = db.Player_Hitter_MonthlyRatios.Where(f => f.MlbId == grouping.Key.MlbId && f.Month == month && f.Year == year && f.LeagueId == grouping.Key.LeagueId).Single();
                        ratio.WRC = wRCPlus;

                        progressBar.Tick();
                    }
                }
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in UpdateMonthRatiosWRC");
                Utilities.LogException(e);
                throw;
            }
        }
    }
}
