using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MinecraftWebExporter.Minecraft.BlockStates.Cache;
using SharpNBT;

namespace MinecraftWebExporter.Minecraft.BlockStates
{
    /// <summary>
    /// Gets one multipart entry
    /// </summary>
    public class BlockStateMultipart
    {
        /// <summary>
        /// Gets and sets the variation to apply
        /// </summary>
        [JsonConverter(typeof(BlockStateVariationArrayJsonConverter))]
        [JsonPropertyName("apply")] public BlockStateVariant[]? Apply { get; set; }
        
        /// <summary>
        /// Gets and sets the when condition
        /// </summary>
        [JsonPropertyName("when")] public BlockStateWhen? When { get; set; }
        
        #region Build

        /// <summary>
        /// Checks if the when condition is met
        /// </summary>
        /// <param name="propertiesTag"></param>
        /// <returns></returns>
        private bool CheckWhen(CompoundTag? propertiesTag)
        {
            if (When is null)
                return true;
            
            return When.Check(propertiesTag);
        }
        
        /// <summary>
        /// Gets the cached model for the given properties
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="faces"></param>
        /// <param name="assetManager"></param>
        /// <param name="propertiesTag"></param>
        /// <returns></returns>
        public async ValueTask BuildCachedFacesAsync(AssetIdentifier asset, List<CachedBlockStateFace> faces, AssetManager assetManager, CompoundTag? propertiesTag)
        {
            // Build the variant
            if (Apply is not null)
            {
                if (CheckWhen(propertiesTag))
                {
                    await Apply[0].BuildCachedFacesAsync(asset, faces, assetManager);
                }
            }
        }

        #endregion Build
    }
}