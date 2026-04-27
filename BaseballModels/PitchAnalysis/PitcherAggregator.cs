using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using ShellProgressBar;
using static PitchDb.DbEnums;

namespace PitchAnalysis
{
    internal class PitcherAggregator
    {
        private record PitchData
        {
            public required int gameId;
            public required int pitcherId;
            public required int year;
            public required int month;
            public required int model;

            public required Scenario scenario;
            public required Db.DbEnums.PitchType pitchType;

            public required float absValue;
            public required float stuffValue;
            public required float locValue;
            public required float combinedValue;

            public required float vel;
            public required float breakHoriz;
            public required float breakVert;
        }

        private static List<PitchData> GetPitchData(IEnumerable<Output_PitchValueAggregation> opvas)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            int count = opvas.Count();
            List<PitchData> pd = new(count);
            using (ProgressBar progressBar = new ProgressBar(count, $"Creating PitchData"))
            {
                foreach (var opva in opvas)
                {
                    var pitchStatcast = db.PitchStatcast.Where(f => f.GameId == opva.GameId && f.PitchId == opva.PitchId).Single();

                    Scenario scenario = Scenario.All;
                    // Same/Opp Side
                    if (pitchStatcast.PitIsR == pitchStatcast.HitIsR)
                        scenario |= Scenario.SameSide;
                    else
                        scenario |= Scenario.OppSide;

                    // Two/Not Two strikes
                    if (pitchStatcast.CountStrike == 2)
                        scenario |= Scenario.TwoStrikes;
                    else
                        scenario |= Scenario.NotTwoStrikes;

                    // Double Play Opportunity
                    if (pitchStatcast.BaseOccupancy.HasFlag(Db.DbEnums.BaseOccupancy.B1) && pitchStatcast.Outs < 2)
                        scenario |= Scenario.DoublePlayOpp;
                    else
                        scenario |= Scenario.NonDoublePlayOpp;

                    // Balance of count
                    if (pitchStatcast.CountBalls > pitchStatcast.CountStrike)
                        scenario |= Scenario.BehindCount;
                    else if (pitchStatcast.CountBalls < pitchStatcast.CountStrike)
                        scenario |= Scenario.AheadCount;
                    else
                        scenario |= Scenario.EvenCount;

                    #pragma warning disable CS8629 // Any null values for pitch statcast data would not have been run through model
                    pd.Add(new PitchData
                    {
                        gameId = pitchStatcast.GameId,
                        pitcherId = pitchStatcast.PitcherId,
                        year = pitchStatcast.Year,
                        month = pitchStatcast.Month,
                        model = opva.Model,

                        scenario = scenario,
                        pitchType = pitchStatcast.PitchType,

                        absValue = opva.AbsValue,
                        stuffValue = opva.StuffOnly,
                        locValue = opva.LocationOnly,
                        combinedValue = opva.Combined,

                        vel = pitchStatcast.VStart.Value,
                        breakVert = pitchStatcast.BreakInduced.Value,
                        breakHoriz = pitchStatcast.BreakHorizontal.Value,
                    });
                    #pragma warning restore CS8629

                    progressBar.Tick();
                }
            }

            return pd;
        }

        private static List<PitcherStuff> GetPitcherYearMonthStuffByScenarios(
            IEnumerable<PitchData> pitches,
            bool isFullYear,
            bool isSingleGame,
            IEnumerable<Scenario> scenarios)
        {
            List<PitcherStuff> pitchSideBreakdowns = new();

            // Group by PitchType
            var pitchGroupings = pitches.GroupBy(f => f.pitchType);

            foreach (var pitchGroup in pitchGroupings)
            {
                var firstPitch = pitchGroup.First();

                var scenarioStats = scenarios.ToDictionary(
                    s => s,
                    s => new PitcherStuff
                    {
                        MlbId = firstPitch.pitcherId,
                        Year = isSingleGame ? -1 : firstPitch.year,
                        Month = isSingleGame ? -1 :
                            isFullYear ? 13 : firstPitch.month,
                        GameId = isSingleGame ? firstPitch.gameId : -1,

                        PitchType = firstPitch.pitchType,
                        Scenario = s,

                        NumPitches = 0,
                        ValueStuff = 0f,
                        ValueLoc = 0f,
                        ValueCombined = 0f,

                        Vel = 0,
                        BreakHoriz = 0,
                        BreakVert = 0,
                    });

                // Accumulate values
                foreach (var p in pitchGroup)
                {
                    foreach (var scen in scenarios)
                    {
                        if (p.scenario.HasFlag(scen) || scen == Scenario.All)
                        {
                            var pointer = scenarioStats[scen];

                            pointer.NumPitches++;
                            pointer.ValueStuff += p.stuffValue;
                            pointer.ValueLoc += p.locValue;
                            pointer.ValueCombined += p.combinedValue;

                            pointer.Vel += p.vel;
                            pointer.BreakHoriz += p.breakHoriz;
                            pointer.BreakVert += p.breakVert;
                        }
                    }
                }

                // Convert sums to per-pitch averages
                foreach (var stats in scenarioStats.Values)
                {
                    if (stats.NumPitches > 0)
                    {
                        stats.ValueStuff /= stats.NumPitches;
                        stats.ValueLoc /= stats.NumPitches;
                        stats.ValueCombined /= stats.NumPitches;

                        stats.Vel /= stats.NumPitches;
                        stats.BreakHoriz /= stats.NumPitches;
                        stats.BreakVert /= stats.NumPitches;
                    }
                    pitchSideBreakdowns.Add(stats);
                }
            }

            return pitchSideBreakdowns;
        }

        public static void CreateStats()
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);

            pitchDb.PitcherStuff.ExecuteDelete();

            List<PitcherStuff> stuffList = new();
            List<Scenario> scenarios = [
                Scenario.All,
                Scenario.SameSide, Scenario.OppSide,
                Scenario.NotTwoStrikes, Scenario.TwoStrikes,
                Scenario.DoublePlayOpp, Scenario.NonDoublePlayOpp,
                Scenario.AheadCount, Scenario.EvenCount, Scenario.BehindCount
                ];

            // Get pitchData
            var pitchDataList = GetPitchData(pitchDb.Output_PitchValueAggregation);

            // Get year stats
            var pitchDataYears = pitchDataList.GroupBy(f => f.year);
            using (ProgressBar progressBar = new ProgressBar(pitchDataYears.Count(), "Creating Pitcher Pitch Stats"))
            {
                foreach (var pdy in pitchDataYears)
                {
                    var yearPitchers = pdy.GroupBy(f => f.pitcherId);
                    using (ChildProgressBar pitcherChild = progressBar.Spawn(yearPitchers.Count(), $"Creating Pitch Stats For Year {pdy.Key}"))
                    {
                        foreach (var pitcher in yearPitchers)
                        {
                            // Year Stats
                            stuffList.AddRange(GetPitcherYearMonthStuffByScenarios
                                (
                                    pitcher,
                                    true,
                                    false,
                                    scenarios
                                )
                            );

                            // Month Stats
                            var monthPitcher = pitcher.GroupBy(f => f.month);
                            foreach (var mp in monthPitcher)
                            {
                                stuffList.AddRange(GetPitcherYearMonthStuffByScenarios
                                    (
                                        mp,
                                        false,
                                        false,
                                        scenarios
                                    )
                                );
                            }

                            // Game stats
                            var gamePitcher = pitcher.GroupBy(f => f.gameId);
                            foreach (var gp in gamePitcher)
                            {
                                stuffList.AddRange(GetPitcherYearMonthStuffByScenarios
                                    (
                                        gp,
                                        false,
                                        true,
                                        scenarios
                                    )
                                );
                            }

                            pitcherChild.Tick();
                        }
                    }

                    progressBar.Tick();
                }
            }

            pitchDb.BulkInsert(stuffList);
        }
    }
}
