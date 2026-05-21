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
                    s => new PitcherStuff
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
                    var modelOutput = JsonSerializer.Deserialize<Dictionary<int, (double, double, double, double)>>(p.ModelOutput);
                    if (modelOutput == null)
                        throw new Exception($"Failed to deserialize modelOutput for PitchId={p.PitchId}");

                    if (!modelOutput.ContainsKey(model)) // Some pitches may be in all model but in none of the specific pitch models
                        continue;

                    (double absValue, double location, double stuff, double combined) = modelOutput[model];

                    foreach (var scen in scenarios)
                    {
                        if (p.Scenario.HasFlag(scen) || scen == PitchScenario.All)
                        {
                            var pointer = scenarioStats[scen];

                            pointer.NumPitches++;
                            pointer.ValueStuff += (float)stuff;
                            pointer.ValueLoc += (float)location;
                            pointer.ValueCombined += (float)combined;

                            #pragma warning disable CS8629 // Will be not null if put through model
                            pointer.Vel += (float)p.VStart;
                            pointer.BreakHoriz += (float)p.BreakHorizontal;
                            pointer.BreakVert += (float)p.BreakInduced;
                            #pragma warning restore CS8629
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
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            pitchDb.PitcherStuff.ExecuteDelete();

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
            var pitches = db.PitchStatcast.Where(f => f.ModelOutput != "").AsNoTracking();
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
