using System.Text.Json;
using System.Text.Json.Serialization;
using static Db.DbEnums;

namespace Db
{
    public record GameScenarios
    {
        public required int outs { get; set; }
        public required BaseOccupancy occupancy { get; set; }
    }

    // Fielding Records
    public record FieldingScenario
    {
        public required int ZoneAngle { get; set; }
        public required int ZoneDist { get; set; }
        public required PBP_HitTrajectory Trajectory { get; set; }
        public required PBP_HitHardness Hardness { get; set; }
    }
    public record FieldingResults
    {
        public required float[] ProbMakeWhenMade { get; set; } // Probs for each position, conditional on making
        public required float ProbMiss { get; set; }
        public required float RunsMake { get; set; }
        public required float RunsMiss { get; set; }
        public required int NumOccurences { get; set; }
    }

    // Baserunning Records
    public record BaserunningResult
    {
        public required float ProbAdvance { get; set; }
        public required float RunsAdvance { get; set; }
        public required float ProbStay { get; set; }
        public required float RunsStay { get; set; }
        public required float ProbOut { get; set; }
        public required float RunsOut { get; set; }
        public required int NumOccurences { get; set; }

        public float GetExpected() {
            return (ProbAdvance * RunsAdvance) + (ProbStay * RunsStay) + (ProbOut * RunsOut);
        }
    }
    public record DoublePlayResult 
    {
        public required float ProbsLeadingOnly { get; set; }
        public required float ProbsHitterOnly { get; set; }
        public required float ProbsDP { get; set; }
        public required float ProbsNeither { get; set; }
        public required float RunsLeading { get; set; }
        public required float RunsHitter { get; set; }
        public required float RunsDP { get; set; }
        public required float RunsNeither { get; set; }
        public required int NumOccurences { get; set; }
    }
    public record BaserunningScenario
    {
        public required int Zone { get; set; }
        public required PBP_HitTrajectory Trajectory { get; set; }
        public required PBP_HitHardness Hardness { get; set; }
    }

    public record DoublePlayScenario
    {
        public required int Zone { get; set; }
    }

    public static class LeagueRunMatrixDicts
    {
        private static readonly JsonSerializerOptions DefaultOptions = CreateOptions();

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };

            options.Converters.Add(new GameScenariosConverter());
            options.Converters.Add(new FieldingScenarioConverter());
            options.Converters.Add(new BaserunningScenarioConverter());
            options.Converters.Add(new DoublePLayScenarioConverter());
            return options;
        }

        public static string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, DefaultOptions);

        public static T? Deserialize<T>(string json)
            => JsonSerializer.Deserialize<T>(json, DefaultOptions);

        //public static string ToJson<T>(T dict)
        //{
        //    return JsonSerializer.Serialize(dict, options);
        //}

        // Run Expectancy Dict
        public static GameScenarioDict GetRunExpectancyDict(string json)
        {
            return JsonSerializer.Deserialize<GameScenarioDict>(json, DefaultOptions)
                ?? throw new Exception("Unable to deserialize RunExpectancyDict");
        }
        
        // Field Outcomes Dict
        public static FieldingDict GetFieldingDict(string json)
        {
            return JsonSerializer.Deserialize<FieldingDict>(json, DefaultOptions)
                ?? throw new Exception("Unable to deserialize GetFieldingDict");
        }

        // Baserunning Outcomes Dict
        public static BaserunningDict GetBaserunningDict(string json)
        {
            return JsonSerializer.Deserialize<BaserunningDict>(json, DefaultOptions)
                ?? throw new Exception("Unable to deserialize GetBaserunningDict");
        }

        // DoublePlay Outcomes Dict
        public static DoublePlayDict GetDoublePlayDict(string json)
        {
            return JsonSerializer.Deserialize<DoublePlayDict>(json, DefaultOptions)
                ?? throw new Exception("Unable to deserialize GetBaserunningDict");
        }
    }

    public static class PBP_TypeConversions
    {
        public static GameScenarios GetGameScenario(GamePlayByPlay pbp)
        {
            return new GameScenarios { outs = pbp.StartOuts, occupancy = pbp.StartBaseOccupancy };
        }

        // https://beanumber.github.io/abdwr3e/C_statcast.html#fig-spray-diagram
        private static float GetHitAngle(GamePlayByPlay pbp)
        {
            if (pbp.HitCoordX == null || pbp.HitCoordY == null)
                throw new Exception("Tried to get HitAngle with null HitCoords");

            return MathF.Atan2((float)pbp.HitCoordX - 125.42f, 198.27f - (float)pbp.HitCoordY);
        }

        private static float GetHitDist(GamePlayByPlay pbp)
        {
            if (pbp.HitCoordX == null || pbp.HitCoordY == null)
                throw new Exception("Tried to get GetHitDist with null HitCoords");

            float deltaX = (float)pbp.HitCoordX - 125.42f;
            float deltaY = 198.27f - (float)pbp.HitCoordY;
            return MathF.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
        }

        private static double[] Linspace(double start, double stop, int numPoints)
        {
            if (numPoints < 2) return new[] { start };

            double step = (stop - start) / (numPoints - 1);

            return Enumerable.Range(0, numPoints)
                             .Select(i => start + i * step)
                             .ToArray();
        }

        private static readonly double[] GroundballZoneXs = Linspace(-1.05 * Math.PI / 4, 1.05 * Math.PI / 4, 31);
        public static FieldingScenario GetGroundballScenario(GamePlayByPlay pbp)
        {
            float hitDist = GetHitDist(pbp);

            // Check for shortly hit ground balls
            if (hitDist < 30)
                return new FieldingScenario
                {
                    Hardness = PBP_HitHardness.None, // Set to none so this is never accidentally hit
                    Trajectory = PBP_HitTrajectory.Groundball,
                    ZoneAngle = 0, // So close to home plate that all are about the same
                    ZoneDist = 0
                };

            // Get hit angle
            float hitAngle = GetHitAngle(pbp);
            int hitZone = GroundballZoneXs.Length;
            for (int i = 0; i < GroundballZoneXs.Length; i++)
            {
                if (hitAngle < GroundballZoneXs[i])
                {
                    hitZone = i;
                    break;
                }
            }

            return new FieldingScenario
            {
                Hardness = (PBP_HitHardness)pbp.HitHardness,
                Trajectory = PBP_HitTrajectory.Groundball,
                ZoneAngle = hitZone,
                ZoneDist = 1
            };
        }

        private static readonly double[] LinedriveZoneAngles = Linspace(-1.05 * Math.PI / 4, 1.05 * Math.PI / 4, 31);
        private static readonly double[] LinedriveZoneDists = Linspace(80.0, 160.0, 22);
        public static FieldingScenario GetLinedriveScenario(GamePlayByPlay pbp)
        {
            float hitDist = GetHitDist(pbp);
            float hitAngle = GetHitAngle(pbp);

            int hitZoneAngle = LinedriveZoneAngles.Length;
            for (int i = 0; i < LinedriveZoneAngles.Length; i++)
            {
                if (hitAngle < LinedriveZoneAngles[i])
                {
                    hitZoneAngle = i;
                    break;
                }
            }

            int hitZoneDist = LinedriveZoneDists.Length;
            for (int i = 0; i < LinedriveZoneDists.Length; i++)
            {
                if (hitDist < LinedriveZoneDists[i])
                {
                    hitZoneDist = i;
                    break;
                }
            }

            return new FieldingScenario
            {
                Hardness = PBP_HitHardness.None, // Set to none so this is never accidentally hit
                                                // Not enough hard/soft hit to split by them
                Trajectory = PBP_HitTrajectory.Linedrive,
                ZoneAngle = hitZoneAngle,
                ZoneDist = hitZoneDist
            };
        }

        // Currently identical to linedrives, but may want to change in future
        private static readonly double[] FlyballZoneAngles = Linspace(-1.05 * Math.PI / 4, 1.05 * Math.PI / 4, 31);
        private static readonly double[] FlyballZoneDists = Linspace(80.0, 160.0, 22);
        public static FieldingScenario GetFlyballScenario(GamePlayByPlay pbp)
        {
            float hitDist = GetHitDist(pbp);
            float hitAngle = GetHitAngle(pbp);

            int hitZoneAngle = FlyballZoneAngles.Length;
            for (int i = 0; i < FlyballZoneAngles.Length; i++)
            {
                if (hitAngle < FlyballZoneAngles[i])
                {
                    hitZoneAngle = i;
                    break;
                }
            }

            int hitZoneDist = FlyballZoneDists.Length;
            for (int i = 0; i < FlyballZoneDists.Length; i++)
            {
                if (hitDist < FlyballZoneDists[i])
                {
                    hitZoneDist = i;
                    break;
                }
            }

            return new FieldingScenario
            {
                Hardness = PBP_HitHardness.None, // Set to none so this is never accidentally hit
                                                 // Not enough hard/soft hit to split by them
                Trajectory = PBP_HitTrajectory.Flyball,
                ZoneAngle = hitZoneAngle,
                ZoneDist = hitZoneDist
            };
        }

        public static FieldingScenario? GetFieldingScenario(GamePlayByPlay pbp)
        {
            if (pbp.HitHardness == null || pbp.HitTrajectory == null || pbp.HitZone == null || pbp.HitCoordX == null || pbp.HitCoordY == null)
                return null;

            if (pbp.Result.HasFlag(PBP_Events.HR))
                return null;

            if (pbp.EventFlag != GameFlags.Valid)
                return null;

            // Basically all popups are caught, and dropped will be caught in errors
            if (pbp.HitTrajectory == PBP_HitTrajectory.Popup) 
                return null;

            // Not handling bunts, too small sample size
            if ((pbp.HitTrajectory & (PBP_HitTrajectory.BuntGrounder | PBP_HitTrajectory.BuntLinedrive | PBP_HitTrajectory.BuntPopup)) != 0)
                return null;

            if (pbp.HitTrajectory == PBP_HitTrajectory.Groundball)
                return GetGroundballScenario(pbp);

            if (pbp.HitTrajectory == PBP_HitTrajectory.Linedrive)
                return GetLinedriveScenario(pbp);

            if (pbp.HitTrajectory == PBP_HitTrajectory.Flyball)
                return GetFlyballScenario(pbp);

            throw new Exception($"GetFieldingScenario unexpectedly reached end for pbpEventId={pbp.EventId}");
        }

        public static BaserunningScenario? GetBaserunningScenario(GamePlayByPlay pbp)
        {
            if (pbp.HitHardness == null || pbp.HitTrajectory == null || pbp.HitZone == null)
                return null;

            if (pbp.EventFlag != GameFlags.Valid)
                return null;

            if (pbp.HitterId == -1)
                return null;

            return new BaserunningScenario
            {
                Zone = (int)pbp.HitZone,
                Hardness = (PBP_HitHardness)pbp.HitHardness,
                Trajectory = (PBP_HitTrajectory)pbp.HitTrajectory!
            };
        }

        public static DoublePlayScenario? GetDoublePlayScenario(GamePlayByPlay pbp)
        {
            if (pbp.HitZone == null || pbp.HitZone < 3 || pbp.HitZone > 6 || pbp.HitTrajectory != PBP_HitTrajectory.Groundball
                || pbp.StartOuts >= 2 || !pbp.StartBaseOccupancy.HasFlag(BaseOccupancy.B1) || ((pbp.Result & PBP_HIT_EVENT) != 0))

                return null;

            if (pbp.EventFlag != GameFlags.Valid)
                return null;

            return new DoublePlayScenario
            {
                Zone = pbp.HitZone.Value,
            };
        }
    }
}
