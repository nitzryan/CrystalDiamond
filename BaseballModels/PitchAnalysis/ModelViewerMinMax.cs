using Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using PitchDb;
using ShellProgressBar;

namespace PitchAnalysis
{
    internal class ModelViewerMinMax
    {
        public static void Update(int year, bool forceRefresh)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);

            if (!forceRefresh && db.PitchModelResultBasis.Any(f => f.Year == year))
                return;

            db.PitchModelResultBasis.Where(f => f.Year == year).ExecuteDelete();

            // Check if any data for year
            if (!db.PitchStatcast.Any(f => f.Year == year))
                return;

            // Iterate through Counts
            using (ProgressBar progressBar = new ProgressBar(12, $"Calculating ModelViewerMinMax for {year}"))
            {
                for (int balls = 0; balls < 4; balls++)
                    for (int strikes = 0; strikes < 3; strikes++)
                    {
                        // Get All MLB pitches
                        var validPitches = pitchDb.Output_PitchValueAggregation
                            .Where(f => f.Year == year && f.LevelId == 1
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
                            continue;

                        // Run Values
                        List<float> values = validPitches
                            .Select(f => f.CombinedRuns)
                            .Order()
                            .ToList();

                        db.PitchModelResultBasis.Add(new PitchModelResultBasis
                        {
                            Year = year,
                            CountBalls = balls,
                            CountStrikes = strikes,
                            OutputType = DbEnums.PitchModelOutputType.Value,
                            Min = values.First(),
                            Perc5 = values.ElementAt(values.Count / 20),
                            Avg = values.Average(),
                            Median = values.ElementAt(values.Count / 2),
                            Perc95 = values.ElementAt((19 * values.Count) / 20),
                            Max = values.Last(),
                        });

                        // CSW
                        values = validPitches
                            .Select(f => f.CombinedCalledStrike + (f.CombinedSwing * f.CombinedWhiff))
                            .Order()
                            .ToList();

                        db.PitchModelResultBasis.Add(new PitchModelResultBasis
                        {
                            Year = year,
                            CountBalls = balls,
                            CountStrikes = strikes,
                            OutputType = DbEnums.PitchModelOutputType.CSW,
                            Min = values.First(),
                            Perc5 = values.ElementAt(values.Count / 20),
                            Avg = values.Average(),
                            Median = values.ElementAt(values.Count / 2),
                            Perc95 = values.ElementAt((19 * values.Count) / 20),
                            Max = values.Last(),
                        });

                        // Ball
                        values = validPitches
                            .Select(f => f.CombinedBall + f.CombinedHBP)
                            .Order()
                            .ToList();

                        db.PitchModelResultBasis.Add(new PitchModelResultBasis
                        {
                            Year = year,
                            CountBalls = balls,
                            CountStrikes = strikes,
                            OutputType = DbEnums.PitchModelOutputType.Ball,
                            Min = values.First(),
                            Perc5 = values.ElementAt(values.Count / 20),
                            Avg = values.Average(),
                            Median = values.ElementAt(values.Count / 2),
                            Perc95 = values.ElementAt((19 * values.Count) / 20),
                            Max = values.Last(),
                        });

                        // CSWFoul
                        values = validPitches
                            .Select(f => f.CombinedCalledStrike + (f.CombinedSwing * (f.CombinedWhiff + f.CombinedFoul)))
                            .Order()
                            .ToList();

                        db.PitchModelResultBasis.Add(new PitchModelResultBasis
                        {
                            Year = year,
                            CountBalls = balls,
                            CountStrikes = strikes,
                            OutputType = DbEnums.PitchModelOutputType.CSWFoul,
                            Min = values.First(),
                            Perc5 = values.ElementAt(values.Count / 20),
                            Avg = values.Average(),
                            Median = values.ElementAt(values.Count / 2),
                            Perc95 = values.ElementAt((19 * values.Count) / 20),
                            Max = values.Last(),
                        });

                        // In Play Percentage
                        values = validPitches
                            .Select(f => f.CombinedSwing * f.CombinedInPlay)
                            .Order()
                            .ToList();

                        db.PitchModelResultBasis.Add(new PitchModelResultBasis
                        {
                            Year = year,
                            CountBalls = balls,
                            CountStrikes = strikes,
                            OutputType = DbEnums.PitchModelOutputType.InPlayPerc,
                            Min = values.First(),
                            Perc5 = values.ElementAt(values.Count / 20),
                            Avg = values.Average(),
                            Median = values.ElementAt(values.Count / 2),
                            Perc95 = values.ElementAt((19 * values.Count) / 20),
                            Max = values.Last(),
                        });

                        // In Play Expected
                        values = validPitches
                            .Select(f => f.CombinedInPlayExpected)
                            .Order()
                            .ToList();

                        db.PitchModelResultBasis.Add(new PitchModelResultBasis
                        {
                            Year = year,
                            CountBalls = balls,
                            CountStrikes = strikes,
                            OutputType = DbEnums.PitchModelOutputType.InPlayExp,
                            Min = values.First(),
                            Perc5 = values.ElementAt(values.Count / 20),
                            Avg = values.Average(),
                            Median = values.ElementAt(values.Count / 2),
                            Perc95 = values.ElementAt((19 * values.Count) / 20),
                            Max = values.Last(),
                        });

                        // Whiff Rate
                        values = validPitches
                            .Select(f => f.CombinedWhiff)
                            .Order()
                            .ToList();

                        db.PitchModelResultBasis.Add(new PitchModelResultBasis
                        {
                            Year = year,
                            CountBalls = balls,
                            CountStrikes = strikes,
                            OutputType = DbEnums.PitchModelOutputType.WhiffRate,
                            Min = values.First(),
                            Perc5 = values.ElementAt(values.Count / 20),
                            Avg = values.Average(),
                            Median = values.ElementAt(values.Count / 2),
                            Perc95 = values.ElementAt((19 * values.Count) / 20),
                            Max = values.Last(),
                        });

                        // Swing Strike Percentage
                        values = validPitches
                            .Select(f => f.CombinedWhiff * f.CombinedSwing)
                            .Order()
                            .ToList();

                        db.PitchModelResultBasis.Add(new PitchModelResultBasis
                        {
                            Year = year,
                            CountBalls = balls,
                            CountStrikes = strikes,
                            OutputType = DbEnums.PitchModelOutputType.SwingStrikePerc,
                            Min = values.First(),
                            Perc5 = values.ElementAt(values.Count / 20),
                            Avg = values.Average(),
                            Median = values.ElementAt(values.Count / 2),
                            Perc95 = values.ElementAt((19 * values.Count) / 20),
                            Max = values.Last(),
                        });

                        progressBar.Tick();
                    }
            }
            db.SaveChanges();
        }
    }
}
