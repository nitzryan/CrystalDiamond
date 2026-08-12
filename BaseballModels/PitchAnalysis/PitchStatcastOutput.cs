using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchDb;
using PitchTrackingDb;
using ShellProgressBar;

namespace PitchAnalysis
{
    internal class PitchStatcastOutput
    {
        public static void Update(bool forceRefresh)
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using PitchTrackingDbContext ptDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);

            if (forceRefresh)
                pitchDb.PitchValue.ExecuteDelete();

            // Pre-load PitchData for year/count lookup
            var pitchDataDict = ptDb.PitchData
                .AsNoTracking()
                .ToDictionary(
                    f => new { f.GameId, f.PitchId },
                    f => new { f.Year, f.CountBalls, f.CountStrike, f.PitcherId });

            // Pre-load Year deviations
            var devDict = pitchDb.YearLeagueDeviations
                .AsNoTracking()
                .ToDictionary(
                    f => new { f.ModelId, f.Year, f.Balls, f.Strikes },
                    f => f);

            // Get data not already logged
            var pitchValueHashSet = pitchDb.PitchValue
                .Select(f => new { f.GameId, f.PitchId })
                .ToHashSet();

            var pitchOutputs = pitchDb.Output_PitchValueAggregation
                .Where(f => !pitchDb.PitchValue.Any(
                    a => a.GameId == f.GameId && a.PitchId == f.PitchId)
                )
                .AsNoTracking();
            int count = pitchOutputs.Count();

            List<PitchValue> buffer = new();
            using (ProgressBar progressBar = new ProgressBar(count, "Aggregating Pitch Model Results"))
            {
                foreach (var opva in pitchOutputs.AsEnumerable())
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

            if (buffer.Count > 0)
            {
                pitchDb.BulkInsert(buffer);
            }
        }
    }
}
