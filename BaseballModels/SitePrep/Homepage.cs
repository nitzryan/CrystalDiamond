using Db;
using ShellProgressBar;
using SiteDb;

namespace SitePrep
{
    internal class Homepage
    {
        private class DatePair {
            public required int PrevYear { get; set; }
            public required int PrevMonth { get; set; }
            public required int CurYear { get; set; }
            public required int CurMonth { get; set; }
        }

        private const int GRADUATED_TYPE = 1;
        private const int MOST_IMPROVED_TYPE = 2;
        private const int LEAST_IMPROVED_TYPE = 3;

        public static bool Main()
        {
            try {
                using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);

                siteDb.HomeData.RemoveRange(siteDb.HomeData);
                siteDb.HomeDataType.RemoveRange(siteDb.HomeDataType);
                siteDb.SaveChanges();
                siteDb.ChangeTracker.Clear();

                // Get dates
                var dates = siteDb.PlayerRank.Select(f => new { f.Month, f.Year }).Distinct().OrderBy(f => f.Year).ThenBy(f => f.Month).ToList();
                List<DatePair> datePairs = new(dates.Count() - 1);
                for (var i = 1; i < dates.Count(); i++)
                {
                    var prevDate = dates[i - 1];
                    var currentDate = dates[i];
                    datePairs.Add(new DatePair
                    {
                        PrevYear = prevDate.Year,
                        PrevMonth = prevDate.Month,
                        CurYear = currentDate.Year,
                        CurMonth = currentDate.Month
                    });
                }

                // Graduated players
                siteDb.HomeDataType.Add(new HomeDataType
                {
                    Name = "Graduated Players",
                    Type = GRADUATED_TYPE
                });
                using (ProgressBar progressBar = new ProgressBar(datePairs.Count(), "Generating Graduated player ranks"))
                {
                    foreach (var datePair in datePairs)
                    {
                        var players = siteDb.PlayerRank.Where(f => f.Year == datePair.PrevYear && f.Month == datePair.PrevMonth)
                            .Where(f => !siteDb.PlayerRank.Any(pr => pr.MlbId == f.MlbId && pr.Year == datePair.CurYear && pr.Month == datePair.CurMonth))
                            .OrderByDescending(f => f.War).ToList();

                        int length = Math.Min(10, players.Count());
                        for (var rank = 0; rank < length; rank++)
                        {
                            var player = players[rank];
                            siteDb.HomeData.Add(new HomeData{
                                Year = datePair.CurYear,
                                Month = datePair.CurMonth,
                                RankType = GRADUATED_TYPE,
                                MlbId = player.MlbId,
                                Data = player.War.ToString("0.0") + " WAR",
                                Rank = rank + 1
                            });
                        }
                        progressBar.Tick();
                    }
                }
                siteDb.SaveChanges();

                // Most-Improved/Degraded
                siteDb.HomeDataType.Add(new HomeDataType
                {
                    Name = "Most Improved",
                    Type = MOST_IMPROVED_TYPE
                });
                siteDb.HomeDataType.Add(new HomeDataType
                {
                    Name = "Least Improved",
                    Type = LEAST_IMPROVED_TYPE
                });
                using (ProgressBar progressBar = new ProgressBar(datePairs.Count(), "Generating Most/Least Improved"))
                {
                    foreach (var datePair in datePairs)
                    {
                        var players = siteDb.PlayerRank
                            .Join(siteDb.PlayerRank, 
                                prev => new { prev.MlbId, prev.ModelName },
                                cur => new { cur.MlbId, cur.ModelName },
                                (prev, cur) => new {prev, cur})
                            .Where(f => f.cur.Year == datePair.CurYear 
                                && f.cur.Month == datePair.CurMonth 
                                && f.prev.Year == datePair.PrevYear 
                                && f.prev.Month == datePair.PrevMonth)
                            .Select(f => new { f.cur.MlbId, delta = f.cur.War - f.prev.War })
                            .OrderByDescending(f => f.delta).ToList();

                        int length = Math.Min(10, players.Count());
                        for (var rank = 0; rank < length; rank++)
                        {
                            var player = players[rank];
                            siteDb.HomeData.Add(new HomeData
                            {
                                Year = datePair.CurYear,
                                Month = datePair.CurMonth,
                                RankType = MOST_IMPROVED_TYPE,
                                MlbId = player.MlbId,
                                Data = player.delta.ToString("0.0") + " WAR",
                                Rank = rank + 1
                            });

                            var revPlayer = players[players.Count() - 1 - rank];
                            siteDb.HomeData.Add(new HomeData
                            {
                                Year = datePair.CurYear,
                                Month = datePair.CurMonth,
                                RankType = LEAST_IMPROVED_TYPE,
                                MlbId = revPlayer.MlbId,
                                Data = revPlayer.delta.ToString("0.0") + " WAR",
                                Rank = rank + 1
                            });
                        }
                        progressBar.Tick();
                    }
                }
                siteDb.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Homepage");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
