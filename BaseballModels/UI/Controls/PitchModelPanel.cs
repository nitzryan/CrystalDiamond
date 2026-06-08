using Db;
using PitchDb;
using Python.Runtime;
using System.ComponentModel;

namespace UI.Controls
{
    public partial class PitchModelPanel : UserControl
    {
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

            // Design-time check
            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            PySetup.Initialize();
            using (Py.GIL())
            {
                // Get Base Pitch Model Location
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string stuffDir = Path.GetFullPath(Path.Combine(baseDir, "../../../../", "PitchModel"));

                // DataPrep
                dynamic dataPrepModule = Py.Import("Stuff.DataPrep.DataPrep");
                Py_DataPrep = dataPrepModule.DataPrep.Load_From_File(stuffDir + $"/{PySetup.Py_Constants.DATA_PREP_BINARY_ALL_FILE}");

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

            // Generate pitches at different points
            List<PitchStatcast> GridPitches = [];
            for (int i = 0; i < X_GRID_POINTS.Count - 1; i++)
            {
                for (int j = 0; j < Z_GRID_POINTS.Count - 1; j++)
                {
                    float x = (X_GRID_POINTS[i] + X_GRID_POINTS[i + 1]) * 0.5f;
                    float z = (Z_GRID_POINTS[j] + Z_GRID_POINTS[j + 1]) * 0.5f;

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
            List<Output_PitchValueAggregation> opvaList = [];
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

            // Get Pitch value for each point
            GridPoints = [];
            var runExpectancyMatrix = Global.db.RunExpectancyMatrix
                .Where(f => f.Year == Pitch.Year && f.LeagueId == 1
                    && f.CountBalls == pmd.CountBalls && f.CountStrikes == pmd.CountStrikes)
                .ToArray();
            float pitchDev = Global.pitchDb.YearLeagueDeviations
                .Where(f => f.Year == Pitch.Year && f.Balls == Pitch.CountBalls && f.Strikes == Pitch.CountStrike)
                .Single().StuffDev;
            for (int i = 0; i < opvaList.Count; i++)
            {
                float pitchValue = 0;
                var opva = opvaList[i];
                
                pitchValue += opva.CombinedBall * runExpectancyMatrix.Where(f => f.Result == DbEnums.PitchResult.Ball).Single().DeltaRuns;
                pitchValue += (opva.CombinedCalledStrike + (opva.CombinedSwing * opva.CombinedWhiff)) * runExpectancyMatrix.Where(f => f.Result == DbEnums.PitchResult.CalledStrike).Single().DeltaRuns;
                pitchValue += (opva.CombinedSwing * opva.CombinedFoul) * runExpectancyMatrix.Where(f => f.Result == DbEnums.PitchResult.Foul).Single().DeltaRuns;
                pitchValue += opva.CombinedHBP * runExpectancyMatrix.Where(f => f.Result == DbEnums.PitchResult.HBP).Single().DeltaRuns;
                pitchValue += opva.CombinedSwing * opva.CombinedInPlay * opva.CombinedInPlayExpected;

                float pitchPlus = 100 - (10 * (pitchValue / pitchDev));

                var pitch = GridPitches[i];
                #pragma warning disable CS8629 // Values will exist due to initial filter
                GridPoints.Add(new PitchGridPoint(pitch.PX.Value, pitch.PZ.Value, pitchPlus));
                #pragma warning restore CS8629
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

        private static SolidBrush GetBrush(float val)
        {
            Color color = Global.GetValueColor(val, VALUE_MIN, VALUE_MAX);
            SolidBrush brush = new(color);
            return brush;
        }
    }
}
