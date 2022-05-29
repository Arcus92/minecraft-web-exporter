namespace MinecraftWebExporter.Wavefront
{
    /// <summary>
    /// A vertex of a quad used by <see cref="ObjGeometry"/>
    /// </summary>
    public readonly struct ObjQuadVertex
    {
        /// <summary>
        /// Gets the index of the position
        /// </summary>
        public int? Position { get; init; }
        
        /// <summary>
        /// Gets the index of the uv
        /// </summary>
        public int? Uv { get; init; }
        
        /// <summary>
        /// Gets the index of the normal
        /// </summary>
        public int? Normal { get; init; }
        
        /// <summary>
        /// Gets the index of the color
        /// </summary>
        public int? Color { get; init; }
    }
    
    /// <summary>
    /// A quad for the faces in the <see cref="ObjFile"/>.
    /// </summary>
    public readonly struct ObjQuad
    {
        /// <summary>
        /// Gets vertex 1
        /// </summary>
        public ObjQuadVertex Vertex1 { get; init; }
        
        /// <summary>
        /// Gets vertex 2
        /// </summary>
        public ObjQuadVertex Vertex2 { get; init; }
        
        /// <summary>
        /// Gets vertex 3
        /// </summary>
        public ObjQuadVertex Vertex3 { get; init; }
        
        /// <summary>
        /// Gets vertex 4
        /// </summary>
        public ObjQuadVertex Vertex4 { get; init; }
    }
}