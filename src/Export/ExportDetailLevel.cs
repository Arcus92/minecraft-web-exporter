using System.Text.Json.Serialization;

namespace MinecraftWebExporter.Export;

/// <summary>
/// A map can have multiple detail levels depending on the camera zoom or gpu power of the viewing device.
/// </summary>
public class ExportDetailLevel
{
    /// <summary>
    /// Gets the filename prefix. This is used to build the output file: {filename}.{x}.{z}.m
    /// </summary>
    [JsonPropertyName("filename")]
    public string Filename { get; init; } = string.Empty;

    /// <summary>
    /// Gets and sets the detail level
    /// </summary>
    [JsonPropertyName("type")]
    public ExportDetailLevelType Type { get; init; }

    /// <summary>
    /// Gets and sets how many Minecraft chunks written to a single geometry file (in one dimension only).
    /// This value has to be a divisor of 32 and can not be larger than 32, so it fits into a region.
    /// </summary>
    [JsonPropertyName("chunkSpan")]
    public int ChunkSpan { get; init; } = 1;

    /// <summary>
    /// Gets and sets how many blocks are merged into one heightmap point. This helps to reduce the geometry
    /// of a heightmap. This is only used it <see cref="Type"/> is set to <see cref="ExportDetailLevelType.Heightmap"/>.
    /// This value has to be a divisor of 16 * <see cref="ChunkSpan"/>.
    /// </summary>
    [JsonPropertyName("blockSpan")]
    public int BlockSpan { get; init; } = 1;
        
    /// <summary>
    /// Gets the zoom distance for the viewer.
    /// </summary>
    [JsonPropertyName("distance")]
    public int Distance { get; init; }

    /// <summary>
    /// Gets the number of chunks per region (in one dimension only).
    /// </summary>
    [JsonIgnore]
    public int ChunksInRegion => 32 / ChunkSpan;
        
    /// <summary>
    /// Gets the number of blocks per chunk (in one dimension only).
    /// </summary>
    [JsonIgnore]
    public int BlocksInChunk => 16 * ChunkSpan;
}