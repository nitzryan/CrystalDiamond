using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using ShellProgressBar;
using System.Text.Json;
using static Db.DbEnums;

namespace PitchAnalysis
{
    internal class PitcherAggregator
    {
        private record YearLeagueDevationKey(int modelId, int year, int balls, int strikes);

        private static Dictionary<YearLeagueDevationKey, YearLeagueDeviations> yldDict = new();

        private static List<PitcherStuff> GetPitcherYearMonthStuffByScenarios(
            IEnumerable<PitchStatcast> pitches,
            int model,
            bool isFullYear,
            bool isSingleGame,
            IEnumerable<PitchScenario> scenarios)
        {
            List<PitcherStuff> pitchSideBreakdowns = new();

            // Group by PitchType
            var pitchGroupings = pitches.GroupBy(f => f.PitchType);

            foreach (var pitchGroup in pitchGroupings)
            {
                var firstPitch = pitchGroup.First();

                var scenarioStats = scenarios.ToDictionary(
                    s => s,
                    s => (
                        0f,
                        new PitcherStuff
                        {
                            MlbId = firstPitch.PitcherId,
                            Year = isSingleGame ? -1 : firstPitch.Year,
                            Month = isSingleGame ? -1 :
                                isFullYear ? 13 : firstPitch.Month,
                            GameId = isSingleGame ? firstPitch.GameId : -1,
                            Model = model,

                            PitchType = firstPitch.PitchType,
                            Scenario = s,

                            NumPitches = 0,
                            ValueActual = 0f,
                            ValueStuff = 0f,
                            ValueLoc = 0f,
                            ValueCombined = 0f,

                            ActualPlus = 0f,
                            StuffPlus = 0f,
                            LocationPlus = 0f,
                            PitchPlus = 0f,

                            Vel = 0,
                            BreakHoriz = 0,
                            BreakVert = 0,
                        })
                );

                // Accumulate values
                foreach (var p in pitchGroup)
                {
                    if (p.ModelStuff == null || p.ModelLocation == null || p.ModelPitch == null)
                        continue;

                    YearLeagueDevationKey yldKey = new(model, p.Year, p.CountBalls, p.CountStrike);
                    YearLeagueDeviations yld = yldDict[yldKey];

                    foreach (var scen in scenarios)
                    {
                        if (p.Scenario.HasFlag(scen) || scen == PitchScenario.All)
                        {
                            var (count, stats) = scenarioStats[scen];

                            count += yld.StuffDev;

                            stats.NumPitches++;
                            stats.ValueActual += p.RunValueSmoothedHitter;
                            stats.ValueStuff += p.ModelStuff.Value;
                            stats.ValueLoc += p.ModelLocation.Value;
                            stats.ValueCombined += p.ModelPitch.Value;

                            #pragma warning disable CS8629 // Will be not null if put through model
                            stats.Vel += (float)p.VStart;
                            stats.BreakHoriz += (float)p.BreakHorizontal;
                            stats.BreakVert += (float)p.BreakInduced;
                            #pragma warning restore CS8629

                            scenarioStats[scen] = (count, stats);
                        }
                    }
                }

                // Convert sums to per-pitch averages
                foreach (var s in scenarioStats.Values)
                {
                    var stats = s.Item2;
                    
                    if (stats.NumPitches > 0)
                    {
                        float avgDev = s.Item1 / stats.NumPitches;

                        // Value Per Pitch
                        stats.ValueActual /= stats.NumPitches;
                        stats.ValueStuff /= stats.NumPitches;
                        stats.ValueLoc /= stats.NumPitches;
                        stats.ValueCombined /= stats.NumPitches;

                        // Pitch Metrics
                        stats.Vel /= stats.NumPitches;
                        stats.BreakHoriz /= stats.NumPitches;
                        stats.BreakVert /= stats.NumPitches;

                        // Normalize to Pitch+
                        stats.ActualPlus = 100 - (10 * stats.ValueActual / avgDev);
                        stats.StuffPlus = 100 - (10 * stats.ValueStuff / avgDev);
                        stats.LocationPlus = 100 - (10 * stats.ValueLoc / avgDev);
                        stats.PitchPlus = 100 - (10 * stats.ValueCombined / avgDev);

                        pitchSideBreakdowns.Add(stats);
                    }
                }
            }

            return pitchSideBreakdowns;
        }

        public static void CreateStats()
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            pitchDb.PitcherStuff.ExecuteDelete();

            yldDict = pitchDb.YearLeagueDeviations
                .ToDictionary(
                    f => new YearLeagueDevationKey(f.ModelId, f.Year, f.Balls, f.Strikes),
                    f => f
                );

            List<PitcherStuff> stuffList = new();

            List<PitchScenario> scenarios = [
                PitchScenario.All,
                PitchScenario.SameSide, PitchScenario.OppSide,
                PitchScenario.NotTwoStrikes, PitchScenario.TwoStrikes,
                PitchScenario.DoublePlayOpp, PitchScenario.NonDoublePlayOpp,
                PitchScenario.AheadCount, PitchScenario.EvenCount, PitchScenario.BehindCount
                ];
            List<int> modelIds = pitchDb.Output_PitchValue.Select(f => f.Model).Distinct().ToList();

            // Get year stats
            var pitches = db.PitchStatcast.Where(f => f.ModelStuff != null).AsNoTracking();
            var pitchDataYears = pitches.GroupBy(f => f.Year);
            using (ProgressBar progressBar = new ProgressBar(pitchDataYears.Count(), "Creating Pitcher Pitch Stats"))
            {
                foreach (var pdy in pitchDataYears)
                {
                    var yearPitchers = pdy.GroupBy(f => f.PitcherId).ToList();
                    using (ChildProgressBar pitcherChild = progressBar.Spawn(yearPitchers.Count, $"Creating Pitch Stats For Year {pdy.Key}"))
                    {
                        foreach (var pitcher in yearPitchers)
                        {
                            foreach (int modelId in modelIds)
                            {
                                // Year Stats
                                stuffList.AddRange(GetPitcherYearMonthStuffByScenarios
                                    (
                                        pitcher,
                                        modelId,
                                        true,
                                        false,
                                        scenarios
                                    )
                                );

                                // Month Stats
                                var monthPitcher = pitcher.GroupBy(f => f.Month);
                                foreach (var mp in monthPitcher)
                                {
                                    stuffList.AddRange(GetPitcherYearMonthStuffByScenarios
                                        (
                                            mp,
                                            modelId,
                                            false,
                                            false,
                                            scenarios
                                        )
                                    );
                                }

                                // Game stats
                                var gamePitcher = pitcher.GroupBy(f => f.GameId);
                                foreach (var gp in gamePitcher)
                                {
                                    stuffList.AddRange(GetPitcherYearMonthStuffByScenarios
                                        (
                                            gp,
                                            modelId,
                                            false,
                                            true,
                                            scenarios
                                        )
                                    );
                                }
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
