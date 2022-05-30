namespace MinecraftWebExporter.Minecraft.Textures
{
    /// <summary>
    /// The transparency info of a texture
    /// </summary>
    public enum TextureTransparency
    {
        /// <summary>
        /// The texture only contains solid (no transparent) pixels
        /// </summary>
        Solid,
        
        /// <summary>
        /// The texture contains only full or non transparent pixels
        /// </summary>
        Cutoff,
        
        /// <summary>
        /// The texture contains semi transparent pixels (not fully transparent but not fully solid)
        /// </summary>
        Semi,
    }
}