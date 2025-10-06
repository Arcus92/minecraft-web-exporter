using System.Collections;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.Textures;

/// <summary>
/// The cached texture information
/// </summary>
public readonly struct CachedTexture
{
    /// <summary>
    /// Gets the width of the texture
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// Gets the height of the texture
    /// </summary>
    public int Height { get; init; }

        

    /// <summary>
    /// Gets the texture animation info
    /// </summary>
    public TextureAnimation? Animation { get; init; }

    /// <summary>
    /// Gets if the transparency info of this texture
    /// </summary>
    public TextureTransparency Transparency { get; init; }
        
    /// <summary>
    /// Gets a cache to check for transparent areas in this texture.
    /// This is null if no pixel has transparency.
    /// </summary>
    public BitArray? TransparencyBits { get; init; }

    /// <summary>
    /// Gets if the given area is transparent
    /// </summary>
    /// <param name="uv"></param>
    /// <returns></returns>
    public bool IsAreaTransparent(Vector4 uv)
    {
        if (TransparencyBits is null)
            return false;
            
        var x1 = (int) (uv.X / 16f * Width);
        var y1 = (int) (uv.Y / 16f * Height);
        var x2 = (int) (uv.Z / 16f * Width);
        var y2 = (int) (uv.W / 16f * Height);

        if (x1 > x2)
        {
            (x1, x2) = (x2, x1);
        }
        if (y1 > y2)
        {
            (y1, y2) = (y2, y1);
        }

        for (var x=x1; x<x2; x++)
        for (var y=y1; y<y2; y++)
        {
            if (TransparencyBits[x + y * Width])
                return true;
        }
            
        return false;
    }
}