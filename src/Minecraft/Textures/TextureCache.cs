using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Drawing;
using System.Text.Json;
using System.Threading.Tasks;

namespace MinecraftWebExporter.Minecraft.Textures
{
    /// <summary>
    /// The cache for <see cref="TextureCache"/>.
    /// </summary>
    public class TextureCache
    {
        /// <summary>
        /// Creates the texture cache
        /// </summary>
        /// <param name="assetManager"></param>
        public TextureCache(AssetManager assetManager)
        {
            m_AssetManager = assetManager;
        }
        
        /// <summary>
        /// The asset manager
        /// </summary>
        private readonly AssetManager m_AssetManager;
        
        /// <summary>
        /// The texture cache
        /// </summary>
        private readonly ConcurrentDictionary<AssetIdentifier, CachedTexture> m_Cache = new();

        /// <summary>
        /// Returns the texture info
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<CachedTexture> GetAsync(AssetIdentifier asset)
        {
            if (asset.Type != AssetType.Texture)
            {
                throw new ArgumentException( "Asset type must be texture!", nameof(asset));
            }

            if (m_Cache.TryGetValue(asset, out var cachedTexture))
            {
                return cachedTexture;
            }
            
            // Opens the texture
            if (!m_AssetManager.Source.TryOpenAsset(asset, out var stream) || stream is null)
            {
                return default;
            }

#pragma warning disable CA1416
            // Gets the size and checks if this texture contains any semi-transparent pixels.
            using var bitmap = new Bitmap(stream);
            var width = bitmap.Width;
            var height = bitmap.Height;
            
            var transparencyBits = new BitArray(width * height);
            var transparencyCutoff = false;
            var transparencySemi = false;
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                var p = bitmap.GetPixel(x, y);
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
#pragma warning restore CA1416
            
            await stream.DisposeAsync();
            
            // Try to read the .mcmeta file to get animation info
            TextureAnimation? animation = default;
            var assetIdentifier = new AssetIdentifier(AssetType.TextureMeta, asset.Namespace,  asset.Name);
            if (m_AssetManager.Source.TryOpenAsset(assetIdentifier, out stream) && stream is not null)
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

            m_Cache.TryAdd(asset, cachedTexture);
            return cachedTexture;
        }
    }
}