using System.Collections.Generic;
using System.Text.Json.Serialization;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.Models;

/// <summary>
/// The face list of a <see cref="ModelElement"/>.
/// </summary>
public class ModelElementFaces
{
    /// <summary>
    /// Gets the bottom face
    /// </summary>
    [JsonPropertyName("down")] public ModelElementFace? Down { get; set; }
        
    /// <summary>
    /// Gets the top face
    /// </summary>
    [JsonPropertyName("up")] public ModelElementFace? Up { get; set; }
        
    /// <summary>
    /// Gets the north face
    /// </summary>
    [JsonPropertyName("north")] public ModelElementFace? North { get; set; }
        
    /// <summary>
    /// Gets the south face
    /// </summary>
    [JsonPropertyName("south")] public ModelElementFace? South { get; set; }
        
    /// <summary>
    /// Gets the west face
    /// </summary>
    [JsonPropertyName("west")] public ModelElementFace? West { get; set; }
        
    /// <summary>
    /// Gets the east face
    /// </summary>
    [JsonPropertyName("east")] public ModelElementFace? East { get; set; }

    /// <summary>
    /// Returns all set faces
    /// </summary>
    /// <returns></returns>
    public IEnumerable<(Direction, ModelElementFace)> GetFaces()
    {
        if (Down is not null) yield return (Direction.Down, Down);
        if (Up is not null) yield return (Direction.Up, Up);
        if (North is not null) yield return (Direction.North, North);
        if (South is not null) yield return (Direction.South, South);
        if (West is not null) yield return (Direction.West, West);
        if (East is not null) yield return (Direction.East, East);
    }
}