using Db;
using ShellProgressBar;

namespace DataAquisition
{
    internal class ModelPlayerWar
    {
        public static bool Main()
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.Model_PlayerWar.RemoveRange(db.Model_PlayerWar);
                db.SaveChanges();

                using (ProgressBar progressBar = new ProgressBar(db.Model_Players.Count(), $"Creating Model_PlayerWar"))
                {
                    foreach (var mp in db.Model_Players)
                    {
                        var pw = db.Player_YearlyWar.Where(f => f.MlbId == mp.MlbId && f.Year <= mp.LastMLBSeason);

                        foreach (var p in pw)
                        {
                            db.Model_PlayerWar.Add(new Model_PlayerWar
                            {
                                MlbId = mp.MlbId,
                                Year = p.Year,
                                IsHitter = p.IsHitter,
                                PA = p.PA,
                                WAR = p.WAR,
                                OFF = p.OFF,
                                DEF = p.DEF,
                                BSR = p.BSR
                            });
                        }
                        progressBar.Tick();
                    }
                }
                db.SaveChanges();

                return true;
            } catch (Exception e)
            {
                Console.WriteLine("Error in ModelPlayerWar");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
