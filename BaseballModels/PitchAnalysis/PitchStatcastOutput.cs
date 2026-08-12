using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using PitchTrackingDb;
using ShellProgressBar;

namespace PitchAnalysis
{
    internal class PitchStatcastOutput
    {
        public static void Update()
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using PitchTrackingDbContext ptDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);

            // Pre-load PitchData for year/count lookup
            var pitchDataDict = ptDb.PitchData
                .AsNoTracking()
                .ToDictionary(
                    f => new { f.GameId, f.PitchId },
                    f => new { f.Year, f.CountBalls, f.CountStrike, f.PitcherId });
            Console.WriteLine("Loaded Pitch Data");

            // Pre-load Year deviations
            var devDict = pitchDb.YearLeagueDeviations
                .AsNoTracking()
                .ToDictionary(
                    f => new { f.ModelId, f.Year, f.Balls, f.Strikes },
                    f => f);
            Console.WriteLine("Loaded Year League Deviations");

            // Get data not already logged
            int maxGameId = pitchDb.PitchValue.Max(f => (int?)f.GameId) ?? 0;
            var pitchOutputs = pitchDb.Output_PitchValueAggregation
                .AsNoTracking()
                .Where(f => f.GameId > maxGameId && !pitchDb.PitchValue.Any(
                    a => a.GameId == f.GameId && a.PitchId == f.PitchId)
                )
                .ToList();
            int count = pitchOutputs.Count();

            List<PitchValue> buffer = new();
            using (ProgressBar progressBar = new ProgressBar(count, "Aggregating Pitch Model Results"))
            {
                foreach (var opva in pitchOutputs)
                {
                    var pd = pitchDataDict[new { opva.GameId, opva.PitchId }];
                    var yld = devDict[new { ModelId = opva.Model, pd.Year, Balls = pd.CountBalls, Strikes = pd.CountStrike }];
                    
                    buffer.Add(new PitchValue
                    {
                        ModelId = opva.Model,
                        GameId = opva.GameId,
                        PitchId = opva.PitchId,
                        PitcherId = pd.PitcherId,
                        StuffPlus = (float)(100 - (10 * (opva.StuffRuns / yld.StuffDev))),
                        StuffRuns = opva.StuffRuns,
                        PitchPlus = (float)(100 - (10 * (opva.CombinedRuns / yld.StuffDev))),
                        PitchRuns = opva.CombinedRuns,
                    });

                    progressBar.Tick();
                }
            }

            pitchDb.BulkInsert(buffer);
        }
    }
}
