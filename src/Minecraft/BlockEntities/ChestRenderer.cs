using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MinecraftWebExporter.Minecraft.BlockStates;
using MinecraftWebExporter.Minecraft.BlockStates.Cache;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.BlockEntities;

/// <summary>
/// The entity block renderer for chests 
/// </summary>
public class ChestRenderer : BlockEntityRenderer
{
    private static readonly CubeBuilder ModelSingleBottom = new CubeBuilder(1.0f, 0.0f, 1.0f, 14.0f, 10.0f, 14.0f)
        .SetTextureOffset(0f, 19f);
    private static readonly CubeBuilder ModelSingleLid = new CubeBuilder(1.0f, 0.0f, 0.0f, 14.0f, 5.0f, 14.0f)
        .SetTextureOffset(0f, 0f).SetMatrix(Matrix4x4.Offset(0.0f, 9.0f, 1.0f));
    private static readonly CubeBuilder ModelSingleLock = new CubeBuilder(7.0f, -1.0f, 15.0f, 2.0f, 4.0f, 1.0f)
        .SetTextureOffset(0f, 0f).SetMatrix(Matrix4x4.Offset(0.0f, 8.0f, 0.0f));
    
    private static readonly CubeBuilder ModelDoubleRightBottom = new CubeBuilder(1.0f, 0.0f, 1.0f, 15.0f, 10.0f, 14.0f)
        .SetTextureOffset(0f, 19f);
    private static readonly CubeBuilder ModelDoubleRightLid = new CubeBuilder(1.0f, 0.0f, 0.0f, 15.0f, 5.0f, 14.0f)
        .SetTextureOffset(0f, 0f).SetMatrix(Matrix4x4.Offset(0.0f, 9.0f, 1.0f));
    private static readonly CubeBuilder ModelDoubleRightLock = new CubeBuilder(15.0f, -1.0f, 15.0f, 1.0f, 4.0f, 1.0f)
        .SetTextureOffset(0f, 0f).SetMatrix(Matrix4x4.Offset(0.0f, 8.0f, 0.0f));
    
    private static readonly CubeBuilder ModelDoubleLeftBottom = new CubeBuilder(0.0f, 0.0f, 1.0f, 15.0f, 10.0f, 14.0f)
        .SetTextureOffset(0f, 19f);
    private static readonly CubeBuilder ModelDoubleLeftLid = new CubeBuilder(0.0f, 0.0f, 0.0f, 15.0f, 5.0f, 14.0f)
        .SetTextureOffset(0f, 0f).SetMatrix(Matrix4x4.Offset(0.0f, 9.0f, 1.0f));
    private static readonly CubeBuilder ModelDoubleLeftLock = new CubeBuilder(0.0f, -1.0f, 15.0f, 1.0f, 4.0f, 1.0f)
        .SetTextureOffset(0f, 0f).SetMatrix(Matrix4x4.Offset(0.0f, 8.0f, 0.0f));
    
    
    private readonly string m_Material;
    
    public ChestRenderer(string material)
    {
        m_Material = material;
    }

    /// <inheritdoc cref="BlockEntityRenderer.Build"/>
    public override void Build(List<CachedBlockStateFace> faces, IBlockStateProperties? properties)
    {
        var type = properties?.GetValueOrDefault("type") ?? "single";
        var facing = properties?.GetValueOrDefault("facing") ?? "south";

        CubeBuilder modelBottom;
        CubeBuilder modelLid;
        CubeBuilder modelLock;
        AssetIdentifier texture;
        
        var rotY = GetYRotationByFacing(facing);

        // Chest type
        switch (type.ToLowerInvariant())
        {
            case "single":
                texture = new AssetIdentifier(AssetType.Texture, $"entity/chest/{m_Material}");
                modelBottom = ModelSingleBottom;
                modelLid = ModelSingleLid;
                modelLock = ModelSingleLock;
                break;
            case "left":
                texture = new AssetIdentifier(AssetType.Texture, $"entity/chest/{m_Material}_left");
                modelBottom = ModelDoubleLeftBottom;
                modelLid = ModelDoubleLeftLid;
                modelLock = ModelDoubleLeftLock;
                break;
            case "right":
                texture = new AssetIdentifier(AssetType.Texture, $"entity/chest/{m_Material}_right");
                modelBottom = ModelDoubleRightBottom;
                modelLid = ModelDoubleRightLid;
                modelLock = ModelDoubleRightLock;
                break;
            default:
                throw new ArgumentException($"Unknown chest type: {type}");
        }
        
        var matrix = Matrix4x4.Identity;
        matrix.Translate(8f, 8f, 8f);
        matrix.Rotate(0f, -rotY, 0f);
        matrix.Translate(-8f, -8f, -8f);
        
        modelBottom.Build(faces, matrix, texture, 64, 64);
        modelLid.Build(faces, matrix, texture, 64, 64);
        modelLock.Build(faces, matrix, texture, 64, 64);
    }
}