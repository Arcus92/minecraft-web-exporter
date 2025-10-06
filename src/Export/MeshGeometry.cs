using System.Collections.Generic;

namespace MinecraftWebExporter.Export;

/// <summary>
/// A geometry for the <see cref="MeshFile"/>.
/// </summary>
public class MeshGeometry
{
    /// <summary>
    /// Gets and sets the material
    /// </summary>
    public string Material { get; set; } = string.Empty;

    /// <summary>
    /// Gets the vertex list
    /// </summary>
    public List<MeshVertex> Vertices { get; } = new();

    /// <summary>
    /// Gets the triangle list
    /// </summary>
    public List<int> Triangles { get; } = new();
}