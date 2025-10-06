using System;
using System.Collections.Generic;
using MinecraftWebExporter.Minecraft.BlockStates;
using MinecraftWebExporter.Minecraft.BlockStates.Cache;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.BlockEntities;

/// <summary>
/// The entity block renderer for signs and wall signs
/// </summary>
public class SignRenderer : BlockEntityRenderer
{
    private static readonly CubeBuilder ModelSign = new CubeBuilder(-12.0f, -14.0f, -1.0f, 24.0f, 12.0f, 2.0f)
        .SetTextureOffset(0f, 0f);
    private static readonly CubeBuilder ModelStick = new CubeBuilder(-1.0f, -2.0f, -1.0f, 2.0f, 14.0f, 2.0f)
        .SetTextureOffset(0f, 14f);

    private readonly bool m_Standing;
    private readonly string m_Material;
    
    public SignRenderer(bool standing, string material)
    {
        m_Standing = standing;
        m_Material = material;
    }

    /// <inheritdoc cref="BlockEntityRenderer.Build"/>
    public override void Build(List<CachedBlockStateFace> faces, IBlockStateProperties? properties)
    {
        var texture = new AssetIdentifier(AssetType.Texture, $"entity/signs/{m_Material}");
        
        var matrix = Matrix4x4.Identity;
        matrix.Translate(8f, 8f, 8f);
        if (m_Standing)
        {
            var rotation = properties?.GetValueOrDefault("rotation") ?? "0";
            var rotY = int.Parse(rotation) / 16f * MathF.PI * 2f;
            
            matrix.Rotate(0f, -rotY, 0f);
        }
        else
        {
            var facing = properties?.GetValueOrDefault("facing") ?? "south";
            var rotY = GetYRotationByFacing(facing);
            
            matrix.Rotate(0f, -rotY, 0f);
            matrix.Translate(0f, -5f, -7f);
        }

        matrix.Scale(2f / 3f, -2f / 3f, -2f / 3f);

        ModelSign.Build(faces, matrix, texture, 64, 32);
        if (m_Standing)
        {
            ModelStick.Build(faces, matrix, texture, 64, 32);
        }
    }
}