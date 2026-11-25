using Microsoft.EntityFrameworkCore;
using PitchDb;
using PitchTrackingDb;
using ShellProgressBar;

namespace PitchAnalysis
{
    internal class YearDeviations
    {
        public static void Update()
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using PitchTrackingDbContext ptDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);
            pitchDb.YearLeagueDeviations.ExecuteDelete();

            List<int> years = pitchDb.Output_PitchValueAggregation
                .Select(f => f.Year)
                .Distinct()
                .ToList()
                .OrderBy(f => f)
                .ToList();
            List<int> models = pitchDb.Output_PitchValueAggregation.Select(f => f.Model).Distinct().ToList();
            List<int> balls = [0, 1, 2, 3];
            List<int> strikes = [0, 1, 2];

            List<string> missing = [];
            using (ProgressBar progressBar = new ProgressBar(years.Count * models.Count * balls.Count * strikes.Count, "Calculating Year Deviations"))
            {
                foreach (int year in years)
                {
                    // MLB pitches only, so every level is later scaled against the same distribution
                    var yearPitches = pitchDb.Output_PitchValueAggregation
                        .AsNoTracking()
                        .Where(f => f.Year == year && f.LevelId == 1 && f.StuffBinRuns != null)
                        .Select(f => new { f.GameId, f.PitchId, f.Model, f.StuffBinRuns })
                        .ToList();

                    Dictionary<(int, int), (int Balls, int Strikes)> countDict = ptDb.PitchData
                        .AsNoTracking()
                        .Where(f => f.Year == year && f.LevelId == 1)
                        .Select(f => new { f.GameId, f.PitchId, f.CountBalls, f.CountStrike })
                        .ToDictionary(
                            f => (f.GameId, f.PitchId),
                            f => (f.CountBalls, f.CountStrike));

                    // Root mean square of StuffBinRuns for each model and count
                    Dictionary<(int Model, int Balls, int Strikes), double> deviations = yearPitches
                        .GroupBy(f =>
                        {
                            var count = countDict[(f.GameId, f.PitchId)];
                            return (f.Model, count.Balls, count.Strikes);
                        })
                        .ToDictionary(
                            g => g.Key,
                            g => Math.Sqrt(g.Average(f => (double)f.StuffBinRuns!.Value * f.StuffBinRuns.Value)));

                    foreach (int modelId in models)
                    {
                        // A model/year with no graded MLB pitches at all is expected (e.g. no prior-year bins)
                        bool hasData = yearPitches.Any(f => f.Model == modelId);
                        foreach (int ball in balls)
                        {
                            foreach (int strike in strikes)
                            {
                                if (deviations.TryGetValue((modelId, ball, strike), out double stuffDev))
                                {
                                    pitchDb.YearLeagueDeviations.Add(new YearLeagueDeviations
                                    {
                                        Year = year,
                                        ModelId = modelId,
                                        Balls = ball,
                                        Strikes = strike,
                                        StuffDev = (float)stuffDev
                                    });
                                }
                                else if (hasData)
                                {
                                    missing.Add($"Model {modelId}, {year}, {ball}-{strike}");
                                }

                                progressBar.Tick();
                            }
                        }
                    }
                }
            }

            foreach (string entry in missing)
                Console.WriteLine($"No graded MLB pitches, no deviation written: {entry}");

            pitchDb.SaveChanges();
        }
    }
}
