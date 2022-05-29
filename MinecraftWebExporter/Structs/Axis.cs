using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftWebExporter.Structs
{
    /// <summary>
    /// Enum for the X, Y or Z axis.
    /// </summary>
    [JsonConverter(typeof(AxisJsonConverter))]
    public enum Axis
    {
        X, Y, Z
    }

    /// <summary>
    /// The json converter for <see cref="Axis"/>
    /// </summary>
    public class AxisJsonConverter : JsonConverter<Axis>
    {
        /// <summary>
        /// Reads the json object
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Axis Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }
            switch (reader.GetString())
            {
                case "x":
                    return Axis.X;
                case "y":
                    return Axis.Y;
                case "z":
                    return Axis.Z;
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
        public override void Write(Utf8JsonWriter writer, Axis value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case Axis.X:
                    writer.WriteStringValue("x");
                    break;
                case Axis.Y:
                    writer.WriteStringValue("y");
                    break;
                case Axis.Z:
                    writer.WriteStringValue("z");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}