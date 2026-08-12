using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using PitchTrackingDb;
using ShellProgressBar;
using static Db.DbEnums;

namespace PitchAnalysis
{
    // TODO: Adding RunValueSmoothedHitter and Month to PitchData allows for PitchStatcast to not be needed

    internal class PitcherAggregator
    {
        private record YearLeagueDevationKey(int modelId, int year, int balls, int strikes);
        private record PitchValueKey(int gameId, int pitchId);
        private record ModelPitchValueKey(int modelId, int gameId, int pitchId);
        private static Dictionary<YearLeagueDevationKey, YearLeagueDeviations> yldDict = new();
        private record PitchStatcastLookup(int Month, float RunValueSmoothedHitter);

        private static PitchScenario GetScenario(PitchData p)
        {
            PitchScenario scenario = (p.PitIsR == p.HitIsR)
                ? PitchScenario.SameSide
                : PitchScenario.OppSide;

            scenario |= (p.CountStrike >= 2)
                ? PitchScenario.TwoStrikes
                : PitchScenario.NotTwoStrikes;

            if (p.CountStrike > p.CountBalls)
                scenario |= PitchScenario.AheadCount;
            else if (p.CountStrike == p.CountBalls)
                scenario |= PitchScenario.EvenCount;
            else
                scenario |= PitchScenario.BehindCount;

            return scenario;
        }

        private static List<PitcherStuff> GetPitcherYearMonthStuffByScenarios(
            IEnumerable<PitchData> pitches,
            Dictionary<PitchValueKey, PitchValue> pvDict,
            Dictionary<PitchValueKey, PitchStatcastLookup> psDict,
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
                var firstInfo = psDict[new PitchValueKey(firstPitch.GameId, firstPitch.PitchId)];

                var scenarioStats = scenarios.ToDictionary(
                    s => s,
                    s => (
                        0f,
                        new PitcherStuff
                        {
                            MlbId = firstPitch.PitcherId,
                            Year = isSingleGame ? -1 : firstPitch.Year,
                            Month = isSingleGame ? -1 :
                                isFullYear ? 13 : firstInfo.Month,
                            GameId = isSingleGame ? firstPitch.GameId : -1,
                            Model = model,

                            PitchType = firstPitch.PitchType,
                            Scenario = s,

                            NumPitches = 0,
                            ValueActual = 0f,
                            ValueStuff = 0f,
                            ValueCombined = 0f,

                            ActualPlus = 0f,
                            StuffPlus = 0f,
                            PitchPlus = 0f,

                            Vel = 0,
                            BreakHoriz = 0,
                            BreakVert = 0,
                        })
                );

                // Accumulate values
                foreach (var p in pitchGroup)
                {
                    PitchValueKey key = new(p.GameId, p.PitchId);
                    PitchValue pv = pvDict[key];
                    PitchStatcastLookup ps = psDict[key];
                    YearLeagueDevationKey yldKey = new(model, p.Year, p.CountBalls, p.CountStrike);
                    YearLeagueDeviations yld = yldDict[yldKey];
                    PitchScenario pitchScenario = GetScenario(p);
                    foreach (var scen in scenarios)
                    {
                        if (pitchScenario.HasFlag(scen) || scen == PitchScenario.All)
                        {
                            var (count, stats) = scenarioStats[scen];
                            count += yld.StuffDev;
                            stats.NumPitches++;
                            stats.ValueActual += ps.RunValueSmoothedHitter;
                            stats.ValueStuff += pv.StuffRuns;
                            stats.ValueCombined += pv.PitchRuns;
                            stats.Vel += p.Vel;
                            stats.BreakHoriz += p.BreakHorizontal;
                            stats.BreakVert += p.BreakInduced;
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
                        stats.ValueCombined /= stats.NumPitches;

                        // Pitch Metrics
                        stats.Vel /= stats.NumPitches;
                        stats.BreakHoriz /= stats.NumPitches;
                        stats.BreakVert /= stats.NumPitches;

                        // Normalize to Pitch+
                        stats.ActualPlus = 100 - (10 * stats.ValueActual / avgDev);
                        stats.StuffPlus = 100 - (10 * stats.ValueStuff / avgDev);
                        stats.PitchPlus = 100 - (10 * stats.ValueCombined / avgDev);

                        pitchSideBreakdowns.Add(stats);
                    }
                }
            }

            return pitchSideBreakdowns;
        }

        public static void CreateStats(bool forceRefresh, int endYear)
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using PitchTrackingDbContext trackingDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);

            if (forceRefresh)
                pitchDb.PitcherStuff.ExecuteDelete();

            // Preload dictionary
            yldDict = pitchDb.YearLeagueDeviations
                .AsNoTracking()
                .ToDictionary(
                    f => new YearLeagueDevationKey(f.ModelId, f.Year, f.Balls, f.Strikes),
                    f => f
                );
            Console.WriteLine("Loaded YearLeagueDeviations");

            var psDict = db.PitchStatcast
                .AsNoTracking()
                .ToDictionary(
                    f => new PitchValueKey(f.GameId, f.PitchId),
                    f => new PitchStatcastLookup(f.Month, f.RunValueSmoothedHitter));
            Console.WriteLine("Loaded PitchStatcast Data");

            var pvDict = pitchDb.PitchValue
                .AsNoTracking()
                .ToDictionary(f => new ModelPitchValueKey(f.ModelId, f.GameId, f.PitchId), f => f);
            Console.WriteLine("Loaded PitchValue Data");

            // Prepare variants
            List<PitchScenario> scenarios = [
                PitchScenario.All,
                PitchScenario.SameSide, PitchScenario.OppSide,
                PitchScenario.NotTwoStrikes, PitchScenario.TwoStrikes,
                PitchScenario.AheadCount, PitchScenario.EvenCount, PitchScenario.BehindCount
                ];

            // Get Pitches
            var pvKeys = pvDict.Select(f => new PitchValueKey(f.Key.gameId, f.Key.pitchId)).ToHashSet();
            var pitchDataByYear = trackingDb.PitchData
                .AsNoTracking()
                .ToList()
                .Where(f => pvKeys.Contains(new PitchValueKey(f.GameId, f.PitchId))
                    && (forceRefresh || f.Year == endYear))
                .GroupBy(f => f.Year);
            Console.WriteLine("Loaded PitchData");

            // Get year stats
            List<PitcherStuff> stuffList = new();
            List<int> modelIds = pitchDb.PitchValue.Select(f => f.ModelId).Distinct().ToList();
            int totalTicks = modelIds.Count * pitchDataByYear.Count();

            using (ProgressBar progressBar = new ProgressBar(totalTicks, "Creating Pitcher Pitch Stats"))
            {
                foreach (int modelId in modelIds)
                {
                    var modelPvDict = pvDict
                        .Where(f => f.Key.modelId == modelId)
                        .ToDictionary(f => new PitchValueKey(f.Key.gameId, f.Key.pitchId), f => f.Value);

                    foreach (var pdy in pitchDataByYear)
                    {
                        var yearPitchers = pdy.GroupBy(f => f.PitcherId).ToList();
                        foreach (var pitcher in yearPitchers)
                        {
                            // Year Stats
                            stuffList.AddRange(GetPitcherYearMonthStuffByScenarios
                                (
                                    pitcher,
                                    modelPvDict,
                                    psDict,
                                    modelId,
                                    true,
                                    false,
                                    scenarios
                                )
                            );

                            // Month Stats
                            var monthPitcher = pitcher.GroupBy(f => psDict[new PitchValueKey(f.GameId, f.PitchId)].Month);
                            foreach (var mp in monthPitcher)
                            {
                                stuffList.AddRange(GetPitcherYearMonthStuffByScenarios
                                    (
                                        mp,
                                        modelPvDict,
                                        psDict,
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
                                        modelPvDict,
                                        psDict,
                                        modelId,
                                        false,
                                        true,
                                        scenarios
                                    )
                                );
                            }
                        }
                        progressBar.Tick();
                    }
                }
            }

            pitchDb.BulkInsert(stuffList);
        }
    }
}
