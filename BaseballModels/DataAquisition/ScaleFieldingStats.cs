using Db;
using ShellProgressBar;

namespace DataAquisition
{
    internal class ScaleFieldingStats
    {
        public static bool Update(int year)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                int[] leagues = db.Player_Fielder_MonthStats.Where(f => f.Year == year).Select(f => f.LeagueId).Distinct().ToArray();
                using (ProgressBar progressBar = new ProgressBar(leagues.Count(), $"Scaling Fielding Stats for Year={year}"))
                {
                    foreach (int league in leagues)
                    {
                        float[] dRAAs = db.Player_Fielder_MonthStats.Where(f => f.Year == year && f.LeagueId == league && f.Position != DbEnums.Position.DH)
                            .GroupBy(f => new { f.MlbId, f.Month }).Select(f => f.Sum(g => g.D_RAA)).ToArray();
                        float avg = dRAAs.Average();
                        float stddev = MathF.Sqrt(dRAAs.Sum(f => MathF.Pow(f - avg, 2)) / dRAAs.Count());

                        // Some leagues have much better fielding data coding for hit positions than others
                        // This leads to worse coding and more varied results in some leagues, and don't want
                        // Those leagues to dominate the variance in the model
                        float scaleFactor = 1;
                        if (stddev > 3)
                        {
                            scaleFactor = 3 / stddev;
                        }

                        var stats = db.Player_Fielder_MonthStats.Where(f => f.Year == year && f.LeagueId == league);
                        foreach (var s in stats)
                            s.ScaledDRAA = s.D_RAA * scaleFactor;

                        var yearFieldingStats = db.Player_Fielder_YearStats.Where(f => f.Year == year && f.LeagueId == league);
                        foreach (var yfs in yearFieldingStats)
                            yfs.ScaledDRAA = yfs.D_RAA * scaleFactor;

                        progressBar.Tick();
                    }
                }

                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in ScaleFieldingStats");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
