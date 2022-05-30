using System;

namespace MinecraftWebExporter.Export
{
    /// <summary>
    /// The export settings used by <see cref="MapExporter"/>.
    /// </summary>
    public class ExportSettings
    {
        /// <summary>
        /// Gets and sets the views
        /// </summary>
        public ExportDetailLevel[] Views { get; set; } = Array.Empty<ExportDetailLevel>();
    }
}