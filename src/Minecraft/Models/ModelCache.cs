using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MinecraftWebExporter.Minecraft.BlockStates;
using MinecraftWebExporter.Minecraft.BlockStates.Cache;
using MinecraftWebExporter.Minecraft.Models.Cache;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.Models
{
    /// <summary>
    /// A cache for <see cref="CachedModel"/>.
    /// </summary>
    public class ModelCache
    {
        /// <summary>
        /// Creates the model cache
        /// </summary>
        /// <param name="assetManager"></param>
        public ModelCache(IAssetManager assetManager)
        {
            m_AssetManager = assetManager;
        }
        
        /// <summary>
        /// The asset manager
        /// </summary>
        private readonly IAssetManager m_AssetManager;
        
        /// <summary>
        /// The model cache
        /// </summary>
        private readonly ConcurrentDictionary<AssetIdentifier, CachedModel> m_Cache = new();

        /// <summary>
        /// Returns the texture info
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<CachedModel> GetAsync(AssetIdentifier asset)
        {
            if (asset.Type != AssetType.Model)
            {
                throw new ArgumentException( "Asset type must be model!", nameof(asset));
            }

            if (m_Cache.TryGetValue(asset, out var cachedModel))
            {
                return cachedModel;
            }

            var model = await m_AssetManager.GetModelAsync(asset);
            cachedModel = await CachedModel.CreateAsync(m_AssetManager, model);
            
            m_Cache.TryAdd(asset, cachedModel);
            return cachedModel;
        }
        
        /// <summary>
        /// The cached water model
        /// </summary>
        private ConcurrentDictionary<(byte, byte, byte, byte), CachedBlockStateVariant> m_WaterModels = new();
        
        /// <summary>
        /// The cached lava model
        /// </summary>
        private ConcurrentDictionary<(byte, byte, byte, byte), CachedBlockStateVariant> m_LavaModels = new();

        /// <summary>
        /// Returns a fluid block with the given level
        /// </summary>
        /// <param name="fluidType"></param>
        /// <param name="heights">Heights for each corner (NW, NE, SE, SW)</param>
        /// <returns></returns>
        public CachedBlockStateVariant GetFluidModel(ModelFluidType fluidType, (byte, byte, byte, byte) heights)
        {
            // Load from cache
            var cache = fluidType == ModelFluidType.Water ? m_WaterModels : m_LavaModels;
            if (cache.TryGetValue(heights, out var model))
            {
                return model;
            }
            
            var heightNorthWest = heights.Item1 / 8f;
            var heightNorthEast = heights.Item2 / 8f;
            var heightSouthEast = heights.Item3 / 8f;
            var heightSouthWest = heights.Item4 / 8f;
            var cull = heights.Item1 == 8 && heights.Item2 == 8 && heights.Item3 == 8 && heights.Item4 == 8;
            
            var textureStill = fluidType == ModelFluidType.Water ? World.World.TextureWaterStill : World.World.TextureLavaStill;
            var textureFlow = fluidType == ModelFluidType.Water ? World.World.TextureWaterFlow : World.World.TextureLavaFlow;
            var tintType = fluidType == ModelFluidType.Water ? ModelTintType.Water : ModelTintType.Default;
            var transparent = fluidType == ModelFluidType.Water;
            var faces = new CachedBlockStateFace[6];
            // Up
            faces[0] = new CachedBlockStateFace()
            {
                Direction = Direction.Up,
                CullFace = cull ? Direction.Up : null,
                Texture = textureStill,
                Transparent = transparent,
                FluidType = fluidType,
                TintType = tintType,
                Normal = Vector3.AxisY,
                VertexA = new Vector3() {X = 0f, Y = heightNorthWest, Z = 0f},
                VertexB = new Vector3() {X = 0f, Y = heightSouthWest, Z = 1f},
                VertexC = new Vector3() {X = 1f, Y = heightSouthEast, Z = 1f},
                VertexD = new Vector3() {X = 1f, Y = heightNorthEast, Z = 0f},
                UvA = new Vector2() {X = 0f, Y = 0f},
                UvB = new Vector2() {X = 0f, Y = 1f},
                UvC = new Vector2() {X = 1f, Y = 1f},
                UvD = new Vector2() {X = 1f, Y = 0f},
            };
            // Down
            faces[1] = new CachedBlockStateFace()
            {
                Direction = Direction.Down,
                CullFace = Direction.Down,
                Texture = textureStill,
                Transparent = transparent,
                FluidType = fluidType,
                TintType = tintType,
                Normal = -Vector3.AxisY,
                VertexA = new Vector3() {X = 0f, Y = 0f, Z = 1f},
                VertexB = new Vector3() {X = 0f, Y = 0f, Z = 0f},
                VertexC = new Vector3() {X = 1f, Y = 0f, Z = 0f},
                VertexD = new Vector3() {X = 1f, Y = 0f, Z = 1f},
                UvA = new Vector2() {X = 0f, Y = 1f},
                UvB = new Vector2() {X = 0f, Y = 0f},
                UvC = new Vector2() {X = 1f, Y = 0f},
                UvD = new Vector2() {X = 1f, Y = 1f},
            };
            // North
            faces[2] = new CachedBlockStateFace()
            {
                Direction = Direction.North,
                CullFace = Direction.North,
                Texture = textureFlow,
                Transparent = transparent,
                FluidType = fluidType,
                TintType = tintType,
                Normal = -Vector3.AxisZ,
                VertexA = new Vector3() {X = 1f, Y = heightNorthEast, Z = 0f},
                VertexB = new Vector3() {X = 1f, Y = 0f, Z = 0f},
                VertexC = new Vector3() {X = 0f, Y = 0f, Z = 0f},
                VertexD = new Vector3() {X = 0f, Y = heightNorthWest, Z = 0f},
                UvA = new Vector2() {X = 1f, Y = heightNorthEast},
                UvB = new Vector2() {X = 1f, Y = 0f},
                UvC = new Vector2() {X = 0f, Y = 0f},
                UvD = new Vector2() {X = 0f, Y = heightNorthWest},
            };
            // South
            faces[3] = new CachedBlockStateFace()
            {
                Direction = Direction.South,
                CullFace = Direction.South,
                Texture = textureFlow,
                Transparent = transparent,
                FluidType = fluidType,
                TintType = tintType,
                Normal = Vector3.AxisZ,
                VertexA = new Vector3() {X = 0f, Y = heightSouthWest, Z = 1f},
                VertexB = new Vector3() {X = 0f, Y = 0f, Z = 1f},
                VertexC = new Vector3() {X = 1f, Y = 0f, Z = 1f},
                VertexD = new Vector3() {X = 1f, Y = heightSouthEast, Z = 1f},
                UvA = new Vector2() {X = 0f, Y = heightSouthWest},
                UvB = new Vector2() {X = 0f, Y = 0f},
                UvC = new Vector2() {X = 1f, Y = 0f},
                UvD = new Vector2() {X = 1f, Y = heightSouthEast},
            };
            // West
            faces[4] = new CachedBlockStateFace()
            {
                Direction = Direction.West,
                CullFace = Direction.West,
                Texture = textureFlow,
                Transparent = transparent,
                FluidType = fluidType,
                TintType = tintType,
                Normal = -Vector3.AxisX,
                VertexA = new Vector3() {X = 0f, Y = heightNorthWest, Z = 0f},
                VertexB = new Vector3() {X = 0f, Y = 0f, Z = 0f},
                VertexC = new Vector3() {X = 0f, Y = 0f, Z = 1f},
                VertexD = new Vector3() {X = 0f, Y = heightSouthWest, Z = 1f},
                UvA = new Vector2() {X = 0f, Y = heightNorthWest},
                UvB = new Vector2() {X = 0f, Y = 0f},
                UvC = new Vector2() {X = 1f, Y = 0f},
                UvD = new Vector2() {X = 1f, Y = heightSouthWest},
            };
            // East
            faces[5] = new CachedBlockStateFace()
            {
                Direction = Direction.East,
                CullFace = Direction.East,
                Texture = textureFlow,
                Transparent = transparent,
                FluidType = fluidType,
                TintType = tintType,
                Normal = Vector3.AxisX,
                VertexA = new Vector3() {X = 1f, Y = heightSouthEast, Z = 1f},
                VertexB = new Vector3() {X = 1f, Y = 0f, Z = 1f},
                VertexC = new Vector3() {X = 1f, Y = 0f, Z = 0f},
                VertexD = new Vector3() {X = 1f, Y = heightNorthEast, Z = 0f},
                UvA = new Vector2() {X = 1f, Y = heightSouthEast},
                UvB = new Vector2() {X = 1f, Y = 0f},
                UvC = new Vector2() {X = 0f, Y = 0f},
                UvD = new Vector2() {X = 0f, Y = heightNorthEast},
            };
            
            model = new CachedBlockStateVariant()
            {
                Faces = faces
            };
            cache.TryAdd(heights, model);
            return model;
        }
        
        /// <summary>
        /// Gets the water model
        /// </summary>
        /// <param name="heights">Heights for each corner (NW, NE, SE, SW)</param>
        /// <returns></returns>
        public CachedBlockStateVariant GetWaterModel((byte, byte, byte, byte) heights)
        {
            return GetFluidModel(ModelFluidType.Water, heights);
        }

        /// <summary>
        /// Gets the laval model
        /// </summary>
        /// <param name="heights">Heights for each corner (NW, NE, SE, SW)</param>
        /// <returns></returns>
        public CachedBlockStateVariant GetLavaModel((byte, byte, byte, byte) heights)
        {
            return GetFluidModel(ModelFluidType.Lava, heights);
        }
    }
}