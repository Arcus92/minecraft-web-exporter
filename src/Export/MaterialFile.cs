using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace MinecraftWebExporter.Export;

/// <summary>
/// A writer to create a binary material definition file.
///
/// Data format:
/// - 4 bytes integer: Number of materials
/// - For each material:
///   - 1 or more bytes: 7 bit encoded integer for the length of the material name.
///   - For each material name characters:
///     - 1 byte: A UTF-8 character for the material name
///   - 1 or more bytes: 7 bit encoded integer for the length of the texture filename.
///   - For each texture filename characters:
///     - 1 byte: A UTF-8 character for the texture filename
///   - 1 byte boolean: A flag (0=false, 1=true) to set the material as transparent.
/// </summary>
public class MaterialFile
{
    /// <summary>
    /// Gets the materials
    /// </summary>
    public readonly List<MeshMaterial> Materials = new();
        
    /// <summary>
    /// Writes the material data to file
    /// </summary>
    /// <param name="file"></param>
    public async Task WriteToFileAsync(string file)
    {
        await Task.Run(() =>
        {
            using var stream = new FileStream(file, FileMode.Create);
            using var gzip = new DeflateStream(stream, CompressionMode.Compress);
            using var writer = new BinaryWriter(gzip);

            writer.Write(Materials.Count);
            foreach (var material in Materials)
            {
                writer.Write(material.Name);
                writer.Write(material.Texture ?? string.Empty);
                writer.Write(material.Transparent);
                writer.Write(material.AnimationFrameCount);
                writer.Write(material.AnimationFrameTime);
                if (material.AnimationFrames is null)
                {
                    writer.Write(0);
                }
                else
                {
                    writer.Write(material.AnimationFrames.Length);
                    foreach (var frame in material.AnimationFrames)
                    {
                        writer.Write(frame);
                    }
                        
                }
            }
        });
    }
}