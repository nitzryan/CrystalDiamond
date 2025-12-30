using Db;
using EFCore.BulkExtensions;
using ShellProgressBar;
using static Db.DbEnums;

namespace DataAquisition
{
    internal class GetPlayByPlayFlags
    {
        // https://beanumber.github.io/abdwr3e/C_statcast.html#fig-spray-diagram Shows coordinates
        
        // Check if most of the events are happening further in +X than expected.  Some MiLB games are shifted by >40 units
        private static GameFlags IsGameSkewed(IEnumerable<GamePlayByPlay> pbp)
        {
            pbp = pbp.Where(f => f.HitCoordX != null);
        
            float[] positionExpectedXs = [125, 125, 160, 145, 90, 110, 60, 125, 190];
            float[] positionCutoffXs = positionExpectedXs.Select(f => f + 30).ToArray();
            float[] positionMeans = [0, 0, 0, 0, 0, 0, 0, 0, 0];
            for (int i = 1; i < 10; i++)
            {
                var hitsInZone = pbp.Where(f => f.HitZone == i);
                if (!hitsInZone.Any())
                {
                    positionMeans[i - 1] = positionExpectedXs[i - 1];
                    continue;
                }

                // Ignore null warning, removed at beginning of function
                positionMeans[i - 1] = hitsInZone.Sum(f => (float)f.HitCoordX) / hitsInZone.Count();
            }

            int numExceeding = 0;
            for (int i = 0; i < positionMeans.Length; i++)
                if (positionMeans[i] > positionCutoffXs[i])
                    numExceeding++;

            return numExceeding >= 5 ? GameFlags.OutputsSkewed : GameFlags.Valid;
        }

        private static GameFlags IsHitPositionImpossible(GamePlayByPlay e)
        {
            if (e.HitCoordX == null)
                return GameFlags.Valid;

            if (e.HitCoordY == null)
                return GameFlags.Valid;

            return ((float)e.HitCoordY + MathF.Abs((float)e.HitCoordX - 125) < 230) ?
                GameFlags.Valid : GameFlags.HitPositionImpossible;
        }

        private static GameFlags IsHitPositionTooFarRight2017(GamePlayByPlay e)
        {
            if (e.LeagueId != 103 && e.LeagueId != 104) // Only in major leagues
                return GameFlags.Valid;

            if (e.Year != 2017)
                return GameFlags.Valid;

            if (e.HitCoordX >= 248.0)
                return GameFlags.HitPositionTooRight2017;

            return GameFlags.Valid;
        }

        public static bool UpdateFlags(int year)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);

                // Remove game flags for current year
                foreach (var item in db.GamePlayByPlay.Where(f => f.Year == year))
                {
                    item.EventFlag = null;
                }
                db.SaveChanges();

                var thisYearPBP = db.GamePlayByPlay.Where(f => f.Year == year).GroupBy(f => f.GameId).AsEnumerable();
                using (ProgressBar progressBar = new(thisYearPBP.Count(), $"Generating GamePlayByPlayGameFlags for {year}"))
                {
                    foreach (var game in thisYearPBP)
                    {
                        GameFlags gameFlag = IsGameSkewed(game);

                        foreach (var e in game)
                        {
                            GameFlags eventFlag = GameFlags.Valid;
                            eventFlag |= IsHitPositionImpossible(e); // Field position would be in the stands
                            eventFlag |= IsHitPositionTooFarRight2017(e); // First year of statcast data, some messups
                            e.EventFlag = eventFlag | gameFlag;
                        }

                        
                        progressBar.Tick();
                    }
                }

                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in GetPlayByPlayFlags");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
