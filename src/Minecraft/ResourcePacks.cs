using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MinecraftWebExporter.Minecraft
{
    /// <summary>
    /// A collection of <see cref="ResourcePack"/>s.
    /// Assets are loaded in the same order the 
    /// </summary>
    public class ResourcePacks : IAssetSource
    {
        /// <summary>
        /// The list of resource packs
        /// </summary>
        private readonly List<ResourcePack> m_List = new();

        /// <summary>
        /// Adds the resource pack
        /// </summary>
        /// <param name="path"></param>
        public void Add(string path)
        {
            m_List.Add(new ResourcePack(path));
        }

        /// <summary>
        /// Adds multiple resource packs
        /// </summary>
        /// <param name="paths"></param>
        public void AddRange(IEnumerable<string> paths)
        {
            m_List.AddRange(paths.Select(p => new ResourcePack(p)));
        }
        
        /// <summary>
        /// Returns if the given asset is contained in this resource pack
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public bool ContainsAsset(AssetIdentifier asset)
        {
            foreach (var resourcePack in m_List)
            {
                if (resourcePack.ContainsAsset(asset))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Opens the asset and returns its stream
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public bool TryOpenAsset(AssetIdentifier asset, out Stream? stream)
        {
            foreach (var resourcePack in m_List)
            {
                if (resourcePack.TryOpenAsset(asset, out stream))
                {
                    return true;
                }
            }

            stream = default;
            return false;
        }

        /// <summary>
        /// Returns all assets for the given type and namespace
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ns"></param>
        /// <returns></returns>
        public IEnumerable<AssetIdentifier> GetAssets(AssetType type, string ns)
        {
            foreach (var resourcePack in m_List)
            {
                foreach (var asset in resourcePack.GetAssets(type, ns))
                {
                    yield return asset;
                }
            }
        }
        
        /// <summary>
        /// Dispose all resource packs
        /// </summary>
        public void Dispose()
        {
            foreach (var resourcePack in m_List)
            {
                resourcePack.Dispose();
            }
            
            m_List.Clear();
        }
    }
}