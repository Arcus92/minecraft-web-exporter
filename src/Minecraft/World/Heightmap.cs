using System;

namespace MinecraftWebExporter.Minecraft.World;

/// <summary>
/// A height map of a <see cref="Chunk"/>
/// </summary>
public readonly struct Heightmap
{
    /// <summary>
    /// Creates the height map
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="height"></param>
    public Heightmap(Chunk chunk, ushort[] height)
    {
        Chunk = chunk;
        m_Heights = height;
    }

    /// <summary>
    /// Gets the chunk
    /// </summary>
    public Chunk Chunk { get; }

    /// <summary>
    /// The height data
    /// </summary>
    private readonly ushort[] m_Heights;

    /// <summary>
    /// Returns the height for the given chunk coordinate
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public int GetHeight(byte x, byte z)
    {
        // Minecraft 1.18
        if (Chunk.DataVersion >= 2825)
        {
            return m_Heights[x + z * 16] - 64;
        }
        else
        {
            return m_Heights[x + z * 16];
        }
    }
        
    #region Static

    /// <summary>
    /// Gets the height map type by the given name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static HeightmapType GetHeightmapType(string name)
    {
        switch (name)
        {
            case "MOTION_BLOCKING":
                return HeightmapType.MotionBlocking;
            case "MOTION_BLOCKING_NO_LEAVES":
                return HeightmapType.MotionBlockingNoLeaves;
            case "OCEAN_FLOOR":
                return HeightmapType.OceanFloor;
            case "OCEAN_FLOOR_WG":
                return HeightmapType.OceanFloorWorldGen;
            case "WORLD_SURFACE":
                return HeightmapType.WorldSurface;
            case "WORLD_SURFACE_WG":
                return HeightmapType.WorldSurfaceWorldGen;
            default:
                throw new ArgumentException("Invalid value!", nameof(name));
        }
    }
        
    #endregion Static
}