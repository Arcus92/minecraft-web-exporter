using System.Collections.Generic;
using SharpNBT;

namespace MinecraftWebExporter.Minecraft.BlockStates;

/// <summary>
/// An interface for the block properties
/// </summary>
public interface IBlockStateProperties
{
    /// <summary>
    /// Gets the value for the given key or returns <c>null</c>.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    string? GetValueOrDefault(string key);

    /// <summary>
    /// Creates the property interface from the NBT element
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static IBlockStateProperties? Create(CompoundTag? tag)
    {
        return tag is null ? null : new NbtBlockStateProperties(tag);
    }
    
    /// <summary>
    /// Creates the property interface from a dictionary
    /// </summary>
    /// <param name="dictionary"></param>
    /// <returns></returns>
    public static IBlockStateProperties? Create(IReadOnlyDictionary<string, string>? dictionary)
    {
        return dictionary is null ? null : new DictionaryBlockStateProperties(dictionary);
    }
}

/// <summary>
/// A wrapper to map a NBT <see cref="CompoundTag"/> to a <see cref="IBlockStateProperties"/>
/// </summary>
public class NbtBlockStateProperties : IBlockStateProperties
{
    public NbtBlockStateProperties(CompoundTag tag)
    {
        Tag = tag;
    }
    
    /// <summary>
    /// Gets the source tag
    /// </summary>
    public CompoundTag Tag { get; }

    /// <inheritdoc cref="IBlockStateProperties.GetValueOrDefault"/>
    public string? GetValueOrDefault(string key)
    {
        if (Tag[key] is StringTag stringTag)
        {
            return stringTag.Value;
        }

        return null;
    }
}

/// <summary>
/// A wrapper to map a NBT <see cref="IReadOnlyDictionary{TKey,TValue}"> to a <see cref="IBlockStateProperties"/>
/// </summary>
public class DictionaryBlockStateProperties : IBlockStateProperties
{
    public DictionaryBlockStateProperties(IReadOnlyDictionary<string, string> dictionary)
    {
        Dictionary = dictionary;
    }
    
    /// <summary>
    /// Gets the source dictionary
    /// </summary>
    public IReadOnlyDictionary<string, string> Dictionary { get; }

    /// <inheritdoc cref="IBlockStateProperties.GetValueOrDefault"/>
    public string? GetValueOrDefault(string key)
    {
        return Dictionary.TryGetValue(key, out var value) ? value : null;
    }
}

