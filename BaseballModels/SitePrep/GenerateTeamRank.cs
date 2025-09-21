using ShellProgressBar;
using SiteDb;

namespace SitePrep
{
    internal class GenerateTeamRank
    {
        public static bool Main()
        {
            try {
                using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
                siteDb.TeamRank.RemoveRange(siteDb.TeamRank);
                siteDb.SaveChanges();
                siteDb.ChangeTracker.Clear();

                var combos = siteDb.PlayerRank.Select(f => new { f.Year, f.Month, f.ModelId }).Distinct();
                using (ProgressBar progressBar = new ProgressBar(combos.Count(), "Creating TeamRanks"))
                {
                    foreach (var combo in combos)
                    {
                        int year = combo.Year;
                        int month = combo.Month;
                        int model = combo.ModelId;
                        //string model = combo.ModelName;

                        var teamRanks = siteDb.PlayerRank.Where(f => f.Year == year && f.Month == month && f.TeamId != 0 && f.ModelId == model)
                            .GroupBy(f => f.TeamId)
                            .Select(g => new TeamRank
                            {
                                TeamId = g.Key,
                                ModelId = model,
                                Year = g.First().Year,
                                Month = g.First().Month,
                                Value = g.Sum(f => f.War),
                                HighestRank = g.Min(f => f.Rank),
                                Top10 = g.Count(f => f.Rank <= 10),
                                Top50 = g.Count(f => f.Rank <= 50),
                                Top100 = g.Count(f => f.Rank <= 100),
                                Top200 = g.Count(f => f.Rank <= 200),
                                Top500 = g.Count(f => f.Rank <= 500),
                                Rank = 0
                            })
                            .OrderByDescending(g => g.Value);

                        int rank = 1;
                        foreach (var tr in teamRanks)
                        {
                            int r = rank;
                            tr.Rank = r;
                            siteDb.TeamRank.Add(tr);
                            rank++;
                        }

                        progressBar.Tick();
                    }
                }
                siteDb.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in GenerateTeamRank");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
