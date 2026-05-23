using Db;
using PitchDb;
using System.Drawing.Drawing2D;
using System.Text.Json;

namespace UI
{
    public enum PitchValueType
    { 
        Actual,
        Stuff,
        Location,
        Exp,
    }

    public enum PitchGridType
    {
        _3x3_Shadow,
        _3x3,
        _5x5
    }

    public class PitchStats
    {
        public int PA { get; set; } = 0;
        public int AB { get; set; } = 0;
        public int Hit1B { get; set; } = 0;
        public int Hit2B { get; set; } = 0;
        public int Hit3B { get; set; } = 0;
        public int HitHR { get; set; } = 0;
        public int BB { get; set; } = 0;
        public int HBP { get; set; } = 0;
        public int K { get; set; } = 0;

        public float AVG { get; private set; } = 0;
        public float OBP { get; private set; } = 0;
        public float SLG { get; private set; } = 0;
        public float OPS { get; private set; } = 0;

        public int Pitches { get; set; } = 0;
        public int Swings { get; set; } = 0;
        public int Whiffs { get; set; } = 0;
        public int Fouls { get; set; } = 0;
        public int InPlay { get; set; } = 0;
        public int CalledStrikes { get; set; } = 0;
        public int Balls { get; set; } = 0;
        public int Other { get; set; } = 0;

        public float WhiffRate { get; private set; } = 0;
        public float CSWRate { get; private set; } = 0;

        public float Vel { get; set; } = 0;
        public float BreakHoriz { get; set; } = 0;
        public float BreakVert { get; set; } = 0;

        public float StuffPlus { get; set; } = 0;
        public float LocPlus { get; set; } = 0;
        public float PitchPlus { get; set; } = 0;
        public float ActualPlus { get; set; } = 0;

        public void CalculateStats()
        {
            // Triple Slash
            if (AB > 0)
            {
                AVG = (float)(Hit1B + Hit2B + Hit3B + HitHR) / AB;
                SLG = (float)(Hit1B + (2 * Hit2B) + (3 * Hit3B) + (4 * HitHR)) / AB;
            }

            if (PA > 0)
                OBP = (float)(Hit1B + Hit2B + Hit3B + HitHR + BB + HBP) / PA;

            OPS = OBP + SLG;

            // Pitch Stats
            if (Swings > 0)
                WhiffRate = (float)Whiffs / Swings;

            if (Pitches > 0)
                CSWRate = (float)(Whiffs + CalledStrikes) / Pitches;
        }
    }

    public class PitchBox
    { 
        public required int NumPitches { get; set; }
        public required float StuffValue { get; set; }
        public required float LocValue { get; set; }
        public required float ExpValue { get; set; }
        public required float ActValue { get; set; }
        public required float Dev { get; set; }

        public required float X { get; set; }
        public required float Y { get; set; }

        public required float Width { get; set; }
        public required float Height { get; set; }

        public required float? FixedDeltaXZone { get; set; }
        public required float? FixedDeltaYZone { get; set; }

        public PitchStats Stats { get; set; } = new();

        public Color GetColor(PitchValueType valueType, float min, float max)
        {
            float value = 0;
            switch (valueType)
            {
                case PitchValueType.Actual:
                    value = ActValue;
                    break;
                case PitchValueType.Exp:
                    value = ExpValue;
                    break;
                case PitchValueType.Stuff:
                    value = StuffValue;
                    break;
                case PitchValueType.Location:
                    value = LocValue;
                    break;
            }
            float clampedValue = Math.Clamp(value, min, max);

            float h, s, v;

            if (clampedValue <= 0)
            {
                // Blue -> Light Gray
                float t = clampedValue / min;
                h = 240f;
                s = t;
                v = 0.85f + (t * 0.15f);
            }
            else
            {
                // Light Gray -> Red
                float t = clampedValue / max;
                h = 0f;
                s = t;
                v = 0.85f + (t * 0.15f);
            }

            return HsvToColor(h, s, v);
        }

        internal Color HsvToColor(float h, float s, float v, byte alpha = 255)
        {
            h = h % 360;
            if (h < 0) h += 360;

            float c = v * s;
            float x = c * (1 - Math.Abs(((h / 60) % 2) - 1));
            float m = v - c;

            float r = 0, g = 0, b = 0;

            if (h < 60) { r = c; g = x; b = 0; }
            else if (h < 120) { r = x; g = c; b = 0; }
            else if (h < 180) { r = 0; g = c; b = x; }
            else if (h < 240) { r = 0; g = x; b = c; }
            else if (h < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return Color.FromArgb(alpha,
                (byte)((r + m) * 255),
                (byte)((g + m) * 255),
                (byte)((b + m) * 255));
        }
    }

    public record YearLeagueDevKey(int modelId, int year, int balls, int strikes);

    public class PitchGrid
    {
        public List<List<PitchBox>> PitchBoxes;
        public PitchValueType PitchValueType;
        public PitchGridType PitchGridType;
        public float Scale;
        public int FilterSize;
        public int ModelId;
        private static float BALL_RADIUS = 0.12f;
        PitchBox? HighlightedBox;
        PitchStats OverallStats = new();

        public static Dictionary<YearLeagueDevKey, YearLeagueDeviations> YldDict = new();

        public PitchGrid(
            IEnumerable<PitchStatcast> pitches,
            int modelId,
            PitchValueType pitchValueType,
            PitchGridType pitchGridType,
            decimal scale,
            float zoneTop,
            float zoneBot,
            float zoneLeft,
            float zoneRight,
            int filterSize
            )
        {
            PitchBoxes = new();
            PitchValueType = pitchValueType;
            PitchGridType = pitchGridType;
            ModelId = modelId;
            Scale = (float)scale;
            FilterSize = filterSize;
            HighlightedBox = null;

            pitches = pitches.Where(f => f.PX != null && f.PZ != null);
            PitchBoxes = PitchGrid.GetPitchBoxes(pitchGridType, zoneTop, zoneBot, zoneLeft, zoneRight);

            List<float> xs = PitchBoxes.First().Select(f => f.X).Order().ToList();
            List<float> widths = PitchBoxes.First().OrderBy(f => f.X).Select(f => f.Width).ToList();

            // Go through all pitches and map to a box
            foreach (var pitch in pitches)
            {
                #pragma warning disable CS8629 // Filtered at beginning of function
                float pitchX = pitch.PX.Value;
                float pitchY = pitch.PZ.Value;
                #pragma warning restore CS8629

                // Determine X and Y bin
                int? xBin = null;
                for (int i = 0; i < xs.Count; i++)
                {

                    if (Math.Abs(xs[i] - pitchX) < (widths[i] / 2.0f))
                    {
                        xBin = i;
                        break;
                    }

                }

                // Calculate heights of y bins
                #pragma warning disable CS8629 // Will not be null
                float pitchZoneTop = pitch.ZoneTop.Value + BALL_RADIUS;
                float pitchZoneBot = pitch.ZoneBot.Value - BALL_RADIUS;
                #pragma warning restore CS8629

                var (pitchYs, _, pitchFixedY) = GetGridCenters(PitchGridType, pitchZoneTop, pitchZoneBot, zoneLeft, zoneRight);
                List<float> pitchHeights = ComputeZoneSizes(pitchYs, pitchFixedY);

                // Calculate Y Bin
                int? yBin = null;
                for (int i = 0; i < pitchYs.Count; i++)
                {

                    if (Math.Abs(pitchYs[i] - pitchY) < (pitchHeights[i] / 2.0f))
                    {
                        yBin = i;
                        break;
                    }

                }

                // Assign extreme bins
                if (xBin == null)
                {
                    if (pitchX < xs[0])
                        xBin = 0;
                    else
                        xBin = xs.Count - 1;
                }
                if (yBin == null)
                {
                    if (pitchY < pitchYs[0])
                        yBin = 0;
                    else
                        yBin = pitchYs.Count - 1;
                }

                // Get model output
                #pragma warning disable CS8600, CS8602 // Non Null References, if wrong will except
                Dictionary<int, PitchAnalysis.PitchModelOutput> modelJson =
                    JsonSerializer.Deserialize<Dictionary<int, PitchAnalysis.PitchModelOutput>>(pitch.ModelOutput);

                var pitchModelOutput = modelJson[modelId];
                #pragma warning restore CS8600, CS8602

                // Assign to bin
                PitchBox bin = PitchBoxes[yBin.Value][xBin.Value];

                bin.NumPitches++;
                bin.ExpValue += (float)pitchModelOutput.cv;
                bin.ActValue += pitch.RunValueSmoothedHitter;
                bin.StuffValue += (float)pitchModelOutput.sv;
                bin.LocValue += (float)pitchModelOutput.lv;
                bin.Dev += YldDict[new YearLeagueDevKey(ModelId, pitch.Year, pitch.CountBalls, pitch.CountStrike)].StuffDev;

                #pragma warning disable CS8629 // Any values here will have these values
                bin.Stats.Vel += pitch.VStart.Value;
                bin.Stats.BreakHoriz += pitch.BreakHorizontal.Value;
                bin.Stats.BreakVert += pitch.BreakInduced.Value;
                #pragma warning restore CS8629

                // Update stats from Pitch
                bin.Stats.Pitches++;
                switch(pitch.Result)
                {
                    case DbEnums.PitchResult.CalledStrike:
                        bin.Stats.CalledStrikes++;

                        if (pitch.CountStrike == 2)
                        {
                            bin.Stats.AB++;
                            bin.Stats.PA++;
                            bin.Stats.K++;
                        }
                        break;
                    case DbEnums.PitchResult.SwingingStrike:
                        bin.Stats.Swings++;
                        bin.Stats.Whiffs++;

                        if (pitch.CountStrike == 2)
                        {
                            bin.Stats.AB++;
                            bin.Stats.PA++;
                            bin.Stats.K++;
                        }
                        break;
                    case DbEnums.PitchResult.Foul:
                        bin.Stats.Swings++;
                        bin.Stats.Fouls++;
                        break;
                    case DbEnums.PitchResult.Ball:
                        bin.Stats.Balls++;

                        if (pitch.CountBalls == 3)
                        {
                            bin.Stats.PA++;
                            bin.Stats.BB++;
                        }
                        break;
                    case DbEnums.PitchResult.HBP:
                        bin.Stats.Balls++;
                        bin.Stats.PA++;
                        bin.Stats.HBP++;
                        break;
                    case DbEnums.PitchResult.InPlay:
                        bin.Stats.Swings++;
                        bin.Stats.PA++;
                        bin.Stats.InPlay++;
                        var pa = pitch.PaResult;

                        // Strikeout / BB / HBP should never be here
                        if (pa.HasFlag(DbEnums.PitchPaResult.Strikeout) ||
                            pa.HasFlag(DbEnums.PitchPaResult.BB) ||
                            pa.HasFlag(DbEnums.PitchPaResult.HBP))
                        {
                            throw new Exception($"Unexpected PaResult on InPlay pitch: {pa}");
                        }

                        // AB logic
                        if (pa.HasFlag(DbEnums.PitchPaResult.Error) ||
                            pa.HasFlag(DbEnums.PitchPaResult.Out) ||
                            pa.HasFlag(DbEnums.PitchPaResult.Groundout) ||
                            pa.HasFlag(DbEnums.PitchPaResult.GIDP))
                        {
                            bin.Stats.AB++;
                        }
                        else if (pa.HasFlag(DbEnums.PitchPaResult.Flyout))
                        {
                            if (pitch.PaResultDirectRuns == 0) // Not a sacrifice fly
                                bin.Stats.AB++;
                        }
                        else if (pa.HasFlag(DbEnums.PitchPaResult.Hit1B))
                        {
                            bin.Stats.AB++;
                            bin.Stats.Hit1B++;
                        }
                        else if (pa.HasFlag(DbEnums.PitchPaResult.Hit2B))
                        {
                            bin.Stats.AB++;
                            bin.Stats.Hit2B++;
                        }
                        else if (pa.HasFlag(DbEnums.PitchPaResult.Hit3B))
                        {
                            bin.Stats.AB++;
                            bin.Stats.Hit3B++;
                        }
                        else if (pa.HasFlag(DbEnums.PitchPaResult.HitHR))
                        {
                            bin.Stats.AB++;
                            bin.Stats.HitHR++;
                        }
                        else if (pa.HasFlag(DbEnums.PitchPaResult.Other))
                        {
                            bin.Stats.Other++;
                        }
                    break;
                }
            }

            // Get per-pitch values (or per 1000 for value)
            float actValue = PitchBoxes.Sum((List<PitchBox>f) => f.Sum((PitchBox g) => g.ActValue));
            float stuffValue = PitchBoxes.Sum((List<PitchBox> f) => f.Sum((PitchBox g) => g.StuffValue));
            float locValue = PitchBoxes.Sum((List<PitchBox> f) => f.Sum((PitchBox g) => g.LocValue));
            float pitchValue = PitchBoxes.Sum((List<PitchBox> f) => f.Sum((PitchBox g) => g.ExpValue));
            float devValue = PitchBoxes.Sum((List<PitchBox> f) => f.Sum((PitchBox g) => g.Dev));

            PitchBoxes.ForEach(g => g.ForEach(f =>
            {
                f.ActValue = f.ActValue / f.NumPitches * 1000;
                f.ExpValue = f.ExpValue / f.NumPitches * 1000;
                f.StuffValue = f.StuffValue / f.NumPitches * 1000;
                f.LocValue = f.LocValue / f.NumPitches * 1000;
                f.Dev = f.Dev / f.NumPitches * 1000;

                f.Stats.StuffPlus = 100 - (10 * f.StuffValue / f.Dev);
                f.Stats.LocPlus = 100 - (10 * f.LocValue / f.Dev);
                f.Stats.PitchPlus = 100 - (10 * f.ExpValue / f.Dev);
                f.Stats.ActualPlus = 100 - (10 * f.ActValue / f.Dev);

                OverallStats.Vel += f.Stats.Vel;
                OverallStats.BreakHoriz += f.Stats.BreakHoriz;
                OverallStats.BreakVert += f.Stats.BreakVert;

                f.Stats.Vel /= f.NumPitches;
                f.Stats.BreakHoriz /= f.NumPitches;
                f.Stats.BreakVert /= f.NumPitches;

                // Update Stats
                f.Stats.CalculateStats();

                // Append to overall stats
                OverallStats.PA += f.Stats.PA;
                OverallStats.AB += f.Stats.AB;
                OverallStats.Hit1B += f.Stats.Hit1B;
                OverallStats.Hit2B += f.Stats.Hit2B;
                OverallStats.Hit3B += f.Stats.Hit3B;
                OverallStats.HitHR += f.Stats.HitHR;
                OverallStats.BB += f.Stats.BB;
                OverallStats.HBP += f.Stats.HBP;
                OverallStats.K += f.Stats.K;

                OverallStats.Pitches += f.Stats.Pitches;
                OverallStats.Swings += f.Stats.Swings;
                OverallStats.Whiffs += f.Stats.Whiffs;
                OverallStats.Fouls += f.Stats.Fouls;
                OverallStats.InPlay += f.Stats.InPlay;
                OverallStats.CalledStrikes += f.Stats.CalledStrikes;
                OverallStats.Balls += f.Stats.Balls;
                OverallStats.Other += f.Stats.Other;
            }));

            if (OverallStats.Pitches > 0)
            {
                OverallStats.Vel /= OverallStats.Pitches;
                OverallStats.BreakHoriz /= OverallStats.Pitches;
                OverallStats.BreakVert /= OverallStats.Pitches;

                OverallStats.StuffPlus = 100 - (10 * stuffValue / devValue);
                OverallStats.LocPlus = 100 - (10 * locValue / devValue);
                OverallStats.PitchPlus = 100 - (10 * pitchValue / devValue);
                OverallStats.ActualPlus = 100 - (10 * actValue / devValue);
            }
            OverallStats.CalculateStats();
        }

        public PitchStats GetPitchStats()
        {
            if (HighlightedBox != null)
                return HighlightedBox.Stats;
            return OverallStats;
        }

        private static List<List<PitchBox>> GetPitchBoxes(
            PitchGridType gridType,
            float zoneTop,
            float zoneBot,
            float zoneLeft,
            float zoneRight
        )
        {
            var (ys, xs, fixedZones) = GetGridCenters(gridType, zoneTop, zoneBot, zoneLeft, zoneRight);
            List<float> rowHeights = ComputeZoneSizes(ys, fixedZones);
            List<float> columnWidths = ComputeZoneSizes(xs, fixedZones);

            List<List<PitchBox>> pitches = [];
            for (int j = 0; j < ys.Count; j++)
            {
                List<PitchBox> pitchRow = [];
                for (int i = 0; i < xs.Count; i++) 
                {
                    PitchBox pb = new()
                    {
                        NumPitches = 0,
                        ActValue = 0,
                        StuffValue = 0,
                        LocValue = 0,
                        ExpValue = 0,
                        Dev=0,
                        X = xs[i],
                        Y = ys[j],
                        Height = rowHeights[j],
                        Width = columnWidths[i],
                        FixedDeltaXZone = fixedZones[i],
                        FixedDeltaYZone = fixedZones[j],
                    };

                    pitchRow.Add(pb);
                }

                pitches.Add(pitchRow);
            }
            return pitches;
        }

        private static (List<float> yCenters, List<float> xCenters, List<float?> fixedFlags)
        GetGridCenters(
            PitchGridType gridType,
            float zoneTop,
            float zoneBot,
            float zoneLeft,
            float zoneRight)
        {
            const float SHADOW_SIZE = 0.40f;
            const float SHADOW_SIZE_3X3 = 0.25f;
            const float HALF_SHADOW = SHADOW_SIZE / 2;
            const float OUTER_ZONE_LOC = 0.9f;

            List<float> ys = gridType switch
            {
                PitchGridType._3x3 => [
                                    zoneBot - OUTER_ZONE_LOC,
                                    zoneBot - HALF_SHADOW,
                                    ((1.0f / 6.0f) * zoneTop) + ((5.0f / 6.0f) * zoneBot),
                                    0.5f * (zoneTop + zoneBot),
                                    ((1.0f / 6.0f) * zoneBot) + ((5.0f / 6.0f) * zoneTop),
                                    zoneTop + HALF_SHADOW,
                                    zoneTop + OUTER_ZONE_LOC
                                    ],
                PitchGridType._3x3_Shadow => [
                                    zoneBot - OUTER_ZONE_LOC,
                                    zoneBot - SHADOW_SIZE_3X3,
                                    zoneBot + SHADOW_SIZE_3X3,
                                    0.5f * (zoneTop + zoneBot),
                                    zoneTop - SHADOW_SIZE_3X3,
                                    zoneTop + SHADOW_SIZE_3X3,
                                    zoneTop + OUTER_ZONE_LOC
                                    ],
                PitchGridType._5x5 => [
                                    zoneBot - (2 * SHADOW_SIZE),
                                    zoneBot - SHADOW_SIZE,
                                    zoneBot,
                                    ((1.0f / 6.0f) * (zoneTop - HALF_SHADOW)) + ((5.0f / 6.0f) * (zoneBot + HALF_SHADOW)),
                                    0.5f * (zoneTop + zoneBot),
                                    ((5.0f / 6.0f) * (zoneTop - HALF_SHADOW)) + ((1.0f / 6.0f) * (zoneBot + HALF_SHADOW)),
                                    zoneTop,
                                    zoneTop + SHADOW_SIZE,
                                    zoneTop + (2 * SHADOW_SIZE)
                                    ],
                _ => throw new Exception("Unhandled PitchGridType")
            };
            List<float> xs = gridType switch
            {
                PitchGridType._3x3 => [
                                    zoneLeft - OUTER_ZONE_LOC,
                                    zoneLeft - HALF_SHADOW,
                                    ((1.0f / 6.0f) * zoneRight) + ((5.0f / 6.0f) * zoneLeft),
                                    0.5f * (zoneRight + zoneLeft),
                                    ((1.0f / 6.0f) * zoneLeft) + ((5.0f / 6.0f) * zoneRight),
                                    zoneRight + HALF_SHADOW,
                                    zoneRight + OUTER_ZONE_LOC
                                    ],
                PitchGridType._3x3_Shadow => [
                                    zoneLeft - OUTER_ZONE_LOC,
                                    zoneLeft - SHADOW_SIZE_3X3,
                                    zoneLeft + SHADOW_SIZE_3X3,
                                    0.5f * (zoneRight + zoneLeft),
                                    zoneRight - SHADOW_SIZE_3X3,
                                    zoneRight + SHADOW_SIZE_3X3,
                                    zoneRight + OUTER_ZONE_LOC
                                    ],
                PitchGridType._5x5 => [
                                    zoneLeft - OUTER_ZONE_LOC,
                                    zoneLeft - SHADOW_SIZE,
                                    zoneLeft,
                                    ((1.0f / 6.0f) * (zoneRight - HALF_SHADOW)) + ((5.0f / 6) * (zoneLeft + HALF_SHADOW)),
                                    0.5f * (zoneRight + zoneLeft),
                                    ((5.0f / 6) * (zoneRight - HALF_SHADOW)) + ((1.0f / 6.0f) * (zoneLeft + HALF_SHADOW)),
                                    zoneRight,
                                    zoneRight + SHADOW_SIZE,
                                    zoneRight + OUTER_ZONE_LOC
                ],
                _ => throw new Exception("Unhandled PitchGridType")
            };
            List<float?> FixedZones = gridType switch
            {
                PitchGridType._3x3 => [null, SHADOW_SIZE, null, null, null, SHADOW_SIZE, null],
                PitchGridType._3x3_Shadow => [null, 2 * SHADOW_SIZE_3X3, 2 * SHADOW_SIZE_3X3, null, 2 * SHADOW_SIZE_3X3, 2 * SHADOW_SIZE_3X3, null],
                PitchGridType._5x5 => [null, SHADOW_SIZE, SHADOW_SIZE, null, null, null, SHADOW_SIZE, SHADOW_SIZE, null],
                _ => throw new Exception("Unhandled PitchGridType")
            };

            return (ys, xs, FixedZones);
        }

        private static List<float> ComputeZoneSizes(List<float> centers, List<float?> fixedFlags)
        {
            #pragma warning disable CS8629 // Checks are properly made
            List<float> sizes = new();
            for (int i = 0; i < centers.Count; i++)
            {
                if (fixedFlags[i].HasValue)
                    sizes.Add(fixedFlags[i].Value);
                else
                {
                    if (i == 0)
                    {
                        if (fixedFlags[i + 1].HasValue)
                            sizes.Add(2 * (centers[1] - centers[0] - (0.5f * fixedFlags[i + 1].Value)));
                        else
                            sizes.Add(centers[1] - centers[0]);
                    }
                    else if (i == centers.Count - 1)
                    {
                        if (fixedFlags[i - 1].HasValue)
                            sizes.Add(2 * (centers[i] - centers[i - 1] - (0.5f * fixedFlags[i - 1].Value)));
                        else
                            sizes.Add(centers[i] - centers[i - 1]);
                    }
                    else if (fixedFlags[i - 1].HasValue && fixedFlags[i + 1].HasValue)
                        sizes.Add(centers[i + 1] - centers[i - 1] - (0.5f * (fixedFlags[i - 1].Value + fixedFlags[i + 1].Value)));
                    else if (fixedFlags[i - 1].HasValue)
                        sizes.Add(centers[i + 1] - centers[i]);
                    else
                        sizes.Add(centers[i] - centers[i - 1]);
                }
            }
            return sizes;
            #pragma warning restore CS8629
        }

        public void DrawPitches(Graphics graphics)
        {
            using (var format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                Font font = new Font("Arial", 0.05f);
                SolidBrush fontBrush = new SolidBrush(Color.Black);

                foreach (var pitchRow in PitchBoxes)
                {
                    foreach (var pitch in pitchRow)
                    {
                        if (pitch.NumPitches < FilterSize)
                            continue;
                    
                        Color color = pitch.GetColor(PitchValueType, -Scale, Scale);
                        SolidBrush brush = new(color);

                        graphics.FillRectangle(
                            brush,
                            pitch.X - (pitch.Width / 2),
                            pitch.Y - (pitch.Height / 2),
                            pitch.Width,
                            pitch.Height
                        );

                        GraphicsState graphicsState = graphics.Save();
                        graphics.ScaleTransform(1f, -1f);
                        graphics.TranslateTransform(0, -2 * pitch.Y);

                        float pitchValue = PitchValueType switch
                        {
                            PitchValueType.Actual => pitch.ActValue,
                            PitchValueType.Stuff => pitch.StuffValue,
                            PitchValueType.Location => pitch.LocValue,
                            PitchValueType.Exp => pitch.ExpValue,
                            _ => pitch.ActValue
                        };

                        graphics.DrawString(
                            Math.Round(pitchValue, 1).ToString() + "\n" + pitch.NumPitches,
                            font,
                            fontBrush,
                            pitch.X,
                            pitch.Y,
                            format
                        );

                        graphics.Restore(graphicsState);
                    }
                }

                if (HighlightedBox != null)
                {
                    Pen highlightedPen = new Pen(Color.Yellow, 0.05f);
                    graphics.DrawRectangle(
                                    highlightedPen,
                                    HighlightedBox.X - (HighlightedBox.Width / 2),
                                    HighlightedBox.Y - (HighlightedBox.Height / 2),
                                    HighlightedBox.Width,
                                    HighlightedBox.Height
                                );
                }
            }
        }

        public float GetLogicalWidth()
        {
            var rightBox = PitchBoxes[0].Last();
            var leftBox = PitchBoxes[0].First();

            return rightBox.X + (0.5f * rightBox.Width) -
                leftBox.X + (0.5f * leftBox.Width) +
                0.25f;
        }

        public float GetLogicalHeight()
        {
            var topBox = PitchBoxes.Last().First();
            var botBox = PitchBoxes.First().First();

            return topBox.Y + (0.5f * topBox.Height) -
                botBox.Y + (0.5f * botBox.Height) +
                0.25f;
        }

        public void OnClick(PointF p)
        {
            foreach (var pitchRow in PitchBoxes)
                foreach (var pitchBox in pitchRow)
                {
                    if ((Math.Abs(p.X - pitchBox.X) < pitchBox.Width / 2) &&
                        (Math.Abs(p.Y - pitchBox.Y) < pitchBox.Height / 2))
                    {
                        if (HighlightedBox == pitchBox)
                            HighlightedBox = null;
                        else
                            HighlightedBox = pitchBox;
                        return;
                    }
                }

            HighlightedBox = null;
        }
    }
}
