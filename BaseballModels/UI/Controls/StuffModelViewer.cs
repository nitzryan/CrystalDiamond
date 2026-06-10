using Db;
using static UI.Controls.PitchModelPanel;
using static Db.DbEnums;

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

            // Set Type and Output ComboBoxes
            cbOutputVar.Items.Add(PitchModelOutputType.Value);
            cbOutputVar.Items.Add(PitchModelOutputType.CSW);
            cbOutputVar.Items.Add(PitchModelOutputType.Ball);
            cbOutputVar.Items.Add(PitchModelOutputType.CSWFoul);
            cbOutputVar.Items.Add(PitchModelOutputType.InPlayPerc);
            cbOutputVar.Items.Add(PitchModelOutputType.InPlayExp);
            cbOutputVar.Items.Add(PitchModelOutputType.WhiffRate);
            cbOutputVar.Items.Add(PitchModelOutputType.SwingStrikePerc);
            cbOutputVar.SelectedIndex = 0;

            cbOutputVar.SelectedIndexChanged += UpdateGridType;
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
            PitchModelData pmd = new PitchModelData
            {
                CountBalls = (int)nudBalls.Value,
                CountStrikes = (int)nudStrikes.Value,

                HitIsR = nudHitR.Value == 1,
                PitIsR = nudPitR.Value == 1,

                Velocity = (float)nudVelocity.Value,
                MoveHoriz = (float)nudBreakHoriz.Value,
                MoveVert = (float)nudBreakVert.Value,
                BreakAngle = (float)nudBreakAngle.Value,

                Extension = (float)nudExtension.Value,
                X0 = (float)nudX0.Value,
                Z0 = (float)nudZ0.Value,

                PX = (float)nudPX.Value,
                PZ = (float)nudPZ.Value,

                ZoneTop = (float)nudZoneTop.Value,
                ZoneBot = (float)nudZoneBot.Value,
            };

            pitchModelPanel.GenerateLocationGrid(pmd);
        }

        private void UpdateGridType(object? sender, EventArgs e)
        {
            if (cbOutputVar.SelectedItem is PitchModelOutputType ocbi)
            {
                pitchModelPanel.UpdateGridType(ocbi);
            }
        }
    }
}
