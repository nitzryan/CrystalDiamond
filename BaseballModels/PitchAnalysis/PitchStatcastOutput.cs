using Db;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using ShellProgressBar;

namespace PitchAnalysis
{
    internal class PitchStatcastOutput
    {
        public static void Update()
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            db.Database.ExecuteSqlRaw("UPDATE PitchStatcast SET ModelStuff=NULL");
            db.Database.ExecuteSqlRaw("UPDATE PitchStatcast SET ModelPitch=NULL");

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
                    var opva = pitch.Single();

                    var pitchStatcast = db.PitchStatcast
                        .Where(f => f.PitchId == pitch.Key.PitchId && f.GameId == pitch.Key.GameId)
                        .Single();

                    pitchStatcast.ModelStuff = opva.StuffRuns;
                    pitchStatcast.ModelPitch = opva.CombinedRuns;

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
