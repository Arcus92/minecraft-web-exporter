namespace MinecraftWebExporter.Export
{
    /// <summary>
    /// The detail type of the map exporter
    /// </summary>
    public enum ExportDetailLevelType
    {
        /// <summary>
        /// Blocks are exported.
        /// This generates the real Minecraft look and is great for close ups, but generates large geometries.
        /// </summary>
        Blocks,
        
        /// <summary>
        /// Only the heightmap is exported.
        /// This generates a low detail view of the world surface. This should be used for far away views due to the
        /// reduced geometry.
        /// </summary>
        Heightmap,
    }
}