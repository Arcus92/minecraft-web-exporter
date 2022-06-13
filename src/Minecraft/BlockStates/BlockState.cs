using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MinecraftWebExporter.Minecraft.BlockStates.Cache;

namespace MinecraftWebExporter.Minecraft.BlockStates
{
    /// <summary>
    /// The block state
    /// </summary>
    public class BlockState
    {
        /// <summary>
        /// Gets and sets the variation list
        /// </summary>
        [JsonPropertyName("variants")] public BlockStateVariants? Variants { get; set; }
        
        /// <summary>
        /// Gets and sets the multipart list
        /// </summary>
        [JsonPropertyName("multipart")] public BlockStateMultipart[]? Multipart { get; set; }
        
        #region Build
        
        /// <summary>
        /// Gets the cached model for the given properties
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="variants"></param>
        /// <param name="assetManager"></param>
        /// <param name="properties"></param>
        public async ValueTask BuildCachedFacesAsync(AssetIdentifier asset, List<CachedBlockStateVariant> variants, IAssetManager assetManager, IBlockStateProperties? properties)
        {
            // Build the variant
            if (Variants is not null)
            {
                var faces = new List<CachedBlockStateFace>();
                foreach (var variant in Variants.GetVariantsByProperties(properties))
                {
                    faces.Clear();
                    await variant.BuildCachedFacesAsync(asset, faces, assetManager);
                    variants.Add(new CachedBlockStateVariant()
                    {
                        Faces = faces.ToArray(),
                        Weight = variant.Weight == 0 ? 1f : variant.Weight,
                    });
                }
            }

            // Build the multipart
            if (Multipart is not null)
            {
                var faces = new List<CachedBlockStateFace>();
                foreach (var multipart in Multipart)
                {
                    await multipart.BuildCachedFacesAsync(asset, faces, assetManager, properties);
                }
                variants.Add(new CachedBlockStateVariant()
                {
                    Faces = faces.ToArray(),
                });
            }
        }
        
        #endregion Build
    }
}