namespace MinecraftWebExporter.Minecraft.BlockStates;

/// <summary>
/// The result for the block state export.
/// This is needed when multiple block models are combined.
/// </summary>
public readonly struct BlockStateResult
{
    /// <summary>
    /// Gets if this is a standard block
    /// </summary>
    public bool IsStandardBlock { get; init; }
}