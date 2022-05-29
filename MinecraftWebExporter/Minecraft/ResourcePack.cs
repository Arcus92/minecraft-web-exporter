using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MinecraftWebExporter.Minecraft
{
    /// <summary>
    /// A minecraft resource pack
    /// </summary>
    public class ResourcePack : IAssetSource
    {
        /// <summary>
        /// Opens the given resource pack
        /// </summary>
        /// <param name="path"></param>
        public ResourcePack(string path)
        {
            Path = path;
            m_ZipArchive = ZipFile.OpenRead(Path);
        }
        
        /// <summary>
        /// Gets the path of the minecraft binary
        /// </summary>
        private string Path { get; }
        
        /// <summary>
        /// Gets the internal zip archive
        /// </summary>
        private readonly ZipArchive m_ZipArchive;


        /// <summary>
        /// Returns if the given asset is contained in this resource pack
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public bool ContainsAsset(AssetIdentifier asset)
        {
            lock (m_ZipArchive)
            {
                return m_ZipArchive.GetEntry(GetAssetPath(asset)) is not null;
            }
        }

        /// <summary>
        /// Opens the asset and returns its stream
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public bool TryOpenAsset(AssetIdentifier asset, out Stream? stream)
        {
            lock (m_ZipArchive)
            {
                var path = GetAssetPath(asset);
                var entry = m_ZipArchive.GetEntry(path);
                if (entry is null)
                {
                    stream = default;
                    return false;
                }
                    
                var zipStream = entry.Open();
                stream = new MemoryStream();
                zipStream.CopyTo(stream);
                stream.Seek(0, SeekOrigin.Begin);
                zipStream.Close();
                return true;
            }
        }

        /// <summary>
        /// Returns all assets for the given type and namespace
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        public IEnumerable<AssetIdentifier> GetAssets(AssetType type, string ns)
        {
            var path = GetAssetPath(type, ns);
            lock (m_ZipArchive)
            {
                foreach (var entry in m_ZipArchive.Entries.Where(e => e.FullName.StartsWith(path)))
                {
                    var name = entry.FullName.Substring(path.Length);
                    var index = name.LastIndexOf('.');
                    if (index >= 0)
                    {
                        name = name.Substring(0, index);
                    }

                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    yield return new AssetIdentifier(type, ns, name);
                }
            }
        }
        
        /// <summary>
        /// Gets the asset path
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static string GetAssetPath(AssetType type, string ns)
        {
            switch (type)
            {
                case AssetType.Texture:
                case AssetType.TextureMeta:
                    return $"assets/{ns}/textures/";
                case AssetType.Model:
                    return $"assets/{ns}/models/";
                case AssetType.Sound:
                    return $"assets/{ns}/sounds/";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Invalid asset type!");
            }
        }
        
        /// <summary>
        /// Gets the asset path
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static string GetAssetPath(AssetIdentifier asset)
        {
            switch (asset.Type)
            {
                case AssetType.Texture:
                    return $"assets/{asset.Namespace}/textures/{asset.Name}.png";
                case AssetType.TextureMeta:
                    return $"assets/{asset.Namespace}/textures/{asset.Name}.png.mcmeta";
                case AssetType.Model:
                    return $"assets/{asset.Namespace}/models/{asset.Name}.json";
                case AssetType.Sound:
                    return $"assets/{asset.Namespace}/sounds/{asset.Name}.ogg";
                case AssetType.BlockState:
                    return $"assets/{asset.Namespace}/blockstates/{asset.Name}.json";
                default:
                    throw new ArgumentOutOfRangeException(nameof(asset.Type), "Invalid asset type!");
            }
        }
        
        /// <summary>
        /// Dispose the resource pack
        /// </summary>
        public void Dispose()
        {
            m_ZipArchive.Dispose();
        }
    }
}