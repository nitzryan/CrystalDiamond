using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using PitchTrackingDb;
using ShellProgressBar;

namespace PitchAnalysis
{
    internal class MonthStats
    {
        private record YearLeagueDevationKey(int modelId, int year, int balls, int strikes);
        private record PitchMonthKey(int gameId, int pitchId);
        private record PitchStatcastMonthLookup(int Month, float RunValueSmoothedHitter);
        private record PitchModelInput(int Year, int Month, int PitcherId, DbEnums.PitchType PitchType, int Balls, int Strikes, float ModelStuff, float ModelPitch, float Actual);
        private record PitchModelOutput(float? stuff, float? pitch, float? actual, int numPitches);
        
        private static PitchModelOutput GetPitchModelOutput(IEnumerable<PitchModelInput> pitches, Dictionary<YearLeagueDevationKey, YearLeagueDeviations> YldDict, int modelId)
        {
            int count = pitches.Count();
            if (count == 0)
                return new PitchModelOutput(null, null, null, 0);
        
            double dev = 0;
            foreach (var pitch in pitches)
            {
                dev += YldDict[new YearLeagueDevationKey(modelId, pitch.Year, pitch.Balls, pitch.Strikes)].StuffDev;
            }

            float stuffValue = pitches.Sum(f => f.ModelStuff);
            float pitchValue = pitches.Sum(f => f.ModelPitch);
            float actualValue = pitches.Sum(f => f.Actual);

            float stuffPlus = 100 - (float)(10 * stuffValue / dev);
            float pitchPlus = 100 - (float)(10 * pitchValue / dev);
            float actualPlus = 100 - (float)(10 * actualValue / dev);

            return new PitchModelOutput(stuffPlus, pitchPlus, actualPlus, count);
        }

        public static void Update(int month, int year, bool forceRefresh)
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using PitchTrackingDbContext trackingDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);
            
            if (forceRefresh)
                pitchDb.PitcherStatcastMonth
                    .Where(f => f.Year == year && f.Month == month)
                    .ExecuteDelete();

            if (pitchDb.PitcherStatcastMonth.Any(f => f.Year == year && f.Month == month))
                return;

            // Deviation values for calculating normalized numbers
            Dictionary<YearLeagueDevationKey, YearLeagueDeviations> yldDict = pitchDb.YearLeagueDeviations
                .AsNoTracking()
                .ToDictionary(
                    f => new YearLeagueDevationKey(f.ModelId, f.Year, f.Balls, f.Strikes),
                    f => f
                );

            // Month/actual-value source
            var psDict = db.PitchStatcast
                .AsNoTracking()
                .Where(f => f.Year == year)
                .ToDictionary(
                    f => new PitchMonthKey(f.GameId, f.PitchId),
                    f => new PitchStatcastMonthLookup(f.Month, f.RunValueSmoothedHitter));

            // Pitches in scope for this month
            var monthPitchData = trackingDb.PitchData
                .AsNoTracking()
                .Where(f => f.Year == year)
                .ToList()
                .Where(f =>
                {
                    int m = psDict[new PitchMonthKey(f.GameId, f.PitchId)].Month;
                    return month == 4 ? m <= 4 :
                           month == 9 ? m >= 9 :
                                        m == month;
                })
                .ToDictionary(f => new PitchMonthKey(f.GameId, f.PitchId), f => f);

            // Values per model, limited to this month's pitches
            var pvByModel = pitchDb.PitchValue
                .AsNoTracking()
                .ToList()
                .Where(f => monthPitchData.ContainsKey(new PitchMonthKey(f.GameId, f.PitchId)))
                .GroupBy(f => f.ModelId)
                .ToList();

            // Iterate through pitchers/dates/models
            List<PitcherStatcastMonth> dbData = new();
            int totalTicks = pvByModel.Sum(f => f.Select(g => g.PitcherId).Distinct().Count());
            using (ProgressBar progressBar = new ProgressBar(totalTicks, $"Creating Pitcher Month Stats for {month}-{year}"))
            {
                foreach (var modelValues in pvByModel)
                {
                    int modelId = modelValues.Key;
                    var modelPitcherPitches = modelValues
                        .Select(pv =>
                        {
                            PitchMonthKey key = new(pv.GameId, pv.PitchId);
                            PitchData pd = monthPitchData[key];
                            PitchStatcastMonthLookup ps = psDict[key];
                            return new PitchModelInput(pd.Year, ps.Month, pd.PitcherId, pd.PitchType, pd.CountBalls, pd.CountStrike, pv.StuffRuns, pv.PitchRuns, ps.RunValueSmoothedHitter);
                        })
                        .GroupBy(f => f.PitcherId);
                    foreach (var pitches in modelPitcherPitches)
                    {
                        // Split by pitch groups
                        PitchModelOutput allPitchOutput = GetPitchModelOutput(pitches, yldDict, modelId);
                        PitchModelOutput fastballOutput = GetPitchModelOutput(
                            pitches.Where(f =>
                                f.PitchType == DbEnums.PitchType.Fastball ||
                                f.PitchType == DbEnums.PitchType.Fourseam ||
                                f.PitchType == DbEnums.PitchType.Sinker ||
                                f.PitchType == DbEnums.PitchType.Twoseam)
                            , yldDict, modelId);
                        PitchModelOutput breakingOutput = GetPitchModelOutput(
                            pitches.Where(f =>
                                f.PitchType == DbEnums.PitchType.Cutter ||
                                f.PitchType == DbEnums.PitchType.Slider ||
                                f.PitchType == DbEnums.PitchType.Sweeper ||
                                f.PitchType == DbEnums.PitchType.Curveball ||
                                f.PitchType == DbEnums.PitchType.KnuckleCurve ||
                                f.PitchType == DbEnums.PitchType.Screwball ||
                                f.PitchType == DbEnums.PitchType.SlowCurve ||
                                f.PitchType == DbEnums.PitchType.Slurve)
                            , yldDict, modelId);
                        PitchModelOutput changeupOutput = GetPitchModelOutput(
                        pitches.Where(f =>
                            f.PitchType == DbEnums.PitchType.Splitter ||
                            f.PitchType == DbEnums.PitchType.Changeup ||
                            f.PitchType == DbEnums.PitchType.Forkball)
                        , yldDict, modelId);

                        // Sanity Check
                        if (allPitchOutput.stuff == null || allPitchOutput.pitch == null || allPitchOutput.actual == null)
                            throw new Exception($"Invalid Pitch Model Data For MlbId={pitches.Key} in {month}-{year}, Model={modelId}");

                        // Add month data
                        dbData.Add(new PitcherStatcastMonth
                        {
                            MlbId = pitches.Key,
                            Year = year,
                            Month = month,
                            ModelId = modelId,

                            IsValid = true,

                            Stuff = allPitchOutput.stuff.Value,
                            Pitch = allPitchOutput.pitch.Value,
                            Actual = allPitchOutput.actual.Value,
                            NumPitches = allPitchOutput.numPitches,

                            StuffFastball = fastballOutput.stuff,
                            PitchFastball = fastballOutput.pitch,
                            ActFastball = fastballOutput.actual,
                            NumFastballs = fastballOutput.numPitches,

                            StuffBreaking = breakingOutput.stuff,
                            PitchBreaking = breakingOutput.pitch,
                            ActBreaking = breakingOutput.actual,
                            NumBreaking = breakingOutput.numPitches,

                            StuffChangeup = changeupOutput.stuff,
                            PitchChangeup = changeupOutput.pitch,
                            ActChangeup = changeupOutput.actual,
                            NumChangeup = changeupOutput.numPitches,
                        });

                        progressBar.Tick();
                    }
                }
            }

            pitchDb.BulkInsert(dbData);
        }
    }
}
