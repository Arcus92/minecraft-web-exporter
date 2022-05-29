using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using MinecraftWebExporter.Utils;

namespace MinecraftWebExporter.Minecraft.BlockStates
{
    /// <summary>
    /// The cache for the block states
    /// </summary>
    public class BlockStateCache
    {
        /// <summary>
        /// Creates the block state cache
        /// </summary>
        /// <param name="assetManager"></param>
        public BlockStateCache(AssetManager assetManager)
        {
            m_AssetManager = assetManager;
        }
        
        /// <summary>
        /// The asset manager
        /// </summary>
        private readonly AssetManager m_AssetManager;
        
        /// <summary>
        /// The block state cache
        /// </summary>
        private readonly ConcurrentDictionary<AssetIdentifier, BlockState> m_Cache = new();

        /// <summary>
        /// Returns the block state
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<BlockState?> GetAsync(AssetIdentifier asset)
        {
            if (asset.Type != AssetType.BlockState)
            {
                throw new ArgumentException( "Asset type must be block state!", nameof(asset));
            }

            if (m_Cache.TryGetValue(asset, out var blockState))
            {
                return blockState;
            }
            
            if (!m_AssetManager.Source.TryOpenAsset(asset, out var stream) || stream is null)
            {
                return default;
            }
            
            var option = new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };
            
            option.Converters.Add(new JsonStringFixConverter());
            
            blockState = await JsonSerializer.DeserializeAsync<BlockState>(stream, option);
            if (blockState is null)
            {
                throw new ArgumentException("Could not read json block state data!");
            }
            
            m_Cache.TryAdd(asset, blockState);
            return blockState;
        }
    }
}