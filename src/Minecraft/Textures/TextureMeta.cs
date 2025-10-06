using System.Text.Json.Serialization;

namespace MinecraftWebExporter.Minecraft.Textures;

/// <summary>
/// The texture meta file (.mcmeta). This contains animation data.
/// </summary>
public class TextureMeta
{
    /// <summary>
    /// Gets the animation
    /// </summary>
    [JsonPropertyName("animation")] public TextureAnimation? Animation { get; set; }
}