using Db;
using ShellProgressBar;

namespace DataAquisition
{
    internal class ParkFactorUpdate
    {
        private const int ROLLING_PERIOD = 3; // Total years to use for park factor (smooths results)

        public static bool Main(int year, bool forceOverride)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                
                if (forceOverride)
                {
                    // Clear data from current year
                    db.Park_Factors.RemoveRange(
                        db.Park_Factors.Where(f => f.Year == year)
                    );
                    db.SaveChanges();
                }

                if (db.Park_Factors.Any(f => f.Year == year))
                    return true;

                // Get all games from the current year (use pitcher data because there are less entries)
                var gameLogs = db.Player_Pitcher_GameLog.Where(f => f.Year == year)
                    .OrderBy(f => f.StadiumId);
                var rollingGameLogs = db.Player_Pitcher_GameLog.Where(f => f.Year <= year && f.Year >= (year - ROLLING_PERIOD))
                    .OrderBy(f => f.StadiumId);

                var stadiums = gameLogs.Select(f => f.StadiumId).Distinct();
                using (ProgressBar progressBar = new ProgressBar(stadiums.Count(), $"Generating Park Factors for {year}"))
                {
                    foreach (int stadiumId in stadiums)
                    {
                        // Get Games at and away from stadium
                        var homeGames = rollingGameLogs.Where(f => f.StadiumId == stadiumId);
                        // Find teams that played home game at stadium 
                        var thisYearHomeGames = gameLogs.Where(f => f.StadiumId == stadiumId && f.IsHome == 1);
                        IEnumerable<int> stadiumTeamIds = thisYearHomeGames.Select(f => f.TeamId).Distinct();
                        
                        int awayOuts = 0;
                        int awayR = 0;
                        int awayHR = 0;
                        foreach (int teamId in stadiumTeamIds) // All results involving team not at stadium
                        {
                            var awayGames = rollingGameLogs.Where(f => f.OppTeamId == teamId || f.TeamId == teamId && f.StadiumId != stadiumId);
                            awayOuts += awayGames.Select(f => f.Outs).Sum();
                            awayR += awayGames.Select(f => f.R).Sum();
                            awayHR += awayGames.Select(f => f.HR).Sum();
                        }

                        int homeOuts = homeGames.Select(f => f.Outs).Sum();
                        int homeR = homeGames.Select(f => f.R).Sum();
                        int homeHR = homeGames.Select(f => f.HR).Sum();
                        

                        // Shift values towards neutral depending on amount of data that exists
                        // Based on https://library.fangraphs.com/park-factors-5-year-regressed/
                        // Scales by 0.6 based on results from 1 year, than 0.1 for each additional year
                        int minOuts = Math.Min(homeOuts, awayOuts);
                        const int FULL_SEASON_OUTS = 4300;
                        float regressionFactor = minOuts < FULL_SEASON_OUTS ?
                            0.6f * minOuts / FULL_SEASON_OUTS :
                            0.6f + (0.1f * (minOuts - FULL_SEASON_OUTS) / FULL_SEASON_OUTS);

                        // Cap at 1.0f
                        regressionFactor = Math.Min(regressionFactor, 1.0f);
                        float runFactor = minOuts > 0 ? (regressionFactor * ((float)homeR / homeOuts) / ((float)awayR / awayOuts)) + (1 - regressionFactor) : 1.0f;
                        float hrFactor = minOuts > 0 ? (regressionFactor * ((float)homeHR / homeOuts) / ((float)awayHR / awayOuts)) + (1 - regressionFactor) : 1.0f;

                        Park_Factors pf = new()
                        {
                            StadiumId = stadiumId,
                            LeagueId = thisYearHomeGames.First().LeagueId,
                            LevelId = thisYearHomeGames.First().LevelId,
                            Year = year,
                            RunFactor = runFactor, 
                            HRFactor = hrFactor
                        };
                        db.Park_Factors.Add(pf);

                        progressBar.Tick();
                    }
                }
                    
                db.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Calculating Park Factors");
                Utilities.LogException(e);
                return false;
            }
        }
    }
}
