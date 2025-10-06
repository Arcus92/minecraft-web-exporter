using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using MinecraftWebExporter.Serialization;

namespace MinecraftWebExporter.Minecraft.BlockStates;

/// <summary>
/// The when block for a <see cref="BlockStateMultipart"/>.
/// The first level list is the OR condition. Only one of the sub-elements need to match to fulfill the condition.
/// The second level directory defines all property that need to match the block property tag to be valid.
/// The third level is the value of the property. This can be split by '|' to allow multiple possible values.
/// Only one of the property values must match to fulfill the property.
/// </summary>
[JsonConverter(typeof(BlockStateWhenJsonConverter))]
public class BlockStateWhen : List<Dictionary<string, string>>
{
    /// <summary>
    /// Checks if the when condition is met
    /// </summary>
    /// <param name="properties"></param>
    /// <returns></returns>
    public bool Check(IBlockStateProperties? properties)
    {
        // No checks
        if (Count == 0)
            return true;

        // At least one iteration must match
        foreach (var when in this)
        {
            // All properties must match
            var allPropertiesMatched = true;
            foreach (var pair in when)
            {
                var value = properties?.GetValueOrDefault(pair.Key);

                // The block definition can define multiple values seperated by an 'or' / '|'.
                // At least one value must match the property.
                var curPropertyMatched = false;
                if (value is not null)
                {
                    var start = 0;
                    while (start < pair.Value.Length)
                    {
                        // Find next pipe
                        var end = pair.Value.IndexOf('|', start + 1);
                        if (end < 0) end = pair.Value.Length;
                        // Use span to not create any more memory allocations in this loop
                        var subValue = pair.Value.AsSpan().Slice(start, end - start);
                        if (subValue.Equals(value, StringComparison.InvariantCulture))
                        {
                            curPropertyMatched = true;
                            break;
                        }
                            
                        start = end + 1;
                    }
                }
                    
                // Early skip
                if (!curPropertyMatched)
                {
                    allPropertiesMatched = false;
                    break;
                }
            }

            // All properties in this OR group matched. We can bail here.
            if (allPropertiesMatched)
                return true;
        }

        // Nothing matched...
        return false;
    }
}
    
/// <summary>
/// The json converter for <see cref="BlockStateWhen"/>.
/// </summary>
public class BlockStateWhenJsonConverter : JsonConverter<BlockStateWhen>
{
    /// <summary>
    /// Reads the json element
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    public override BlockStateWhen Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }
            
        var when = new BlockStateWhen();
   
        var start = reader;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var name = reader.GetString();
                if (name == "OR")
                {
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject)
                        {
                            break;
                        }
                            
                        if (reader.TokenType == JsonTokenType.StartArray)
                        {
                            while (reader.Read())
                            {
                                if (reader.TokenType == JsonTokenType.EndArray)
                                {
                                    break;
                                }

                                if (reader.TokenType == JsonTokenType.StartObject)
                                {
                                    var item = JsonSerializer.Deserialize(ref reader, JsonContext.Default.DictionaryStringString);
                                    if (item is not null)
                                    {
                                        when.Add(item);
                                    }
                                }
                                else if (reader.TokenType != JsonTokenType.Comment)
                                {
                                    throw new JsonException();
                                }
                            }
                        }
                        else if (reader.TokenType != JsonTokenType.Comment)
                        {
                            throw new JsonException();
                        }
                    }
                }
                else if (name == "AND")
                {
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject)
                        {
                            break;
                        }
                            
                        if (reader.TokenType == JsonTokenType.StartArray)
                        {
                            var item = new Dictionary<string, string>();
                            while (reader.Read())
                            {
                                if (reader.TokenType == JsonTokenType.EndArray)
                                {
                                    break;
                                }

                                if (reader.TokenType == JsonTokenType.StartObject)
                                {
                                    var subItem = JsonSerializer.Deserialize(ref reader, JsonContext.Default.DictionaryStringString);
                                    if (subItem is not null)
                                    {
                                        foreach (var pair in subItem)
                                        {
                                            item.Add(pair.Key, pair.Value);
                                        }
                                    }
                                }
                                else if (reader.TokenType != JsonTokenType.Comment)
                                {
                                    throw new JsonException();
                                }
                            }
                            when.Add(item);
                        }
                        else if (reader.TokenType != JsonTokenType.Comment)
                        {
                            throw new JsonException();
                        }
                    }
                }
                else
                {
                    reader = start;
                    var item = JsonSerializer.Deserialize(ref reader, JsonContext.Default.DictionaryStringString);
                    if (item is not null)
                    {
                        when.Add(item);
                    }
                }
                break;
            }
            else if (reader.TokenType != JsonTokenType.Comment)
            {
                throw new JsonException();
            }
        }
        return when;
    }

    /// <summary>
    /// Writes the json element
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, BlockStateWhen value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
}