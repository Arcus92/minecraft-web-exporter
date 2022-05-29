using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftWebExporter.Structs
{
    /// <summary>
    /// The direction
    /// </summary>
    [JsonConverter(typeof(DirectionJsonConverter))]
    public enum Direction
    {
        Down,
        Up,
        North,
        South,
        West,
        East
    }

    /// <summary>
    /// The json converter for <see cref="Direction"/>
    /// </summary>
    public class DirectionJsonConverter : JsonConverter<Direction>
    {
        /// <summary>
        /// Reads the json object
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Direction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            switch (reader.GetString())
            {
                case "down":
                case "bottom":
                    return Direction.Down;
                case "up":
                case "top":
                    return Direction.Up;
                case "north":
                    return Direction.North;
                case "south":
                    return Direction.South;
                case "west":
                    return Direction.West;
                case "east":
                    return Direction.East;
                default:
                    throw new JsonException();
            }
        }

        /// <summary>
        /// Writes the json object
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, Direction value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case Direction.Down:
                    writer.WriteStringValue("down");
                    break;
                case Direction.Up:
                    writer.WriteStringValue("up");
                    break;
                case Direction.North:
                    writer.WriteStringValue("north");
                    break;
                case Direction.South:
                    writer.WriteStringValue("south");
                    break;
                case Direction.West:
                    writer.WriteStringValue("west");
                    break;
                case Direction.East:
                    writer.WriteStringValue("east");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }

    /// <summary>
    /// The json converter for <see cref="Direction"/>
    /// </summary>
    public class NullableDirectionJsonConverter : JsonConverter<Direction?>
    {
        /// <summary>
        /// Reads the json object
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Direction? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }
            
            switch (reader.GetString())
            {
                case "down":
                case "bottom":
                    return Direction.Down;
                case "up":
                case "top":
                    return Direction.Up;
                case "north":
                    return Direction.North;
                case "south":
                    return Direction.South;
                case "west":
                    return Direction.West;
                case "east":
                    return Direction.East;
                default:
                    throw new JsonException();
            }
        }

        /// <summary>
        /// Writes the json object
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, Direction? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }
            
            switch (value.Value)
            {
                case Direction.Down:
                    writer.WriteStringValue("down");
                    break;
                case Direction.Up:
                    writer.WriteStringValue("up");
                    break;
                case Direction.North:
                    writer.WriteStringValue("north");
                    break;
                case Direction.South:
                    writer.WriteStringValue("south");
                    break;
                case Direction.West:
                    writer.WriteStringValue("west");
                    break;
                case Direction.East:
                    writer.WriteStringValue("east");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}