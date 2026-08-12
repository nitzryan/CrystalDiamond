using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using ShellProgressBar;
using static Db.DbEnums;

namespace PitchAnalysis
{
    internal class ModelViewerMinMax
    {
        private static PitchModelResultBasis BuildBasis(
            int year,
            int modelId,
            int balls,
            int strikes,
            PitchModelOutputType outputType,
            List<float> values)
        {
            return new PitchModelResultBasis
            {
                Year = year,
                ModelId = modelId,
                CountBalls = balls,
                CountStrikes = strikes,
                OutputType = outputType,
                Min = values.First(),
                Perc5 = values.ElementAt(values.Count / 20),
                Avg = values.Average(),
                Median = values.ElementAt(values.Count / 2),
                Perc95 = values.ElementAt((19 * values.Count) / 20),
                Max = values.Last(),
            };
        }

        public static void Update(int year, bool forceRefresh)
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);

            if (!forceRefresh && pitchDb.PitchModelResultBasis.Any(f => f.Year == year))
            {
                Console.WriteLine($"No data logged in ModelViewMinMax for {year}");
                return;
            }
                

            pitchDb.PitchModelResultBasis.Where(f => f.Year == year).ExecuteDelete();

            // Check if any data for year
            if (!pitchDb.YearLeagueDeviations.Any(f => f.Year == year))
                return;

            // Iterate through Counts/Models
            List<int> modelIds = pitchDb.YearLeagueDeviations
                .Where(f => f.Year == year)
                .Select(f => f.ModelId)
                .Distinct()
                .ToList();
            List<PitchModelResultBasis> dbData = new();
            using (ProgressBar progressBar = new ProgressBar(modelIds.Count * 12, $"Calculating ModelViewerMinMax for {year}"))
            {
                foreach (int modelId in modelIds)
                    for (int balls = 0; balls < 4; balls++)
                        for (int strikes = 0; strikes < 3; strikes++)
                        {
                            // Get All MLB pitches
                            var validPitches = pitchDb.Output_PitchValueAggregation
                            .AsNoTracking()
                            .Where(f => f.Year == year && f.LevelId == 1
                                && f.Model == modelId
                                && f.CountBalls == balls && f.CountStrikes == strikes)
                            .Select(f => new
                            {
                                f.CombinedBall,
                                f.CombinedCalledStrike,
                                f.CombinedSwing,
                                f.CombinedHBP,

                                f.CombinedWhiff,
                                f.CombinedFoul,
                                f.CombinedInPlay,

                                f.CombinedInPlayExpected,

                                f.CombinedRuns
                            })
                            .ToList();

                            if (validPitches.Count < 100)
                            {
                                progressBar.Tick();
                                continue;
                            }

                            // Run Values
                            List<float> values = validPitches
                                .Select(f => f.CombinedRuns)
                                .Order()
                                .ToList();

                            // Run Values
                            dbData.Add(BuildBasis(year, modelId, balls, strikes, PitchModelOutputType.Value,
                                validPitches.Select(f => f.CombinedRuns).Order().ToList()));
                            
                            // CSW
                            dbData.Add(BuildBasis(year, modelId, balls, strikes, PitchModelOutputType.CSW,
                                validPitches.Select(f => f.CombinedCalledStrike + (f.CombinedSwing * f.CombinedWhiff)).Order().ToList()));
                            
                            // Ball
                            dbData.Add(BuildBasis(year, modelId, balls, strikes, PitchModelOutputType.Ball,
                                validPitches.Select(f => f.CombinedBall + f.CombinedHBP).Order().ToList()));
                            
                            // CSWFoul
                            dbData.Add(BuildBasis(year, modelId, balls, strikes, PitchModelOutputType.CSWFoul,
                                validPitches.Select(f => f.CombinedCalledStrike + (f.CombinedSwing * (f.CombinedWhiff + f.CombinedFoul))).Order().ToList()));
                            
                            // In Play Percentage
                            dbData.Add(BuildBasis(year, modelId, balls, strikes, PitchModelOutputType.InPlayPerc,
                                validPitches.Select(f => f.CombinedSwing * f.CombinedInPlay).Order().ToList()));
                            
                            // In Play Expected
                            dbData.Add(BuildBasis(year, modelId, balls, strikes, PitchModelOutputType.InPlayExp,
                                validPitches.Select(f => f.CombinedInPlayExpected).Order().ToList()));
                            
                            // Whiff Rate
                            dbData.Add(BuildBasis(year, modelId, balls, strikes, PitchModelOutputType.WhiffRate,
                                validPitches.Select(f => f.CombinedWhiff).Order().ToList()));
                            
                            // Swing Strike Percentage
                            dbData.Add(BuildBasis(year, modelId, balls, strikes, PitchModelOutputType.SwingStrikePerc,
                                validPitches.Select(f => f.CombinedWhiff * f.CombinedSwing).Order().ToList()));

                            progressBar.Tick();
                        }
            }

            pitchDb.BulkInsert(dbData);
        }
    }
}
