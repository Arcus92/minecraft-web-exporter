using System.Collections.Generic;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Wavefront
{
    /// <summary>
    /// A geometry for the <see cref="ObjFile"/>.
    /// </summary>
    public class ObjGeometry
    {
        /// <summary>
        /// Gets and sets the material
        /// </summary>
        public string? Material { get; set; }

        /// <summary>
        /// Gets the vertices
        /// </summary>
        public readonly List<Vector3> Vertices = new();
        
        /// <summary>
        /// Gets the uvs
        /// </summary>
        public readonly List<Vector2> Uvs = new();
        
        /// <summary>
        /// Gets the normals
        /// </summary>
        public readonly List<Vector3> Normals = new();
        
        /// <summary>
        /// Gets the quads
        /// </summary>
        public readonly List<ObjQuad> Quads = new();

        /// <summary>
        /// Writes the geometry
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="positionOffset"></param>
        /// <param name="uvOffset"></param>
        /// <param name="normalOffset"></param>
        public void Write(ObjWriter writer, int positionOffset, int uvOffset, int normalOffset)
        {
            if (Material != null)
            {
                writer.Write("usemtl", Material);
            }

            foreach (var vector in Vertices)
            {
                writer.Write("v", vector);
            }
            
            foreach (var vector in Uvs)
            {
                writer.Write("vt", vector);
            }
            
            foreach (var vector in Normals)
            {
                writer.Write("vn", vector);
            }

            foreach (var quad in Quads)
            {
                writer.Write("f", quad, positionOffset, uvOffset, normalOffset);
            }
        }
    }
}