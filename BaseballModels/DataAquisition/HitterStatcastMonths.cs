using Db;
using static Db.DbEnums;
using Microsoft.EntityFrameworkCore;
using ShellProgressBar;
using EFCore.BulkExtensions;

namespace DataAquisition
{
    internal class HitterStatcastMonths
    {
        private record HitterPitchStatcastData(
            int mlbId, 
            PitchType pitchType, 
            bool hadSwing, 
            bool hadContact, 
            float pX,
            float pZ,
            float zoneTop,
            float zoneBot,
            float? launchSpeed);

        private static bool IsInZone(HitterPitchStatcastData pitch)
        {
            const float PlateHalfWidth = 17f / 2f / 12f;   // 0.708333 ft
            const float BallRadius = 1.45f / 12f;      // 0.120833 ft

            float left = -PlateHalfWidth;
            float right = PlateHalfWidth;
            float bottom = pitch.zoneBot;
            float top = pitch.zoneTop;

            // Find the closest point on the strike-zone rectangle to the ball center
            float closestX = Math.Max(left, Math.Min(pitch.pX, right));
            float closestZ = Math.Max(bottom, Math.Min(pitch.pZ, top));

            float dx = pitch.pX - closestX;
            float dz = pitch.pZ - closestZ;

            // True if the distance from center to closest point ≤ ball radius
            return ((dx * dx) + (dz * dz)) <= (BallRadius * BallRadius);
        }

        private static (List<HitterPitchStatcastData>, List<HitterPitchStatcastData>) GetInZoneOutZonePitches
            (List<HitterPitchStatcastData> data)
        {
            List<HitterPitchStatcastData> inZoneData = [];
            List<HitterPitchStatcastData> outZoneData = [];

            data.ForEach(f =>
            {
                if (IsInZone(f))
                    inZoneData.Add(f);
                else
                    outZoneData.Add(f);
            });

            return (inZoneData, outZoneData);
        }

        private static HitterStatcastMonth GetMonthData(List<HitterPitchStatcastData> data, int year, int month)
        {
            List<HitterPitchStatcastData> inPlayEvents = data
                .Where(f => f.launchSpeed != null)
                .ToList();

            List<HitterPitchStatcastData> swungEvents = data
                .Where(f => f.hadSwing)
                .ToList();

            (var inZoneData, var outZoneData) = GetInZoneOutZonePitches(data);

            int inZoneSwingCount = inZoneData.Where(f => f.hadSwing).Count();

            List<HitterPitchStatcastData> fastballs = data
                .Where(f => f.pitchType == PitchType.Fastball ||
                    f.pitchType == PitchType.Sinker ||
                    f.pitchType == PitchType.Fourseam ||
                    f.pitchType == PitchType.Twoseam)
                .ToList();

            List<HitterPitchStatcastData> changeups = data
                .Where(f => f.pitchType == PitchType.Changeup ||
                    f.pitchType == PitchType.Splitter ||
                    f.pitchType == PitchType.Forkball)
                .ToList();

            List<HitterPitchStatcastData> breakingBalls = data
                .Where(f => f.pitchType == PitchType.Cutter ||
                    f.pitchType == PitchType.Slider ||
                    f.pitchType == PitchType.Sweeper ||
                    f.pitchType == PitchType.Slurve ||
                    f.pitchType == PitchType.Curveball ||
                    f.pitchType == PitchType.KnuckleCurve ||
                    f.pitchType == PitchType.Screwball ||
                    f.pitchType == PitchType.SlowCurve)
                .ToList();

            HitterStatcastMonth hsm = new HitterStatcastMonth
            {
                MlbId = data.First().mlbId,
                Year = year,
                Month = month,
                BattedBallEvents = inPlayEvents.Count,
                AvgExitVelo = inPlayEvents.Average(f => f.launchSpeed) ?? 70.0f,
                PeakExitVelo = inPlayEvents.Max(f => f.launchSpeed) ?? 100.0f,
                NumPitches = data.Count(),
                NumSwings = swungEvents.Count,
                ChasePerc = Utilities.SafeDivide(outZoneData.Where(f => f.hadSwing).Count(),
                    outZoneData.Count),
                WhiffPerc = Utilities.SafeDivide(swungEvents.Where(f => !f.hadContact).Count(),
                    swungEvents.Count, 0.3f),
                ZoneSwingPerc = Utilities.SafeDivide(inZoneSwingCount, inZoneData.Count),
                ZoneContactPerc = Utilities.SafeDivide(inZoneData.Where(f => f.hadSwing && f.hadContact).Count(),
                    inZoneSwingCount),
                NumFastballs = fastballs.Count,
                FastballContactPerc = Utilities.SafeDivide(fastballs.Where(f => f.hadContact).Count(),
                    fastballs.Where(f => f.hadSwing).Count(), 0.3f),
                NumBreaking = breakingBalls.Count,
                BreakingContactPerc = Utilities.SafeDivide(breakingBalls.Where(f => f.hadContact).Count(),
                    breakingBalls.Where(f => f.hadSwing).Count(), 0.3f),
                NumChangeup = changeups.Count,
                ChangeupContactPerc = Utilities.SafeDivide(changeups.Where(f => f.hadContact).Count(),
                    changeups.Where(f => f.hadSwing).Count(), 0.3f),
            };

            return hsm;
        }

        public static void Update(int month, int year)
        {
            using SqliteDbContext db = new(Constants.DB_OPTIONS);

            db.HitterStatcastMonth
                .Where(f => f.Year == year && f.Month == month)
                .ExecuteDelete();

            // Calculate league averages of stats
            var hitterSelection = db.PitchStatcast
                .Where(f => f.LevelId == 1 && f.Year == year
                    && f.PX != null && f.PZ != null
                    && f.ZoneTop != null && f.ZoneBot != null);
            if (month == 4)
                hitterSelection = hitterSelection.Where(f => f.Month <= 4);
            else if (month == 9)
                hitterSelection = hitterSelection.Where(f => f.Month >= 9);
            else
                hitterSelection = hitterSelection.Where(f => f.Month == month);

            #pragma warning disable CS8629 // Null values filtered out by previous query
            List<HitterPitchStatcastData> mlbHitterData =
                hitterSelection
                .Select(f => new HitterPitchStatcastData(
                    f.HitterId,
                    f.PitchType,
                    f.HadSwing,
                    f.HadContact,
                    f.PX.Value,
                    f.PZ.Value,
                    f.ZoneTop.Value,
                    f.ZoneBot.Value,
                    f.LaunchSpeed
                )).ToList();
            #pragma warning restore CS8629

            if (mlbHitterData.Count == 0)
                return;

            // Get avg for MLB
            HitterStatcastMonth mlbAvgData = GetMonthData(mlbHitterData, year, month);

            // Peak exit velocity will be 99th percentile to avoid a singular outlier event
            #pragma warning disable CS8629 // Null values filtered out by previous query
            List<float> mlbExitVelos = mlbHitterData
                .Where(f => f.launchSpeed != null)
                .Select(f => f.launchSpeed.Value)
                .OrderDescending()
                .ToList();
            #pragma warning restore CS8629
            mlbAvgData.PeakExitVelo = mlbExitVelos[(int)Math.Round(0.01f * mlbExitVelos.Count)];

            // Get MLB and MiLB data
            var hitterData = db.PitchStatcast
                .Where(f => f.Year == year
                    && f.PX != null && f.PZ != null
                    && f.ZoneTop != null && f.ZoneBot != null);
            if (month == 4)
                hitterData = hitterData.Where(f => f.Month <= 4);
            else if (month == 9)
                hitterData = hitterData.Where(f => f.Month >= 9);
            else
                hitterData = hitterData.Where(f => f.Month == month);
            
            #pragma warning disable CS8629 // Null values filtered out by previous query
            List<HitterPitchStatcastData> allHitterData =
                hitterData
                .Select(f => new HitterPitchStatcastData(
                    f.HitterId,
                    f.PitchType,
                    f.HadSwing,
                    f.HadContact,
                    f.PX.Value,
                    f.PZ.Value,
                    f.ZoneTop.Value,
                    f.ZoneBot.Value,
                    f.LaunchSpeed
                )).ToList();
            #pragma warning restore CS8629

            // Iterate through each player
            var allHitterGroupings = allHitterData.GroupBy(f => f.mlbId);
            int count = allHitterGroupings.Count();
            List<HitterStatcastMonth> dbData = new(count);
            using (ProgressBar progressBar = new(count, $"Generating Hitter Statcast Data for {month}-{year}"))
            {
                foreach (var hitter in allHitterGroupings)
                {
                    HitterStatcastMonth rawData = GetMonthData(hitter.ToList(), year, month);

                    // Normalize data
                    rawData.AvgExitVelo = Utilities.SafeDivide(rawData.AvgExitVelo, mlbAvgData.AvgExitVelo);
                    rawData.PeakExitVelo = Utilities.SafeDivide(rawData.PeakExitVelo, mlbAvgData.PeakExitVelo);

                    rawData.ChasePerc = Utilities.SafeDivide(rawData.ChasePerc, mlbAvgData.ChasePerc);
                    rawData.WhiffPerc = Utilities.SafeDivide(rawData.WhiffPerc, mlbAvgData.WhiffPerc);
                    rawData.ZoneSwingPerc = Utilities.SafeDivide(rawData.ZoneSwingPerc, mlbAvgData.ZoneSwingPerc);
                    rawData.ZoneContactPerc = Utilities.SafeDivide(rawData.ZoneContactPerc, mlbAvgData.ZoneContactPerc);

                    rawData.FastballContactPerc = Utilities.SafeDivide(rawData.FastballContactPerc, mlbAvgData.FastballContactPerc);
                    rawData.BreakingContactPerc = Utilities.SafeDivide(rawData.BreakingContactPerc, mlbAvgData.BreakingContactPerc);
                    rawData.ChangeupContactPerc = Utilities.SafeDivide(rawData.ChangeupContactPerc, mlbAvgData.ChangeupContactPerc);

                    dbData.Add(rawData);
                    progressBar.Tick();
                }
            }

            db.BulkInsert(dbData);
        }
    }
}
