namespace UI
{
    public partial class RangeSelector : UserControl
    {
        public RangeSelector()
        {
            InitializeComponent();
        }

        public void Update(decimal min, decimal max, string title)
        {
            name.Text = title;

            minRange.Minimum = min;
            minRange.Maximum = max;

            maxRange.Minimum = min;
            maxRange.Minimum = max;
        }

        private void maxRange_ValueChanged(object sender, EventArgs e)
        {
            minRange.Maximum = maxRange.Value;
        }

        private void minRange_ValueChanged(object sender, EventArgs e)
        {
            maxRange.Minimum = minRange.Value;
        }

        public decimal GetMinimum()
        {
            return minRange.Value;
        }

        public decimal GetMaximum()
        {
            return maxRange.Value;
        }
    }
}
