using Microsoft.EntityFrameworkCore;
using PitchAnalysis.PitchZoneBinNS;
using PitchDb;
using PitchTrackingDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchAnalysis.Plots
{
    internal class High_HBP_PitchTest
    {
        private const string HIGH_HBP_DIRECTORY = "High_HBP_PitchTest";
        private const float HIGH_HBP_THRESHOLD = 0.20f;

        // Plots every pitch that received a stuff value despite a high predicted HBP probability.
        // Output is split by MLB/MiLB and hitter handedness, and file names sort by HBP probability.
        public static void PlotHighHbpPitches(PitchZoneBin bin)
        {
            using PitchDbContext pitchDb = new(Constants.PITCHDB_OPTIONS);
            using PitchTrackingDbContext ptDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);

            // OPVA has one row per model, so collapse to one entry per pitch using the highest probability
            var highHbp = pitchDb.Output_PitchValueAggregation
                .AsNoTracking()
                .Where(f => f.StuffBinRuns != null && f.CombinedHBP > HIGH_HBP_THRESHOLD)
                .Select(f => new { f.GameId, f.PitchId, f.CombinedHBP })
                .ToList()
                .GroupBy(f => (f.GameId, f.PitchId))
                .ToDictionary(g => g.Key, g => g.Max(f => f.CombinedHBP));

            var gameIds = highHbp.Keys.Select(k => k.GameId).Distinct().ToList();
            List<PitchData> pitches = ptDb.PitchData
                .AsNoTracking()
                .Where(f => gameIds.Contains(f.GameId))
                .AsEnumerable()
                .Where(f => highHbp.ContainsKey((f.GameId, f.PitchId)))
                .ToList();

            if (pitches.Count != highHbp.Count)
                throw new InvalidOperationException($"Found {pitches.Count} of {highHbp.Count} high-HBP pitches in PitchData.");

            if (Directory.Exists(HIGH_HBP_DIRECTORY))
                Directory.Delete(HIGH_HBP_DIRECTORY, recursive: true);

            foreach (PitchData pitch in pitches)
            {
                string directory = Path.Combine(HIGH_HBP_DIRECTORY,
                    pitch.LevelId == 1 ? "MLB" : "MiLB",
                    pitch.HitIsR ? "RHH" : "LHH");
                Directory.CreateDirectory(directory);

                string fileName = $"HBP{highHbp[(pitch.GameId, pitch.PitchId)]:F3}_L{pitch.LevelId}_{pitch.PitchClass}_" +
                    $"{(pitch.PitIsR ? "RHP" : "LHP")}_{pitch.CountBalls}-{pitch.CountStrike}_{pitch.Result}_" +
                    $"{pitch.GameId}_{pitch.PitchId}.png";

                bin.PlotPitchZone(pitch, Path.Combine(directory, fileName));
            }

            foreach (var level in pitches.GroupBy(p => p.LevelId).OrderBy(g => g.Key))
            {
                Console.WriteLine($"Level {level.Key}: {level.Count()} pitches, " +
                    $"{level.Count(p => p.Result == Db.DbEnums.PitchResult.HBP)} actually hit the batter");
            }
        }
    }
}
