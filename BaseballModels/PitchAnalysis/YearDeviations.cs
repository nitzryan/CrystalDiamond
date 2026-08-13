using Microsoft.EntityFrameworkCore;
using PitchDb;
using ShellProgressBar;

namespace PitchAnalysis
{
    internal class YearDeviations
    {
        const float PITCH_CAP = 0.1f;

        public static void Update(int endYear)
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);

            pitchDb.YearLeagueDeviations.Where(f => f.Year >= endYear).ExecuteDelete();

            var years = pitchDb.Output_PitchValueAggregation.Select(f => f.Year).Distinct();
            years = years.Where(f => !pitchDb.YearLeagueDeviations.Select(f => f.Year).Contains(f));
            var models = pitchDb.Output_PitchValueAggregation.Select(f => f.Model).Distinct();

            List<int> balls = [0, 1, 2, 3];
            List<int> strikes = [0, 1, 2];

            using (ProgressBar progressBar = new ProgressBar(years.Count() * models.Count() * balls.Count * strikes.Count, "Calculating Year Deviations"))
            {
                foreach (int year in years)
                {
                    var yearPitches = pitchDb.Output_PitchValueAggregation
                        .Where(f => f.Year == year && f.LevelId == 1)
                        .Select(f => new { f.GameId, f.PitchId, f.StuffRuns, f.CombinedRuns, f.CountBalls, f.CountStrikes, f.Model })
                        .AsEnumerable()
                        .GroupBy(f => f.Model)
                        .ToDictionary(f => f.Key, f => f.ToList());
                    foreach (int modelId in models)
                    {
                        // Make sure model has data for this year.
                        if (!yearPitches.ContainsKey(modelId))
                        {
                            for (int i = 0; i < balls.Count * strikes.Count; i++)
                                progressBar.Tick();
                            continue;
                        }

                        var yearModelPitches = yearPitches[modelId];

                        foreach (int ball in balls)
                        {
                            var yearModelBallPitches = yearModelPitches
                                .Where(f => f.CountBalls == ball)
                                .ToList();

                            foreach (int strike in strikes)
                            {
                                var yearModelCountPitches = yearModelPitches
                                    .Where(f => f.CountStrikes == strike)
                                    .ToArray();

                                double stuffDev = Math.Sqrt(yearModelCountPitches
                                    .Select(f => f.StuffRuns)
                                    .AsEnumerable()
                                    .Select(v => Math.Clamp(v, -PITCH_CAP, PITCH_CAP))
                                    .Select(v => v * v)
                                    .Average());
                                double pitchDev = Math.Sqrt(yearModelCountPitches
                                    .Select(f => f.CombinedRuns)
                                    .AsEnumerable()
                                    .Select(v => Math.Clamp(v, -PITCH_CAP, PITCH_CAP))
                                    .Select(v => v * v)
                                    .Average());

                                pitchDb.YearLeagueDeviations.Add(new YearLeagueDeviations
                                {
                                    Year = year,
                                    ModelId = modelId,
                                    Balls=ball,
                                    Strikes=strike,
                                    StuffDev = (float)stuffDev,
                                    PitchDev = (float)pitchDev
                                });

                                progressBar.Tick();
                            }
                        }
                        
                    }
                }
            }

            pitchDb.SaveChanges();
        }
    }
}
