using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftWebExporter.Utils
{
    /// <summary>
    /// There are a few resource packs that use native types instead of fixed strings.
    /// For example: JohnSmith Legacy uses the JSON false/true types instead of "true", "false" as string.
    /// This class converts these types to plain strings.
    /// </summary>
    public class JsonStringFixConverter : JsonConverter<string>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(string);
        }

        /// <summary>
        /// Reads the json element
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString() ?? string.Empty;
                case JsonTokenType.True:
                    return "true";
                case JsonTokenType.False:
                    return "false";
                default:
                    throw new JsonException();
            }
        }

        /// <summary>
        /// Writes the json element
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}