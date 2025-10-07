using System;
using MinecraftWebExporter.Minecraft.BlockStates;
using MinecraftWebExporter.Minecraft.Models.Cache;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.Data;

/// <summary>
/// Gets the hard-coded block information from vanilla Minecraft.
/// </summary>
public static class BlockData
{
    /// <summary>
    /// Gets the spruce leave color.
    /// </summary>
    public static readonly Vector3 SpruceLeavesColor = new() {X = 97f / 255f, Y = 153f / 255f, Z = 97f / 255f};
    
    /// <summary>
    /// Gets the birch leave color.
    /// </summary>
    public static readonly Vector3 BirchLeavesColor = new() {X = 128f / 255f, Y = 167f / 255f, Z = 85f / 255f};
    
    /// <summary>
    /// Gets the lily pad color.
    /// </summary>
    public static readonly Vector3 LilyPadColor = new() {X = 20f / 255f, Y = 128f / 255f, Z = 48f / 255f};

    /// <summary>
    /// Gets the stam color by age.
    /// </summary>
    /// <param name="age"></param>
    /// <returns></returns>
    public static Vector3 GetStemColor(byte age)
    {
        return new Vector3() { X = age * 32f / 255f, Y = 1f - age * 8f / 255f, Z = age * 4f / 255f };
    }

    /// <summary>
    /// Gets the redstone power color by level.
    /// </summary>
    /// <param name="power"></param>
    /// <returns></returns>
    public static Vector3 GetPowerColor(byte power)
    {
        var f = power / 15f;
        var r = f * 0.6f + (f > 0.0f ? 0.4f : 0.3f);
        var g = Math.Clamp(f * f * 0.7f - 0.5f, 0.0f, 1.0f);
        var b = Math.Clamp(f * f * 0.6f - 0.7f, 0.0f, 1.0f);
        return new Vector3() {X = r, Y = g, Z = b};
    }
    
    /// <summary>
    /// The grass tint color map
    /// </summary>
    public static readonly ColorMap GrassTint = new()
    {
        Uv00 = new() {X = 191f / 255f, Y = 183f / 255f, Z = 85f / 255f},
        Uv10 = new() {X = 128f / 255f, Y = 180f / 255f, Z = 151f / 255f},
        Uv01 = new() {X = 71f / 255f, Y = 205f / 255f, Z = 51f / 255f},
    };
        
    /// <summary>
    /// The foliage tint color map
    /// </summary>
    public static readonly ColorMap FoliageTint = new()
    {
        Uv00 = new() {X = 174f / 255f, Y = 164f / 255f, Z = 42f / 255f},
        Uv10 = new() {X = 96f / 255f, Y = 161f / 255f, Z = 123f / 255f},
        Uv01 = new() {X = 26f / 255f, Y = 191f / 255f, Z = 0f / 255f},
    };
    
    /// <summary>
    /// The dry foliage tint color map
    /// </summary>
    public static readonly ColorMap FoliageDryTint = new()
    {
        Uv00 = new() {X = 163f / 255f, Y = 128f / 255f, Z = 70f / 255f},
        Uv10 = new() {X = 163f / 255f, Y = 95f / 255f, Z = 70f / 255f},
        Uv01 = new() {X = 143f / 255f, Y = 122f / 255f, Z = 90f / 255f},
    };
    
    /// <summary>
    /// Gets the tint type from the given block and face.
    /// </summary>
    /// <param name="blockAsset"></param>
    /// <param name="face"></param>
    /// <returns></returns>
    public static ModelTintType GetTintType(AssetIdentifier blockAsset, CachedModelFace face)
    {
        // There is this thing 'tintIndex'. This allows one block to have multiple tinted faces but currently
        // Minecraft doesn't use it. I simply check if this value is set ot not, because I don't know the intended
        // order of the indexes. Since it is not used in vanilla Minecraft, this should be fine. 
        // Also, the tint-able blocks are hard-coded. Maybe I could block-tags in the future.
        if (!face.TintIndex.HasValue) 
            return ModelTintType.Default;

        return blockAsset.Name switch
        {
            // Grass (two blocks)
            "large_fern" or "tall_grass" => ModelTintType.Grass,
            
            // Grass (normal)
            "grass" or "grass_block" or "fern" or "short_grass" or "potted_fern" or "bush" => ModelTintType.Grass,
            
            // Grass (flowers)
            "pink_petals" or "wildflowers" => ModelTintType.Grass,
            
            // Spruce leaves
            "spruce_leaves" => ModelTintType.SpruceLeaves,
            
            // Birch leaves
            "birch_leaves" => ModelTintType.BirchLeaves,
            
            // Other leaves
            "oak_leaves" or "jungle_leaves" or "acacia_leaves" or "dark_oak_leaves" or "vine" or "mangrove_leaves" => ModelTintType.Foliage,
            
            // Dry leaves
            "leaf_litter" => ModelTintType.FoliageDry,
            
            // Water
            "water" or "bubble_column" or "water_cauldron" => ModelTintType.Water,
            
            // Redstone
            "redstone_wire" => ModelTintType.Redstone,
            
            // Sugar cane
            "sugar_cane" => ModelTintType.Grass,
            
            // Attached stem
            "attached_melon_stem" or "attached_pumpkin_stem" => ModelTintType.AttachedStem,
            
            // Stem
            "melon_stem" or "pumpkin_stem" => ModelTintType.Stem,
            
            // Lily pad
            "lily_pad" => ModelTintType.LilyPad,
            
            _ => ModelTintType.Default
        };
    }
}