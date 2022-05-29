using System.Collections.Generic;

namespace MinecraftWebExporter.Minecraft.Models
{
    /// <summary>
    /// The texture dictionary of a <see cref="Model"/>.
    /// </summary>
    public class ModelTextures : Dictionary<string, string>
    {
        /// <summary>
        /// Returns the texture id by resolving the key in the texture list.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public AssetIdentifier Get(string? key)
        {
            if (key is null) return default;
            if (!key.StartsWith('#')) return new AssetIdentifier(AssetType.Texture, key);
            key = key.Substring(1);
            return TryGetValue(key, out var value) ? Get(value) : default;

        }
        
        /// <summary>
        /// Merges the two texture lists
        /// </summary>
        /// <param name="textures"></param>
        /// <param name="parentTextures"></param>
        /// <returns></returns>
        public static ModelTextures? Merge(ModelTextures? textures, ModelTextures? parentTextures)
        {
            if (textures is null)
            {
                return parentTextures;
            }

            if (parentTextures is null)
            {
                return textures;
            }

            var result = new ModelTextures();
            foreach (var pair in parentTextures)
            {
                result[pair.Key] = pair.Value;
            }
            foreach (var pair in textures)
            {
                result[pair.Key] = pair.Value;
            }
            return result;
        }
    }
}