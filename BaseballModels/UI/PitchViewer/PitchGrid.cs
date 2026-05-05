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

    public class PitchBox
    { 
        public required int NumPitches { get; set; }
        public required float StuffValue { get; set; }
        public required float LocValue { get; set; }
        public required float ExpValue { get; set; }
        public required float ActValue { get; set; }

        public required float X { get; set; }
        public required float Y { get; set; }

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
        public List<PitchBox> Pitches;
        private float GridSize;
        public PitchValueType PitchValueType = PitchValueType.Actual;
        public float Scale;

        public PitchGrid(
            IEnumerable<PitchStatcast> pitches, 
            float gridSize, 
            float minX, 
            float minY, 
            int cellsX, 
            int cellsY, 
            int modelId,
            PitchValueType pitchValueType,
            decimal scale
            )
        {
            Pitches = new();
            GridSize = gridSize;
            PitchValueType = pitchValueType;
            Scale = (float)scale;

            pitches = pitches.Where(f => f.PX != null && f.PZ != null);

            // Create Cell Spacing
            List<float> xs = new();
            for (int i = 0; i < cellsX; i++)
                xs.Add(minX + (i * gridSize));

            List<float> ys = new();
            for (int i = 0; i < cellsY; i++)
                ys.Add(minY + (i * gridSize));

            // Create PitchBoxes
            foreach (var x in xs)
                foreach (var y in ys)
                {
                    PitchBox pb = new()
                    {
                        NumPitches = 0,
                        ActValue = 0,
                        StuffValue = 0,
                        LocValue = 0,
                        ExpValue = 0,
                        X = x,
                        Y = y,
                        BreakHoriz = 0,
                        BreakVert = 0,
                        Vel = 0,
                    };
                    
                    Pitches.Add(pb);
                }

            // Go through all pitches and map to a box
            float halfBoxSize = gridSize / 2.0f;
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
                    
                    if (Math.Abs(xs[i] - pitchX) < halfBoxSize)
                    {
                        xBin = i;
                        break;
                    }
                    
                }
                int? yBin = null;
                for (int i = 0; i < ys.Count; i++)
                {

                    if (Math.Abs(ys[i] - pitchY) < halfBoxSize)
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
                PitchBox bin = Pitches[(xBin.Value * ys.Count) + yBin.Value];

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

            // Filter out bins without enough data
            Pitches = Pitches.Where(f => f.NumPitches >= 10).ToList();

            // Normalize values to per 1000 pitches on value, per pitch on info
            Pitches.ForEach(f =>
            {
                f.ActValue = f.ActValue / f.NumPitches * 1000;
                f.ExpValue = f.ExpValue / f.NumPitches * 1000;
                f.StuffValue = f.StuffValue / f.NumPitches * 1000;
                f.LocValue = f.LocValue / f.NumPitches * 1000;

                f.Vel /= f.NumPitches;
                f.BreakHoriz /= f.NumPitches;
                f.BreakVert /= f.NumPitches;
            });
        }

        public void DrawPitches(Graphics graphics)
        {
            using (var format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                Font font = new Font("Arial", 0.05f);
                SolidBrush fontBrush = new SolidBrush(Color.Black);

                foreach (var pitch in Pitches)
                {
                    Color color = pitch.GetColor(PitchValueType, -Scale, Scale);
                    SolidBrush brush = new(color);

                    graphics.FillRectangle(
                        brush,
                        pitch.X - (GridSize / 2),
                        pitch.Y - (GridSize / 2),
                        GridSize,
                        GridSize
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
