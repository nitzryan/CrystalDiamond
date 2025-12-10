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

                        var playerRanks = siteDb.PlayerRank.Where(f => f.Year == year && f.Month == month && f.TeamId != 0 && f.ModelId == model)
                            .GroupBy(f => f.TeamId);
                        
                        var teamRanksWar = playerRanks
                            .Select(g => new TeamRank
                            {
                                TeamId = g.Key,
                                ModelId = model,
                                Year = g.First().Year,
                                Month = g.First().Month,
                                HighestRank = g.Min(f => f.RankWar),
                                Top10 = g.Count(f => f.RankWar <= 10),
                                Top50 = g.Count(f => f.RankWar <= 50),
                                Top100 = g.Count(f => f.RankWar <= 100),
                                Top200 = g.Count(f => f.RankWar <= 200),
                                Top500 = g.Count(f => f.RankWar <= 500),
                                Rank = 0,
                                War = g.Sum(f => f.War)
                            })
                            .OrderByDescending(g => g.War);

                        int rank = 1;
                        foreach (var tr in teamRanksWar)
                        {
                            int r = rank;
                            tr.Rank = r;
                            siteDb.TeamRank.Add(tr);
                            rank++;
                        }
                        rank = 1;

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
