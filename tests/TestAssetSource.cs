using System.Collections.Generic;
using System.IO;
using MinecraftWebExporter.Minecraft;

namespace MinecraftWebExporter.Tests;

/// <summary>
/// An asset source without any content. Used by <see cref="TestAssetManager"/>.
/// </summary>
public class TestAssetSource : IAssetSource
{
    /// <inheritdoc cref="IAssetSource.ContainsAsset"/>
    public bool ContainsAsset(AssetIdentifier asset)
    {
        return false;
    }

    /// <inheritdoc cref="IAssetSource.TryOpenAsset"/>
    public bool TryOpenAsset(AssetIdentifier asset, out Stream? stream)
    {
        stream = null;
        return false;
    }

    /// <inheritdoc cref="IAssetSource.GetAssets"/>
    public IEnumerable<AssetIdentifier> GetAssets(AssetType type, string ns)
    {
        yield break;
    }
    
    /// <inheritdoc cref="IAssetSource.GetNamespaces"/>
    public IEnumerable<string> GetNamespaces()
    {
        yield return "minecraft";
    }
    
    /// <inheritdoc cref="IAssetSource.Dispose"/>
    public void Dispose()
    {
        // Do nothing
    }
}