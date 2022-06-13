using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using MinecraftWebExporter.Minecraft.BlockStates;
using MinecraftWebExporter.Minecraft.Models;
using MinecraftWebExporter.Minecraft.Models.Cache;
using MinecraftWebExporter.Minecraft.Textures;
using MinecraftWebExporter.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MinecraftWebExporter.Minecraft
{
    /// <summary>
    /// The asset manager handles the loading of textures, models and block states.
    /// </summary>
    public class AssetManager : IAssetManager
    {
        public AssetManager(IAssetSource source)
        {
            Source = source;
            ModelCache = new ModelCache(this);
        }
        
        /// <summary>
        /// The main asset source
        /// </summary>
        public IAssetSource Source { get; }
        
        /// <summary>
        /// Gets the model cache
        /// </summary>
        public ModelCache ModelCache { get; }
        
        #region Models
        
        /// <summary>
        /// The model cache
        /// </summary>
        private readonly ConcurrentDictionary<AssetIdentifier, Model> m_Models = new();
        
        /// <summary>
        /// The model cache
        /// </summary>
        private readonly ConcurrentDictionary<AssetIdentifier, CachedModel> m_CachedModels = new();

        /// <summary>
        /// Returns the model for the given asset
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Model?> GetModelAsync(AssetIdentifier asset)
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

        /// <summary>
        /// Returns the model for the given asset
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<CachedModel> GetCachedModelAsync(AssetIdentifier asset)
        {
            if (asset.Type != AssetType.Model)
            {
                throw new ArgumentException( "Asset type must be model!", nameof(asset));
            }

            if (m_CachedModels.TryGetValue(asset, out var cachedModel))
            {
                return cachedModel;
            }
            
            var model = await GetModelAsync(asset);
            cachedModel = await CachedModel.CreateAsync(this, model);
            
            m_CachedModels.TryAdd(asset, cachedModel);
            return cachedModel;
        }
        
        #endregion Models

        #region Block states

        /// <summary>
        /// The block state cache
        /// </summary>
        private readonly ConcurrentDictionary<AssetIdentifier, BlockState> m_BlockStates = new();

        /// <summary>
        /// Returns the block state
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<BlockState?> GetBlockStateAsync(AssetIdentifier asset)
        {
            if (asset.Type != AssetType.BlockState)
            {
                throw new ArgumentException( "Asset type must be block state!", nameof(asset));
            }

            if (m_BlockStates.TryGetValue(asset, out var blockState))
            {
                return blockState;
            }
            
            if (!Source.TryOpenAsset(asset, out var stream) || stream is null)
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
            
            m_BlockStates.TryAdd(asset, blockState);
            return blockState;
        }
        
        #endregion Block states
        
        #region Textures
        
        /// <summary>
        /// The texture cache
        /// </summary>
        private readonly ConcurrentDictionary<AssetIdentifier, CachedTexture> m_Textures = new();

        /// <summary>
        /// Returns the texture info
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<CachedTexture> GetTextureAsync(AssetIdentifier asset)
        {
            if (asset.Type != AssetType.Texture)
            {
                throw new ArgumentException( "Asset type must be texture!", nameof(asset));
            }

            if (m_Textures.TryGetValue(asset, out var cachedTexture))
            {
                return cachedTexture;
            }
            
            // Opens the texture
            if (!Source.TryOpenAsset(asset, out var stream) || stream is null)
            {
                return default;
            }
            
            // Gets the size and checks if this texture contains any semi-transparent pixels.
            
            using var image = Image.Load<Rgba32>(stream);
            var width = image.Width;
            var height = image.Height;
            
            var transparencyBits = new BitArray(width * height);
            var transparencyCutoff = false;
            var transparencySemi = false;
            
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                var p = image[x, y];
                // Some texture packs need some additional tolerance. 
                
                // Check for semi transparency
                if (p.A is > 5 and < 250)
                {
                    transparencySemi = true;
                }

                // Check for hard transparency
                if (p.A < 250)
                {
                    transparencyBits[x + y * width] = true;
                    transparencyCutoff = true;
                }
            }

            TextureTransparency transparency;
            if (transparencySemi)
            {
                transparency = TextureTransparency.Semi;
            }
            else if (transparencyCutoff)
            {
                transparency = TextureTransparency.Cutoff;
            }
            else
            {
                transparency = TextureTransparency.Solid;
                transparencyBits = null;
            }

            await stream.DisposeAsync();
            
            // Try to read the .mcmeta file to get animation info
            TextureAnimation? animation = default;
            var assetIdentifier = new AssetIdentifier(AssetType.TextureMeta, asset.Namespace,  asset.Name);
            if (Source.TryOpenAsset(assetIdentifier, out stream) && stream is not null)
            {
                var option = new JsonSerializerOptions()
                {
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                };
                
                var meta = await JsonSerializer.DeserializeAsync<TextureMeta>(stream, option);
                if (meta is not null)
                {
                    animation = meta.Animation;
                }

                await stream.DisposeAsync();
            }
            

            cachedTexture = new CachedTexture()
            {
                Width = width,
                Height = height,
                Animation = animation,
                Transparency = transparency,
                TransparencyBits = transparencyBits,
            };

            m_Textures.TryAdd(asset, cachedTexture);
            return cachedTexture;
        }
        
        #endregion Textures

        /// <summary>
        /// Dispose the asset source
        /// </summary>
        public void Dispose()
        {
            Source.Dispose();
        }
    }
}