using System;
using System.Collections.Generic;
using MinecraftWebExporter.Minecraft.BlockStates;
using MinecraftWebExporter.Minecraft.BlockStates.Cache;

namespace MinecraftWebExporter.Minecraft.BlockEntities;

/// <summary>
/// Entity blocks are blocks without a model file. Minecraft renders these blocks in code. So there is no way to load
/// the geometry from the Minecraft binary nor a resource pack.
/// All supported entity blocks must be hardcoded in the export. There is a table with all supported renderer in <see cref="Map"/>.
/// </summary>
public abstract class BlockEntityRenderer
{
    /// <summary>
    /// Builds the face list of the entity block
    /// </summary>
    /// <param name="faces"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    public abstract void Build(List<CachedBlockStateFace> faces, IBlockStateProperties? properties);
    
    #region Static

    /// <summary>
    /// Returns the rotation around the y axis (in rad) by the facing type
    /// </summary>
    /// <param name="facing"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static float GetYRotationByFacing(string facing)
    {
        return facing.ToLowerInvariant() switch
        {
            "south" => 0f,
            "west" => MathF.PI / 2f,
            "north" => MathF.PI,
            "east" => MathF.PI / 2f * 3f,
            _ => throw new ArgumentException($"Unknown facing direction: {facing}"),
        };
    }
    
    /// <summary>
    /// Gets a map of all build-in entity blocks
    /// </summary>
    public static readonly Dictionary<AssetIdentifier, BlockEntityRenderer> Map =
        new()
        {
            { new AssetIdentifier(AssetType.BlockState, "chest"), new ChestRenderer("normal") },
            { new AssetIdentifier(AssetType.BlockState, "ender_chest"), new ChestRenderer("ender") },
            { new AssetIdentifier(AssetType.BlockState, "trapped_chest"), new ChestRenderer("trapped") },
            
            { new AssetIdentifier(AssetType.BlockState, "black_bed"), new BedRenderer("black") },
            { new AssetIdentifier(AssetType.BlockState, "blue_bed"), new BedRenderer("blue") },
            { new AssetIdentifier(AssetType.BlockState, "brown_bed"), new BedRenderer("brown") },
            { new AssetIdentifier(AssetType.BlockState, "cyan_bed"), new BedRenderer("cyan") },
            { new AssetIdentifier(AssetType.BlockState, "gray_bed"), new BedRenderer("gray") },
            { new AssetIdentifier(AssetType.BlockState, "green_bed"), new BedRenderer("green") },
            { new AssetIdentifier(AssetType.BlockState, "light_blue_bed"), new BedRenderer("light_blue") },
            { new AssetIdentifier(AssetType.BlockState, "light_gray_bed"), new BedRenderer("light_gray") },
            { new AssetIdentifier(AssetType.BlockState, "magenta_bed"), new BedRenderer("magenta") },
            { new AssetIdentifier(AssetType.BlockState, "orange_bed"), new BedRenderer("orange") },
            { new AssetIdentifier(AssetType.BlockState, "pink_bed"), new BedRenderer("pink") },
            { new AssetIdentifier(AssetType.BlockState, "purple_bed"), new BedRenderer("purple") },
            { new AssetIdentifier(AssetType.BlockState, "red_bed"), new BedRenderer("red") },
            { new AssetIdentifier(AssetType.BlockState, "white_bed"), new BedRenderer("white") },
            { new AssetIdentifier(AssetType.BlockState, "yellow_bed"), new BedRenderer("yellow") },
            
            { new AssetIdentifier(AssetType.BlockState, "acacia_sign"), new SignRenderer(true, "acacia") },
            { new AssetIdentifier(AssetType.BlockState, "birch_sign"), new SignRenderer(true, "birch") },
            { new AssetIdentifier(AssetType.BlockState, "crimson_sign"), new SignRenderer(true, "crimson") },
            { new AssetIdentifier(AssetType.BlockState, "dark_oak_sign"), new SignRenderer(true, "dark_oak") },
            { new AssetIdentifier(AssetType.BlockState, "jungle_sign"), new SignRenderer(true, "jungle") },
            { new AssetIdentifier(AssetType.BlockState, "mangrove_sign"), new SignRenderer(true, "mangrove") },
            { new AssetIdentifier(AssetType.BlockState, "oak_sign"), new SignRenderer(true, "oak") },
            { new AssetIdentifier(AssetType.BlockState, "spruce_sign"), new SignRenderer(true, "spruce") },
            { new AssetIdentifier(AssetType.BlockState, "warped_sign"), new SignRenderer(true, "warped_oak") },
            
            { new AssetIdentifier(AssetType.BlockState, "acacia_wall_sign"), new SignRenderer(false, "acacia") },
            { new AssetIdentifier(AssetType.BlockState, "birch_wall_sign"), new SignRenderer(false, "birch") },
            { new AssetIdentifier(AssetType.BlockState, "crimson_wall_sign"), new SignRenderer(false, "crimson") },
            { new AssetIdentifier(AssetType.BlockState, "dark_oak_wall_sign"), new SignRenderer(false, "dark_oak") },
            { new AssetIdentifier(AssetType.BlockState, "jungle_wall_sign"), new SignRenderer(false, "jungle") },
            { new AssetIdentifier(AssetType.BlockState, "mangrove_wall_sign"), new SignRenderer(false, "mangrove") },
            { new AssetIdentifier(AssetType.BlockState, "oak_wall_sign"), new SignRenderer(false, "oak") },
            { new AssetIdentifier(AssetType.BlockState, "spruce_wall_sign"), new SignRenderer(false, "spruce") },
            { new AssetIdentifier(AssetType.BlockState, "warped_wall_sign"), new SignRenderer(false, "warped_oak") },
        };

    #endregion Static
}