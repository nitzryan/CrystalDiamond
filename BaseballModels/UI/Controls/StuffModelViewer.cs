using Db;

namespace UI.Controls
{
    public partial class StuffModelViewer : UserControl
    {
        private List<PitchStatcast> Pitches = [];
        private class ListBoxPitchItem
        {
            public required PitchStatcast Pitch { get; set; }
            public override string ToString()
            {
#pragma warning disable CS8629 // Will be filtered at this point
                return Pitch.PitchType.ToString() + $" {Math.Round(Pitch.VStart.Value, 1)}mph ({Math.Round(Pitch.BreakHorizontal.Value, 1)},{Pitch.BreakVertical.Value,1})";
#pragma warning restore CS8629
            }
        }

        public StuffModelViewer()
        {
            InitializeComponent();

            lbPitches.SelectedIndexChanged += (sender, e) =>
            {
                if (lbPitches.SelectedItem is ListBoxPitchItem item)
                {
                    SelectPitch(item.Pitch);
                }
                else
                {
                    throw new Exception("Unexpected Item in lbPitches List Box");
                }
            };
        }

        private void SelectPitch(PitchStatcast pitch)
        {
#pragma warning disable CS8629 // Will be filtered out at this point
            nudVelocity.Value = (decimal)pitch.VStart.Value;
            nudBreakHoriz.Value = (decimal)pitch.BreakHorizontal.Value;
            nudBreakVert.Value = (decimal)pitch.BreakInduced.Value;
            nudBreakAngle.Value = (decimal)pitch.BreakAngle.Value;

            nudExtension.Value = (decimal)pitch.Extension.Value;
            nudX0.Value = (decimal)pitch.X0.Value;
            nudZ0.Value = (decimal)pitch.Z0.Value;

            nudPX.Value = (decimal)pitch.PX.Value;
            nudPZ.Value = (decimal)pitch.PZ.Value;
            nudZoneTop.Value = (decimal)pitch.ZoneTop.Value;
            nudZoneBot.Value = (decimal)pitch.ZoneBot.Value;

            nudBalls.Value = pitch.CountBalls;
            nudStrikes.Value = pitch.CountStrike;
            nudPitR.Value = (pitch.PitIsR ? 1 : 0);
            nudHitR.Value = (pitch.HitIsR ? 1 : 0);

            nudVelX.Value = (decimal)pitch.VX.Value;
            nudVelY.Value = (decimal)pitch.VY.Value;
            nudVelZ.Value = (decimal)pitch.VZ.Value;
            nudAccelX.Value = (decimal)pitch.AX.Value;
            nudAccelY.Value = (decimal)pitch.AY.Value;
            nudAccelZ.Value = (decimal)pitch.AZ.Value;
            #pragma warning restore CS8629

            pitchModelPanel.SetPitch(pitch);
        }

        public void SetPitches(List<PitchStatcast> pitches)
        {
            Pitches = pitches;

            lbPitches.Items.Clear();
            foreach (var p in pitches)
            {
                lbPitches.Items.Add(new ListBoxPitchItem { Pitch = p });
            }

            if (Pitches.Count > 0)
            {
                SelectPitch(Pitches.First());
            }
        }

        private void pbLocation_Click(object sender, EventArgs e)
        {
            pitchModelPanel.GenerateLocationGrid();
        }
    }
}
