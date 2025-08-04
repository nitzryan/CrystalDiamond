using Db;

namespace DataAquisition
{
    internal class SingleGameData
    {
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public int R { get; set; } = 0;
        public int HR { get; set; } = 0;
        public int PA { get; set; } = 0;
        public int Outs { get; set; } = 0;
        public int LeagueId { get; set; }
        public int LevelId { get; set; }

        public SingleGameData Add(SingleGameData sgd)
        {
            R += sgd.R;
            HR += sgd.HR;
            PA += sgd.PA;
            Outs += sgd.Outs;
            return this;
        }
    }

    internal class ParkFactorUpdate
    {
        private const int ROLLING_PERIOD = 3; // Total years to use for park factor (smooths results)
        private const int OUTS_CUTTOFF = 300; // Don't give factors for parks below this level

        static Func<Park_ScoringData, Park_ScoringData, Park_ScoringData> AddParkScoringData = (a, b) => new Park_ScoringData
        {
            TeamId = a.TeamId,
            Year = a.Year,
            LeagueId = a.LeagueId,
            LevelId = a.LevelId,
            HomePa = a.HomePa + b.HomePa,
            HomeOuts = a.HomeOuts + b.HomeOuts,
            HomeRuns = a.HomeRuns + b.HomeRuns,
            HomeHRs = a.HomeHRs + b.HomeHRs,
            AwayPa = a.AwayPa + b.AwayPa,
            AwayOuts = a.AwayOuts + b.AwayOuts,
            AwayRuns = a.AwayRuns + b.AwayRuns,
            AwayHRs = a.AwayHRs + b.AwayHRs,
        };

        public static bool Main(int year)
        {
            try {
                using SqliteDbContext db = new(Constants.DB_OPTIONS);
                // Clear data from current year
                db.Park_Factors.RemoveRange(
                    db.Park_Factors.Where(f => f.Year == year)
                );
                db.Park_ScoringData.RemoveRange(
                    db.Park_ScoringData.Where(f => f.Year == year)
                );
                db.League_Factors.RemoveRange(
                    db.League_Factors.Where(f => f.Year == year)
                );
                db.Level_Factors.RemoveRange(
                    db.Level_Factors.Where(f => f.Year == year)
                );
                db.SaveChanges();

                // Get all games from the current year (use pitcher data because there are less entries)
                var gameLogs = db.Player_Pitcher_GameLog.Where(f => f.Year == year);

                // Get summation data for all games individually
                Dictionary<int, SingleGameData> gameData = new();
                foreach (var log in gameLogs)
                {
                    if (!gameData.ContainsKey(log.GameId))
                    {
                        gameData.Add(log.GameId, new SingleGameData
                        {
                            HomeTeamId = log.HomeTeamId,
                            AwayTeamId = 0,
                            LeagueId = log.LeagueId,
                            LevelId = log.LevelId
                        });
                    }

                    var data = gameData.GetValueOrDefault(log.GameId) ?? throw new Exception("Game data doesn't exist");
                    data.PA += log.BattersFaced;
                    data.Outs += log.Outs;
                    data.HR += log.HR;
                    data.R += log.R;
                    if (data.HomeTeamId != log.TeamId)
                        data.AwayTeamId = log.TeamId;
                }

                // Get sums for each team, splitting home/away
                Dictionary<int, (SingleGameData, SingleGameData)> teamYearSums = new();
                IEnumerable<int> ids = gameData.Values.Select(f => f.AwayTeamId).Distinct();
                foreach (int id in ids)
                {
                    var homeGames = gameData.Values.Where(f => f.HomeTeamId == id);
                    var awayGames = gameData.Values.Where(f => f.AwayTeamId == id);

                    Func<SingleGameData, SingleGameData, SingleGameData> gameReduce = (a, b) => a.Add(b);

                    teamYearSums.Add(id, (homeGames.Aggregate(gameReduce), awayGames.Aggregate(gameReduce)));
                }

                // Insert scoring for individual parks
                foreach (var (id, data) in teamYearSums)
                {
                    db.Park_ScoringData.Add(new Park_ScoringData
                    {
                        TeamId = id,
                        Year = year,
                        LeagueId = data.Item1.LeagueId,
                        LevelId = data.Item1.LevelId,
                        HomePa = data.Item1.PA,
                        HomeOuts = data.Item1.Outs,
                        HomeRuns = data.Item1.R,
                        HomeHRs = data.Item1.HR,
                        AwayPa = data.Item2.PA,
                        AwayOuts = data.Item2.Outs,
                        AwayRuns = data.Item2.R,
                        AwayHRs = data.Item2.HR,
                    });
                }
                db.SaveChanges();

                // Calculate Park Factors
                IEnumerable<int> parkIds = db.Park_ScoringData.Where(f => f.Year == year).Select(f => f.TeamId);
                var parkScoringData = db.Park_ScoringData.Where(f => f.Year <= year && f.Year > year - ROLLING_PERIOD && parkIds.Contains(f.TeamId));
                var teamIds = parkScoringData.Select(f => f.TeamId).Distinct();
                foreach (var teamId in teamIds)
                {
                    var teamParkScoringData = parkScoringData.Where(f => f.TeamId == teamId).OrderByDescending(f => f.Year);
                    var teamData = teamParkScoringData.Aggregate(AddParkScoringData);

                    // Prevents extreme values for teams with little data
                    if (teamData.HomeOuts < OUTS_CUTTOFF || teamData.AwayOuts < OUTS_CUTTOFF)
                        continue;

                    Park_Factors pf = new()
                    {
                        TeamId = teamData.TeamId,
                        LeagueId = teamData.LeagueId,
                        LevelId = teamData.LevelId,
                        Year = year,
                        RunFactor = ((float)teamData.HomeRuns / teamData.HomeOuts) / ((float)teamData.AwayRuns / teamData.AwayOuts),
                        HRFactor = ((float)teamData.HomeHRs / teamData.HomeOuts) / ((float)teamData.AwayHRs / teamData.AwayOuts),
                    };

                    // There is an outlier DSL year that messes with stats, so use less extreme values
                    if (pf.TeamId == 5086 && pf.Year == 2016)
                    {
                        pf.RunFactor = 1.2f;
                        pf.HRFactor = 2.0f;
                    }
                    else if (pf.TeamId == 5086 && pf.Year == 2017)
                    {
                        pf.HRFactor = 2.0f;
                    }

                    db.Park_Factors.Add(pf);
                }
                db.SaveChanges();

                // Accumulation function, shared for level, league
                Func<Park_ScoringData, Park_ScoringData, (float, float)> FactorFunction = (a, b) =>
                    (((float)(a.HomeRuns) / (a.HomeOuts)) / ((float)(b.HomeRuns) / (b.HomeOuts)),
                    ((float)(a.HomeHRs) / (a.HomeOuts)) / ((float)(b.HomeHRs) / (b.HomeOuts)));

                // Calculate Level Factors
                var levels = db.Park_Factors.Where(f => f.Year == year).Select(f => f.LevelId).Distinct();
                Dictionary<int, Park_ScoringData> levelScoringData = [];
                foreach (var level in levels)
                {
                    var levelStats = db.Park_ScoringData.Where(f => f.Year == year && f.LevelId == level)
                        .Aggregate(AddParkScoringData);
                    levelScoringData.Add(level, levelStats);
                }
                var totalLevelStats = levelScoringData.Values.Aggregate(AddParkScoringData);

                foreach (var level in levels)
                {
                    var levelFactors = FactorFunction(levelScoringData.GetValueOrDefault(level) ?? throw new Exception("Key not found"),
                                                    totalLevelStats);
                    db.Level_Factors.Add(new Level_Factors
                    {
                        LevelId = level,
                        Year = year,
                        RunFactor = levelFactors.Item1,
                        HRFactor = levelFactors.Item2
                    });
                }
                db.SaveChanges();

                // Calculate League Factors
                var leagues = db.Park_Factors.Where(f => f.Year == year).Select(f => f.LeagueId).Distinct();
                Dictionary<int, Park_ScoringData> leagueScoringData = [];
                foreach (var league in leagues)
                {
                    var leagueStats = db.Park_ScoringData.Where(f => f.Year == year && f.LeagueId == league)
                        .Aggregate(AddParkScoringData);
                    leagueScoringData.Add(league, leagueStats);
                }
                var totalLeagueStats = leagueScoringData.Values.Aggregate(AddParkScoringData);

                foreach (var league in leagues)
                {
                    var leagueFactors = FactorFunction(leagueScoringData.GetValueOrDefault(league) ?? throw new Exception("Key not found"),
                                                    totalLeagueStats);
                    db.League_Factors.Add(new League_Factors
                    {
                        LeagueId = league,
                        Year = year,
                        RunFactor = leagueFactors.Item1,
                        HRFactor = leagueFactors.Item2
                    });
                }
                db.SaveChanges();
                db.ChangeTracker.Clear();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Calculating Park Factors");
                Console.WriteLine(e.Message);
                Console.Write(e.StackTrace);
                return false;
            }
        }
    }
}
