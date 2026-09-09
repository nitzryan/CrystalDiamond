using Db;
using static UI.Controls.PitchModelPanel;
using static Db.DbEnums;

namespace UI.Controls
{
    public partial class StuffModelViewer : UserControl
    {
        private List<PitchAggregation> Pitches = [];
        private class ListBoxPitchItem
        {
            public required PitchAggregation Pitch { get; set; }
            public override string ToString()
            {
                #pragma warning disable CS8629 // Will be filtered at this point
                return Pitch.Data.PitchType.ToString() + $" {Math.Round(Pitch.Data.Vel, 1)}mph";
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

        private void SelectPitch(PitchAggregation pitch)
        {
            var data = pitch.Data;

            nudVelocity.Value = (decimal)data.Vel;
            nudBreakHoriz.Value = (decimal)data.BreakHorizontal;
            nudBreakVert.Value = (decimal)data.BreakInduced;

            nudExtension.Value = (decimal)data.Extension;

            nudPX.Value = (decimal)data.PlateX;
            nudPZ.Value = (decimal)data.PlateZ;
            nudZoneTop.Value = (decimal)data.ZoneTop;
            nudZoneBot.Value = (decimal)data.ZoneBot;

            nudBalls.Value = data.CountBalls;
            nudStrikes.Value = data.CountStrike;
            nudPitR.Value = (data.PitIsR ? 1 : 0);
            nudHitR.Value = (data.HitIsR ? 1 : 0);

            pitchModelPanel.SetPitch(pitch);
        }

        public void SetPitches(List<PitchAggregation> pitches)
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

            // TODO : Add modelId to the UI
            pitchModelPanel.GenerateLocationGrid(pmd, 1);
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
