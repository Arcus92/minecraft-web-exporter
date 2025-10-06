using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MinecraftWebExporter.Minecraft.BlockEntities;

namespace MinecraftWebExporter.Minecraft.BlockStates.Cache
{
    /// <summary>
    /// The cached block information
    /// </summary>
    public readonly struct CachedBlockState
    {
        /// <summary>
        /// Gets and sets all variants for this block state
        /// </summary>
        public CachedBlockStateVariant[]? Variants { get; init; }

        /// <summary>
        /// Gets the water level (0 = full - 7 = lowest, 8 = falling)
        /// </summary>
        public byte? WaterLevel { get; init; }
        
        /// <summary>
        /// Gets the lava level (0 = full - 7 = lowest, 8 = falling)
        /// </summary>
        public byte? LavaLevel { get; init; }
        
        /// <summary>
        /// Gets the total sum of weights for all <see cref="Variants"/>
        /// </summary>
        public float Weights { get; init; }

        /// <summary>
        /// Gets the default variant
        /// </summary>
        public CachedBlockStateVariant DefaultVariant => Variants?[0] ?? default;

        /// <summary>
        /// The random number generator
        /// </summary>
        [ThreadStatic] private static Random? _random;
        
        /// <summary>
        /// Returns a random block variant
        /// </summary>
        /// <returns></returns>
        public CachedBlockStateVariant GetRandomVariant()
        {
            if (Variants is null)
                return default;
            
            _random ??= new Random();
            var random = (float)_random.NextDouble() * Weights;

            foreach (var variant in Variants)
            {
                random -= variant.Weight;

                if (random <= 0f)
                    return variant;
            }

            return default;
        }

        #region Static

        /// <summary>
        /// Calculate all faces
        /// </summary>
        /// <param name="assetManager"></param>
        /// <param name="blockName"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static async ValueTask<CachedBlockState> CreateAsync(IAssetManager assetManager, string blockName, IBlockStateProperties? properties)
        {
            var blockStateAsset = new AssetIdentifier(AssetType.BlockState, blockName);
            var blockState = await assetManager.GetBlockStateAsync(blockStateAsset);
            if (blockState is null)
                return default;

            string? levelText;
            byte? waterLevel = null;
            byte? lavaLevel = null;
            
            // As far as I know water and lava blocks must be hard coded. I've checked if there is a block-tag.

            if (blockStateAsset.Name is "water")
            {
                if ((levelText = properties?.GetValueOrDefault("level")) is not null)
                {
                    waterLevel = byte.Parse(levelText);
                }
            }

            if (blockStateAsset.Name is "lava")
            {
                if ((levelText = properties?.GetValueOrDefault("level")) is not null)
                {
                    lavaLevel = byte.Parse(levelText);
                }
            }
            
            if (blockStateAsset.Name is "seagrass" or "tall_seagrass" or "kelp_plant" or "kelp" or "bubble_column")
            {
                waterLevel = 0;
            }
            
            // Handle water logged objects
            if (properties?.GetValueOrDefault("waterlogged") == "true")
            {
                waterLevel = 0;
            }
            
            // Check if this is an entity block and use the build-in renderer instead
            if (BlockEntityRenderer.Map.TryGetValue(blockStateAsset, out var renderer))
            {
                var faces = new List<CachedBlockStateFace>();
                renderer.Build(faces, properties);
                return new CachedBlockState()
                {
                    Variants = new[]
                    {
                        new CachedBlockStateVariant()
                        {
                            Faces = faces.ToArray(),
                        }
                    },
                    WaterLevel = waterLevel,
                    LavaLevel = lavaLevel,
                };
            }
            
            var variants = new List<CachedBlockStateVariant>();
            await blockState.BuildCachedFacesAsync(blockStateAsset, variants, assetManager, properties);
            
            return new CachedBlockState()
            {
                Variants = variants.ToArray(),
                WaterLevel = waterLevel,
                LavaLevel = lavaLevel,
                Weights = variants.Sum(v => v.Weight),
            };
        }
        
        /// <summary>
        /// Calculate all faces
        /// </summary>
        /// <param name="assetManager"></param>
        /// <param name="blockId"></param>
        /// <param name="blockData"></param>
        /// <returns></returns>
        public static async ValueTask<CachedBlockState> CreateAsync(IAssetManager assetManager, byte blockId, byte blockData)
        {
            ConvertBlockIdToBlockName(blockId, blockData, out var blockName, out var propertiesTag);
            var block = await CreateAsync(assetManager, blockName, propertiesTag);

            if (block.Variants is not null && block.Variants.Length == 0)
            {
                // throw new ArgumentException();
            }
            
            return block;
        }

        /// <summary>
        /// Converts the legacy (pre 1.12) block id / data to a modern block name with properties.
        /// https://minecraft.fandom.com/wiki/Java_Edition_data_values/Pre-flattening
        /// </summary>
        /// <param name="blockId"></param>
        /// <param name="blockData"></param>
        /// <param name="blockName"></param>
        /// <param name="properties"></param>
        private static void ConvertBlockIdToBlockName(byte blockId, byte blockData, out string blockName,
            out IBlockStateProperties? properties)
        {
            properties = null;
            blockName = BlockIdMap.LegacyBlockIdToBlockName[blockId];

            // Converts the legacy block name with subtype to the modern block name
            void GetBlockNameFromSubtype(ref string name, int data)
            {
                if (BlockIdMap.LegacyBlockSubtypeToBlockName.TryGetValue($"{name}.{data}", out var blockDataName))
                {
                    name = blockDataName;
                }
            }

            // Convert block data to propertyTag
            switch (blockName)
            {
                case "minecraft:grass":
                    GetBlockNameFromSubtype(ref blockName, blockData);
                    properties = IBlockStateProperties.Create(new Dictionary<string, string>()
                    {
                        {
                            "snowy", "false"
                        }
                    });
                    break;
                case "minecraft:water" or "minecraft:lava":
                    properties = IBlockStateProperties.Create(new Dictionary<string, string>()
                    {
                        {
                            "level", "0"
                        }
                    });
                    break;
                case "minecraft:flowing_water" or "minecraft:flowing_lava":
                    properties = IBlockStateProperties.Create(new Dictionary<string, string>()
                    {
                        {
                            "level", blockData.ToString()
                        }
                    });
                    break;
                case "minecraft:log" or "minecraft:log2":
                    GetBlockNameFromSubtype(ref blockName, blockData & 0b0011);
                    var axis = (blockData >> 2) switch
                    {
                        0 => "y",
                        1 => "x",
                        2 => "z",
                        _ => throw new InvalidDataException("Unknown axis!")
                    };

                    properties = IBlockStateProperties.Create(new Dictionary<string, string>()
                    {
                        {
                            "axis", axis
                        }
                    });
                    break;
                case "minecraft:leaves" or "minecraft:leaves2" or "minecraft:sapling":
                    GetBlockNameFromSubtype(ref blockName, blockData & 0b0111);
                    break;
                case "minecraft:double_plant" or "minecraft:tallgrass":
                    GetBlockNameFromSubtype(ref blockName, blockData & 0b0111);
                    var upper = (blockData & 0b1000) != 0;
                    properties = IBlockStateProperties.Create(new Dictionary<string, string>()
                    {
                        {
                            "half", upper ? "upper" : "lower"
                        }
                    });
                    break;
                
                default:
                    GetBlockNameFromSubtype(ref blockName, blockData);
                    break;
            }
        }

        #endregion Static
    }
}