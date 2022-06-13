using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftWebExporter.Minecraft.BlockStates
{
    /// <summary>
    /// The variation list for block states
    /// </summary>
    [JsonConverter(typeof(BlockStateVariationsJsonConverter))]
    public class BlockStateVariants : Dictionary<string, BlockStateVariant[]>
    {
        /// <summary>
        /// Returns all model variants by the given properties
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public BlockStateVariant[] GetVariantsByProperties(IBlockStateProperties? properties)
        {
            foreach (var pair in this)
            {
                var pos = 0;
                int equal;
                var success = true;
                while ((equal = pair.Key.IndexOf('=', pos)) != -1)
                {
                    var end = pair.Key.IndexOf(',', equal);
                    if (end == -1) end = pair.Key.Length;
                    
                    var name = pair.Key.Substring(pos, equal - pos);
                    var value = pair.Key.Substring(equal + 1, end - equal - 1);
                    
                    var tagValue = properties?.GetValueOrDefault(name);
                    if (value != tagValue)
                    {
                        success = false;
                        break;
                    }
                    
                    if (end == pair.Key.Length)
                        break;
                    pos = end + 1;
                }

                if (success)
                {
                    return pair.Value;
                }
            }

            return Array.Empty<BlockStateVariant>();
        }
    }
    
    /// <summary>
    /// The json converter for <see cref="BlockStateVariants"/>.
    /// </summary>
    public class BlockStateVariationsJsonConverter : JsonConverter<BlockStateVariants>
    {
        /// <summary>
        /// Reads the json element
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override BlockStateVariants Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var variants = new BlockStateVariants();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var name = reader.GetString();
                    Debug.Assert(name != null, nameof(name) + " != null");
                    
                    if (reader.Read())
                    {
                        var converter = new BlockStateVariationArrayJsonConverter();
                        var array = converter.Read(ref reader, typeof(BlockStateVariant[]), options);
                        
                        variants.Add(name, array);
                    }
                }
                else if (reader.TokenType != JsonTokenType.Comment)
                {
                    throw new JsonException();
                }
            }

            return variants;
        }

        /// <summary>
        /// Writes the json element
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, BlockStateVariants value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}