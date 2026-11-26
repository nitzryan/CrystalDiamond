using MathNet.Numerics.Statistics;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using PitchTrackingDb;

namespace PitchAnalysis.PitchDecomp
{
    internal static class PitcherScale
    {
        private const int MIN_STUFF_PITCHES = 100;   // per MLB pitcher/pitch type season

        // Calculates and logs the pitcher-level scale for one model and year from its graded MLB pitches
        public static YearPitcherDeviations CalculateAndSave(
            PitchDbContext pitchDb,
            int year,
            int modelId,
            IEnumerable<PitchData> gradedPitches,
            Func<PitchData, float?> stuffRuns,
            Func<PitchData, float> countDev)
        {
            List<PitchData> mlbPitches = gradedPitches.Where(p => p.LevelId == 1).ToList();

            // Pitch types are pooled, giving one scale for comparing any pitch to any other
            List<double> scores = mlbPitches
                .GroupBy(p => (p.PitcherId, p.PitchType))
                .Select(g => new
                {
                    Stuff = g.Select(stuffRuns).Where(v => v.HasValue).Select(v => (double)v!.Value).ToList(),
                    // Averaged over all graded pitches, matching the pitcher aggregation
                    CountDev = g.Average(p => (double)countDev(p))
                })
                .Where(g => g.Stuff.Count >= MIN_STUFF_PITCHES)
                .Select(g => g.Stuff.Average() / g.CountDev)
                .ToList();

            YearPitcherDeviations deviation = new()
            {
                ModelId = modelId,
                Year = year,
                ScoreDev = (float)scores.StandardDeviation(),
                MeanCountDev = (float)mlbPitches.Average(p => (double)countDev(p)),
                NumQualified = scores.Count,
            };

            pitchDb.YearPitcherDeviations
                .Where(f => f.ModelId == modelId && f.Year == year)
                .ExecuteDelete();
            pitchDb.YearPitcherDeviations.Add(deviation);
            pitchDb.SaveChanges();

            return deviation;
        }
    }
}
