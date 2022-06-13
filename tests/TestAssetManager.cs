using System.Collections.Generic;
using System.Threading.Tasks;
using MinecraftWebExporter.Minecraft;
using MinecraftWebExporter.Minecraft.BlockStates;
using MinecraftWebExporter.Minecraft.Models;
using MinecraftWebExporter.Minecraft.Textures;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Tests;

public class TestAssetManager : IAssetManager
{
    public TestAssetManager()
    {
        Source = new TestAssetSource();
        ModelCache = new ModelCache(this);
    }
    
    /// <summary>
    /// Gets the main asset source
    /// </summary>
    public IAssetSource Source { get; }

    /// <summary>
    /// Gets the model cache
    /// </summary>
    public ModelCache ModelCache { get; }

    #region Models
    
    /// <summary>
    /// A list of model defined by Minecraft
    /// </summary>
    private static readonly Dictionary<string, Model> Models = new()
    {
        {
            "block/stone_stairs", new Model()
            {
                Parent = "minecraft:block/stairs",
                Textures = new ModelTextures()
                {
                    { "bottom", "minecraft:block/stone" },
                    { "top", "minecraft:block/stone" },
                    { "side", "minecraft:block/stone" },
                },
                Elements = new []
                {
                    new ModelElement()
                    {
                        From = new Vector3(0, 0, 0),
                        To = new Vector3(16, 8, 16),
                        Faces = new ModelElementFaces()
                        {
                            Down = new ModelElementFace()
                            {
                                Uv = new Vector4(0, 0, 16, 16),
                                Texture = "#bottom", 
                                CullFace = Direction.Down,
                            },
                            Up = new ModelElementFace()
                            {
                                Uv = new Vector4(0, 0, 16, 16),
                                Texture = "#top",
                            },
                            North = new ModelElementFace()
                            {
                                Uv = new Vector4(0, 8, 16, 16),
                                Texture = "#side", 
                                CullFace = Direction.North,
                            },
                            South = new ModelElementFace()
                            {
                                Uv = new Vector4(0, 8, 16, 16),
                                Texture = "#side", 
                                CullFace = Direction.South,
                            },
                            West = new ModelElementFace()
                            {
                                Uv = new Vector4(0, 8, 16, 16),
                                Texture = "#side", 
                                CullFace = Direction.West,
                            },
                            East = new ModelElementFace()
                            {
                                Uv = new Vector4(0, 8, 16, 16),
                                Texture = "#side", 
                                CullFace = Direction.East,
                            }
                        }
                    },
                    new ModelElement()
                    {
                        From = new Vector3(8, 8, 0),
                        To = new Vector3(16, 16, 16),
                        Faces = new ModelElementFaces()
                        {
                            Up = new ModelElementFace()
                            {
                                Uv = new Vector4(8, 0, 16, 16),
                                Texture = "#top",
                                CullFace = Direction.Up,
                            },
                            North = new ModelElementFace()
                            {
                                Uv = new Vector4(0, 0, 8, 8),
                                Texture = "#side", 
                                CullFace = Direction.North,
                            },
                            South = new ModelElementFace()
                            {
                                Uv = new Vector4(8, 0, 16, 8),
                                Texture = "#side", 
                                CullFace = Direction.South,
                            },
                            West = new ModelElementFace()
                            {
                                Uv = new Vector4(0, 0, 16, 8),
                                Texture = "#side",
                            },
                            East = new ModelElementFace()
                            {
                                Uv = new Vector4(0, 0, 16, 8),
                                Texture = "#side", 
                                CullFace = Direction.East,
                            }
                        }
                    }
                },
            }
        }
    };

    /// <inheritdoc cref="IAssetManager.GetModelAsync"/>
    public Task<Model?> GetModelAsync(AssetIdentifier assetIdentifier)
    {
        Models.TryGetValue(assetIdentifier.Name, out var model);
        return Task.FromResult(model);
    }

    #endregion Models
    
    #region Block states

    /// <summary>
    /// A list of block states defined by Minecraft
    /// </summary>
    private static readonly Dictionary<string, BlockState> BlockStates = new()
    {
        {
            "stone_stairs", new BlockState()
            {
                Variants = new BlockStateVariants()
                {
                    {
                        "facing=north,half=bottom,shape=straight", new[]
                        {
                            new BlockStateVariant()
                            {
                                Model = "minecraft:block/stone_stairs",
                                Y = 270,
                                UvLock = true,
                            }
                        }
                    },
                    {
                        "facing=north,half=top,shape=straight", new[]
                        {
                            new BlockStateVariant()
                            {
                                Model = "minecraft:block/stone_stairs",
                                X = 180,
                                Y = 270,
                                UvLock = true,
                            }
                        }
                    },
                    {
                        "facing=east,half=bottom,shape=straight", new[]
                        {
                            new BlockStateVariant()
                            {
                                Model = "minecraft:block/stone_stairs",
                                UvLock = true,
                            }
                        }
                    },
                    {
                        "facing=east,half=top,shape=straight", new[]
                        {
                            new BlockStateVariant()
                            {
                                Model = "minecraft:block/stone_stairs",
                                X = 180,
                                UvLock = true,
                            }
                        }
                    },
                    {
                        "facing=south,half=bottom,shape=straight", new[]
                        {
                            new BlockStateVariant()
                            {
                                Model = "minecraft:block/stone_stairs",
                                Y = 90,
                                UvLock = true,
                            }
                        }
                    },
                    {
                        "facing=south,half=top,shape=straight", new[]
                        {
                            new BlockStateVariant()
                            {
                                Model = "minecraft:block/stone_stairs",
                                X = 180,
                                Y = 90,
                                UvLock = true,
                            }
                        }
                    },
                    {
                        "facing=west,half=bottom,shape=straight", new[]
                        {
                            new BlockStateVariant()
                            {
                                Model = "minecraft:block/stone_stairs",
                                Y = 180,
                                UvLock = true,
                            }
                        }
                    },
                    {
                        "facing=west,half=top,shape=straight", new[]
                        {
                            new BlockStateVariant()
                            {
                                Model = "minecraft:block/stone_stairs",
                                X = 180,
                                Y = 180,
                                UvLock = true,
                            }
                        }
                    }
                }
            }
        }
    };
    
    /// <inheritdoc cref="IAssetManager.GetBlockStateAsync"/>
    public Task<BlockState?> GetBlockStateAsync(AssetIdentifier assetIdentifier)
    {
        BlockStates.TryGetValue(assetIdentifier.Name, out var blockState);
        return Task.FromResult(blockState);
    }
    
    #endregion Block states

    #region Textures
    
    /// <inheritdoc cref="IAssetManager.GetTextureAsync"/>
    public Task<CachedTexture> GetTextureAsync(AssetIdentifier assetIdentifier)
    {
        return Task.FromResult(default(CachedTexture));
    }
    
    #endregion Textures
    
    /// <summary>
    /// Dispose the asset manager
    /// </summary>
    public void Dispose()
    {
        Source.Dispose();
    }
}