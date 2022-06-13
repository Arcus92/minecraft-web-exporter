using System;
using System.Threading.Tasks;
using MinecraftWebExporter.Minecraft.BlockStates;
using MinecraftWebExporter.Minecraft.Models;
using MinecraftWebExporter.Minecraft.Textures;

namespace MinecraftWebExporter.Minecraft;

public interface IAssetManager : IDisposable
{
    /// <summary>
    /// The main asset source
    /// </summary>
    IAssetSource Source { get; }

    /// <summary>
    /// Gets the model cache
    /// </summary>
    ModelCache ModelCache { get; }

    /// <summary>
    /// Returns the model
    /// </summary>
    /// <param name="assetIdentifier"></param>
    /// <returns></returns>
    Task<Model?> GetModelAsync(AssetIdentifier assetIdentifier);
    
    /// <summary>
    /// Returns the block state
    /// </summary>
    /// <param name="assetIdentifier"></param>
    /// <returns></returns>
    Task<BlockState?> GetBlockStateAsync(AssetIdentifier assetIdentifier);
    
    /// <summary>
    /// Returns the texture info
    /// </summary>
    /// <param name="assetIdentifier"></param>
    /// <returns></returns>
    Task<CachedTexture> GetTextureAsync(AssetIdentifier assetIdentifier);
}