using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using PitchTrackingDb;
using ShellProgressBar;
using static Db.DbEnums;

namespace DataAquisition.PitchTracking
{
    internal class GenerateFlightpathDeltas
    {
        public static void Calculate(bool forceRefresh)
        {
            PitchTrackingDbContext pitchDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_OPTIONS);

            if (forceRefresh)
                pitchDb.PitchFlightpathGameDelta.ExecuteDelete();

            var pitcherGames = pitchDb.PitchFlightpath
                .GroupBy(f => new { f.GameId, f.PitcherId })
                .AsNoTracking()
                .ToList();

            List<PitchFlightpathGameDelta> deltas = new(pitchDb.PitchFlightpath.Count() * 3);
            using (ProgressBar progressBar = new(pitcherGames.Count, "Calculating Pitch Game Deltas"))
            {
                foreach (var pg in pitcherGames)
                {
                    // Get average for each type of fastball
                    var fastballs = pg
                        .Where(f => f.PitchClass == PitchClass.Fastball)
                        .GroupBy(f => f.PitchType)
                        .Select(f => new PitchFlightpath
                        {
                            GameId = -1,
                            PitchId = -1,
                            Year = -1,
                            PitcherId = -1,
                            PitchClass = PitchClass.Fastball,
                            PitchType = f.First().PitchType,
                            BreakHoriz_05 = f.Average(g => g.BreakHoriz_05),
                            BreakVer_05 = f.Average(g => g.BreakVer_05),
                            BreakHoriz_10 = f.Average(g => g.BreakHoriz_10),
                            BreakVer_10 = f.Average(g => g.BreakVer_10),
                            BreakHoriz_15 = f.Average(g => g.BreakHoriz_15),
                            BreakVer_15 = f.Average(g => g.BreakVer_15),
                            BreakHoriz_20 = f.Average(g => g.BreakHoriz_20),
                            BreakVer_20 = f.Average(g => g.BreakVer_20),
                            BreakHoriz_25 = f.Average(g => g.BreakHoriz_25),
                            BreakVer_25 = f.Average(g => g.BreakVer_25),
                            HAA = -1000,
                            VAA = -1000,
                            TrackingError = -1000,
                            HB = f.Average(g => g.HB),
                            IVB = f.Average(g => g.IVB),
                            VB = f.Average(g => g.VB),
                            Vel = f.Average(g => g.Vel),
                        });

                    // Compare each non-fastball to each type of fastball
                    var nonfastballs = pg.Where(f => f.PitchClass != PitchClass.Fastball);
                    foreach (var fastball in fastballs)
                    {
                        foreach (var pitch in nonfastballs)
                        {
                            deltas.Add(new PitchFlightpathGameDelta
                            {
                                GameId = pitch.GameId,
                                PitchId = pitch.PitchId,
                                PitcherId = pitch.PitcherId,
                                FastballPitchType = fastball.PitchType,
                                BreakHoriz_05Delta = pitch.BreakHoriz_05 - fastball.BreakHoriz_05,
                                BreakVer_05Delta = pitch.BreakVer_05 - fastball.BreakVer_05,
                                BreakHoriz_10Delta = pitch.BreakHoriz_10 - fastball.BreakHoriz_10,
                                BreakVer_10Delta = pitch.BreakVer_10 - fastball.BreakVer_10,
                                BreakHoriz_15Delta = pitch.BreakHoriz_15 - fastball.BreakHoriz_15,
                                BreakVer_15Delta = pitch.BreakVer_15 - fastball.BreakVer_15,
                                BreakHoriz_20Delta = pitch.BreakHoriz_20 - fastball.BreakHoriz_20,
                                BreakVer_20Delta = pitch.BreakVer_20 - fastball.BreakVer_20,
                                BreakHoriz_25Delta = pitch.BreakHoriz_25 - fastball.BreakHoriz_25,
                                BreakVer_25Delta = pitch.BreakVer_25 - fastball.BreakVer_25,
                                BreakHoriz_Delta = pitch.HB - fastball.HB,
                                BreakVert_Delta = pitch.VB - fastball.VB,
                                BreakIVB_Delta = pitch.IVB - fastball.IVB,
                                Vel_Delta = pitch.Vel - fastball.Vel,
                            });
                        }
                    }

                    progressBar.Tick();
                }
            }

            pitchDb.BulkInsert(deltas);
        }
    }
}
