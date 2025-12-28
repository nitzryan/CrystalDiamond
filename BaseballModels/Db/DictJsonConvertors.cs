using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Db.DbEnums;


namespace Db
{
    public class GameScenariosConverter : JsonConverter<GameScenarios>
    {
        public override GameScenarios Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected StartObject");

            int? outs = null;
            BaseOccupancy? occupancy = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                string? prop = reader.GetString();

                reader.Read();

                switch (prop)
                {
                    case nameof(GameScenarios.outs):
                        outs = reader.GetInt32();
                        break;
                    case nameof(GameScenarios.occupancy):
                        occupancy = JsonSerializer.Deserialize<BaseOccupancy>(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            if (outs is null || occupancy is null)
                throw new JsonException("Missing required properties for GameScenarios");

            return new GameScenarios
            {
                outs = outs.Value,
                occupancy = occupancy.Value
            };
        }

        public override void Write(
            Utf8JsonWriter writer,
            GameScenarios value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber(nameof(GameScenarios.outs), value.outs);
            writer.WritePropertyName(nameof(GameScenarios.occupancy));
            JsonSerializer.Serialize(writer, value.occupancy, options);
            writer.WriteEndObject();
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] GameScenarios value, JsonSerializerOptions options)
        {
            writer.WritePropertyName($"{value.outs}:{(int)value.occupancy}");
        }

        public override GameScenarios ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? key = reader.GetString();
            if (string.IsNullOrEmpty(key))
                throw new JsonException("Dictionary key cannot be empty");

            var parts = key.Split(':', 2);
            if (parts.Length != 2 || !int.TryParse(parts[0], out int outs) || !Enum.TryParse<BaseOccupancy>(parts[1], out var occ))
                throw new JsonException($"Invalid GameScenarios key format: {key}");

            return new GameScenarios { outs = outs, occupancy = occ };
        }
    }

    public class FieldingScenarioConverter : JsonConverter<FieldingScenario>
    {
        public override FieldingScenario Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected StartObject");

            int? zone = null;
            PBP_HitTrajectory? trajectory = null;
            PBP_HitHardness? hardness = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                string? prop = reader.GetString();

                reader.Read();

                switch (prop)
                {
                    case nameof(FieldingScenario.Zone):
                        zone = reader.GetInt32();
                        break;
                    case nameof(FieldingScenario.Trajectory):
                        trajectory = JsonSerializer.Deserialize<PBP_HitTrajectory>(ref reader, options);
                        break;
                    case nameof(FieldingScenario.Hardness):
                        hardness = JsonSerializer.Deserialize<PBP_HitHardness>(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            if (zone is null || trajectory is null || hardness is null)
                throw new JsonException("Missing required properties for FieldingScenario");

            return new FieldingScenario
            {
                Zone = zone.Value,
                Trajectory = trajectory.Value,
                Hardness = hardness.Value
            };
        }

        public override void Write(
            Utf8JsonWriter writer,
            FieldingScenario value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber(nameof(FieldingScenario.Zone), value.Zone);
            writer.WritePropertyName(nameof(FieldingScenario.Trajectory));
            JsonSerializer.Serialize(writer, value.Trajectory, options);
            writer.WritePropertyName(nameof(FieldingScenario.Hardness));
            JsonSerializer.Serialize(writer, value.Hardness, options);
            writer.WriteEndObject();
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] FieldingScenario value, JsonSerializerOptions options)
        {
            writer.WritePropertyName($"{value.Zone}:{(int)value.Trajectory}:{(int)value.Hardness}");
        }

        public override FieldingScenario ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? key = reader.GetString();
            if (string.IsNullOrEmpty(key))
                throw new JsonException("Dictionary key cannot be empty");

            var parts = key.Split(':', 3);
            if (parts.Length != 3 || !int.TryParse(parts[0], out int zone) || !Enum.TryParse<PBP_HitTrajectory>(parts[1], out var traj) || !Enum.TryParse<PBP_HitHardness>(parts[2], out var hard))
                throw new JsonException($"Invalid GameScenarios key format: {key}");

            return new FieldingScenario { Zone = zone, Trajectory = traj, Hardness = hard };
        }
    }

    public class BaserunningScenarioConverter : JsonConverter<BaserunningScenario>
    {
        public override BaserunningScenario Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected StartObject");

            int? zone = null;
            PBP_HitTrajectory? trajectory = null;
            PBP_HitHardness? hardness = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                string? prop = reader.GetString();

                reader.Read();

                switch (prop)
                {
                    case nameof(BaserunningScenario.Zone):
                        zone = reader.GetInt32();
                        break;
                    case nameof(BaserunningScenario.Trajectory):
                        trajectory = JsonSerializer.Deserialize<PBP_HitTrajectory>(ref reader, options);
                        break;
                    case nameof(BaserunningScenario.Hardness):
                        hardness = JsonSerializer.Deserialize<PBP_HitHardness>(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            if (zone is null || trajectory is null || hardness is null)
                throw new JsonException("Missing required properties for BaserunningScenario");

            return new BaserunningScenario
            {
                Zone = zone.Value,
                Trajectory = trajectory.Value,
                Hardness = hardness.Value
            };
        }

        public override void Write(
            Utf8JsonWriter writer,
            BaserunningScenario value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber(nameof(BaserunningScenario.Zone), value.Zone);
            writer.WritePropertyName(nameof(BaserunningScenario.Trajectory));
            JsonSerializer.Serialize(writer, value.Trajectory, options);
            writer.WritePropertyName(nameof(BaserunningScenario.Hardness));
            JsonSerializer.Serialize(writer, value.Hardness, options);
            writer.WriteEndObject();
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] BaserunningScenario value, JsonSerializerOptions options)
        {
            writer.WritePropertyName($"{value.Zone}:{(int)value.Trajectory}:{(int)value.Hardness}");
        }

        public override BaserunningScenario ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? key = reader.GetString();
            if (string.IsNullOrEmpty(key))
                throw new JsonException("Dictionary key cannot be empty");

            var parts = key.Split(':', 3);
            if (parts.Length != 3 || !int.TryParse(parts[0], out int zone) || !Enum.TryParse<PBP_HitTrajectory>(parts[1], out var traj) || !Enum.TryParse<PBP_HitHardness>(parts[2], out var hard))
                throw new JsonException($"Invalid GameScenarios key format: {key}");

            return new BaserunningScenario { Zone = zone, Trajectory = traj, Hardness = hard };
        }
    }
}
