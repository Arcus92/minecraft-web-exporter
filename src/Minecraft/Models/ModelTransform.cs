using System.Text.Json.Serialization;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.Models
{
    /// <summary>
    /// The model transformation
    /// </summary>
    public class ModelTransform
    {
        /// <summary>
        /// Gets the rotation
        /// </summary>
        [JsonPropertyName("rotation")] public Vector3 Rotation { get; set; }
        
        /// <summary>
        /// Gets the translation
        /// </summary>
        [JsonPropertyName("translation")] public Vector3 Translation { get; set; }
        
        /// <summary>
        /// Gets the scale
        /// </summary>
        [JsonPropertyName("scale")] public Vector3 Scale { get; set; }
    }
}