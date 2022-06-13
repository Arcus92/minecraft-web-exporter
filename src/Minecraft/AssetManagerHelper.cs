using System.Threading.Tasks;
using MinecraftWebExporter.Minecraft.BlockStates;
using MinecraftWebExporter.Minecraft.Models;
using MinecraftWebExporter.Minecraft.Models.Cache;
using MinecraftWebExporter.Minecraft.Textures;

namespace MinecraftWebExporter.Minecraft;

/// <summary>
/// Collection of extended methods for <see cref="IAssetManager"/>. 
/// </summary>
public static class AssetManagerHelper
{
    /// <summary>
    /// Returns the model
    /// </summary>
    /// <param name="assetManager"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Task<Model?> GetModelAsync(this IAssetManager assetManager, string name)
    {
        return assetManager.GetModelAsync(new AssetIdentifier(AssetType.Model, name));
    }
    
    /// <summary>
    /// Returns the cached model
    /// </summary>
    /// <param name="assetManager"></param>
    /// <param name="assetIdentifier"></param>
    /// <returns></returns>
    public static Task<CachedModel> GetCachedModelAsync(this IAssetManager assetManager, AssetIdentifier assetIdentifier)
    {
        return assetManager.ModelCache.GetAsync(assetIdentifier);
    }
    
    /// <summary>
    /// Returns the cached model
    /// </summary>
    /// <param name="assetManager"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Task<CachedModel> GetCachedModelAsync(this IAssetManager assetManager, string name)
    {
        return assetManager.GetCachedModelAsync(new AssetIdentifier(AssetType.Model, name));
    }
    
    /// <summary>
    /// Returns the block state
    /// </summary>
    /// <param name="assetManager"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Task<BlockState?> GetBlockStateAsync(this IAssetManager assetManager, string name)
    {
        return assetManager.GetBlockStateAsync(new AssetIdentifier(AssetType.BlockState, name));
    }
    
    /// <summary>
    /// Returns the texture info
    /// </summary>
    /// <param name="assetManager"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Task<CachedTexture> GetTextureAsync(this IAssetManager assetManager, string name)
    {
        return assetManager.GetTextureAsync(new AssetIdentifier(AssetType.Texture, name));
    }
}