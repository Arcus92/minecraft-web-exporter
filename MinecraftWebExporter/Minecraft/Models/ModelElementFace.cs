using System.Text.Json.Serialization;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.Models
{
    /// <summary>
    /// A single face of a <see cref="ModelElement"/>.
    /// </summary>
    public class ModelElementFace
    {
        /// <summary>
        /// Gets the uv coordinates
        /// </summary>
        [JsonPropertyName("uv")] public Vector4? Uv { get; set; }

        /// <summary>
        /// Gets the uv rotation
        /// </summary>
        [JsonPropertyName("rotation")] public int? Rotation { get; set; }

        /// <summary>
        /// Gets the name in the texture map <see cref="ModelTextures"/>
        /// </summary>
        [JsonPropertyName("texture")] public string? Texture { get; set; }
        
        /// <summary>
        /// Gets the name of the cull face
        /// </summary>
        [JsonConverter(typeof(NullableDirectionJsonConverter))]
        [JsonPropertyName("cullface")] public Direction? CullFace { get; set; }
        
        /// <summary>
        /// Gets the tint index
        /// </summary>
        [JsonPropertyName("tintindex")] public int? TintIndex { get; set; }
    }
}