using Db;
using PitchDb;
using PitchTrackingDb;
using Python.Runtime;
using UI.Python;
using static Db.DbEnums;

namespace UI.Controls
{
    public class PitchAggregation
    {
        public PitchData Data;
        public PitchValue Value;
        private List<PitchValue> Values;

        public PitchAggregation(PitchData Data, List<PitchValue> Values, int modelId)
        {
            this.Data = Data;
            this.Values = Values;

            Value = null!; // Gets set in SetModel
            SetModel(modelId);
        }

        public void SetModel(int modelId)
        {
            Value = Values.Where(f => f.ModelId == modelId).Single();
        }
    }

    public partial class PitchModelPanel : UserControl
    {
        private const float BALL_SIZE = 0.24f;
        private const float ZONE_LEFT = -0.83f, ZONE_RIGHT = 0.83f;

        private const float X_GRID_SIZE = 0.1f;
        private const float Z_GRID_SIZE = 0.1f;
        private static List<float> X_GRID_POINTS = Enumerable.Range(-20, 41).Select(f => X_GRID_SIZE * f).ToList();
        private static List<float> Z_GRID_POINTS = Enumerable.Range(0, 51).Select(f => Z_GRID_SIZE * f).ToList();

        private PitchAggregation? Pitch = null;

        private record PitchGridPoint(float X, float Z, float Val);
        List<PitchGridPoint> GridPoints = [];
        List<Output_PitchValueAggregation> opvaList = [];
        private PitchModelData? pitchModelData = null;

        private PitchModelOutputType outputVarType = PitchModelOutputType.Value;

        // Color Min/Max values
        private record PitchScaleValues(
            float pitchMin, float pitchMax,
            float ballAvg,
            float cswMax, float cswAvg,
            float cswFoulMax, float cswFoulAvg,
            float inPlayMax, float inPlayAvg,
            float inPlayExpectedMin, float inPlayExpectedMax, float inPlayExpectedAvg,
            float whiffRateMin, float whiffRateMax, float whiffRateAvg,
            float swingStrikeMax, float swingStrikeAvg
            );
        PitchScaleValues? scaleValues = null;

        public PitchModelPanel()
        {
            InitializeComponent();

            // Properly draw background in a TabControl
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint |
                  ControlStyles.AllPaintingInWmPaint |
                  ControlStyles.OptimizedDoubleBuffer |
                  ControlStyles.ResizeRedraw, true
            );
            this.BackColor = SystemColors.Control;
        }

        public void SetPitch(PitchAggregation pitch)
        {
            Pitch = pitch;
            this.Invalidate();
        }

        public void Clear()
        {
            Pitch = null;
            GridPoints = [];
        }

        public class PitchModelData
        {
            public required int CountBalls;
            public required int CountStrikes;

            public required bool HitIsR;
            public required bool PitIsR;

            public required float Velocity;
            public required float MoveHoriz;
            public required float MoveVert;
            public required float BreakAngle;

            public required float Extension;
            public required float X0;
            public required float Z0;

            public required float PX;
            public required float PZ;
            public required float ZoneTop;
            public required float ZoneBot;
        }

        public async void GenerateLocationGrid(PitchModelData pmd, int modelId)
        {
            if (Pitch == null || PitchPy.DataPrep == null)
                return;

            pitchModelData = pmd;

            // Get Color Scales for count/year
            var resultBasis = Global.pitchDb.PitchModelResultBasis
                .Where(f => f.Year == Pitch.Data.Year &&
                    f.CountBalls == pmd.CountBalls &&
                    f.CountStrikes == pmd.CountStrikes &&
                    f.ModelId == modelId)
                .ToList();
            scaleValues = new PitchScaleValues(
                resultBasis.Where(f => f.OutputType == PitchModelOutputType.Value).Single().Perc5,
                resultBasis.Where(f => f.OutputType == PitchModelOutputType.Value).Single().Perc95,

                resultBasis.Where(f => f.OutputType == PitchModelOutputType.Ball).Single().Avg,

                resultBasis.Where(f => f.OutputType == PitchModelOutputType.CSW).Single().Perc95,
                resultBasis.Where(f => f.OutputType == PitchModelOutputType.CSW).Single().Avg,

                resultBasis.Where(f => f.OutputType == PitchModelOutputType.CSWFoul).Single().Perc95,
                resultBasis.Where(f => f.OutputType == PitchModelOutputType.CSWFoul).Single().Avg,

                resultBasis.Where(f => f.OutputType == PitchModelOutputType.InPlayPerc).Single().Perc95,
                resultBasis.Where(f => f.OutputType == PitchModelOutputType.InPlayPerc).Single().Avg,

                resultBasis.Where(f => f.OutputType == PitchModelOutputType.InPlayExp).Single().Perc5,
                resultBasis.Where(f => f.OutputType == PitchModelOutputType.InPlayExp).Single().Perc95,
                resultBasis.Where(f => f.OutputType == PitchModelOutputType.InPlayExp).Single().Avg,

                resultBasis.Where(f => f.OutputType == PitchModelOutputType.WhiffRate).Single().Perc5,
                resultBasis.Where(f => f.OutputType == PitchModelOutputType.WhiffRate).Single().Perc95,
                resultBasis.Where(f => f.OutputType == PitchModelOutputType.WhiffRate).Single().Avg,

                resultBasis.Where(f => f.OutputType == PitchModelOutputType.SwingStrikePerc).Single().Perc95,
                resultBasis.Where(f => f.OutputType == PitchModelOutputType.SwingStrikePerc).Single().Avg
            );

            // Generate pitches at different points
            List<PitchData> GridPitches = [];
            PitchData basePitch = Pitch.Data;
            for (int i = 0; i < Z_GRID_POINTS.Count - 1; i++)
            {
                for (int j = 0; j < X_GRID_POINTS.Count - 1; j++)
                {
                    float x = (X_GRID_POINTS[j] + X_GRID_POINTS[j + 1]) * 0.5f;
                    float z = (Z_GRID_POINTS[i] + Z_GRID_POINTS[i + 1]) * 0.5f;

                    PitchData p = new PitchData
                    {
                        GameId = basePitch.GameId,
                        PitchId = basePitch.PitchId,
                        Year = basePitch.Year,
                        Month = basePitch.Month,
                        RunValueHitter = basePitch.RunValueHitter,
                        RunValueSmoothedHitter = basePitch.RunValueSmoothedHitter,
                        PaResult = basePitch.PaResult,
                        PaResultDirectRuns = basePitch.PaResultDirectRuns,
                        LevelId = basePitch.LevelId,
                        Scenario = basePitch.Scenario,
                        PitcherId = basePitch.PitcherId,
                        PitchType = basePitch.PitchType,
                        PitchClass = basePitch.PitchClass,
                        CountBalls = pmd.CountBalls,
                        CountStrike = pmd.CountStrikes,
                        PitIsR = pmd.PitIsR,
                        HitIsR = pmd.HitIsR,
                        Result = basePitch.Result,
                        HadSwing = basePitch.HadSwing,
                        HadContact = basePitch.HadContact,
                        IsInPlay = basePitch.IsInPlay,
                        RunValueInPlay = basePitch.RunValueInPlay,
                        Vel = pmd.Velocity,
                        Extension = pmd.Extension,
                        BreakInduced = pmd.MoveVert,
                        BreakHorizontal = pmd.MoveHoriz,
                        SpinRate = basePitch.SpinRate,
                        SpinAxis = basePitch.SpinAxis,
                        ActiveSpin = basePitch.ActiveSpin,
                        VaaAboveAverage = basePitch.VaaAboveAverage,
                        HaaAboveAverage = basePitch.HaaAboveAverage,
                        PlateX = x,
                        PlateZ = z,
                        ZoneTop = pmd.ZoneTop,
                        ZoneBot = pmd.ZoneBot,
                    };
                    GridPitches.Add(p);
                }
            }

            // Run Through Model
            try{
                opvaList = await PyThread.InvokeAsync(() =>
                {
                    var pyPitches = GridPitches
                        .Select(f => Global.CreateFromCSharp(f, PitchPy.PitchTrackingDBTypes.DB_PitchData))
                        .Select(dyn => (PyObject)dyn)
                        .ToArray();
                    PyList pitchList = new PyList(pyPitches);

                    var PitchModel = PitchPy.PitchModel.GetAttr("PitchModel");
                    var modelOutputAggregation = PitchModel.InvokeMethod(
                        "GetPitchOutput",
                        PitchPy.DataPrep,
                        PyCore.PitchModelingModelDir.ToPython(),
                        pitchList
                    );

                    PyList moaList = new PyList(modelOutputAggregation);
                    return moaList
                        .Select(f => Global.CreateFromPython<Output_PitchValueAggregation>(f))
                        .ToList();
                });
            }
            catch (PythonException pyEx)  // Specific to Python.NET
            {
                PyCore.WriteException(pyEx);

                return;
            }

            DrawGrid();
        }

        public void UpdateGridType(PitchModelOutputType ocbi)
        {
            outputVarType = ocbi;

            DrawGrid();
        }

        private void DrawGrid()
        {
            if (opvaList.Count == 0 || pitchModelData == null || Pitch == null)
                return;

            // Get Data for current scenario
            GridPoints = [];
            var runExpectancyMatrix = Global.db.RunExpectancyMatrix
                .Where(f => f.Year == Pitch.Data.Year && f.LeagueId == 1
                    && f.CountBalls == pitchModelData.CountBalls && f.CountStrikes == pitchModelData.CountStrikes)
                .ToArray();

            // Get desired output from model
            List<float> modelValue = [];
            switch(outputVarType)
            {
                case PitchModelOutputType.Value:
                    modelValue = opvaList.Select(f => {
                        return 
                            (f.CombinedBall * runExpectancyMatrix.Where(f => f.Result == DbEnums.PitchResult.Ball).Single().DeltaRuns) +
                            ((f.CombinedCalledStrike + (f.CombinedSwing * f.CombinedWhiff)) * runExpectancyMatrix.Where(f => f.Result == DbEnums.PitchResult.CalledStrike).Single().DeltaRuns) +
                            ((f.CombinedSwing * f.CombinedFoul) * runExpectancyMatrix.Where(f => f.Result == DbEnums.PitchResult.Foul).Single().DeltaRuns) +
                            (f.CombinedHBP * runExpectancyMatrix.Where(f => f.Result == DbEnums.PitchResult.HBP).Single().DeltaRuns) +
                            (f.CombinedSwing * f.CombinedInPlay * f.CombinedInPlayExpected);
                    }).ToList();
                    break;
                case PitchModelOutputType.CSW:
                    modelValue = opvaList.Select(f => {
                        return f.CombinedCalledStrike +
                        (f.CombinedSwing * f.CombinedWhiff);
                    }).ToList();
                    break;
                case PitchModelOutputType.Ball:
                    modelValue = opvaList.Select(f => f.CombinedBall + f.CombinedHBP).ToList();
                    break;
                case PitchModelOutputType.CSWFoul:
                    modelValue = opvaList.Select(f =>
                    {
                        return f.CombinedCalledStrike +
                            (f.CombinedSwing * (f.CombinedWhiff * f.CombinedFoul));
                    }).ToList();
                    break;
                case PitchModelOutputType.InPlayPerc:
                    modelValue = opvaList.Select(f =>
                    {
                        return f.CombinedSwing * f.CombinedInPlay;
                    }).ToList();
                    break;
                case PitchModelOutputType.InPlayExp:
                    modelValue = opvaList.Select(f => f.CombinedInPlayExpected).ToList();
                    break;
                case PitchModelOutputType.WhiffRate:
                    modelValue = opvaList.Select(f => f.CombinedWhiff).ToList();
                    break;
                case PitchModelOutputType.SwingStrikePerc:
                    modelValue = opvaList.Select(f => f.CombinedSwing * f.CombinedWhiff).ToList();
                    break;
            }
                    

            // Plot points based on value
            for (int i = 0; i < modelValue.Count; i++)
            {
                int row = i / (X_GRID_POINTS.Count - 1);
                int col = i % (X_GRID_POINTS.Count - 1);
                float x = (X_GRID_POINTS[col] + X_GRID_POINTS[col + 1]) * 0.5f;
                float z = (Z_GRID_POINTS[row] + Z_GRID_POINTS[row + 1]) * 0.5f;

                GridPoints.Add(new PitchGridPoint(x, z, modelValue[i]));
            }

            // Plot points
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(this.BackColor);

            if (Pitch == null)
            {
                return;
            }

            // Get scale to transform pitch space to screen space
            float logicalWidth = X_GRID_POINTS.Last() - X_GRID_POINTS.First();
            float logicalHeight = Z_GRID_POINTS.Last() - Z_GRID_POINTS.First();

            float scaleX = this.ClientSize.Width / logicalWidth;
            float scaleY = this.ClientSize.Height / logicalHeight;
            float scale = Math.Min(scaleX, scaleY);

            // Calculate offset so that image remains centered
            float drawWidth = logicalWidth * scale;
            float drawHeight = logicalHeight * scale;
            float offsetX = (this.ClientSize.Width - drawWidth) / 2f;
            float offsetY = (this.ClientSize.Height - drawHeight) / 2f;

            g.ResetTransform();
            g.TranslateTransform(offsetX, offsetY);
            g.ScaleTransform(scale, -scale);
            g.TranslateTransform(logicalWidth / 2, -(Z_GRID_POINTS.Last()));

            // Pitches
            foreach (var gp in GridPoints)
            {
                RectangleF rect = new RectangleF(
                    gp.X - (X_GRID_SIZE / 2),
                    gp.Z - (Z_GRID_SIZE / 2),
                    X_GRID_SIZE,
                    Z_GRID_SIZE
                );
                Brush brush = GetBrush(gp.Val);
                g.FillRectangle(brush, rect);
            }

            // Strike Zone
            using (Pen pen = new Pen(Color.FromArgb(64, 0, 0, 0), 4.0f / scaleX))
            {
                RectangleF zoneRect = new(
                    ZONE_LEFT,
                    Pitch.Data.ZoneBot,
                    ZONE_RIGHT - ZONE_LEFT,
                    Pitch.Data.ZoneTop - Pitch.Data.ZoneBot
                );
                g.DrawRectangle(pen, zoneRect);
            }

            using (Pen pen = new Pen(Color.Black, 4.0f / scale))
            {
                RectangleF pitchRect = new(
                    Pitch.Data.PlateX - (BALL_SIZE / 2),
                    Pitch.Data.PlateZ - (BALL_SIZE / 2),
                    BALL_SIZE,
                    BALL_SIZE

                );
                g.DrawEllipse(pen, pitchRect);
            }
        }

        private SolidBrush GetBrush(float val)
        {
            if (scaleValues == null)
                throw new Exception("Null Scale Values");

            Color color;
            switch(outputVarType)
            {
                case PitchModelOutputType.Value:
                    color = Global.GetValueColor(val, scaleValues.pitchMin, scaleValues.pitchMax, 0);
                    break;
                case PitchModelOutputType.CSW:
                    color = Global.GetValueColor(val, 0, scaleValues.cswMax, scaleValues.cswAvg);
                    break;
                case PitchModelOutputType.Ball:
                    color = Global.GetValueColor(val, 0, 1, scaleValues.ballAvg);
                    break;
                case PitchModelOutputType.CSWFoul:
                    color = Global.GetValueColor(val, 0, scaleValues.cswFoulMax, scaleValues.cswFoulAvg);
                    break;
                case PitchModelOutputType.InPlayPerc:
                    color = Global.GetValueColor(val, 0, scaleValues.inPlayMax, scaleValues.inPlayAvg);
                    break;
                case PitchModelOutputType.InPlayExp:
                    color = Global.GetValueColor(val, scaleValues.inPlayExpectedMin, scaleValues.inPlayExpectedMax, scaleValues.inPlayExpectedAvg);
                    break;
                case PitchModelOutputType.WhiffRate:
                    color = Global.GetValueColor(val, scaleValues.whiffRateMin, scaleValues.whiffRateMax, scaleValues.whiffRateAvg);
                    break;
                case PitchModelOutputType.SwingStrikePerc:
                    color = Global.GetValueColor(val, 0, scaleValues.swingStrikeMax, scaleValues.swingStrikeAvg);
                    break;
                default:
                    throw new Exception($"No Output type programmed for {outputVarType}");
            }
            SolidBrush brush = new(color);
            return brush;
        }
    }
}
