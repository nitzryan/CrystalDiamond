using Db;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;
using System.Reflection;

namespace DataAquisition
{
    internal class PitchAggregation
    {
        public static void CreatePitcherGameBaselines(int year)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            var gamePitcherPitches = db.PitchStatcast.Where(
                f => f.Year == year)
                .GroupBy(f => new { f.PitcherId, f.GameId });

            List<PitcherStatcastGame> gameAverages = new(gamePitcherPitches.Count());
            using (ProgressBar progressBar = new(gamePitcherPitches.Count(), $"Generating Pitch Game Averages for {year}"))
            {
                foreach (var gpp in gamePitcherPitches)
                {
                    progressBar.Tick();

                    if (db.PitcherStatcastGame.Any(f => f.MlbId == gpp.Key.PitcherId && f.GameId == gpp.Key.GameId))
                        continue;

                    float? fastballVelo = null, fastballBreakHoriz = null, fastballBreakVert = null, fastballBreakInduced = null;
                    float? sinkerVelo = null, sinkerBreakHoriz = null, sinkerBreakVert = null, sinkerBreakInduced = null;

                    var fastballs = gpp.Where(f => f.PitchType == DbEnums.PitchType.Fourseam || f.PitchType == DbEnums.PitchType.Fastball);
                    var sinkers = gpp.Where(f => f.PitchType == DbEnums.PitchType.Sinker || f.PitchType == DbEnums.PitchType.Twoseam);

                    if (fastballs.Any())
                    {
                        fastballVelo = fastballs.Average(f => f.VStart);
                        fastballBreakHoriz = fastballs.Average(f => f.BreakHorizontal);
                        fastballBreakInduced = fastballs.Average(f => f.BreakInduced);
                        fastballBreakVert = fastballs.Average(f => f.BreakVertical);
                    }

                    if (sinkers.Any())
                    {
                        sinkerVelo = sinkers.Average(f => f.VStart);
                        sinkerBreakHoriz = sinkers.Average(f => f.BreakHorizontal);
                        sinkerBreakInduced = sinkers.Average(f => f.BreakInduced);
                        sinkerBreakVert = sinkers.Average(f => f.BreakVertical);
                    }

                    gameAverages.Add(new PitcherStatcastGame
                    {
                        MlbId = gpp.Key.PitcherId,
                        GameId = gpp.Key.GameId,
                        Year = gpp.First().Year,
                        Month = gpp.First().Month,
                        LevelId = gpp.First().LevelId,
                        FastballVelo = fastballVelo,
                        FastballBreakHoriz = fastballBreakHoriz,
                        FastballBreakInduced = fastballBreakInduced,
                        FastballBreakVert = fastballBreakVert,
                        SinkerVelo = sinkerVelo,
                        SinkerBreakHoriz = sinkerBreakHoriz,
                        SinkerBreakInduced = sinkerBreakInduced,
                        SinkerBreakVert = sinkerBreakVert,
                    });
                }
            }

            db.BulkInsert(gameAverages);
        }

        public static void CreateLeagueDateAverages(int year, int month)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            db.PitchDateAverages.Where(f => f.Year == year && f.Month == month).ExecuteDelete();

            List<PitchStatcast> monthPitches = db.PitchStatcast
                .Where(f => f.Year == year 
                && f.Month == month 
                && f.LevelId == 1
                && f.Extension != null
                && f.VStart != null
                && f.BreakInduced != null
                && f.BreakHorizontal != null).ToList();

            if (monthPitches.Count == 0)
                return;
            
            var fastballs = monthPitches.Where(f => f.PitchType == DbEnums.PitchType.Fastball || f.PitchType == DbEnums.PitchType.Fourseam);
            var sinkers = monthPitches.Where(f => f.PitchType == DbEnums.PitchType.Sinker || f.PitchType == DbEnums.PitchType.Twoseam);
            var curveballs = monthPitches.Where(f => f.PitchType == DbEnums.PitchType.Curveball || f.PitchType == DbEnums.PitchType.KnuckleCurve || f.PitchType == DbEnums.PitchType.SlowCurve || f.PitchType == DbEnums.PitchType.Slurve);

            try {
                db.PitchDateAverages.Add(new PitchDateAverages
                {
                    Year = year,
                    Month = month,
                    Extension = monthPitches.Average(f => f.Extension) ?? throw new ArgumentNullException("No Non-Null Pitches for Extension"),

                    FastballVelo = fastballs.Average(f => f.VStart) ?? throw new ArgumentNullException("No Non-Null Pitches for Fastball Velo"),
                    Fastball4SeamVert = fastballs.Average(f => f.BreakInduced) ?? throw new ArgumentNullException("No Non-Null Pitches for Fastball Induced"),
                    Fastball4SeamHoriz = fastballs.Average(f => f.PitIsR ? f.BreakHorizontal : -f.BreakHorizontal) ?? throw new ArgumentNullException("No Non-Null Pitches for Fastball Horizontal"),
                    FastballCount = fastballs.Count(),

                    SinkerVelo = sinkers.Average(f => f.VStart) ?? throw new ArgumentNullException("No Non-Null Pitches for Sinker Velo"),
                    SinkerVert = sinkers.Average(f => f.BreakInduced) ?? throw new ArgumentNullException("No Non-Null Pitches for Sinker Induced"),
                    SinkerHoriz = sinkers.Average(f => f.PitIsR ? f.BreakHorizontal : -f.BreakHorizontal) ?? throw new ArgumentNullException("No Non-Null Pitches for Sinker Horizontal"),
                    SinkerCount = sinkers.Count(),

                    CurveballVelo = curveballs.Average(f => f.VStart) ?? throw new ArgumentNullException("No Non-Null Pitches for Curveball Velo"),
                    CurveballVert = curveballs.Average(f => f.BreakInduced) ?? throw new ArgumentNullException("No Non-Null Pitches for Curveball Induced"),
                    CurveballHoriz = curveballs.Average(f => f.PitIsR ? f.BreakHorizontal : -f.BreakHorizontal) ?? throw new ArgumentNullException("No Non-Null Pitches for Curveball Horizontal"),
                    CurveballCount = curveballs.Count(),
                });
            } catch(ArgumentNullException ane)
            {
                Console.WriteLine($"Null pitch data for {year}-{month}: {ane.Message}");
            }
            

            db.SaveChanges();
        }
    }
}
