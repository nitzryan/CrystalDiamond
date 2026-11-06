using DataAquisition.PitchTracking;
using Microsoft.EntityFrameworkCore;
using PitchTrackingDb;

namespace PitchAnalysis.Plots
{
    //internal static class PitchValueSurface
    //{
    //    private const string PLOT_DIRECTORY = "PitchValueSurface";
    //    private const int MIN_GROUP_PITCHES = 500;    // skip PitchType/hand/count groups smaller than this
    //    private const int MIN_CELL_PITCHES = 20;      // cells with fewer pitches are left grey (too noisy to judge shape)
    //    private const double RUNS_SCALE = 100;        // hitter runs per 100 pitches

    //    public static void GeneratePlots(int year, PitchZoneBinArgs binArgs)
    //    {
    //        PitchZoneBin bin = new(binArgs with { Kernel = null });
    //        using PitchTrackingDbContext ptDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);

    //        // 0-0 and 2-2 only, MLB only
    //        List<PitchData> pitches = ptDb.PitchData
    //            .AsNoTracking()
    //            .Where(f => f.Year == year && f.LevelId == 1 && f.PitchClass != Db.DbEnums.PitchClass.Other
    //                && ((f.CountBalls == 0 && f.CountStrike == 0) || (f.CountBalls == 2 && f.CountStrike == 2)))
    //            .ToList();
    //        Console.WriteLine($"Loaded {pitches.Count:N0} pitches for {year}");

    //        Directory.CreateDirectory(PLOT_DIRECTORY);

    //        var groups = pitches
    //            .GroupBy(p => new { p.PitchClass, p.PitIsR, p.HitIsR, p.CountBalls, p.CountStrike })
    //            .Where(g => g.Count() >= MIN_GROUP_PITCHES)
    //            .OrderBy(g => g.Key.PitchClass).ThenBy(g => g.Key.PitIsR).ThenBy(g => g.Key.HitIsR).ThenBy(g => g.Key.CountBalls);

    //        foreach (var group in groups)
    //        {
    //            List<PitchData> groupPitches = group.ToList();

    //            // Single PitchType => single PitchClass => exactly one combo comes back
    //            var (competitive, nonCompetitive) = bin.BuildCompetitiveRows(groupPitches);

    //            // Shown relative to the group's own mean so the shape reads as above/below average
    //            double mean = groupPitches.Average(p => (double)p.RunValueSmoothedHitter);

    //            Dictionary<PitchZone, (double Value, string Text)> cells = new();
    //            foreach (PitchZoneCompetitive row in competitive.Where(r => r.PitchCount >= MIN_CELL_PITCHES))
    //            {
    //                double value = (row.ExpectedValueHitter - mean) * RUNS_SCALE;
    //                string flag = row.IsCompetitive ? "" : " *";
    //                cells[new PitchZone(row.ZoneRow, row.ZoneCol)] = (value, $"{value:+0.0;-0.0;0.0}{flag}\n{row.PitchCount:N0}");
    //            }

    //            PitchZoneNonCompetitive noZoneRow = nonCompetitive[0];
    //            double noZoneValue = (noZoneRow.ExpectedValueHitter - mean) * RUNS_SCALE;
    //            (double, string) noZone = (noZoneValue, $"{noZoneValue:+0.0;-0.0;0.0}\n{noZoneRow.PitchCount:N0}");

    //            float zoneTop = (float)groupPitches.Average(p => p.ZoneTop);
    //            float zoneBot = (float)groupPitches.Average(p => p.ZoneBot);

    //            string pit = group.Key.PitIsR ? "RHP" : "LHP";
    //            string hit = group.Key.HitIsR ? "RHH" : "LHH";
    //            string count = $"{group.Key.CountBalls}-{group.Key.CountStrike}";
    //            string label = $"{year} {group.Key.PitchClass} {pit} v {hit} {count} | n={groupPitches.Count:N0} | " +
    //                           $"hitter runs/100 vs group mean (green = hitter-favourable), * = non-competitive";

    //            bin.PlotCellValues(cells, noZone, label,
    //                Path.Combine(PLOT_DIRECTORY, $"{year}_{group.Key.PitchClass}_{pit}_{hit}_{count}.png"),
    //                zoneTop, zoneBot);
    //        }
    //    }
    //}

    internal static class PitchValueSurface
    {
        private const string PLOT_DIRECTORY = "PitchValueSurface";
        private const int MIN_GROUP_PITCHES = 2000;   // groups with fewer pitches aren't plotted
        private const int MIN_CELL_PITCHES = 25;      // cells with fewer pitches are left grey
        private const double RUNS_SCALE = 100;        // hitter runs per 100 pitches

        public static void GeneratePlots(int year, PitchZoneBinArgs args)
        {
            PitchZoneBin bin = new(args with { Kernel = null });
            using PitchTrackingDbContext ptDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_READONLY_OPTIONS);
            List<PitchData> pitches = ptDb.PitchData
                .AsNoTracking()
                .Where(f => f.Year == year && f.LevelId == 1 && f.PitchClass != Db.DbEnums.PitchClass.Other
                    && ((f.CountBalls == 0 && f.CountStrike == 0) || (f.CountBalls == 2 && f.CountStrike == 2)))
                .ToList();
            Console.WriteLine($"Loaded {pitches.Count:N0} {year} pitches");
            if (Directory.Exists(PLOT_DIRECTORY))
            {
                Directory.Delete(PLOT_DIRECTORY, recursive: true);
            }
            Directory.CreateDirectory(PLOT_DIRECTORY);
            var groups = pitches.GroupBy(p => new { p.PitchClass, p.PitIsR, p.HitIsR, p.CountBalls, p.CountStrike });
            foreach (var group in groups)
            {
                List<PitchData> groupPitches = group.ToList();
                if (groupPitches.Count < MIN_GROUP_PITCHES)
                {
                    continue;
                }
                string pitStr = group.Key.PitIsR ? "RHP" : "LHP";
                string hitStr = group.Key.HitIsR ? "RHH" : "LHH";
                string count = $"{group.Key.CountBalls}-{group.Key.CountStrike}";
                string label = $"{year} {group.Key.PitchClass} {pitStr} vs {hitStr} {count} | n={groupPitches.Count:N0}";
                string path = Path.Combine(PLOT_DIRECTORY,
                    $"{year}_{group.Key.PitchClass}_{pitStr}_{hitStr}_{count}.png");
                PlotGroup(bin, groupPitches, label, path);
            }
        }

        // One plot: each cell's value relative to the group's pitch-weighted mean cell value.
        private static void PlotGroup(PitchZoneBin bin, List<PitchData> pitches, string label, string path)
        {
            var (rows, _) = bin.BuildCompetitiveRows(pitches);
            var shown = rows.Where(r => r.PitchCount >= MIN_CELL_PITCHES).ToList();
            if (shown.Count == 0)
            {
                return;
            }
            double totalPitches = shown.Sum(r => (double)r.PitchCount);
            double mean = shown.Sum(r => (double)r.ExpectedValueHitter * r.PitchCount) / totalPitches;
            Dictionary<PitchZone, (double Value, string Text)> cells = new();
            foreach (PitchZoneCompetitive row in shown)
            {
                double value = (row.ExpectedValueHitter - mean) * RUNS_SCALE;
                cells[new PitchZone(row.ZoneRow, row.ZoneCol)] =
                    (value, $"{value:+0.0;-0.0;0.0}\nn={row.PitchCount:N0}");
            }
            string fullLabel = $"{label} | mean {mean * RUNS_SCALE:F1} | green = better for hitter, vs mean";
            bin.PlotCellValues(cells, null, fullLabel, path);
        }
    }
}
