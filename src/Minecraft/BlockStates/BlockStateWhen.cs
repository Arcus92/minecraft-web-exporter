using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using SharpNBT;

namespace MinecraftWebExporter.Minecraft.BlockStates
{
    /// <summary>
    /// The when block for a <see cref="BlockStateMultipart"/>.
    /// </summary>
    [JsonConverter(typeof(BlockStateWhenJsonConverter))]
    public class BlockStateWhen : List<Dictionary<string, string>>
    {
        /// <summary>
        /// Checks if the when condition is met
        /// </summary>
        /// <param name="propertiesTag"></param>
        /// <returns></returns>
        public bool Check(CompoundTag? propertiesTag)
        {
            // No checks
            if (Count == 0)
                return true;

            foreach (var when in this)
            {
                var success = true;
                foreach (var pair in when)
                {
                    var tag = propertiesTag?[pair.Key] as StringTag;
                    var value = tag?.Value;

                    if (value != pair.Value)
                    {
                        success = false;
                        break;
                    }
                }

                if (success)
                    return true;
            }

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

            // Dictionary converter
            var converter =
                options.GetConverter(typeof(Dictionary<string, string>)) as JsonConverter<Dictionary<string, string>>;
            Debug.Assert(converter != null, nameof(converter) + " != null");
            
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
                                        var item = converter.Read(ref reader, typeof(Dictionary<string, string>), options);
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
                    else
                    {
                        reader = start;
                        var item = converter.Read(ref reader, typeof(Dictionary<string, string>), options);
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
            throw new NotImplementedException();
        }
    }
}