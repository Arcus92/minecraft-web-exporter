using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MinecraftWebExporter.Minecraft.BlockStates;
using MinecraftWebExporter.Minecraft.BlockStates.Cache;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.BlockEntities;

/// <summary>
/// The entity block renderer for beds
/// </summary>
public class BedRenderer : BlockEntityRenderer
{
    private static readonly CubeBuilder ModelHeadMain = new CubeBuilder(0.0f, 0.0f, 0.0f, 16.0f, 16.0f, 6.0f)
        .SetTextureOffset(0f, 0f);
    private static readonly CubeBuilder ModelHeadLeftLeg = new CubeBuilder(0.0f, 6.0f, 0.0f, 3.0f, 3.0f, 3.0f)
        .SetTextureOffset(50f, 6f).SetMatrix(Matrix4x4.Rotation(1.5707964f, 0.0f, 1.5707964f));
    private static readonly CubeBuilder ModelHeadRightLeg = new CubeBuilder(-16.0f, 6.0f, 0.0f, 3.0f, 3.0f, 3.0f)
        .SetTextureOffset(50f, 18f).SetMatrix(Matrix4x4.Rotation(1.5707964f, 0.0f, 3.1415927f));
    
    
    private static readonly CubeBuilder ModelFootMain = new CubeBuilder(0.0f, 0.0f, 0.0f, 16.0f, 16.0f, 6.0f)
        .SetTextureOffset(0f, 22f);
    private static readonly CubeBuilder ModelFootLeftLeg = new CubeBuilder(0.0f, 6.0f, -16.0f, 3.0f, 3.0f, 3.0f)
        .SetTextureOffset(50f, 0f).SetMatrix(Matrix4x4.Rotation(1.5707964f, 0.0f, 0.0f));
    private static readonly CubeBuilder ModelFootRightLeg = new CubeBuilder(-16.0f, 6.0f, -16.0f, 3.0f, 3.0f, 3.0f)
        .SetTextureOffset(50f, 12f).SetMatrix(Matrix4x4.Rotation(1.5707964f, 0.0f, 4.712389f));
    

    private readonly string m_Material;
    
    public BedRenderer(string material)
    {
        m_Material = material;
    }
    
    /// <inheritdoc cref="BlockEntityRenderer.Build"/>
    public override void Build(List<CachedBlockStateFace> faces, IBlockStateProperties? properties)
    {
        var part = properties?.GetValueOrDefault("part") ?? "head";
        var facing = properties?.GetValueOrDefault("facing") ?? "south";

        CubeBuilder modelMain;
        CubeBuilder modelLeftLeg;
        CubeBuilder modelRightLeg;

        var rotY = GetYRotationByFacing(facing);

        // Chest type
        switch (part.ToLowerInvariant())
        {
            case "head":
                modelMain = ModelHeadMain;
                modelLeftLeg = ModelHeadLeftLeg;
                modelRightLeg = ModelHeadRightLeg;
                break;
            case "foot":
                modelMain = ModelFootMain;
                modelLeftLeg = ModelFootLeftLeg;
                modelRightLeg = ModelFootRightLeg;
                break;
            default:
                throw new ArgumentException($"Unknown bed part: {part}");
        }
        var texture = new AssetIdentifier(AssetType.Texture, $"entity/bed/{m_Material}");

        var matrix = Matrix4x4.Identity;
        matrix.Translate(8f, 1f, 8f);
        matrix.Rotate(MathF.PI / 2f, MathF.PI - rotY, 0f);
        matrix.Translate(-8f, -8f, -8f);
        
        modelMain.Build(faces, matrix, texture, 64, 64);
        modelLeftLeg.Build(faces, matrix, texture, 64, 64);
        modelRightLeg.Build(faces, matrix, texture, 64, 64);
    }
}