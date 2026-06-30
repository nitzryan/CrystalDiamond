namespace SiteDb
{
    public class DbEnums
    {
        public enum TimestepQuality
        {
            ProEarlyLow = 12,
            ProEarlyMed = 1,
            ProEarlyHigh = 2,
            ProPeak = 3,
            ProDeclineHigh = 4,
            ProDeclineLow = 5,
            ProDeclineVeryLow = 6,

            CollegeLow = 7,
            CollegeMed = 8,
            CollegeHigh = 9,
            CollegeVeryLow = 10,

            HSVeryLow = 11,

            NotInTraining = -1,
            InTraining = -2,
        }

        public enum Severity
        {
            VeryLow = 0,
            Low = 1,
            Medium = 2,
            High = 3,
            VeryHigh = 4
        }
    }
}
