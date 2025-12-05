using Db;
using ShellProgressBar;

namespace DataAquisition
{
    internal class CreateLevelGameCounts
    {
        public static bool Main(int year, int month)
        {
            try
            {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                db.RemoveRange(db.Level_GameCounts.Where(f => f.Year == year && f.Month == month));
                db.SaveChanges();

                var monthGames = db.Player_Hitter_GameLog.Where(f => f.Year == year && f.Month == month);
                var levels = monthGames.Select(f => f.LevelId).Distinct();
                using (ProgressBar progressBar = new(levels.Count(), $"Generating LevelGameCounts for {year}-{month}"))
                {
                    foreach (int levelId in levels)
                    {
                        var levelGames = monthGames.Where(f => f.LevelId == levelId);
                        int maxPA = 0;
                        if (levelGames.Any())
                        {
                            var levelGroups = levelGames.GroupBy(f => f.MlbId).Select(f => f.Sum(p => p.PA));
                            maxPA = levelGroups.Max(f => f);
                        }

                        db.Level_GameCounts.Add(new Level_GameCounts
                        {
                            LevelId = levelId,
                            Year = year,
                            Month = month,
                            MaxPA = maxPA
                        });

                        progressBar.Tick();
                    }
                }

                db.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in CreateLevelGameCounts");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
