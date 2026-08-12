using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using PitchTrackingDb;
using ShellProgressBar;

namespace PitchAnalysis
{
    internal class CalculatePitchValues
    {
        public static void Calcuate()
        {
            PitchTrackingDbContext trackingDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);
            PitchDbContext pitchDb = new(PitchDb.Connection.PITCHDB_OPTIONS);

            pitchDb.PitchValue.ExecuteDelete();

            var pitchDataDict = trackingDb.PitchData
                .Where(f => f.Year >= pitchDb.ModelTrainingHistory_PitchValue.Min(f => f.Year))
                .ToDictionary(
                    f => new { f.GameId, f.PitchId },
                    f => f
                );
            var pitches = pitchDb.Output_PitchValueAggregation
                .ToList();

            // Pre-load Year deviations
            var devDict = pitchDb.YearLeagueDeviations.ToDictionary(
                f => new { f.Year, f.ModelId, f.Balls, f.Strikes },
                f => f);

            List<PitchValue> pitchValues = new(pitches.Count);
            using (ProgressBar progressBar = new ProgressBar(pitches.Count, "Calculating Pitch Value"))
            {
                foreach (var pitch in pitches)
                {
                    PitchData pd = pitchDataDict[new { pitch.GameId, pitch.PitchId }];

                    YearLeagueDeviations yld = devDict[new
                    {
                        pd.Year,
                        ModelId=pitch.Model,
                        Balls=pd.CountBalls,
                        Strikes=pd.CountStrike
                    }];

                    pitchValues.Add(new PitchValue
                    {
                        ModelId = pitch.Model,
                        GameId = pitch.GameId,
                        PitchId = pitch.PitchId,
                        PitcherId = pd.PitcherId,
                        StuffPlus = 100 + (pitch.StuffRuns / yld.StuffDev * 10),
                        StuffRuns = pitch.StuffRuns,
                        PitchPlus = 100 + (pitch.CombinedRuns / yld.StuffDev * 10),
                        PitchRuns = pitch.CombinedRuns,
                    });

                    progressBar.Tick();
                }
            }

            pitchDb.BulkInsert(pitchValues);
        }
    }
}
