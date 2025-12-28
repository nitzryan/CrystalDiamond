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
        public required int Zone { get; set; }
        public required PBP_HitTrajectory Trajectory { get; set; }
        public required PBP_HitHardness Hardness { get; set; }
    }
    public record FieldingResults
    {
        public required float ProbMake { get; set; }
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
    public record BaserunningScenario
    {
        public required int Zone { get; set; }
        public required PBP_HitTrajectory Trajectory { get; set; }
        public required PBP_HitHardness Hardness { get; set; }
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

        // Baserunning Outcoomes Dict
        public static BaserunningDict GetBaserunningDict(string json)
        {
            return JsonSerializer.Deserialize<BaserunningDict>(json, DefaultOptions)
                ?? throw new Exception("Unable to deserialize GetBaserunningDict");
        }
    }

    public static class PBP_TypeConversions
    {
        public static GameScenarios GetGameScenario(GamePlayByPlay pbp)
        {
            return new GameScenarios { outs = pbp.StartOuts, occupancy = pbp.StartBaseOccupancy };
        }

        public static FieldingScenario? GetFieldingScenario(GamePlayByPlay pbp)
        {
            if (pbp.HitHardness == null || pbp.HitTrajectory == null || pbp.HitZone == null)
                return null;

            return new FieldingScenario { 
                Zone = (int)pbp.HitZone, 
                Hardness = (PBP_HitHardness)pbp.HitHardness, 
                Trajectory = (PBP_HitTrajectory)pbp.HitTrajectory! 
            };
        }

        public static BaserunningScenario? GetBaserunningScenario(GamePlayByPlay pbp)
        {
            if (pbp.HitHardness == null || pbp.HitTrajectory == null || pbp.HitZone == null)
                return null;

            return new BaserunningScenario
            {
                Zone = (int)pbp.HitZone,
                Hardness = (PBP_HitHardness)pbp.HitHardness,
                Trajectory = (PBP_HitTrajectory)pbp.HitTrajectory!
            };
        }
    }
}
