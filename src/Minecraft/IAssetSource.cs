using System;
using System.Collections.Generic;
using System.IO;

namespace MinecraftWebExporter.Minecraft;

/// <summary>
/// The interface for <see cref="ResourcePack"/>.
/// </summary>
public interface IAssetSource : IDisposable
{
    /// <summary>
    /// Returns if the given asset is contained in this resource pack
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
    public bool ContainsAsset(AssetIdentifier asset);

    /// <summary>
    /// Opens the asset if it exists and returns its stream.
    /// Returns false, if the file could not be found.
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    public bool TryOpenAsset(AssetIdentifier asset, out Stream? stream);

    /// <summary>
    /// Returns all assets for the given type and namespace
    /// </summary>
    /// <param name="type"></param>
    /// <param name="ns"></param>
    /// <returns></returns>
    public IEnumerable<AssetIdentifier> GetAssets(AssetType type, string ns);

    /// <summary>
    /// Returns all namespaces used in this asset source
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetNamespaces();
}