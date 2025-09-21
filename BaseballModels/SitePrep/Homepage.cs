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
            public required int ModelId { get; set; }
        }

        private const int GRADUATED_TYPE = 1;
        private const int MOST_IMPROVED_TYPE = 2;
        private const int LEAST_IMPROVED_TYPE = 3;
        private const int BREAKOUT_TYPE = 4;

        public static bool Main()
        {
            try {
                using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);

                siteDb.HomeData.RemoveRange(siteDb.HomeData);
                siteDb.HomeDataType.RemoveRange(siteDb.HomeDataType);
                siteDb.SaveChanges();
                siteDb.ChangeTracker.Clear();

                // Get dates
                var dates = siteDb.PlayerRank.Select(f => new { f.Month, f.Year, f.ModelId }).Distinct().OrderBy(f => f.Year).ThenBy(f => f.Month).ToList();
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
                        CurMonth = currentDate.Month,
                        ModelId = currentDate.ModelId
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
                        var players = siteDb.PlayerRank.Where(f => f.Year == datePair.PrevYear && f.Month == datePair.PrevMonth && f.ModelId == datePair.ModelId)
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
                                ModelId = datePair.ModelId,
                                MlbId = player.MlbId,
                                Data = player.War.ToString("0.0") + " WAR",
                                Rank = rank + 1
                            });
                        }
                        progressBar.Tick();
                    }
                }
                siteDb.SaveChanges();

                // Most-Improved/Degraded/Breakout
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
                siteDb.HomeDataType.Add(new HomeDataType
                {
                    Name = "Breakout Players",
                    Type = BREAKOUT_TYPE
                });
                using (ProgressBar progressBar = new ProgressBar(datePairs.Count(), "Generating Most/Least Improved"))
                {
                    foreach (var datePair in datePairs)
                    {
                        var players = siteDb.PlayerRank
                            .Join(siteDb.PlayerRank, 
                                prev => new { prev.MlbId, prev.ModelId, prev.IsHitter },
                                cur => new { cur.MlbId, cur.ModelId, cur.IsHitter },
                                (prev, cur) => new {prev, cur})
                            .Where(f => f.cur.Year == datePair.CurYear 
                                && f.cur.Month == datePair.CurMonth 
                                && f.prev.Year == datePair.PrevYear 
                                && f.prev.Month == datePair.PrevMonth)
                            .Select(f => new { f.cur.MlbId, delta = f.cur.War - f.prev.War, prevWar = f.prev.War, curWar = f.cur.War })
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
                                ModelId = datePair.ModelId,
                                MlbId = player.MlbId,
                                Data = player.curWar.ToString("0.0") + " (+" + player.delta.ToString("0.0") + ") WAR",
                                Rank = rank + 1
                            });

                            var revPlayer = players[players.Count() - 1 - rank];
                            siteDb.HomeData.Add(new HomeData
                            {
                                Year = datePair.CurYear,
                                Month = datePair.CurMonth,
                                RankType = LEAST_IMPROVED_TYPE,
                                ModelId = datePair.ModelId,
                                MlbId = revPlayer.MlbId,
                                Data = revPlayer.curWar.ToString("0.0") + " (" + revPlayer.delta.ToString("0.0") + ") WAR",
                                Rank = rank + 1
                            });
                        }

                        // Breakout should go by multiple, with a 0.5 min floor
                        players = [.. players.OrderByDescending(f => f.delta / Math.Max(f.prevWar, 0.5f))];
                        for (var rank = 0; rank < length; rank++)
                        {
                            var player = players[rank];
                            siteDb.Add(new HomeData
                            {
                                Year = datePair.CurYear,
                                Month = datePair.CurMonth,
                                RankType = BREAKOUT_TYPE,
                                ModelId = datePair.ModelId,
                                MlbId = player.MlbId,
                                Data = player.curWar.ToString("0.0") + " (+" + player.delta.ToString("0.0") + ") WAR",
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
