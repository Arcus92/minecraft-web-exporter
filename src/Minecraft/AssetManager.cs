using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using MinecraftWebExporter.Minecraft.BlockStates;
using MinecraftWebExporter.Minecraft.Models;
using MinecraftWebExporter.Minecraft.Textures;

namespace MinecraftWebExporter.Minecraft
{
    /// <summary>
    /// The asset manager handles the loading of textures, models and block states.
    /// </summary>
    public class AssetManager : IDisposable
    {
        public AssetManager(IAssetSource source)
        {
            Source = source;
            ModelCache = new ModelCache(this);
            TextureCache = new TextureCache(this);
            BlockStateCache = new BlockStateCache(this);
        }

        /// <summary>
        /// The main asset source
        /// </summary>
        public IAssetSource Source { get; }

        #region Models
        
        /// <summary>
        /// The model cache
        /// </summary>
        private readonly ConcurrentDictionary<AssetIdentifier, Model> m_Models = new();

        /// <summary>
        /// Returns the model for the given asset
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Model?> GetModel(AssetIdentifier asset)
        {
            if (asset.Type != AssetType.Model)
            {
                throw new ArgumentException( "Asset type must be model!", nameof(asset));
            }

            if (m_Models.TryGetValue(asset, out var model))
            {
                return model;
            }
            
            if (!Source.TryOpenAsset(asset, out var stream) || stream is null)
            {
                return null;
            }
            
            var option = new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };

            model = await JsonSerializer.DeserializeAsync<Model>(stream, option);
            if (model == null)
            {
                throw new ArgumentException("Could not read json model data!");
            }

            model = await model.Combine(this);
            
            m_Models.TryAdd(asset, model);
            
            await stream.DisposeAsync();
            return model;
        }
        
        #endregion Models
        
        #region Cache

        /// <summary>
        /// Gets the model cache
        /// </summary>
        public ModelCache ModelCache { get; }
        
        /// <summary>
        /// Gets the block state cache
        /// </summary>
        public BlockStateCache BlockStateCache { get; }
        
        /// <summary>
        /// Gets the texture cache
        /// </summary>
        public TextureCache TextureCache { get; }

        #endregion Cache

        /// <summary>
        /// Dispose the asset source
        /// </summary>
        public void Dispose()
        {
            Source.Dispose();
        }
    }
}