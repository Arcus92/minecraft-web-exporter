using System.Text.Json.Serialization;

namespace MinecraftWebExporter.Minecraft.Textures;

/// <summary>
/// The texture animation info
/// </summary>
public struct TextureAnimation
{
    /// <summary>
    /// Gets the number of frames per animation
    /// </summary>
    [JsonPropertyName("frametime")] public int FrameTime { get; set; }

    /// <summary>
    /// Gets the frames for this animation
    /// </summary>
    [JsonPropertyName("frames")] public int[] Frames { get; set; }
}