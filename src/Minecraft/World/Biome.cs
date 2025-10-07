using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.World;

/// <summary>
/// The biome data
/// </summary>
public readonly struct Biome
{
    /// <summary>
    /// Gets the name of the biome
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the default temperature
    /// </summary>
    public float Temperature { get; init; }
        
    /// <summary>
    /// Gets the default rainfall value
    /// </summary>
    public float Rainfall { get; init; }

    /// <summary>
    /// Gets the water surface color
    /// </summary>
    public Vector3 WaterSurfaceColor { get; init; }
}