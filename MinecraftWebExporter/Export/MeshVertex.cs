using MinecraftWebExporter.Minecraft;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Export
{
    /// <summary>
    /// The mesh vertex data
    /// </summary>
    public readonly struct MeshVertex
    {
        /// <summary>
        /// Gets and sets the position
        /// </summary>
        public Vector3 Position { get; init; }
        
        /// <summary>
        /// Gets and sets the texture uv
        /// </summary>
        public Vector2 Uv { get; init; }
        
        /// <summary>
        /// Gets and sets the normal
        /// </summary>
        public Vector3 Normal { get; init; }
        
        /// <summary>
        /// Gets and sets the color
        /// </summary>
        public Vector3 Color { get; init; }
    }
}