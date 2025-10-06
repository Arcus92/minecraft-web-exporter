using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWebExporter.Wavefront;

/// <summary>
/// The wavefront (.obj) file
/// </summary>
public class ObjFile
{
    /// <summary>
    /// Gets and sets the name of the mtl lib
    /// </summary>
    public string? MtlLib { get; set; }

    /// <summary>
    /// Gets the geometries
    /// </summary>
    public readonly List<ObjGeometry> Geometries = new();

    /// <summary>
    /// Gets or create a geometry by the given material name
    /// </summary>
    /// <param name="material"></param>
    /// <returns></returns>
    public ObjGeometry GetOrCreateGeometryByMaterial(string material)
    {
        var geometry = Geometries.FirstOrDefault(g => g.Material == material);
        if (geometry is not null)
        {
            return geometry;
        }

        geometry = new ObjGeometry()
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
            using var writer = new ObjWriter(file);

            if (MtlLib is not null)
            {
                writer.Write("mtllib ", MtlLib);
            }

            var positionOffset = 1;
            var uvOffset = 1;
            var normalOffset = 1;
            foreach (var geometry in Geometries)
            {
                geometry.Write(writer, positionOffset, uvOffset, normalOffset);
                positionOffset += geometry.Vertices.Count;
                uvOffset += geometry.Uvs.Count;
                normalOffset += geometry.Normals.Count;
            }
        });
    }
}