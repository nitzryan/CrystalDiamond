using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using ShellProgressBar;

namespace PitchAnalysis
{
    internal class MonthStats
    {
        private record YearLeagueDevationKey(int modelId, int year, int balls, int strikes);

        private record PitchModelInput(int Year, int Month, int PitcherId, DbEnums.PitchType PitchType, int Balls, int Strikes, float ModelStuff, float ModelPitch, float Actual);
        private record PitchModelOutput(float? stuff, float? pitch, float? actual, int numPitches);
        private static PitchModelOutput GetPitchModelOutput(IEnumerable<PitchModelInput> pitches, Dictionary<YearLeagueDevationKey, YearLeagueDeviations> YldDict)
        {
            int count = pitches.Count();
            if (count == 0)
                return new PitchModelOutput(null, null, null, 0);
        
            double dev = 0;
            foreach (var pitch in pitches)
            {
                dev += YldDict[new YearLeagueDevationKey(1, pitch.Year, pitch.Balls, pitch.Strikes)].StuffDev;
            }

            float stuffValue = pitches.Sum(f => f.ModelStuff);
            float pitchValue = pitches.Sum(f => f.ModelPitch);
            float actualValue = pitches.Sum(f => f.Actual);

            float stuffPlus = 100 - (float)(10 * stuffValue / dev);
            float pitchPlus = 100 - (float)(10 * pitchValue / dev);
            float actualPlus = 100 - (float)(10 * actualValue / dev);

            return new PitchModelOutput(stuffPlus, pitchPlus, actualPlus, count);
        }

        public static void Update(int month, int year)
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            db.PitcherStatcastMonth.
                Where(f => f.Year == year && f.Month == month)
                .ExecuteDelete();

            Dictionary<YearLeagueDevationKey, YearLeagueDeviations> yldDict = pitchDb.YearLeagueDeviations
                .ToDictionary(
                    f => new YearLeagueDevationKey(f.ModelId, f.Year, f.Balls, f.Strikes),
                    f => f
                );

            // Get pitches for this month
            #pragma warning disable CS8629 // Where clause makes impossible
            var yearPitches = db.PitchStatcast
                .Where(f => f.Year == year &&
                    f.ModelStuff != null &&
                    f.ModelPitch != null)
                .Select(f => new PitchModelInput(f.Year, f.Month, f.PitcherId, f.PitchType, f.CountBalls, f.CountStrike, f.ModelStuff.Value, f.ModelPitch.Value, f.RunValueSmoothedHitter))
                .AsEnumerable();
            #pragma warning restore CS8629

            var monthPitches =
                (month == 4 ? yearPitches.Where(f => f.Month <= 4) :
                 month == 9 ? yearPitches.Where(f => f.Month >= 9) :
                              yearPitches.Where(f => f.Month == month))
                .ToList();

            // Go through each pitcher
            var monthPitcherPitches = monthPitches
                .GroupBy(f => f.PitcherId);
            int count = monthPitcherPitches.Count();
            List<PitcherStatcastMonth> dbData = new(count);
            using (ProgressBar progressBar = new ProgressBar(count, $"Creating Pitcher Month Stats for {month}-{year}"))
            {
                foreach (var pitches in monthPitcherPitches)
                {
                    PitchModelOutput allPitchOutput = GetPitchModelOutput(pitches, yldDict);
                    PitchModelOutput fastballOutput = GetPitchModelOutput(
                        pitches.Where(f =>
                            f.PitchType == DbEnums.PitchType.Fastball ||
                            f.PitchType == DbEnums.PitchType.Fourseam ||
                            f.PitchType == DbEnums.PitchType.Sinker ||
                            f.PitchType == DbEnums.PitchType.Twoseam)
                        , yldDict);
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
                        , yldDict);
                    PitchModelOutput changeupOutput = GetPitchModelOutput(
                    pitches.Where(f =>
                        f.PitchType == DbEnums.PitchType.Splitter ||
                        f.PitchType == DbEnums.PitchType.Changeup ||
                        f.PitchType == DbEnums.PitchType.Forkball)
                    , yldDict);

                    if (allPitchOutput.stuff == null || allPitchOutput.pitch == null || allPitchOutput.actual == null)
                        throw new Exception($"Invalid Pitch Model Data For MlbId={pitches.Key} in {month}-{year}");

                    dbData.Add(new PitcherStatcastMonth
                    {
                        MlbId = pitches.Key,
                        Year = year,
                        Month = month,

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

            db.BulkInsert(dbData);
        }
    }
}
