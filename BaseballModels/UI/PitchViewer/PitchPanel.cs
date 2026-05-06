using Db;

namespace UI
{
    public partial class PitchPanel : UserControl
    {
        private const float BALL_SIZE = 0.24f;
        private float ZoneBot = 1.4f, ZoneTop = 3.15f;
        private const float ZONE_OFFSET = 0.75f;
        private const float ZONE_LEFT = -0.83f, ZONE_RIGHT = 0.83f;
        

        public float GridSize = (ZONE_RIGHT - ZONE_LEFT) / 4;

        private PitchGrid? pitchGrid = null;

        public PitchPanel()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint |
                  ControlStyles.AllPaintingInWmPaint |
                  ControlStyles.OptimizedDoubleBuffer |
                  ControlStyles.ResizeRedraw, true
            );
        }

        public void ShowPitches(
            IEnumerable<PitchStatcast> pitches, 
            int modelId, 
            PitchValueType pvt, 
            PitchGridType pgt, 
            decimal scale, 
            int minPitches)
        {
            SetPitchGrid(
                new PitchGrid(
                    pitches,
                    modelId,
                    pvt,
                    pgt,
                    scale,
                    ZoneTop,
                    ZoneBot,
                    ZONE_LEFT,
                    ZONE_RIGHT,
                    minPitches
                )
            );
        }

        public void HidePitches()
        {
            SetPitchGrid(null);
        }

        public event EventHandler<PitchStats?> PitchStatsUpdate;

        private void SetPitchGrid(PitchGrid? pg)
        {
            pitchGrid = pg;
            Invalidate();
        }

        private void PitchPanel_Load(object sender, EventArgs e)
        {

        }

        private PointF ClientToLogical(Point clientPoint)
        {
            PointF p = new PointF(clientPoint.X, clientPoint.Y);

            float logicalWidth = ZONE_RIGHT - ZONE_LEFT + (2 * ZONE_OFFSET);
            float logicalHeight = ZoneTop - ZoneBot + (2 * ZONE_OFFSET);

            if (pitchGrid != null)
            {
                logicalWidth = pitchGrid.GetLogicalWidth();
                logicalHeight = pitchGrid.GetLogicalHeight();
            }

            float scaleX = this.ClientSize.Width / logicalWidth;
            float scaleY = this.ClientSize.Height / logicalHeight;
            float scale = Math.Min(scaleX, scaleY);

            if (scale <= 0) return new PointF(0, 0);

            float drawWidth = logicalWidth * scale;
            float drawHeight = logicalHeight * scale;

            float offsetX = (this.ClientSize.Width - drawWidth) / 2f;
            float offsetY = (this.ClientSize.Height - drawHeight) / 2f;

            
            float tx2 = logicalWidth / 2f;
            float ty2 = -(ZoneTop + (1.5f * ZONE_OFFSET));

            // Undo TranslateTransform
            p.X -= offsetX;
            p.Y -= offsetY;

            //Undo ScaleTransform
            p.X /= scale;
            p.Y /= -scale;

            //TranslateTransform
            p.X -= tx2;
            p.Y -= ty2;

            return p;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(this.BackColor);

            // Get scale to transform pitch space to screen space
            float logicalWidth = ZONE_RIGHT - ZONE_LEFT + (2 * ZONE_OFFSET);
            float logicalHeight = ZoneTop - ZoneBot + (2 * ZONE_OFFSET);

            if (pitchGrid != null)
            {
                logicalWidth = pitchGrid.GetLogicalWidth();
                logicalHeight = pitchGrid.GetLogicalHeight();
            }

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
            g.TranslateTransform(logicalWidth / 2, -(ZoneTop + (1.5f * ZONE_OFFSET)));
            
            // Pitches
            if (pitchGrid != null)
            {
                pitchGrid.DrawPitches(g);
            }

            // Strike Zone
            using (Pen pen = new Pen(Color.FromArgb(64, 0, 0, 0), 4.0f / scaleX))
            {
                RectangleF zoneRect = new(
                    ZONE_LEFT,
                    ZoneBot,
                    ZONE_RIGHT - ZONE_LEFT,
                    ZoneTop - ZoneBot
                );
                g.DrawRectangle(pen, zoneRect);
            }

            // Send event for pitchStats
            if (pitchGrid == null)
                PitchStatsUpdate?.Invoke(this, null);
            else
                PitchStatsUpdate?.Invoke(this, pitchGrid.GetPitchStats());
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (pitchGrid != null)
            {
                PointF logicalPoint = ClientToLogical(e.Location);
                pitchGrid.OnClick(logicalPoint);
            }

            Invalidate();
        }
    }
}
