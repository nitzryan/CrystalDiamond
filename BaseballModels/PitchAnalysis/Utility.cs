namespace PitchAnalysis
{
    internal class Utility
    {
        private const float PLUS_BASELINE = 100;
        private const float PLUS_PER_DEVIATION = 10;

        public static float CalculatePitchPlus(double meanRunValue, double deviation)
        {
            return (float)(PLUS_BASELINE - (PLUS_PER_DEVIATION * meanRunValue / deviation));
        }
    }
}
