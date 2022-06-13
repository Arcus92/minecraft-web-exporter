using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MinecraftWebExporter.Minecraft.BlockStates.Cache;

namespace MinecraftWebExporter.Minecraft.BlockStates
{
    /// <summary>
    /// The block state variation
    /// </summary>
    public class BlockStateVariant
    {
        /// <summary>
        /// Gets and sets the name of the model
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets and sets the x rotation in deg
        /// </summary>
        [JsonPropertyName("x")] public float X { get; set; }
        
        /// <summary>
        /// Gets and sets the y rotation in deg
        /// </summary>
        [JsonPropertyName("y")] public float Y { get; set; }

        /// <summary>
        /// Gets and sets if the uv is locked
        /// </summary>
        [JsonPropertyName("uvlock")] public bool UvLock { get; set; }

        /// <summary>
        /// Gets and sets the weight for random variation placement
        /// </summary>
        [JsonPropertyName("weight")] public float Weight { get; set; }

        #region Build

        /// <summary>
        /// Gets the cached model
        /// </summary>
        /// <param name="block"></param>
        /// <param name="faces"></param>
        /// <param name="assetManager"></param>
        /// <returns></returns>
        public async ValueTask BuildCachedFacesAsync(AssetIdentifier block, List<CachedBlockStateFace> faces, IAssetManager assetManager)
        {
            // Loads the actual model
            var asset = new AssetIdentifier(AssetType.Model, Model);
            var model = await assetManager.GetCachedModelAsync(asset);
            if (model.Faces is null)
                return;
            
            // Adds all faces
            foreach (var face in model.Faces)
            {
                faces.Add(CachedBlockStateFace.Rotate(CachedBlockStateFace.Create(block, face), X, Y, UvLock));
            }
        }

        #endregion Build
    }
    
    /// <summary>
    /// The json converter for <see cref="BlockStateVariant"/> array.
    /// </summary>
    public class BlockStateVariationArrayJsonConverter : JsonConverter<BlockStateVariant[]>
    {
        /// <summary>
        /// Reads the json element
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override BlockStateVariant[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var variantConverter = options.GetConverter(typeof(BlockStateVariant)) as JsonConverter<BlockStateVariant>;
            if (variantConverter is null)
                throw new ArgumentNullException();
            
            var list = new List<BlockStateVariant>();
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var variant = variantConverter.Read(ref reader, typeof(BlockStateVariant), options);
                if (variant is not null)
                {
                    list.Add(variant);
                }
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }
                    else if (reader.TokenType == JsonTokenType.StartObject)
                    {
                        var variant = variantConverter.Read(ref reader, typeof(BlockStateVariant), options);
                        if (variant is not null)
                        {
                            list.Add(variant);
                        }
                    }
                }
            }
            else
            {
                throw new JsonException();
            }

            return list.ToArray();
        }

        /// <summary>
        /// Writes the json element
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, BlockStateVariant[] value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}