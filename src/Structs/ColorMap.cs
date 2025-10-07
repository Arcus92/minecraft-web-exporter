namespace MinecraftWebExporter.Structs;

/// <summary>
/// A struct for a tint color map
/// </summary>
public readonly struct ColorMap
{
    /// <summary>
    /// The lower left color
    /// </summary>
    public Vector3 Uv00 { get; init; }
            
    /// <summary>
    /// The lower right color
    /// </summary>
    public Vector3 Uv10 { get; init; }
            
    /// <summary>
    /// The upper left color
    /// </summary>
    public Vector3 Uv01 { get; init; }

    /// <summary>
    /// Gets the value
    /// </summary>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public Vector3 GetValue(float u, float v)
    {
        var x = u - v;
        var y = 1f - u;
        var z = v;
                
        return new Vector3()
        {
            X = x * Uv00.X + y * Uv10.X + z * Uv01.X, 
            Y = x * Uv00.Y + y * Uv10.Y + z * Uv01.Y, 
            Z = x * Uv00.Z + y * Uv10.Z + z * Uv01.Z,
        };
    }
}