using System;

namespace MinecraftWebExporter.Minecraft;

/// <summary>
/// The asset type enum
/// </summary>
public enum AssetType
{
    Texture,
    TextureMeta, // May remove meta types from the enum and use a bool or separate method to access meta files.
    Model,
    Sound,
    BlockState,
}
    
/// <summary>
/// The asset name in a resource pack
/// </summary>
public readonly struct AssetIdentifier : IEquatable<AssetIdentifier>
{
    public AssetIdentifier(AssetType type, string ns, string name)
    {
        Namespace = ns;
        Type = type;
        Name = name;
    }

    public AssetIdentifier(AssetType type, string name)
    {
        Type = type;
        var index = name.IndexOf(':');
        if (index >= 0)
        {
            Namespace = name.Substring(0, index);
            Name = name.Substring(index + 1);
        }
        else
        {
            Namespace = "minecraft";
            Name = name;
        }
    }

    /// <summary>
    /// Gets the namespace
    /// </summary>
    public string Namespace { get; init; }

    /// <summary>
    /// Gets the asset type
    /// </summary>
    public AssetType Type { get; init; }

    /// <summary>
    /// Gets the asset name
    /// </summary>
    public string Name { get; init; }

    public bool Equals(AssetIdentifier other)
    {
        return Namespace == other.Namespace && Type == other.Type && Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        return obj is AssetIdentifier other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Namespace, (int) Type, Name);
    }

    public static bool operator ==(AssetIdentifier left, AssetIdentifier right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AssetIdentifier left, AssetIdentifier right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"{Namespace}:{Type}/{Name}";
    }
}