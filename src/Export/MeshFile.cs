using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWebExporter.Export
{
    /// <summary>
    /// The binary export file for the Minecraft web viewer.
    /// This is a non-standard file format that is optimized for faster and memory efficient reading in WebGL.
    /// The mesh data is compressed by the deflate algorithm.
    ///
    /// Data format:
    /// - 4 byte integer: Number of vertexes.
    /// - For each vertex:
    ///   - 3 x 4 byte floating point number: The x, y and z component of the vertex position.
    ///   - 2 x 4 byte floating point number: The x and y component of the vertex texture coordinate.
    ///   - 3 x 4 byte floating point number: The x, y and z component of the vertex normal.
    ///   - 3 x 4 byte floating point number: The r, g and b component of the vertex color.
    /// - 4 byte integer: Number of vertex indices. 3 indices form one triangle.
    /// - For each vertex index
    ///   - 4 byte integer: The index of the vertex (0 = first).
    /// - 4 byte integer: Number of materials.
    /// - For each material:
    ///   - 4 byte integer: Number of vertex indices with this material.
    ///   - 1 or more bytes: 7 bit encoded integer for the length of the material name.
    ///   - For each material name characters:
    ///     - 1 byte: A UTF-8 character for the material name
    /// </summary>
    public class MeshFile
    {
        /// <summary>
        /// Gets the geometries
        /// </summary>
        public readonly List<MeshGeometry> Geometries = new();

        /// <summary>
        /// Gets or create a geometry by the given material name
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public MeshGeometry GetOrCreateGeometryByMaterial(string material)
        {
            var geometry = Geometries.FirstOrDefault(g => g.Material == material);
            if (geometry is not null)
            {
                return geometry;
            }

            geometry = new MeshGeometry()
            {
                Material = material,
            };
            Geometries.Add(geometry);
            return geometry;
        }
        
        /// <summary>
        /// Writes the obj data to file
        /// </summary>
        /// <param name="file"></param>
        public async Task WriteToFileAsync(string file)
        {
            await Task.Run(() =>
            {
                using var stream = new FileStream(file, FileMode.Create);
                using var gzip = new DeflateStream(stream, CompressionMode.Compress);
                using var writer = new BinaryWriter(gzip);

                var vertexCount = Geometries.Sum(g => g.Vertices.Count);
                var triangleCount = Geometries.Sum(g => g.Triangles.Count);

                // Writes the vertices
                writer.Write(vertexCount);
                foreach (var geometry in Geometries)
                {
                    foreach (var vertex in geometry.Vertices)
                    {
                        writer.Write(vertex.Position.X);
                        writer.Write(vertex.Position.Y);
                        writer.Write(vertex.Position.Z);
                        writer.Write(vertex.Uv.X);
                        writer.Write(vertex.Uv.Y);
                        writer.Write(vertex.Normal.X);
                        writer.Write(vertex.Normal.Y);
                        writer.Write(vertex.Normal.Z);
                        writer.Write(vertex.Color.X);
                        writer.Write(vertex.Color.Y);
                        writer.Write(vertex.Color.Z);
                    }
                }

                // Writes the triangles
                writer.Write(triangleCount);
                var offset = 0;
                foreach (var geometry in Geometries)
                {
                    foreach (var triangle in geometry.Triangles)
                    {
                        writer.Write(offset + triangle);
                    }

                    offset += geometry.Vertices.Count;
                }
                
                // Writes all materials
                writer.Write(Geometries.Count);
                foreach (var geometry in Geometries)
                {
                    writer.Write(geometry.Triangles.Count);
                    writer.Write(geometry.Material);
                }
            });
        }
    }
}