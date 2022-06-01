namespace MinecraftWebExporter.Export
{
    /// <summary>
    /// A material definition
    /// </summary>
    public class MeshMaterial
    {
        /// <summary>
        /// Gets and sets the name of the material
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets and sets the texture map
        /// </summary>
        public string? Texture { get; set; }

        /// <summary>
        /// Gets and sets if this material uses a semi transparent texture
        /// </summary>
        public bool Transparent { get; set; }

        /// <summary>
        /// Gets and sets the number of animation frames
        /// </summary>
        public int AnimationFrameCount { get; set; }
        
        /// <summary>
        /// Gets and sets the animation frame array
        /// </summary>
        public int[]? AnimationFrames { get; set; }
        
        /// <summary>
        /// Gets and sets the number of ticks for a single animation frame
        /// </summary>
        public int AnimationFrameTime { get; set; }
    }
}