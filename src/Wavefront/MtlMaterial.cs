using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Wavefront
{
    /// <summary>
    /// A material for the <see cref="MtlFile"/>.
    /// </summary>
    public class MtlMaterial
    {
        /// <summary>
        /// Gets and sets the name of the material
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets and sets the diffuse color
        /// </summary>
        public Vector3? Diffuse { get; set; }

        /// <summary>
        /// Gets and sets the diffuse map
        /// </summary>
        public string? DiffuseMap { get; set; }

        /// <summary>
        /// Writes the material
        /// </summary>
        /// <param name="writer"></param>
        public void Write(ObjWriter writer)
        {
            writer.Write("newmtl", Name);

            if (Diffuse.HasValue)
            {
                writer.Write("Kd", Diffuse.Value);
            }

            if (DiffuseMap is not null)
            {
                writer.Write("map_Kd", DiffuseMap);
            }
        }
    }
}