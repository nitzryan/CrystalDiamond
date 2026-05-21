using Db;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using ShellProgressBar;
using System.Text.Json;

namespace PitchAnalysis
{
    public record PitchModelOutput(double absValue, double stuffValue, double locationValue, double expValue, double stuffPlus, double locPlus, double pitchPlus);

    internal class PitchStatcastOutput
    {
        public static void Update()
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            db.Database.ExecuteSqlRaw("UPDATE PitchStatcast SET ModelOutput=''");

            var pitchOutputs = pitchDb.Output_PitchValueAggregation
                .AsNoTracking()
                .GroupBy(f => new { f.GameId, f.PitchId});
            int count = pitchOutputs.Count();

            int pitchCount = 0;

            // Pre-load Year deviations
            var devDict = pitchDb.YearLeagueDeviations.ToDictionary(
                f => new { f.Year, f.ModelId, f.Balls, f.Strikes},
                f => f);

            using (ProgressBar progressBar = new ProgressBar(count, "Aggregating Pitch Model Results"))
            {
                foreach (var pitch in pitchOutputs)
                {
                    Dictionary<int, PitchModelOutput> modelValues = new();

                    var modelPitches = pitch.GroupBy(f => new { f.Model, f.CountBalls, f.CountStrikes });
                    foreach (var mp in modelPitches)
                    {
                        int size = mp.Count();
                        var opva = mp.Single();

                        double absValue = 0;
                        double stuffValue = opva.StuffRuns;
                        double locValue = opva.LocationRuns;
                        double combinedValue = opva.CombinedRuns;

                        var yld = devDict[
                            new { 
                                mp.First().Year, 
                                ModelId=mp.First().Model, 
                                Balls=mp.First().CountBalls, 
                                Strikes=mp.First().CountStrikes 
                                }
                            ];

                        // TODO: Not sure whether they should all use the stuff deviation or their individual deviations
                        double stuffPlus = 100 - (10 * stuffValue / yld.StuffDev);
                        double locPlus = 100 - (10 * locValue / yld.StuffDev);
                        double pitchPlus = 100 - (10 * combinedValue / yld.StuffDev);

                        modelValues[mp.Key.Model] = new PitchModelOutput(Math.Round(absValue, 3), Math.Round(stuffValue, 3), Math.Round(locValue, 3), Math.Round(combinedValue, 3), Math.Round(stuffPlus, 1), Math.Round(locPlus, 1), Math.Round(pitchPlus, 1));
                    }

                    // Write to JSON 
                    string json = JsonSerializer.Serialize(modelValues, new JsonSerializerOptions
                    {
                        WriteIndented = false
                    });
                    db.PitchStatcast
                        .Where(f => f.PitchId == pitch.Key.PitchId && f.GameId == pitch.Key.GameId)
                        .Single().ModelOutput = json;

                    // Make sure that too many pitches don't get logged to run out of memory
                    pitchCount++;
                    if (pitchCount >= 1000000)
                    {
                        db.SaveChanges();
                        db.ChangeTracker.Clear();
                        pitchCount = 0;
                    }

                    progressBar.Tick();
                }
            }

            db.SaveChanges();
        }
    }
}
