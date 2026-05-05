using Db;
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

    public class PitchBox
    { 
        public required int NumPitches { get; set; }
        public required float StuffValue { get; set; }
        public required float LocValue { get; set; }
        public required float ExpValue { get; set; }
        public required float ActValue { get; set; }

        public required float X { get; set; }
        public required float Y { get; set; }

        public required float Width { get; set; }
        public required float Height { get; set; }

        public required float? FixedDeltaXZone { get; set; }
        public required float? FixedDeltaYZone { get; set; }

        public required float Vel { get; set; }
        public required float BreakHoriz { get; set; }
        public required float BreakVert { get; set; }

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


    public class PitchGrid
    {
        public List<List<PitchBox>> PitchBoxes;
        public PitchValueType PitchValueType;
        public PitchGridType PitchGridType;
        public float Scale;
        public int FilterSize;

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
            Scale = (float)scale;
            FilterSize = filterSize;

            pitches = pitches.Where(f => f.PX != null && f.PZ != null);
            PitchBoxes = PitchGrid.GetPitchBoxes(pitchGridType, zoneTop, zoneBot, zoneLeft, zoneRight);

            List<float> xs = PitchBoxes.First().Select(f => f.X).Order().ToList();
            List<float> ys = PitchBoxes.Select(f => f.First().Y).Order().ToList();
            List<float> widths = PitchBoxes.First().OrderBy(f => f.X).Select(f => f.Width).ToList();
            List<float> heights = PitchBoxes.Select(f => f.First()).OrderBy(f => f.Y).Select(f => f.Height).ToList();

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

                    if (Math.Abs(xs[i] - pitchX) < widths[i])
                    {
                        xBin = i;
                        break;
                    }

                }
                int? yBin = null;
                for (int i = 0; i < ys.Count; i++)
                {

                    if (Math.Abs(ys[i] - pitchY) < heights[i])
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
                    if (pitchY < ys[0])
                        yBin = 0;
                    else
                        yBin = ys.Count - 1;
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
                bin.ExpValue += (float)pitchModelOutput.expValue;
                bin.ActValue += pitch.RunValueHitter;
                bin.StuffValue += (float)pitchModelOutput.stuffValue;
                bin.LocValue += (float)pitchModelOutput.locationValue;

                #pragma warning disable CS8629 // Any values here will have these values
                bin.Vel += pitch.VStart.Value;
                bin.BreakHoriz += pitch.BreakHorizontal.Value;
                bin.BreakVert += pitch.BreakInduced.Value;
                #pragma warning restore CS8629
            }

            // Normalize values to per 1000 pitches on value, per pitch on info
            PitchBoxes.ForEach(g => g.ForEach(f =>
            {
                f.ActValue = f.ActValue / f.NumPitches * 1000;
                f.ExpValue = f.ExpValue / f.NumPitches * 1000;
                f.StuffValue = f.StuffValue / f.NumPitches * 1000;
                f.LocValue = f.LocValue / f.NumPitches * 1000;

                f.Vel /= f.NumPitches;
                f.BreakHoriz /= f.NumPitches;
                f.BreakVert /= f.NumPitches;
            }));
        }

        private static List<List<PitchBox>> GetPitchBoxes(
            PitchGridType gridType,
            float zoneTop,
            float zoneBot,
            float zoneLeft,
            float zoneRight
        )
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



            List<float> rowHeights = ComputeZoneSizes(ys, FixedZones);
            List<float> columnWidths = ComputeZoneSizes(xs, FixedZones);

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
                        X = xs[i],
                        Y = ys[j],
                        Height = rowHeights[j],
                        Width = columnWidths[i],
                        FixedDeltaXZone = FixedZones[i],
                        FixedDeltaYZone = FixedZones[j],
                        BreakHoriz = 0,
                        BreakVert = 0,
                        Vel = 0,
                    };

                    pitchRow.Add(pb);
                }

                pitches.Add(pitchRow);
            }
            return pitches;
        }

        private static List<float> ComputeZoneSizes(List<float> centers, List<float?> fixedFlags)
        {
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
            }
        }
    }
}
