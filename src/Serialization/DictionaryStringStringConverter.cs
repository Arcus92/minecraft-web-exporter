using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftWebExporter.Serialization;

/// <summary>
/// There are a few resource packs that use native types instead of fixed strings.
/// For example: JohnSmith Legacy uses the JSON false/true types instead of "true", "false" as string.
/// This class converts these types to plain strings.
/// </summary>
public class DictionaryStringStringConverter : JsonConverter<Dictionary<string, string>>
{
    /// <inheritdoc />
    public override Dictionary<string, string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dict = new Dictionary<string, string>();

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dict;
            }

            // Read the key
            var key = reader.GetString() ?? string.Empty;
            reader.Read();
            
            var value = reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.False => "false",
                JsonTokenType.True => "true",
                JsonTokenType.Null => null,
                _ => throw new JsonException()
            };

            dict[key] = value ?? string.Empty;
        }

        throw new JsonException();
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Dictionary<string, string> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var pair in value)
        {
            writer.WritePropertyName(pair.Key);
            writer.WriteStringValue(pair.Value);
        }
        writer.WriteEndObject();
    }
}