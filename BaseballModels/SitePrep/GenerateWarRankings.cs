using Db;
using ShellProgressBar;
using SitePrep;

namespace SiteDb
{
    internal class GenerateWarRankings
    {
        internal class PlayerMonthWar
        {
            public required int MLbId;
            public required int ModelId;
            public required string position;
            public required float War;
            public required float Pa;
            public required int ParentOrgId;
        }

        internal class PlayerYearPosition
        {
            public required int Year;
            public required string position;
        }

        private static bool GenerateHitterRankings(int endYear, int endMonth)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
            siteDb.HitterWarRank.RemoveRange(siteDb.HitterWarRank);
            siteDb.SaveChanges();
            siteDb.ChangeTracker.Clear();

            // Iterate through months
            int year = 2015;
            int month = 4;
            List<(int, int)> dates = new();
            while (year < endYear || (year == endYear && month <= endMonth))
            {
                dates.Add((month, year));
                month++;
                if (month > 9)
                {
                    month = 4;
                    year++;
                }
            }

            using (ProgressBar progressBar = new ProgressBar(db.ModelIdx.Count() * dates.Count, "Generating Hitter War Rankings for Models"))
            {
                var ids = db.Output_HitterValueAggregation.Select(f => f.MlbId).Distinct();
            
                foreach (var model in db.ModelIdx)
                {
                    foreach (var date in dates)
                    {
                        month = date.Item1;
                        year = date.Item2;
                        var hitterWars = db.Output_HitterValueAggregation.Where(f => f.Year == year && f.Month == month && f.Model == model.Id && f.PA1Year >= 50)
                            .OrderByDescending(f => f.WAR1Year);

                        int rank = 1;
                        List<HitterWarRank> warRanks = new(hitterWars.Count());
                        foreach (var hw in hitterWars)
                        {
                            var poms = db.Player_OrgMap.Where(f => f.MlbId == hw.MlbId &&
                                                (f.Year < year || (f.Year == year && f.Month <= month)))
                                                .OrderByDescending(f => f.Year).ThenByDescending(f => f.Month);

                            int r = rank;
                            var pyps = siteDb.PlayerYearPositions.Where(f => f.MlbId == hw.MlbId && f.Year <= year)
                                .OrderByDescending(f => f.Year);
                            string position = pyps.Any() ? pyps.First().Position : "H";
                            warRanks.Add(new HitterWarRank
                            {
                                MlbId = hw.MlbId,
                                ModelId = model.Id,
                                Year = year,
                                Month = month,
                                TeamId = poms.Any() ?  // Few players only played on VSL teams that were multiple teams (no parent)
                                                poms.First().ParentOrgId : 0,
                                Position = position,
                                War = hw.WAR1Year,
                                RankWar = r,
                                Pa = hw.PA1Year,
                            });
                            rank++;
                        }

                        siteDb.HitterWarRank.AddRange(warRanks);
                        progressBar.Tick();
                    }
                }
            }

            siteDb.SaveChanges();
            return true;
        }

        private static bool GeneratePitcherRankings(int endYear, int endMonth)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using SiteDbContext siteDb = new(Constants.SITEDB_OPTIONS);
            siteDb.PitcherWarRank.RemoveRange(siteDb.PitcherWarRank);
            siteDb.SaveChanges();
            siteDb.ChangeTracker.Clear();

            // Iterate through months
            int year = 2015;
            int month = 4;
            List<(int, int)> dates = new();
            while (year < endYear || (year == endYear && month <= endMonth))
            {
                dates.Add((month, year));
                month++;
                if (month > 9)
                {
                    month = 4;
                    year++;
                }
            }

            using (ProgressBar progressBar = new ProgressBar(db.ModelIdx.Count() * dates.Count, "Generating Pitcher War Rankings for Models"))
            {
                var ids = db.Output_PitcherValueAggregation.Select(f => f.MlbId).Distinct();

                foreach (var model in db.ModelIdx)
                {
                    foreach (var date in dates)
                    {
                        month = date.Item1;
                        year = date.Item2;
                        var starterWars = db.Output_PitcherValueAggregation.Where(f => f.Year == year && f.Month == month && f.Model == model.Id && f.IPSP1Year >= 20)
                            .OrderByDescending(f => f.WarSP1Year);
                        var relieverWars = db.Output_PitcherValueAggregation.Where(f => f.Year == year && f.Month == month && f.Model == model.Id && f.IPRP1Year >= 20)
                            .OrderByDescending(f => f.WarRP1Year);

                        // Add Starter War
                        int rank = 1;
                        List<PitcherWarRank> warRanks = new(starterWars.Count() + relieverWars.Count());
                        foreach (var sw in starterWars)
                        {
                            var poms = db.Player_OrgMap.Where(f => f.MlbId == sw.MlbId &&
                                                (f.Year < year || (f.Year == year && f.Month <= month)))
                                                .OrderByDescending(f => f.Year).ThenByDescending(f => f.Month);

                            int r = rank;
                            warRanks.Add(new PitcherWarRank
                            {
                                MlbId = sw.MlbId,
                                ModelId = model.Id,
                                Year = year,
                                Month = month,
                                TeamId = poms.Any() ? poms // Few players only played on VSL teams that were multiple teams (no parent)
                                                .First().ParentOrgId : 0,
                                SpWar = sw.WarSP1Year,
                                RpWar = sw.WarRP1Year,
                                SpRank = r,
                                RpRank = null,
                                SpIP = sw.IPSP1Year,
                                RpIP = sw.IPRP1Year
                            });
                            rank++;
                        }

                        // Add reliever wars, modifying those that have a starter war
                        foreach (var rw in relieverWars)
                        {
                            var poms = db.Player_OrgMap.Where(f => f.MlbId == rw.MlbId &&
                                                (f.Year < year || (f.Year == year && f.Month <= month)))
                                                .OrderByDescending(f => f.Year).ThenByDescending(f => f.Month);

                            int r = rank;
                            var starterRank = warRanks.Where(f => f.MlbId == rw.MlbId);
                            if (starterRank.Any())
                            {
                                starterRank.Single().RpRank = r;
                            } else {
                                warRanks.Add(new PitcherWarRank
                                {
                                    MlbId = rw.MlbId,
                                    ModelId = model.Id,
                                    Year = year,
                                    Month = month,
                                    TeamId = poms.Any() ? poms // Few players only played on VSL teams that were multiple teams (no parent)
                                                .First().ParentOrgId : 0,
                                    SpWar = rw.WarSP1Year,
                                    RpWar = rw.WarRP1Year,
                                    SpRank = null,
                                    RpRank = r,
                                    SpIP = rw.IPSP1Year,
                                    RpIP = rw.IPRP1Year
                                });
                            }
                            rank++;
                        }

                        siteDb.PitcherWarRank.AddRange(warRanks);
                        progressBar.Tick();
                    }
                }
            }

            siteDb.SaveChanges();
            return true;
        }

        public static bool MainFunc(int endYear, int endMonth)
        {
            try {
                
                if (!GenerateHitterRankings(endYear, endMonth))
                    return false;
                if (!GeneratePitcherRankings(endYear, endMonth))
                    return false;

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in GenerateWarRankings");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
