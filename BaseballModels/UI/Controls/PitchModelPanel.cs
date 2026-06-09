using Db;
using PitchDb;
using Python.Runtime;
using System.ComponentModel;
using static UI.Controls.PitchModelPanel;

namespace UI.Controls
{
    public partial class PitchModelPanel : UserControl
    {
        public enum OutputComboBoxItem
        {
            Value,
            CSW,
            Ball,
            Strike,
            InPlayPerc,
            InPlayExp,
        }

        private const float BALL_SIZE = 0.24f;
        private const float ZONE_LEFT = -0.83f, ZONE_RIGHT = 0.83f;

        private const float X_GRID_SIZE = 0.1f;
        private const float Z_GRID_SIZE = 0.1f;
        private static List<float> X_GRID_POINTS = Enumerable.Range(-20, 41).Select(f => X_GRID_SIZE * f).ToList();
        private static List<float> Z_GRID_POINTS = Enumerable.Range(0, 51).Select(f => Z_GRID_SIZE * f).ToList();

        private const float VALUE_MIN = 50;
        private const float VALUE_MAX = 150;

        private PitchStatcast? Pitch = null;

        private PyObject? Py_DataPrep = null, Py_PitchModel = null;

        private record PitchGridPoint(float X, float Z, float Val);
        List<PitchGridPoint> GridPoints = [];
        List<Output_PitchValueAggregation> opvaList = [];
        private PitchModelData? pitchModelData = null;

        private OutputComboBoxItem outputVarType = OutputComboBoxItem.Value;

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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // This check is now 100% reliable
            if (DesignMode)
                return;

            PySetup.Initialize();
            using (Py.GIL())
            {
                // Get Base Pitch Model Location
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string stuffDir = Path.GetFullPath(Path.Combine(baseDir, "../../../../", "PitchModel"));

                // DataPrep
                dynamic dataPrepModule = Py.Import("Stuff.DataPrep.DataPrep");
                Py_DataPrep = dataPrepModule.DataPrep.Load_From_File(
                    stuffDir + $"/{PySetup.Py_Constants.DATA_PREP_BINARY_ALL_FILE}");

                // PitchModel
                dynamic pitchModelModule = Py.Import("Stuff.Model.PitchModel");
                Py_PitchModel = pitchModelModule.PitchModel(Py_DataPrep);
            }
        }

        public void SetPitch(PitchStatcast pitch)
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

        public void GenerateLocationGrid(PitchModelData pmd)
        {
            if (Pitch == null)
                return;

            pitchModelData = pmd;

            // Generate pitches at different points
            List<PitchStatcast> GridPitches = [];
            for (int i = 0; i < Z_GRID_POINTS.Count - 1; i++)
            {
                for (int j = 0; j < X_GRID_POINTS.Count - 1; j++)
                {
                    float x = (X_GRID_POINTS[j] + X_GRID_POINTS[j + 1]) * 0.5f;
                    float z = (Z_GRID_POINTS[i] + Z_GRID_POINTS[i + 1]) * 0.5f;

                    PitchStatcast p = new PitchStatcast
                    {
                        GameId = Pitch.GameId,
                        PitchId = Pitch.PitchId,
                        PaId = Pitch.PaId,
                        PitcherId = Pitch.PitcherId,
                        HitterId = Pitch.HitterId,
                        LeagueId = Pitch.LeagueId,
                        LevelId = Pitch.LevelId,
                        Year = Pitch.Year,
                        Month = Pitch.Month,
                        PitcherPitchNum = Pitch.PitcherPitchNum,
                        CountBalls = pmd.CountBalls,
                        CountStrike = pmd.CountStrikes,
                        Outs = Pitch.Outs,
                        BaseOccupancy = Pitch.BaseOccupancy,
                        PitchType = Pitch.PitchType,
                        PaResult = Pitch.PaResult,
                        PaResultOccupancy = Pitch.PaResultOccupancy,
                        PaResultOuts = Pitch.PaResultOuts,
                        PaResultDirectRuns = Pitch.PaResultDirectRuns,
                        RunsAfterPa = Pitch.RunsAfterPa,
                        Result = Pitch.Result,
                        HadSwing = true,
                        HadContact = Pitch.HadContact,
                        IsInPlay = Pitch.IsInPlay,
                        HitIsR = pmd.HitIsR,
                        PitIsR = pmd.PitIsR,
                        VX = 0,
                        VY = 0,
                        VZ = 0,
                        VStart = pmd.Velocity,
                        VEnd = Pitch.VEnd,
                        AX = 0,
                        AY = 0,
                        AZ = 0,
                        PfxX = Pitch.PfxX,
                        PfxZ = Pitch.PfxZ,
                        BreakAngle = pmd.BreakAngle,
                        BreakVertical = Pitch.BreakVertical,
                        BreakInduced = pmd.MoveVert,
                        BreakHorizontal = pmd.MoveHoriz,
                        SpinRate = Pitch.SpinRate,
                        SpinDirection = Pitch.SpinDirection,
                        PX = x,
                        PZ = z,
                        ZoneTop = pmd.ZoneTop,
                        ZoneBot = pmd.ZoneBot,
                        Extension = pmd.Extension,
                        X0 = pmd.X0,
                        Y0 = Pitch.Y0,
                        Z0 = pmd.Z0,
                        PlateTime = Pitch.PlateTime,
                        LaunchSpeed = Pitch.LaunchSpeed,
                        LaunchAngle = Pitch.LaunchAngle,
                        TotalDist = Pitch.TotalDist,
                        HitCoordX = Pitch.HitCoordX,
                        HitCoordY = Pitch.HitCoordY,
                        RunValueHitter = Pitch.RunValueHitter,
                        RunValueSmoothedHitter = Pitch.RunValueSmoothedHitter,
                        Scenario = Pitch.Scenario,
                        ModelStuff = Pitch.ModelStuff,
                        ModelLocation = Pitch.ModelLocation,
                        ModelPitch = Pitch.ModelPitch,
                    };
                    GridPitches.Add(p);
                }
            }

            // Run Through Model
            try{
                using (Py.GIL())
                {
                    var pyPitches = GridPitches
                        .Select(f => Global.CreateFromCSharp(f, PySetup.Py_DbTypes.DB_PitchStatcast))
                        .Select(dyn => (PyObject)dyn)
                        .ToArray();
                    PyList pitchList = new PyList(pyPitches);
                    var modelOutputAggregation = Py_PitchModel.InvokeMethod(
                        "GetPitchOutput",
                        "../../../../PitchModel/Models/".ToPython(),
                        pitchList
                    );

                    PyList moaList = new PyList(modelOutputAggregation);
                    opvaList = moaList.Select(f => Global.CreateFromPython<Output_PitchValueAggregation>(f))
                        .ToList();
                }
            }
            catch (PythonException pyEx)  // Specific to Python.NET
            {
                PySetup.WriteException(pyEx);

                return;
            }

            DrawGrid();
        }

        public void UpdateGridType(OutputComboBoxItem ocbi)
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
                .Where(f => f.Year == Pitch.Year && f.LeagueId == 1
                    && f.CountBalls == pitchModelData.CountBalls && f.CountStrikes == pitchModelData.CountStrikes)
                .ToArray();
            float pitchDev = Global.pitchDb.YearLeagueDeviations
                .Where(f => f.Year == Pitch.Year && f.Balls == Pitch.CountBalls && f.Strikes == Pitch.CountStrike)
                .Single().StuffDev;

            // Get desired output from model
            List<float> modelValue = [];
            switch(outputVarType)
            {
                case OutputComboBoxItem.Value:
                    modelValue = opvaList.Select(f => {
                        return 100 - 
                        (10 * 
                        ((
                            (f.CombinedBall * runExpectancyMatrix.Where(f => f.Result == DbEnums.PitchResult.Ball).Single().DeltaRuns) +
                            ((f.CombinedCalledStrike + (f.CombinedSwing * f.CombinedWhiff)) * runExpectancyMatrix.Where(f => f.Result == DbEnums.PitchResult.CalledStrike).Single().DeltaRuns) +
                            ((f.CombinedSwing * f.CombinedFoul) * runExpectancyMatrix.Where(f => f.Result == DbEnums.PitchResult.Foul).Single().DeltaRuns) +
                            (f.CombinedHBP * runExpectancyMatrix.Where(f => f.Result == DbEnums.PitchResult.HBP).Single().DeltaRuns) +
                            (f.CombinedSwing * f.CombinedInPlay * f.CombinedInPlayExpected)
                        )
                        / pitchDev));
                    }).ToList();
                    break;
                case OutputComboBoxItem.CSW:
                    modelValue = opvaList.Select(f => {
                        return f.CombinedCalledStrike +
                        (f.CombinedSwing * f.CombinedWhiff);
                    }).ToList();
                    break;
                case OutputComboBoxItem.Ball:
                    modelValue = opvaList.Select(f => f.CombinedBall + f.CombinedHBP).ToList();
                    break;
                case OutputComboBoxItem.Strike:
                    modelValue = opvaList.Select(f =>
                    {
                        return f.CombinedCalledStrike +
                            (f.CombinedSwing * (f.CombinedWhiff * f.CombinedFoul));
                    }).ToList();
                    break;
                case OutputComboBoxItem.InPlayPerc:
                    modelValue = opvaList.Select(f =>
                    {
                        return f.CombinedSwing * f.CombinedInPlay;
                    }).ToList();
                    break;
                case OutputComboBoxItem.InPlayExp:
                    modelValue = opvaList.Select(f => f.CombinedInPlayExpected).ToList();
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

            if (Pitch == null || Pitch.PX == null || Pitch.PZ == null || Pitch.ZoneTop == null || Pitch.ZoneBot == null)
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
                    Pitch.ZoneBot.Value,
                    ZONE_RIGHT - ZONE_LEFT,
                    Pitch.ZoneTop.Value - Pitch.ZoneBot.Value
                );
                g.DrawRectangle(pen, zoneRect);
            }

            using (Pen pen = new Pen(Color.Black, 4.0f / scale))
            {
                RectangleF pitchRect = new(
                    Pitch.PX.Value - (BALL_SIZE / 2),
                    Pitch.PZ.Value - (BALL_SIZE / 2),
                    BALL_SIZE,
                    BALL_SIZE

                );
                g.DrawEllipse(pen, pitchRect);
            }
        }

        private SolidBrush GetBrush(float val)
        {
            Color color;
            switch(outputVarType)
            {
                case OutputComboBoxItem.Value:
                    color = Global.GetValueColor(val, VALUE_MIN, VALUE_MAX);
                    break;
                case OutputComboBoxItem.CSW:
                    color = Global.GetValueColor(val, 0, 1);
                    break;
                case OutputComboBoxItem.Ball:
                    color = Global.GetValueColor(val, 0, 1);
                    break;
                case OutputComboBoxItem.Strike:
                    color = Global.GetValueColor(val, 0, 1);
                    break;
                case OutputComboBoxItem.InPlayPerc:
                    color = Global.GetValueColor(val, 0, 0.5f);
                    break;
                case OutputComboBoxItem.InPlayExp:
                    color = Global.GetValueColor(val, -0.5f, 0.5f);
                    break;
                default:
                    throw new Exception($"No Output type programmed for {outputVarType}");
            }
            SolidBrush brush = new(color);
            return brush;
        }
    }
}
