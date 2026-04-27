namespace PitchDb
{
    public static class DbEnums
    {
        [Flags]
        public enum Scenario
        { 
            All = 0,

            SameSide = 1 << 0,
            OppSide = 1 << 1,

            NotTwoStrikes = 1 << 2,
            TwoStrikes = 1 << 3,

            DoublePlayOpp = 1 << 4,
            NonDoublePlayOpp = 1 << 5,

            AheadCount = 1 << 6,
            EvenCount = 1 << 7,
            BehindCount = 1 << 8,
        }

    }
}
