using System.Text.Json.Serialization;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.Models;

/// <summary>
/// A element of a <see cref="Model"/>.
/// </summary>
public class ModelElement
{
    /// <summary>
    /// Gets the start position of the element
    /// </summary>
    [JsonPropertyName("from")] public Vector3 From { get; set; }
        
    /// <summary>
    /// Gets the end position of the element
    /// </summary>
    [JsonPropertyName("to")] public Vector3 To { get; set; }
        
    /// <summary>
    /// Gets the rotation of the element
    /// </summary>
    [JsonPropertyName("rotation")] public ModelRotation? Rotation { get; set; }
        
    /// <summary>
    /// Gets if shading is enabled for this element
    /// </summary>
    [JsonPropertyName("shade")] public bool? Shade { get; set; }
        
    /// <summary>
    /// Gets the list of faces
    /// </summary>
    [JsonPropertyName("faces")] public ModelElementFaces? Faces { get; set; }
}