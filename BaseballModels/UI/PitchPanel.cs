using Db;

namespace UI
{
    public partial class PitchPanel : UserControl
    {
        private float ZoneBot = 1.4f, ZoneTop = 3.15f;
        private const float ZONE_OFFSET = 0.75f;
        private const float ZONE_LEFT = -10.0f / 12.0f, ZONE_RIGHT = 10.0f / 12.0f;
        private const float BALL_SIZE = 0.24f;

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

        public void ShowPitches(IEnumerable<PitchStatcast> pitches)
        {
            SetPitchGrid(
                new PitchGrid(
                    pitches,
                    GridSize,
                    ZONE_LEFT - (2 * GridSize),
                    ZoneBot - (2 * GridSize), 
                    8, 
                    9
                )
            );
        }

        public void HidePitches()
        {
            SetPitchGrid(null);
        }

        private void SetPitchGrid(PitchGrid? pg)
        {
            pitchGrid = pg;
            Invalidate();
        }

        private void PitchPanel_Load(object sender, EventArgs e)
        {

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(this.BackColor);

            // Get scale to transform pitch space to screen space
            float logicalWidth = ZONE_RIGHT - ZONE_LEFT + (2 * ZONE_OFFSET);
            float logicalHeight = ZoneTop - ZoneBot + (2 * ZONE_OFFSET);
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
            g.TranslateTransform(-(ZONE_LEFT - ZONE_OFFSET), -(ZoneTop + ZONE_OFFSET));
            
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
        }
    }
}
