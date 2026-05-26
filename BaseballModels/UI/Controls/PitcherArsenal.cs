using Db;

namespace UI.Controls
{
    public partial class PitcherArsenal : UserControl
    {
        private List<PitchStatcast> Pitches = [];

        public PitcherArsenal()
        {
            InitializeComponent();

            cbArsenalYear.SelectedIndexChanged += YearIndexChangeEvent;
        }

        public void SetPitches(List<PitchStatcast> pitches)
        {
            Pitches = pitches;

            List<int> years = pitches
                .Select(f => f.Year)
                .Distinct()
                .OrderDescending()
                .ToList();

            cbArsenalYear.Items.Clear();
            foreach (int year in years)
                cbArsenalYear.Items.Add(year);

            cbArsenalYear.SelectedIndex = 0;
        }

        private void YearIndexChangeEvent(object sender, EventArgs e)
        {
            if (cbArsenalYear.SelectedItem is int year)
            {
                for (int i = tableArsenal.Controls.Count - 1; i >= 0; i--)
                {
                    Control ctrl = tableArsenal.Controls[i];
                    if (tableArsenal.GetRow(ctrl) >= 1)
                    {
                        tableArsenal.Controls.RemoveAt(i);
                    }
                }

                List<PitchStatcast> yearPitches = Pitches
                    .Where(f => f.Year == year)
                    .ToList();

                int pitchCount = yearPitches.Count;

                var pitchTypes = yearPitches
                    .GroupBy(f => f.PitchType)
                    .OrderByDescending(f => f.Count());
                tableArsenal.RowCount = 1 + pitchTypes.Count();
                int currentRow = 1;
                foreach (var pt in pitchTypes)
                {
                    int count = pt.Count();

                    double sumDev = 0;
                    foreach (var p in pt)
                    {
                        sumDev += Global.YldDict[new Global.YearLeagueDevKey(1, p.Year, p.CountBalls, p.CountStrike)].StuffDev;
                    }

                    #pragma warning disable CS8629 // Not null at this point
                    float stuffValue = pt.Sum(f => f.ModelStuff.Value);
                    float pitchValue = pt.Sum(f => f.ModelPitch.Value);
                    #pragma warning restore CS8629
                    float actValue = pt.Sum(f => f.RunValueSmoothedHitter);

                    float stuffPlus = 100 - (float)(10 * stuffValue / sumDev);
                    float pitchPlus = 100 - (float)(10 * pitchValue / sumDev);
                    float actPlus = 100 - (float)(10 * actValue / sumDev);
                    float pitchProportion = (float)count / (float)pitchCount;
                    string name = pt.Key.ToString();

                    // Format data
                    string pitchProportionStr = $"{Math.Round(pitchProportion * 100):0}%";
                    int stuffPlusInt = (int)Math.Round(stuffPlus);
                    int pitchPlusInt = (int)Math.Round(pitchPlus);
                    int actPlusInt = (int)Math.Round(actPlus);

                    // Add Row
                    tableArsenal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    // Column 0: Name (left aligned)
                    tableArsenal.Controls.Add(
                        new Label
                        {
                            Text = name,
                            TextAlign = ContentAlignment.MiddleLeft,
                            AutoSize = true,
                            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom
                        },
                        0, currentRow);

                    // Column 1: Pitch % (right aligned)
                    tableArsenal.Controls.Add(
                        new Label
                        {
                            Text = pitchProportionStr,
                            TextAlign = ContentAlignment.MiddleRight,
                            AutoSize = true,
                            Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
                        },
                        1, currentRow);

                    // Column 2: Stuff+
                    tableArsenal.Controls.Add(
                        new Label
                        {
                            Text = stuffPlusInt.ToString(),
                            TextAlign = ContentAlignment.MiddleRight,
                            AutoSize = true,
                            Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
                        },
                        2, currentRow);

                    // Column 3: Pitch+
                    tableArsenal.Controls.Add(
                        new Label
                        {
                            Text = pitchPlusInt.ToString(),
                            TextAlign = ContentAlignment.MiddleRight,
                            AutoSize = true,
                            Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
                        },
                        3, currentRow);

                    // Column 4: Act+
                    tableArsenal.Controls.Add(
                        new Label
                        {
                            Text = actPlusInt.ToString(),
                            TextAlign = ContentAlignment.MiddleRight,
                            AutoSize = true,
                            Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
                        },
                        4, currentRow);
                    // Column 5: Count
                    tableArsenal.Controls.Add(
                        new Label
                        {
                            Text = count.ToString(),
                            TextAlign = ContentAlignment.MiddleRight,
                            AutoSize = true,
                            Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
                        },
                        5, currentRow);

                    currentRow++;
                }
            }
        }

        private void PitcherArsenal_Load(object sender, EventArgs e)
        {

        }
    }
}
