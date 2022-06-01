namespace MinecraftWebExporter.Minecraft.BlockStates.Cache
{
    /// <summary>
    /// A single variant for a block state
    /// </summary>
    public readonly struct CachedBlockStateVariant
    {
        /// <summary>
        /// The faces for this block state
        /// </summary>
        public CachedBlockStateFace[]? Faces { get; init; }
        
        /// <summary>
        /// Gets and sets the weight for random variation placement
        /// </summary>
        public float Weight { get; init; }
    }
}