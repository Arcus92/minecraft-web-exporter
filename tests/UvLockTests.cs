using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinecraftWebExporter.Minecraft.BlockStates;
using MinecraftWebExporter.Minecraft.BlockStates.Cache;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Tests
{
    [TestClass]
    public class UvLockTests
    {
        [TestMethod]
        [DataRow("north", "bottom")]
        [DataRow("north", "top")]
        [DataRow("east", "bottom")]
        [DataRow("east", "top")]
        [DataRow("south", "bottom")]
        [DataRow("south", "top")]
        [DataRow("west", "bottom")]
        [DataRow("west", "top")]
        public async Task TestUvLockOfStairBlocks(string facing, string half)
        {
            var assetManager = new TestAssetManager();
            var properties = IBlockStateProperties.Create(new Dictionary<string, string>()
            {
                { "facing", facing },
                { "half", half },
                { "shape", "straight" },
            });
            
            var block = await CachedBlockState.CreateAsync(assetManager, "minecraft:stone_stairs", properties);
            var variant = block.DefaultVariant;
            Assert.IsNotNull(variant.Faces);
            
            foreach (var face in variant.Faces)
            {
                switch (face.Direction)
                {
                    case Direction.Up:
                        Assert.AreEqual(face.VertexA.X, face.UvA.X);
                        Assert.AreEqual(face.VertexA.Z, 1 - face.UvA.Y);
                        Assert.AreEqual(face.VertexC.X, face.UvC.X);
                        Assert.AreEqual(face.VertexC.Z, 1 - face.UvC.Y);
                        break;
                    case Direction.Down:
                        Assert.AreEqual(face.VertexA.X, face.UvA.X);
                        Assert.AreEqual(face.VertexA.Z, face.UvA.Y);
                        Assert.AreEqual(face.VertexC.X, face.UvC.X);
                        Assert.AreEqual(face.VertexC.Z, face.UvC.Y);
                        break;
                    case Direction.South:
                        Assert.AreEqual(face.VertexA.X, face.UvA.X);
                        Assert.AreEqual(face.VertexA.Y, face.UvA.Y);
                        Assert.AreEqual(face.VertexC.X, face.UvC.X);
                        Assert.AreEqual(face.VertexC.Y, face.UvC.Y);
                        break;
                    case Direction.North:
                        Assert.AreEqual(face.VertexA.X, 1 - face.UvA.X);
                        Assert.AreEqual(face.VertexA.Y, face.UvA.Y);
                        Assert.AreEqual(face.VertexC.X, 1 - face.UvC.X);
                        Assert.AreEqual(face.VertexC.Y, face.UvC.Y);
                        break;
                    case Direction.West:
                        Assert.AreEqual(face.VertexA.Z, face.UvA.X);
                        Assert.AreEqual(face.VertexA.Y, face.UvA.Y);
                        Assert.AreEqual(face.VertexC.Z, face.UvC.X);
                        Assert.AreEqual(face.VertexC.Y, face.UvC.Y);
                        break;
                    case Direction.East:
                        Assert.AreEqual(face.VertexA.Z, 1 - face.UvA.X);
                        Assert.AreEqual(face.VertexA.Y, face.UvA.Y);
                        Assert.AreEqual(face.VertexC.Z, 1 - face.UvC.X);
                        Assert.AreEqual(face.VertexC.Y, face.UvC.Y);
                        break;
                    default:
                        Assert.Fail($"Unknown direction: {face.Direction}");
                        break;
                }
            }
        }
    }
}