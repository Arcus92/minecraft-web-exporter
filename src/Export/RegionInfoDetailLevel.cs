namespace MinecraftWebExporter.Export;

/// <summary>
/// The timestamp info for a <see cref="ExportDetailLevel"/>.
/// </summary>
public class RegionInfoDetailLevel
{
    /// <summary>
    /// Gets and sets the filename of the <see cref="ExportDetailLevel"/>.
    /// </summary>
    public string? Filename { get; set; }

    /// <summary>
    /// Gets and sets the timestamp
    /// </summary>
    public long[]? Timestamps { get; set; }
}