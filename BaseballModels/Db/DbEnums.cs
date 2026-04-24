using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Db
{
    public static class DbEnums
    {
        // Game PBP Event Codes
        [Flags]
        public enum PBP_Events : long
        {
            NONE = 0,
            SINGLE = 1 << 0,
            DOUBLE = 1 << 1,
            TRIPLE = 1 << 2,
            HR = 1 << 3,
            BB = 1 << 4,
            HBP = 1 << 5,
            K = 1 << 6,
            GIDP = 1 << 7,
            POPOUT = 1 << 8,
            GNDOUT = 1 << 9,
            FLYOUT = 1 << 10,
            FIELD_ERROR = 1 << 11,
            SB = 1 << 12,
            CS = 1 << 13,
            PB = 1 << 14,
            BALK = 1 << 15,
            LINEOUT = 1 << 16,
            DEF_INDIF = 1 << 17,
            WILD_PITCH = 1 << 18,
            FORCEOUT = 1 << 19,
            SAC_FLY = 1 << 20,
            RUNNER_OUT = 1 << 21,
            BUNT_OUT = 1 << 22,
            IBB = 1 << 23,
            SAC_BUNT = 1 << 24,
            FIELDERS_CHOICE = 1 << 25,
            FB_DOUBLE_PLAY = 1 << 26,
            FIELDERS_CHOICE_OUT = 1 << 27,
            PICKOFF = 1 << 28,
            TRIPLE_PLAY = 1 << 29,
            OTHER = 1 << 30, // Batter Interference, Offical Scorer Pending, Disengagement Violation, Defensive Indifference, Bunt Out, Catchers Interference
        }

        public static PBP_Events PBP_OUT_EVENTS = PBP_Events.K |
                                                PBP_Events.GIDP |
                                                PBP_Events.POPOUT |
                                                PBP_Events.GNDOUT |
                                                PBP_Events.FLYOUT |
                                                PBP_Events.LINEOUT |
                                                PBP_Events.FORCEOUT |
                                                PBP_Events.SAC_BUNT |
                                                PBP_Events.BUNT_OUT |
                                                PBP_Events.SAC_FLY |
                                                PBP_Events.FIELDERS_CHOICE_OUT |
                                                PBP_Events.FB_DOUBLE_PLAY |
                                                PBP_Events.TRIPLE_PLAY;

        public static PBP_Events PBP_HIT_IP_EVENTS = PBP_Events.SINGLE | PBP_Events.DOUBLE | PBP_Events.TRIPLE;

        public static PBP_Events PBP_IN_PLAY_EVENT = PBP_HIT_IP_EVENTS | PBP_OUT_EVENTS | PBP_Events.FIELD_ERROR | PBP_Events.FIELDERS_CHOICE;

        public static PBP_Events PBP_HIT_EVENT = PBP_Events.SINGLE | PBP_Events.DOUBLE | PBP_Events.TRIPLE | PBP_Events.HR;

        [Flags]
        public enum PBP_HitTrajectory
        {
            None = 0,
            Groundball = 1 << 0,
            Flyball = 1 << 1,
            Linedrive = 1 << 2,
            Popup = 1 << 3,
            BuntGrounder = 1 << 4,
            BuntPopup = 1 << 5,
            BuntLinedrive = 1 << 6,
        }
        
        [Flags]
        public enum PBP_HitHardness
        {
            None = 0,
            Soft = 1 << 0,
            Medium = 1 << 1,
            Hard = 1 << 2,
        }

        [Flags]
        public enum BaseOccupancy
        { 
            Empty = 0,
            B1 = 1 << 0,
            B2 = 1 << 1,
            B3 = 1 << 2,
            Invalid = 1 << 3,
        }

        [Flags]
        public enum GameFlags
        { 
            Valid = 0,
            OutputsSkewed = 1 << 0,
            HitPositionImpossible = 1 << 1,
            HitPositionTooRight2017 = 1 << 2,
        }

        // Should not be combined
        public enum Position
        {
            P = 1,
            C = 2,
            B1 = 3,
            B2 = 4,
            B3 = 5,
            SS = 6,
            LF = 7,
            CF = 8,
            RF = 9,
            DH = 10,
        }

        [Flags]
        public enum CollegePosition
        { 
            None = 0,
            P = 1 << 0,
            C = 1 << 1,
            [Description("1B")]
            B1 = 1 << 2,
            [Description("2B")]
            B2 = 1 << 3,
            [Description("3B")]
            B3 = 1 << 4,
            SS = 1 << 5,
            LF = 1 << 6,
            CF = 1 << 7,
            RF = 1 << 8,
            DH = 1 << 9,
            IF = 1 << 10,
            OF = 1 << 11,
        }

        public static string GetFlagsDescription(Enum value)
        {
            var sb = new StringBuilder();
            var type = value.GetType();
            var first = true;

            // Iterate through all possible flags
            foreach (Enum flag in Enum.GetValues(type))
            {
                if (flag.Equals(value.GetType().GetEnumValues().GetValue(0))) // Skip None (0)
                    continue;

                if (value.HasFlag(flag))
                {
                    if (!first) sb.Append(", ");
                    first = false;

                    var field = type.GetField(flag.ToString());
                    var attr = field?.GetCustomAttribute<DescriptionAttribute>();
                    sb.Append(attr?.Description ?? flag.ToString());
                }
            }

            return sb.Length > 0 ? sb.ToString() : value.ToString();
        }

        public enum PitchType
        {
            Unknown = 0,

            Fourseam = 1,
            Sinker = 2,
            Fastball = 14,
            Twoseam = 16,

            Splitter = 3,
            Changeup = 4,
            Forkball = 12,

            Cutter = 5,
            Slider = 6,
            Sweeper = 7,
            Slurve = 8,
            Curveball = 9,
            KnuckleCurve = 11,
            Screwball = 13,
            SlowCurve = 15,
            
            Knuckleball = 10,

            Eephus = 12,
        }

        public enum PitchResult
        {
            CalledStrike = 1,
            SwingingStrike = 2,
            Foul = 3,
            Ball = 4,
            InPlay = 5,
            HBP = 6,
        }

        [Flags]
        public enum PitchPaResult
        {
            Out = 1 << 0,   
            Strikeout = 1 << 1,
            Groundout = 1 << 2,
            Flyout = 1 << 3,
            GIDP = 1 << 4,
            BB = 1 << 5,
            HBP = 1 << 6,
            Hit1B = 1 << 7,
            Hit2B = 1 << 8,
            Hit3B = 1 << 9,
            HitHR = 1 << 10,
            Error = 1 << 11,
            Other = 1 << 12
        }
    }
}
