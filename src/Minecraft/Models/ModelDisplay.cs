using System.Text.Json.Serialization;

namespace MinecraftWebExporter.Minecraft.Models;

/// <summary>
/// The model display option
/// </summary>
public class ModelDisplay
{
    /// <summary>
    /// Gets the transformation for the gui icon
    /// </summary>
    [JsonPropertyName("gui")] public ModelTransform? Gui { get; set; }
        
    /// <summary>
    /// Gets the transformation for the ground icon
    /// </summary>
    [JsonPropertyName("ground")] public ModelTransform? Ground { get; set; }
        
    /// <summary>
    /// Gets the transformation for the fixed icon
    /// </summary>
    [JsonPropertyName("fixed")] public ModelTransform? Fixed { get; set; }
        
    /// <summary>
    /// Gets the transformation for the right-handed third person icon
    /// </summary>
    [JsonPropertyName("thirdperson_righthand")] public ModelTransform? ThirdPersonRightHand { get; set; }
        
    /// <summary>
    /// Gets the transformation for the right-handed first person icon
    /// </summary>
    [JsonPropertyName("firstperson_righthand")] public ModelTransform? FirstPersonRightHand { get; set; }
        
    /// <summary>
    /// Gets the transformation for the left-handed first person icon
    /// </summary>
    [JsonPropertyName("firstperson_lefthand")] public ModelTransform? FirstPersonLeftHand { get; set; }

    /// <summary>
    /// Merges the two display options
    /// </summary>
    /// <param name="display"></param>
    /// <param name="parentDisplay"></param>
    /// <returns></returns>
    public static ModelDisplay? Merge(ModelDisplay? display, ModelDisplay? parentDisplay)
    {
        if (display is null)
        {
            return parentDisplay;
        }

        if (parentDisplay is null)
        {
            return display;
        }

        return new ModelDisplay()
        {
            Gui = display.Gui ?? parentDisplay.Gui,
            Ground = display.Ground ?? parentDisplay.Ground,
            Fixed = display.Fixed ?? parentDisplay.Fixed,
            ThirdPersonRightHand = display.ThirdPersonRightHand ?? parentDisplay.ThirdPersonRightHand,
            FirstPersonRightHand = display.FirstPersonRightHand ?? parentDisplay.FirstPersonRightHand,
            FirstPersonLeftHand = display.FirstPersonLeftHand ?? parentDisplay.FirstPersonLeftHand,
        };
    }
}