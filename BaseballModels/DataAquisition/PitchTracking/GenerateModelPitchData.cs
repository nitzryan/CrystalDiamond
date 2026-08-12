using Db;
using EFCore.BulkExtensions;
using MathNet.Numerics;
using Microsoft.EntityFrameworkCore;
using PitchTrackingDb;
using ShellProgressBar;
using static Db.DbEnums;

namespace DataAquisition.PitchTracking
{
    internal class GenerateModelPitchData
    {
        public static void Generate()
        {
            PitchTrackingDbContext pitchDb = new(PitchTrackingDb.Connection.PITCHTRACK_DB_OPTIONS);
            SqliteDbContext db = new(Db.Connection.DB_READONLY_OPTIONS);

            pitchDb.PitchData.ExecuteDelete();

            // Fetch data that will need to be queried
            var pitchStatcastDict = db
                .PitchStatcast
                .Select(f => new {f.GameId, f.PitchId, f.Year, f.LevelId, f.HitterId, f.PX, f.PZ, 
                        f.PitIsR, f.PitcherId, f.CountBalls, f.CountStrike, f.HitIsR,
                        f.Result, f.HadSwing, f.HadContact, f.IsInPlay, f.RunValueSmoothedHitter,
                        f.Extension, f.SpinRate, f.SpinDirection})
                .ToDictionary(
                    f => (f.GameId, f.PitchId),
                    f => f
                );

            Dictionary<(int year, int mlbId), (float zoneTop, float zoneBot)> hitterZoneDict = db
                .HitterYearZoneData
                .AsNoTracking()
                .ToDictionary(
                    f => (f.Year, f.MlbId),
                    f => (f.ZoneTop, f.ZoneBot)
                );


            // Get pitches from flightpath that are not already in PitchData                
            var pitches = pitchDb.PitchFlightpath.AsNoTracking();

            int pitchCount = pitchDb.PitchFlightpath.Count();
            if (pitchCount == 0)
                return;

            // Determine a linear relationship between plateX/HAA and plateZ/VAA
            var haaaaDict = GetHAAAADict(pitches);
            var vaaaaDict = GetVAAAADict(pitches);

            // Go through each pitch that has a flightpath (those without have been filtered out)
            List<PitchData> pitchData = new(pitchCount);
            using (ProgressBar progressBar = new(pitchCount, $"Generating Pitch Data for model"))
            { 
                foreach (var pitch in pitches)
                {
                    #pragma warning disable CS8629 // Null values filtered out

                    // Get processed data for that pitch
                    var ps = pitchStatcastDict[(pitch.GameId, pitch.PitchId)];
                    (float zoneTop, float zoneBot) = hitterZoneDict[(ps.Year, ps.HitterId)];

                    // Get VAA/HAA above averge
                    float haaaa = pitch.HAA - EvaluateRegression(ps.PX.Value, haaaaDict[new HAAKey(ps.PitIsR, pitch.PitchClass)]);
                    float vaaaa = pitch.VAA - EvaluateRegression(ps.PZ.Value, vaaaaDict[pitch.PitchClass]);

                    // Aggregate data into 1 location
                    pitchData.Add(new PitchData
                    {
                        GameId = ps.GameId,
                        PitchId = ps.PitchId,
                        Year = ps.Year,
                        LevelId = ps.LevelId,
                        PitcherId = ps.PitcherId,
                        PitchClass = pitch.PitchClass,
                        PitchType = pitch.PitchType,
                        CountBalls = ps.CountBalls,
                        CountStrike = ps.CountStrike,
                        PitIsR = ps.PitIsR,
                        HitIsR = ps.HitIsR,
                        Result = ps.Result,
                        HadSwing = ps.HadSwing,
                        HadContact = ps.HadContact,
                        IsInPlay = ps.IsInPlay,
                        RunValueInPlay = ps.IsInPlay ? ps.RunValueSmoothedHitter : 0,
                        Vel = pitch.Vel,
                        Extension = ps.Extension.Value,
                        BreakInduced = pitch.IVB,
                        BreakHorizontal = pitch.HB,
                        SpinRate = ps.SpinRate.Value,
                        SpinAxis = ps.SpinDirection.Value,
                        ActiveSpin = -1000,
                        VaaAboveAverage = vaaaa,
                        HaaAboveAverage = haaaa,
                        PlateX = ps.PX.Value,
                        PlateZ = ps.PZ.Value,
                        ZoneTop = zoneTop,
                        ZoneBot = zoneBot,
                    });
                    #pragma warning restore CS8629

                    progressBar.Tick();
                }
            }

            pitchDb.BulkInsert(pitchData);
        }

        // Use to calculate VAA/HAA above average for that pitch class and location
        public record HAAKey(bool pitIsR, PitchClass pitchClass);
        public record RegressionValues(float a, float b); // y = Ax + b

        private static Dictionary<HAAKey, RegressionValues> GetHAAAADict(IEnumerable<PitchFlightpath> flightpaths)
        {
            Dictionary<HAAKey, RegressionValues> d = new();

            var keys = flightpaths.GroupBy(f => new HAAKey(f.PitIsR, f.PitchClass));
            foreach(var k in keys)
            {
                double[] xs = k.Select(f => (double)f.PlateX).ToArray();
                double[] haa = k.Select(f => (double)f.HAA).ToArray();

                var (intercept, slope) = Fit.Line(xs, haa);
                d[new HAAKey(k.Key.pitIsR, k.Key.pitchClass)] = new RegressionValues((float)slope, (float)intercept);
            }

            return d;
        }

        private static Dictionary<PitchClass, RegressionValues> GetVAAAADict(IEnumerable<PitchFlightpath> flightpaths)
        {
            Dictionary<PitchClass, RegressionValues> d = new();

            var keys = flightpaths.GroupBy(f => f.PitchClass);
            foreach (var k in keys)
            {
                double[] zs = k.Select(f => (double)f.PlateZ).ToArray();
                double[] vaa = k.Select(f => (double)f.VAA).ToArray();

                var (intercept, slope) = Fit.Line(zs, vaa);
                d[k.Key] = new RegressionValues((float)slope, (float)intercept);
            }

            return d;
        }

        private static float EvaluateRegression(float x, RegressionValues rv)
        {
            return (rv.a * x) + rv.b;
        }
    }
}
