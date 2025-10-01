using Db;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;

namespace DataAquisition
{
    internal class Model_MonthValue
    {
        public static bool Main()
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                db.Model_HitterValue.RemoveRange(db.Model_HitterValue);
                db.Model_PitcherValue.RemoveRange(db.Model_PitcherValue);
                db.SaveChanges();
                db.ChangeTracker.Clear();

                var hitterIds = db.Model_HitterStats.AsEnumerable().DistinctBy(f => f.MlbId).Select(f => f.MlbId);

                using (ProgressBar progressBar = new(hitterIds.Count(), "Creating Month value stats for hitters"))
                {
                    foreach (var id in hitterIds)
                    {
                        var mhs = db.Model_HitterStats.Where(f => f.MlbId == id).AsNoTracking().AsEnumerable();
                        var pmw = db.Player_MonthlyWar.Where(f => f.MlbId == id && f.IsHitter == 1).AsNoTracking().AsEnumerable();

                        foreach (var m in mhs)
                        {
                            int month = m.Month;
                            int year = m.Year;

                            var warStats1Year = pmw.Where(f => (f.Year == year && f.Month > month) || (f.Year == (year + 1) && f.Month <= month));
                            var warStats2Year = pmw.Where(f => (f.Year == (year + 1) && f.Month > month) || (f.Year == (year + 2) && f.Month <= month));
                            var warStats3Year = pmw.Where(f => (f.Year == (year + 2) && f.Month > month) || (f.Year == (year + 3) && f.Month <= month));

                            db.Model_HitterValue.Add(new Model_HitterValue
                            {
                                MlbId = id,
                                Year = m.Year,
                                Month = m.Month,
                                War1Year = warStats1Year.Sum(f => f.WAR_h),
                                War2Year = warStats2Year.Sum(f => f.WAR_h),
                                War3Year = warStats3Year.Sum(f => f.WAR_h),
                                Off1Year = warStats1Year.Sum(f => f.OFF),
                                Off2Year = warStats2Year.Sum(f => f.OFF),
                                Off3Year = warStats3Year.Sum(f => f.OFF),
                                Bsr1Year = warStats1Year.Sum(f => f.BSR),
                                Bsr2Year = warStats2Year.Sum(f => f.BSR),
                                Bsr3Year = warStats3Year.Sum(f => f.BSR),
                                Def1Year = warStats1Year.Sum(f => f.DEF),
                                Def2Year = warStats2Year.Sum(f => f.DEF),
                                Def3Year = warStats3Year.Sum(f => f.DEF),
                                Rep1Year = warStats1Year.Sum(f => f.REP),
                                Rep2Year = warStats2Year.Sum(f => f.REP),
                                Rep3Year = warStats3Year.Sum(f => f.REP),
                                Pa1Year = warStats1Year.Sum(f => f.PA),
                                Pa2Year = warStats2Year.Sum(f => f.PA),
                                Pa3Year = warStats3Year.Sum(f => f.PA),
                            });
                        }

                        progressBar.Tick();
                    }
                }
                db.SaveChanges();
                db.ChangeTracker.Clear();

                //////////////////// PITCHERS ///////////////////////////////

                var pitcherIds = db.Model_PitcherStats.AsEnumerable().DistinctBy(f => f.MlbId).Select(f => f.MlbId);

                using (ProgressBar progressBar = new(pitcherIds.Count(), "Creating Month value stats for pitchers"))
                {
                    foreach (var id in pitcherIds)
                    {
                        var mhs = db.Model_PitcherStats.Where(f => f.MlbId == id).AsNoTracking().AsEnumerable();
                        var pmw = db.Player_MonthlyWar.Where(f => f.MlbId == id && f.IsHitter == 0).AsNoTracking().AsEnumerable();

                        foreach (var m in mhs)
                        {
                            int month = m.Month;
                            int year = m.Year;

                            var warStats1Year = pmw.Where(f => (f.Year == year && f.Month > month) || (f.Year == year + 1 && f.Month <= month));
                            var warStats2Year = pmw.Where(f => (f.Year == year + 1 && f.Month > month) || (f.Year == year + 2 && f.Month <= month));
                            var warStats3Year = pmw.Where(f => (f.Year == year + 2 && f.Month > month) || (f.Year == year + 3 && f.Month <= month));

                            db.Model_PitcherValue.Add(new Model_PitcherValue
                            {
                                MlbId = id,
                                Year = m.Year,
                                Month = m.Month,
                                WarSP1Year = warStats1Year.Sum(f => f.WAR_s),
                                WarSP2Year = warStats2Year.Sum(f => f.WAR_s),
                                WarSP3Year = warStats3Year.Sum(f => f.WAR_s),
                                WarRP1Year = warStats1Year.Sum(f => f.WAR_r),
                                WarRP2Year = warStats2Year.Sum(f => f.WAR_r),
                                WarRP3Year = warStats3Year.Sum(f => f.WAR_r),
                                IPSP1Year = warStats1Year.Sum(f => f.IP_SP),
                                IPSP2Year = warStats2Year.Sum(f => f.IP_SP),
                                IPSP3Year = warStats3Year.Sum(f => f.IP_SP),
                                IPRP1Year = warStats1Year.Sum(f => f.IP_RP),
                                IPRP2Year = warStats2Year.Sum(f => f.IP_RP),
                                IPRP3Year = warStats3Year.Sum(f => f.IP_RP),
                            });
                        }

                        progressBar.Tick();
                    }
                }
                db.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Model_MonthValue");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
