using System.Collections.Generic;
using MinecraftWebExporter.Minecraft.BlockStates.Cache;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.BlockEntities;

/// <summary>
/// A helper to build faces from a cube
/// </summary>
public struct CubeBuilder
{
    /// <summary>
    /// Gets abd sets the origin coordinate of the cube
    /// </summary>
    public Vector3 Origin { get; set; }

    /// <summary>
    /// Gets abd sets the size coordinate of the cube
    /// </summary>
    public Vector3 Size { get; set; }

    /// <summary>
    /// The texture offset
    /// </summary>
    public Vector2 TextureOffset { get; set; }

    /// <summary>
    /// The transformation matrix
    /// </summary>
    public Matrix4x4 Matrix { get; set; }

    public CubeBuilder(float x, float y, float z, float sizeX, float sizeY, float sizeZ)
    {
        Origin = new Vector3(x, y, z);
        Size = new Vector3(sizeX, sizeY, sizeZ);
        TextureOffset = Vector2.Zero;
        Matrix = Matrix4x4.Identity;
    }

    /// <summary>
    /// Sets the texture offset
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public CubeBuilder SetTextureOffset(float x, float y)
    {
        TextureOffset = new Vector2(x, y);
        return this;
    }

    /// <summary>
    /// Sets the transformation matrix
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public CubeBuilder SetMatrix(Matrix4x4 matrix)
    {
        Matrix = matrix;
        return this;
    }
    
    /// <summary>
    /// Builds the faces of this box
    /// </summary>
    /// <param name="faces"></param>
    /// <param name="localMatrix"></param>
    /// <param name="texture"></param>
    /// <param name="textureWidth"></param>
    /// <param name="textureHeight"></param>
    public readonly void Build(List<CachedBlockStateFace> faces, Matrix4x4 localMatrix, AssetIdentifier texture, int textureWidth, int textureHeight)
    {
        var matrix = localMatrix * Matrix;
        
        var v1 = matrix.MultiplyPoint(Origin.X, Origin.Y, Origin.Z) / 16f;
        var v2 = matrix.MultiplyPoint(Origin.X + Size.X, Origin.Y, Origin.Z) / 16f;
        var v3 = matrix.MultiplyPoint(Origin.X + Size.X, Origin.Y + Size.Y, Origin.Z) / 16f;
        var v4 = matrix.MultiplyPoint(Origin.X, Origin.Y + Size.Y, Origin.Z) / 16f;
        var v5 = matrix.MultiplyPoint(Origin.X, Origin.Y, Origin.Z + Size.Z) / 16f;
        var v6 = matrix.MultiplyPoint(Origin.X + Size.X, Origin.Y, Origin.Z + Size.Z) / 16f;
        var v7 = matrix.MultiplyPoint(Origin.X + Size.X, Origin.Y + Size.Y, Origin.Z + Size.Z) / 16f;
        var v8 = matrix.MultiplyPoint(Origin.X, Origin.Y + Size.Y, Origin.Z + Size.Z) / 16f;

        var texCol0 = (TextureOffset.X) / textureWidth;
        var texCol1 = (TextureOffset.X + Size.Z) / textureWidth;
        var texCol2 = (TextureOffset.X + Size.Z + Size.X) / textureWidth;
        var texCol3 = (TextureOffset.X + Size.Z + Size.X + Size.X) / textureWidth;
        var texCol4 = (TextureOffset.X + Size.Z + Size.X + Size.Z) / textureWidth;
        var texCol5 = (TextureOffset.X + Size.Z + Size.X + Size.Z + Size.X) / textureWidth;

        var texRow0 = -(TextureOffset.Y) / textureHeight;
        var texRow1 = -(TextureOffset.Y + Size.Z) / textureHeight;
        var texRow2 = -(TextureOffset.Y + Size.Z + Size.Y) / textureHeight;
        
        faces.Add(new CachedBlockStateFace()
        {
            Direction = Direction.Down,
            Texture = texture,
            VertexA = v6, VertexB = v5, VertexC = v1, VertexD = v2,
            UvA = new Vector2(texCol2, texRow0),
            UvB = new Vector2(texCol1, texRow0),
            UvC = new Vector2(texCol1, texRow1),
            UvD = new Vector2(texCol2, texRow1),
            Normal = matrix.MultiplyVector(0f, -1f, 0f),
        });
        faces.Add(new CachedBlockStateFace()
        {
            Direction = Direction.Up,
            Texture = texture,
            VertexA = v3, VertexB = v4, VertexC = v8, VertexD = v7,
            UvA = new Vector2(texCol3, texRow1),
            UvB = new Vector2(texCol2, texRow1),
            UvC = new Vector2(texCol2, texRow0),
            UvD = new Vector2(texCol3, texRow0),
            Normal = matrix.MultiplyVector(0f, 1f, 0f),
        });
        faces.Add(new CachedBlockStateFace()
        {
            Direction = Direction.West,
            Texture = texture,
            VertexA = v1, VertexB = v5, VertexC = v8, VertexD = v4,
            UvA = new Vector2(texCol1, texRow1),
            UvB = new Vector2(texCol0, texRow1),
            UvC = new Vector2(texCol0, texRow2),
            UvD = new Vector2(texCol1, texRow2),
            Normal = matrix.MultiplyVector(-1f, 0f, 0f),
        });
        faces.Add(new CachedBlockStateFace()
        {
            Direction = Direction.North,
            Texture = texture,
            VertexA = v2, VertexB = v1, VertexC = v4, VertexD = v3,
            UvA = new Vector2(texCol2, texRow1),
            UvB = new Vector2(texCol1, texRow1),
            UvC = new Vector2(texCol1, texRow2),
            UvD = new Vector2(texCol2, texRow2),
            Normal = matrix.MultiplyVector(0f, 0f, 1f),
        });
        faces.Add(new CachedBlockStateFace()
        {
            Direction = Direction.East,
            Texture = texture,
            VertexA = v6, VertexB = v2, VertexC = v3, VertexD = v7,
            UvA = new Vector2(texCol4, texRow1),
            UvB = new Vector2(texCol2, texRow1),
            UvC = new Vector2(texCol2, texRow2),
            UvD = new Vector2(texCol4, texRow2),
            Normal = matrix.MultiplyVector(1f, 0f, 0f),
        });
        faces.Add(new CachedBlockStateFace()
        {
            Direction = Direction.South,
            Texture = texture,
            VertexA = v5, VertexB = v6, VertexC = v7, VertexD = v8,
            UvA = new Vector2(texCol5, texRow1),
            UvB = new Vector2(texCol4, texRow1),
            UvC = new Vector2(texCol4, texRow2),
            UvD = new Vector2(texCol5, texRow2),
            Normal = matrix.MultiplyVector(0f, 0f, -1f),
        });
    }
}