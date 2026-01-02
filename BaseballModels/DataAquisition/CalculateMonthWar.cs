using Db;
using ShellProgressBar;

namespace DataAquisition
{
    internal class CalculateMonthWar
    {
        public static bool Update(int year, int month)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                var phma = db.Player_Hitter_MonthAdvanced.Where(f => f.Year == year && f.Month == month);

                Dictionary<int, LeagueStats> leagueDict = new();
                foreach (int leagueId in phma.Select(f => f.LeagueId).Distinct())
                {
                    leagueDict.Add(leagueId, db.LeagueStats.Where(f => f.Year == year && f.LeagueId == leagueId).Single());
                }

                using (ProgressBar progressBar = new ProgressBar(phma.Count(), $"Calculating Hitter War for month={month} Year={year}"))
                {
                    foreach (var ma in phma)
                    {
                        var fieldStats = db.Player_Fielder_MonthStats.Where(f => f.MlbId == ma.MlbId && f.Month == ma.Month && f.Year == ma.Year && f.TeamId == ma.TeamId);

                        float crDef = fieldStats.Sum(f => f.ScaledDRAA + f.PosAdjust);

                        float crBsr = db.Player_Hitter_MonthBaserunning.Where(f => f.MlbId == ma.MlbId && f.Month == ma.Month && f.Year == ma.Year && f.TeamId == ma.TeamId)
                            .Sum(f => f.RBSR);

                        float runsAboveReplacement = ma.CrOFF + ma.CrREP + crDef + crBsr;
                        ma.CrWAR = runsAboveReplacement / leagueDict[ma.LeagueId].RPerWin;

                        progressBar.Tick();
                    }
                }
                db.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in CalculateMonthlyWar");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
