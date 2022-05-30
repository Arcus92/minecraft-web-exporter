using System.Collections.Generic;
using System.Threading.Tasks;

namespace MinecraftWebExporter.Wavefront
{
    /// <summary>
    /// The material lib file for the wavefront format
    /// </summary>
    public class MtlFile
    {
        /// <summary>
        /// Gets the materials
        /// </summary>
        public readonly List<MtlMaterial> Materials = new();
        
        /// <summary>
        /// Writes the obj data to file
        /// </summary>
        /// <param name="file"></param>
        public async Task WriteToFileAsync(string file)
        {
            await Task.Run(() =>
            {
                using var writer = new ObjWriter(file);
                foreach (var material in Materials)
                {
                    material.Write(writer);
                }
            });
        }
    }
}