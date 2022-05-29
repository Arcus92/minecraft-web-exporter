using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MinecraftWebExporter.Minecraft.Models
{
    /// <summary>
    /// A minecraft model (item or block).
    /// </summary>
    public class Model
    {
        /// <summary>
        /// Gets the gui light type
        /// </summary>
        [JsonPropertyName("gui_light")] public string? GuiLight { get; set; }
        
        /// <summary>
        /// Gets the display option for this block
        /// </summary>
        [JsonPropertyName("display")] public ModelDisplay? Display { get; set; }
        
        /// <summary>
        /// Gets the parent block name
        /// </summary>
        [JsonPropertyName("parent")] public string? Parent { get; set; }

        /// <summary>
        /// Gets if ambient occlusion should be used
        /// </summary>
        [JsonPropertyName("ambientocclusion")] public bool? AmbientOcclusion { get; set; }
        
        /// <summary>
        /// Gets the texture map for this block
        /// </summary>
        [JsonPropertyName("textures")] public ModelTextures? Textures { get; set; }
        
        /// <summary>
        /// Gets the list of elements for this block
        /// </summary>
        [JsonPropertyName("elements")] public ModelElement[]? Elements { get; set; }

        /// <summary>
        /// Returns a new model instance with merged parent properties
        /// </summary>
        /// <param name="assets"></param>
        /// <returns></returns>
        public async Task<Model> Combine(AssetManager assets)
        {
            if (string.IsNullOrEmpty(Parent))
            {
                return this;
            }

            var parent = await assets.GetModel(new AssetIdentifier(AssetType.Model, Parent));
            if (parent is null)
            {
                return this;
            }
            
            return Merge(this, parent);
        }

        /// <summary>
        /// Merges the two blocks
        /// </summary>
        /// <param name="model"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private static Model Merge(Model model, Model parent)
        {
            return new Model
            {
                GuiLight = model.GuiLight ?? parent.GuiLight,
                Display = ModelDisplay.Merge(model.Display, parent.Display),
                Elements = model.Elements ?? parent.Elements,
                Textures = ModelTextures.Merge(model.Textures, parent.Textures),
                AmbientOcclusion = model.AmbientOcclusion ?? parent.AmbientOcclusion
            };
        }
    }
}