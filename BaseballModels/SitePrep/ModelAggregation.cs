using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace SitePrep
{
    internal class ModelAggregation
    {
        private static bool PlayerWar()
        {
            using (SqliteDbContext db_write = new(Constants.DB_WRITE_OPTIONS))
            {
                db_write.Database.ExecuteSqlRaw("DELETE FROM Output_PlayerWarAggregation;");
            }

            List<Db.Output_PlayerWarAggregation> items = new();

            using (SqliteDbContext db = new(Constants.DB_OPTIONS))
            {
                var opws = db.Output_PlayerWar.GroupBy(f => new { f.MlbId, f.Model, f.IsHitter, f.Year, f.Month });
                int count = opws.Count();
                items.Capacity = count;
                //var opw = db.Output_PlayerWar.Where(f => f.ModelIdx == 0);
                using (ProgressBar progressBar = new ProgressBar(count, "Aggregating Model Results"))
                {
                    foreach (var o in opws)
                    {
                        int size = o.Count();
                        if (size == 0)
                            throw new Exception("No elements in model_results, should not happen");

                        Db.Output_PlayerWarAggregation owa = new()
                        {
                            MlbId = o.Key.MlbId,
                            Model = o.Key.Model,
                            IsHitter = o.Key.IsHitter,
                            Year = o.Key.Year,
                            Month = o.Key.Month,
                            War0 = 0,
                            War1 = 0,
                            War2 = 0,
                            War3 = 0,
                            War4 = 0,
                            War5 = 0,
                            War6 = 0,
                            War = 0,
                        };

                        foreach (var result in o)
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
                db_write.BulkInsert(items);
            }

            return true;
        }

        private static bool HitterStats()
        {
            using (SqliteDbContext db_write = new(Constants.DB_WRITE_OPTIONS))
            {
                db_write.Database.ExecuteSqlRaw("DELETE FROM Output_HitterStatsAggregation;");
            }

            List<Db.Output_HitterStatsAggregation> items = new();

            using (SqliteDbContext db = new(Constants.DB_OPTIONS))
            {
                var ohs = db.Output_HitterStats.GroupBy(f => new { f.MlbId, f.Model, f.LevelId, f.Year, f.Month });
                int count = ohs.Count();
                items.Capacity = count;
                using (ProgressBar progressBar = new ProgressBar(count, "Aggregating Hitter Stats"))
                {
                    foreach (var o in ohs)
                    {
                        int size = o.Count();
                        if (size == 0)
                            throw new Exception("No elements in model_results, should not happen");

                        Db.Output_HitterStatsAggregation ohsa = new()
                        {
                            MlbId = o.Key.MlbId,
                            Model = o.Key.Model,
                            Year = o.Key.Year,
                            Month = o.Key.Month,
                            LevelId = o.Key.LevelId,
                            Pa = 0,
                            Hit1B = 0,
                            Hit2B = 0,
                            Hit3B = 0,
                            HitHR = 0,
                            BB = 0,
                            HBP = 0,
                            K = 0,
                            SB = 0,
                            CS = 0,
                            BSR = 0,
                            DRAA = 0,
                            ParkRunFactor = 0,
                            PercC = 0,
                            Perc1B = 0,
                            Perc2B = 0,
                            Perc3B = 0,
                            PercSS = 0,
                            PercLF = 0,
                            PercCF = 0,
                            PercRF = 0,
                            PercDH = 0,
                        };

                        foreach (var result in o)
                        {
                            ohsa.Pa += result.Pa / size;
                            ohsa.Hit1B += result.Hit1B / size;
                            ohsa.Hit2B += result.Hit2B / size;
                            ohsa.Hit3B += result.Hit3B / size;
                            ohsa.HitHR += result.HitHR / size;
                            ohsa.BB += result.BB / size;
                            ohsa.HBP += result.HBP / size;
                            ohsa.K += result.K / size;
                            ohsa.SB += result.SB / size;
                            ohsa.CS += result.CS / size;
                            ohsa.BSR += result.BSR / size;
                            ohsa.DRAA += result.DRAA / size;
                            ohsa.ParkRunFactor += result.ParkRunFactor / size;
                            ohsa.PercC += result.PercC / size;
                            ohsa.Perc1B += result.Perc1B / size;
                            ohsa.Perc2B += result.Perc2B / size;
                            ohsa.Perc3B += result.Perc3B / size;
                            ohsa.PercSS += result.PercSS / size;
                            ohsa.PercLF += result.PercLF / size;
                            ohsa.PercCF += result.PercCF / size;
                            ohsa.PercRF += result.PercRF / size;
                            ohsa.PercDH += result.PercDH / size;
                        }

                        items.Add(ohsa);
                        progressBar.Tick();
                    }
                }
            }


            using (SqliteDbContext db_write = new(Constants.DB_WRITE_OPTIONS))
            {
                db_write.BulkInsert(items);
            }

            return true;
        }

        private static bool PitcherStats()
        {
            using (SqliteDbContext db_write = new(Constants.DB_WRITE_OPTIONS))
            {
                db_write.Database.ExecuteSqlRaw("DELETE FROM Output_PitcherStatsAggregation;");
            }

            List<Db.Output_PitcherStatsAggregation> items = new();

            using (SqliteDbContext db = new(Constants.DB_OPTIONS))
            {
                var ops = db.Output_PitcherStats.GroupBy(f => new { f.MlbId, f.Model, f.LevelId, f.Year, f.Month });
                int count = ops.Count();
                items.Capacity = count;
                using (ProgressBar progressBar = new ProgressBar(count, "Aggregating Pitcher Stats"))
                {
                    foreach (var o in ops)
                    {
                        int size = o.Count();
                        if (size == 0)
                            throw new Exception("No elements in model_results, should not happen");

                        Db.Output_PitcherStatsAggregation opsa = new()
                        {
                            MlbId = o.Key.MlbId,
                            Model = o.Key.Model,
                            Year = o.Key.Year,
                            Month = o.Key.Month,
                            LevelId = o.Key.LevelId,
                            Outs_SP = 0,
                            Outs_RP = 0,
                            GS = 0,
                            GR = 0,
                            ERA = 0,
                            FIP = 0,
                            HR = 0,
                            BB = 0,
                            HBP = 0,
                            K = 0,
                            ParkRunFactor = 0,
                            SP_Perc = 0,
                            RP_Perc = 0,
                        };

                        foreach (var result in o)
                        {
                            opsa.Outs_SP += result.Outs_SP / size;
                            opsa.Outs_RP += result.Outs_RP / size;
                            opsa.GS += result.GS / size;
                            opsa.GR += result.GR / size;
                            opsa.ERA += result.ERA / size;
                            opsa.FIP += result.FIP / size;
                            opsa.HR += result.HR / size;
                            opsa.BB += result.BB / size;
                            opsa.HBP += result.HBP / size;
                            opsa.K += result.K / size;
                            opsa.ParkRunFactor += result.ParkRunFactor / size;
                            opsa.SP_Perc += result.SP_Perc / size;
                            opsa.RP_Perc += result.RP_Perc / size;
                        }

                        items.Add(opsa);
                        progressBar.Tick();
                    }
                }
            }


            using (SqliteDbContext db_write = new(Constants.DB_WRITE_OPTIONS))
            {
                db_write.BulkInsert(items);
            }

            return true;
        }

        public static bool Main()
        {
            try
            {
                if (!ModelAggregation.PlayerWar())
                    return false;

                if (!ModelAggregation.HitterStats())
                    return false;

                if (!ModelAggregation.PitcherStats())
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
