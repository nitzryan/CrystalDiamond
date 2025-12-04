using Db;
using ShellProgressBar;

namespace DataAquisition
{
    internal class CalculateAnnualWRC
    {
        public static bool Main(int year)
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
                        Player_Hitter_GameLog lps = db.Player_Hitter_GameLog.Where(f => f.Year == year && f.LeagueId == league && f.Position != 1)
                            .Aggregate(Utilities.HitterGameLogAggregation);

                        LeagueStats ls = db.LeagueStats.Where(f => f.LeagueId == league && f.Year == year).Single();

                        // Calculate League wRC
                        double leaguewRC = (lps.BB * ls.WBB) +
                            (lps.HBP * ls.WHBP) +
                            ((lps.H - lps.Hit2B - lps.Hit3B - lps.HR) * ls.W1B) +
                            (lps.Hit2B * ls.W2B) +
                            (lps.Hit3B * ls.W3B) +
                            (lps.HR * ls.WHR);

                        float leagueHittersWOBA = (float)leaguewRC / lps.PA;
                        float leaguewRCperPA = (((leagueHittersWOBA - ls.AvgWOBA) / ls.WOBAScale) + ls.RPerPA);

                        // Iterate through player month stats
                        var monthsAdvanced = db.Player_Hitter_MonthAdvanced.Where(f => f.Year == year && f.LeagueId == league);
                        foreach (var ma in monthsAdvanced)
                        {
                            float wRAAPerPA = (ma.WOBA - ls.AvgWOBA) / ls.WOBAScale;
                            // wRC+ = (a + (b - c)) / d * 100
                            float a = wRAAPerPA + ls.RPerPA;
                            float b = ls.RPerPA;
                            float c = (ma.ParkFactor * ls.RPerPA);
                            float d = leaguewRCperPA;
                            ma.WRC = 100 * (a + (b - c)) / d;
                        }

                        var yearAdvanced = db.Player_Hitter_YearAdvanced.Where(f => f.Year == year && f.LeagueId == league);
                        foreach (var ya in yearAdvanced)
                        {
                            float wRAAPerPA = (ya.WOBA - ls.AvgWOBA) / ls.WOBAScale;
                            // wRC+ = (a + (b - c)) / d * 100
                            float a = wRAAPerPA + ls.RPerPA;
                            float b = ls.RPerPA;
                            float c = (ya.ParkFactor * ls.RPerPA);
                            float d = leaguewRCperPA;
                            ya.WRC = 100 * (a + (b - c)) / d;
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

        public static bool UpdateMonthRatiosWRC(int year, int month)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                var phma = db.Player_Hitter_MonthAdvanced.Where(f => f.Year == year && f.Month == month)
                    .GroupBy(f => new { f.MlbId, f.LevelId });
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

                        Db.Player_Hitter_MonthlyRatios ratio = db.Player_Hitter_MonthlyRatios.Where(f => f.MlbId == grouping.Key.MlbId && f.Month == month && f.Year == year && f.LevelId == grouping.Key.LevelId).Single();
                        ratio.WRC = wRCPlus;

                        progressBar.Tick();
                    }
                }
                db.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in UpdateMonthRatiosWRC");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
