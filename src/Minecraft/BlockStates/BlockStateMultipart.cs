using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MinecraftWebExporter.Minecraft.BlockStates.Cache;

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
        /// <param name="properties"></param>
        /// <returns></returns>
        private bool CheckWhen(IBlockStateProperties? properties)
        {
            if (When is null)
                return true;
            
            return When.Check(properties);
        }
        
        /// <summary>
        /// Gets the cached model for the given properties
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="faces"></param>
        /// <param name="assetManager"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public async ValueTask BuildCachedFacesAsync(AssetIdentifier asset, List<CachedBlockStateFace> faces, IAssetManager assetManager, IBlockStateProperties? properties)
        {
            // Build the variant
            if (Apply is not null)
            {
                if (CheckWhen(properties))
                {
                    await Apply[0].BuildCachedFacesAsync(asset, faces, assetManager);
                }
            }
        }

        #endregion Build
    }
}