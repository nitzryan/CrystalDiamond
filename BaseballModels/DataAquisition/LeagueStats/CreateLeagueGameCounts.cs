using Db;
using ShellProgressBar;

namespace DataAquisition.LeagueStats
{
    internal class CreateLeagueGameCounts
    {
        public static void Update(int year, int month)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                db.RemoveRange(db.League_GameCounts.Where(f => f.Year == year && f.Month == month));
                db.SaveChanges();

                var monthGames = db.Player_Hitter_GameLog.Where(f => f.Year == year && f.Month == month);
                var leagues = monthGames.Select(f => f.LeagueId).Distinct();
                using (ProgressBar progressBar = new(leagues.Count(), $"Generating League_GameCounts for {year}-{month}"))
                {
                    foreach (int leagueId in leagues)
                    {
                        var leagueGames = monthGames.Where(f => f.LeagueId == leagueId);
                        int maxPA = 0;
                        if (leagueGames.Any())
                        {
                            var levelGroups = leagueGames.GroupBy(f => f.MlbId).Select(f => f.Sum(p => p.PA));
                            maxPA = levelGroups.Max(f => f);
                        }

                        db.League_GameCounts.Add(new League_GameCounts
                        {
                            LeagueId = leagueId,
                            Year = year,
                            Month = month,
                            MaxPA = maxPA
                        });

                        progressBar.Tick();
                    }
                }

                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in CreateLevelGameCounts");
                Utilities.LogException(e);
                throw;
            }
        }
    }
}
