using Db;
using ShellProgressBar;

namespace SitePrep
{
    internal class ModelAggregation
    {
        private static bool PlayerWar()
        {
            using (SqliteDbContext db_write = new(Constants.DB_WRITE_OPTIONS))
            {
                db_write.Output_PlayerWarAggregation.RemoveRange(db_write.Output_PlayerWarAggregation);
                db_write.SaveChanges();
            }

            List<Db.Output_PlayerWarAggregation> items = new();

            using (SqliteDbContext db = new(Constants.DB_OPTIONS))
            {
                var opw = db.Output_PlayerWar.Where(f => f.ModelIdx == 0);
                using (ProgressBar progressBar = new ProgressBar(opw.Count(), "Aggregating Model Results"))
                {
                    foreach (var o in opw)
                    {
                        var model_results = db.Output_PlayerWar.Where(f => f.MlbId == o.MlbId && f.Year == o.Year && f.Month == o.Month && f.Model == o.Model && f.IsHitter == o.IsHitter);
                        int size = model_results.Count();
                        if (size == 0)
                            throw new Exception("No elements in model_results, should not happen");

                        Db.Output_PlayerWarAggregation owa = new()
                        {
                            MlbId = o.MlbId,
                            Model = o.Model,
                            IsHitter = o.IsHitter,
                            Year = o.Year,
                            Month = o.Month,
                            War0 = 0,
                            War1 = 0,
                            War2 = 0,
                            War3 = 0,
                            War4 = 0,
                            War5 = 0,
                            War6 = 0,
                            War = 0,
                        };

                        foreach (var result in model_results)
                        {
                            owa.War0 += result.War0 / size;
                            owa.War1 += result.War1 / size;
                            owa.War2 += result.War2 / size;
                            owa.War3 += result.War3 / size;
                            owa.War4 += result.War4 / size;
                            owa.War5 += result.War5 / size;
                            owa.War6 += result.War6 / size;
                            owa.War += result.War / size;
                        }

                        items.Add(owa);
                        progressBar.Tick();
                    }
                }
            }


            using (SqliteDbContext db_write = new(Constants.DB_WRITE_OPTIONS))
            {
                db_write.Output_PlayerWarAggregation.AddRange(items);
                db_write.SaveChanges();
            }

            return true;
        }

        private static bool HitterValue()
        {
            using (SqliteDbContext db_write = new(Constants.DB_WRITE_OPTIONS))
            {
                db_write.Output_HitterValueAggregation.RemoveRange(db_write.Output_HitterValueAggregation);
                db_write.SaveChanges();
            }

            List<Db.Output_HitterValueAggregation> items = new();

            using (SqliteDbContext db = new(Constants.DB_OPTIONS))
            {
                var ohv = db.Output_HitterValue.Where(f => f.ModelIdx == 0);
                using (ProgressBar progressBar = new ProgressBar(ohv.Count(), "Aggregating Hitter Values"))
                {
                    foreach (var o in ohv)
                    {
                        var model_results = db.Output_HitterValue.Where(f => f.MlbId == o.MlbId && f.Year == o.Year && f.Month == o.Month && f.Model == o.Model);
                        int size = model_results.Count();
                        if (size == 0)
                            throw new Exception("No elements in model_results, should not happen");

                        Db.Output_HitterValueAggregation owa = new()
                        {
                            MlbId = o.MlbId,
                            Model = o.Model,
                            Year = o.Year,
                            Month = o.Month,
                            WAR1Year = 0,
                            WAR2Year = 0,
                            WAR3Year = 0,
                            OFF1Year = 0,
                            OFF2Year = 0,
                            OFF3Year = 0,
                            DEF1Year = 0,
                            DEF2Year = 0,
                            DEF3Year = 0,
                            BSR1Year = 0,
                            BSR2Year = 0,
                            BSR3Year = 0,
                            PA1Year = 0,
                            PA2Year = 0,
                            PA3Year = 0
                        };

                        foreach (var result in model_results)
                        {
                            owa.WAR1Year += result.WAR1Year / size;
                            owa.WAR2Year += result.WAR2Year / size;
                            owa.WAR3Year += result.WAR3Year / size;
                            owa.OFF1Year += result.OFF1Year / size;
                            owa.OFF2Year += result.OFF2Year / size;
                            owa.OFF3Year += result.OFF3Year / size;
                            owa.DEF1Year += result.DEF1Year / size;
                            owa.DEF2Year += result.DEF2Year / size;
                            owa.DEF3Year += result.DEF3Year / size;
                            owa.BSR1Year += result.BSR1Year / size;
                            owa.BSR2Year += result.BSR2Year / size;
                            owa.BSR3Year += result.BSR3Year / size;
                            owa.PA1Year += result.PA1Year / size;
                            owa.PA2Year += result.PA2Year / size;
                            owa.PA3Year += result.PA3Year / size;
                        }

                        items.Add(owa);
                        progressBar.Tick();
                    }
                }
            }


            using (SqliteDbContext db_write = new(Constants.DB_WRITE_OPTIONS))
            {
                db_write.Output_HitterValueAggregation.AddRange(items);
                db_write.SaveChanges();
            }

            return true;
        }

        private static bool PitcherValue()
        {
            using (SqliteDbContext db_write = new(Constants.DB_WRITE_OPTIONS))
            {
                db_write.Output_PitcherValueAggregation.RemoveRange(db_write.Output_PitcherValueAggregation);
                db_write.SaveChanges();
            }

            List<Db.Output_PitcherValueAggregation> items = new();

            using (SqliteDbContext db = new(Constants.DB_OPTIONS))
            {
                var ohv = db.Output_PitcherValue.Where(f => f.ModelIdx == 0);
                using (ProgressBar progressBar = new ProgressBar(ohv.Count(), "Aggregating Pitcher Values"))
                {
                    foreach (var o in ohv)
                    {
                        var model_results = db.Output_PitcherValue.Where(f => f.MlbId == o.MlbId && f.Year == o.Year && f.Month == o.Month && f.Model == o.Model);
                        int size = model_results.Count();
                        if (size == 0)
                            throw new Exception("No elements in model_results, should not happen");

                        Db.Output_PitcherValueAggregation owa = new()
                        {
                            MlbId = o.MlbId,
                            Model = o.Model,
                            Year = o.Year,
                            Month = o.Month,
                            WarSP1Year = 0,
                            WarSP2Year = 0,
                            WarSP3Year = 0,
                            WarRP1Year = 0,
                            WarRP2Year = 0,
                            WarRP3Year = 0,
                            IPSP1Year = 0,
                            IPSP2Year = 0,
                            IPSP3Year = 0,
                            IPRP1Year = 0,
                            IPRP2Year = 0,
                            IPRP3Year = 0,
                        };

                        foreach (var result in model_results)
                        {
                            owa.WarSP1Year += result.WarSP1Year / size;
                            owa.WarSP2Year += result.WarSP2Year / size;
                            owa.WarSP3Year += result.WarSP3Year / size;
                            owa.WarRP1Year += result.WarRP1Year / size;
                            owa.WarRP2Year += result.WarRP2Year / size;
                            owa.WarRP3Year += result.WarRP3Year / size;
                            owa.IPSP1Year += result.IPSP1Year / size;
                            owa.IPSP2Year += result.IPSP2Year / size;
                            owa.IPSP3Year += result.IPSP3Year / size;
                            owa.IPRP1Year += result.IPRP1Year / size;
                            owa.IPRP2Year += result.IPRP2Year / size;
                            owa.IPRP3Year += result.IPRP3Year / size;
                        }

                        items.Add(owa);
                        progressBar.Tick();
                    }
                }
            }


            using (SqliteDbContext db_write = new(Constants.DB_WRITE_OPTIONS))
            {
                db_write.Output_PitcherValueAggregation.AddRange(items);
                db_write.SaveChanges();
            }

            return true;
        }

        public static bool Main()
        {
            try
            {
                if (!ModelAggregation.PlayerWar())
                    return false;

                if (!ModelAggregation.HitterValue())
                    return false;

                if (!ModelAggregation.PitcherValue())
                    return false;

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in ModelAggregation");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
