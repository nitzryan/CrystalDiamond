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
                                Prob0 = 0,
                                Prob1 = 0,
                                Prob2 = 0,
                                Prob3 = 0,
                                Prob4 = 0,
                                Prob5 = 0,
                                Prob6 = 0
                            };

                            foreach (var result in model_results)
                            {
                                owa.Prob0 += result.Prob0 / size;
                                owa.Prob1 += result.Prob1 / size;
                                owa.Prob2 += result.Prob2 / size;
                                owa.Prob3 += result.Prob3 / size;
                                owa.Prob4 += result.Prob4 / size;
                                owa.Prob5 += result.Prob5 / size;
                                owa.Prob6 += result.Prob6 / size;
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
