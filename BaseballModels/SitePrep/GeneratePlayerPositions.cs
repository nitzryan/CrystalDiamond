using Db;
using ShellProgressBar;
using SiteDb;

namespace SitePrep
{
    internal class GeneratePlayerPositions
    {
        public static bool MainFunc()
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
                siteDb.PlayerYearPositions.RemoveRange(siteDb.PlayerYearPositions);
                siteDb.SaveChanges();
                siteDb.ChangeTracker.Clear();

                // Create position list
                using (ProgressBar progressBar = new ProgressBar(db.Model_Players.Count(), "Creating playerYearPositions"))
                {
                    foreach (var mp in db.Model_Players)
                    {
                        List<PlayerYearPositions> pyps = new();

                        IEnumerable<int> hitterYears = mp.IsHitter == 1 ? db.Player_Hitter_YearAdvanced.Where(f => f.MlbId == mp.MlbId).Select(f => f.Year).Distinct().OrderBy(f => f) : [];
                        IEnumerable<int> pitcherYears = mp.IsPitcher == 1 ? db.Player_Pitcher_YearAdvanced.Where(f => f.MlbId == mp.MlbId).Select(f => f.Year).Distinct().OrderBy(f => f) : [];

                        if (mp.IsPitcher == 1)
                        {
                            foreach (var y in pitcherYears)
                            {
                                pyps.Add(new PlayerYearPositions
                                {
                                    Year = y,
                                    Position = "P",
                                    IsHitter = 0,
                                    MlbId = mp.MlbId
                                });
                            }
                        }
                        if (mp.IsHitter == 1)
                        {
                            foreach (var y in hitterYears)
                            {
                                pyps.Add(new PlayerYearPositions
                                {
                                    Year = y,
                                    Position = Utilities.GetPosition(db, mp.MlbId, y),
                                    IsHitter = 1,
                                    MlbId = mp.MlbId
                                });
                            }
                        }

                        siteDb.PlayerYearPositions.AddRange(pyps);

                        progressBar.Tick();
                    }
                }
                siteDb.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in GeneratePlayerPositions");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
