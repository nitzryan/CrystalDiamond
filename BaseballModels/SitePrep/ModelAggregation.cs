using Db;
using ShellProgressBar;

namespace SitePrep
{
    internal class ModelAggregation
    {
        public static bool Main()
        {
            try
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
                                Value0 = 0,
                                Value1 = 0,
                                Value2 = 0,
                                Value3 = 0,
                                Value4 = 0,
                                Value5 = 0,
                                Value6 = 0,
                                Value = 0,
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
                                owa.Value0 += result.Value0 / size;
                                owa.Value1 += result.Value1 / size;
                                owa.Value2 += result.Value2 / size;
                                owa.Value3 += result.Value3 / size;
                                owa.Value4 += result.Value4 / size;
                                owa.Value5 += result.Value5 / size;
                                owa.Value6 += result.Value6 / size;
                                owa.Value += result.Value / size;
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
            catch (Exception e)
            {
                Console.WriteLine("Error in ModelAggregation");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
