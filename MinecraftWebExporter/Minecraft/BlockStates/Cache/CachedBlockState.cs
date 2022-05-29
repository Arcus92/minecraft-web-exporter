using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpNBT;

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
        public CachedBlockStateVariant[] Variants { get; init; }

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
        public CachedBlockStateVariant DefaultVariant => Variants[0];

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
        /// <param name="propertiesTag"></param>
        /// <returns></returns>
        public static async ValueTask<CachedBlockState> CreateAsync(AssetManager assetManager, string blockName, CompoundTag? propertiesTag)
        {
            var blockStateAsset = new AssetIdentifier(AssetType.BlockState, blockName);
            var blockState = await assetManager.BlockStateCache.GetAsync(blockStateAsset);
            if (blockState is null)
                return default;
            
            byte? waterLevel = default;
            byte? lavaLevel = default;
            
            // As far as I know water and lava blocks must be hard coded. I've checked if there is a block-tag.

            if (blockStateAsset.Name is "water")
            {
                if (propertiesTag?["level"] is StringTag levelTag)
                {
                    waterLevel = byte.Parse(levelTag.Value);
                }
            }

            if (blockStateAsset.Name is "lava")
            {
                if (propertiesTag?["level"] is StringTag levelTag)
                {
                    lavaLevel = byte.Parse(levelTag.Value);
                }
            }
            
            if (blockStateAsset.Name is "seagrass" or "tall_seagrass" or "kelp_plant" or "kelp" or "bubble_column")
            {
                waterLevel = 0;
            }
            
            // Handle water logged objects
            if ((propertiesTag?["waterlogged"] as StringTag)?.Value == "true")
            {
                waterLevel = 0;
            }
            
            var variants = new List<CachedBlockStateVariant>();
            await blockState.BuildCachedFacesAsync(blockStateAsset, variants, assetManager, propertiesTag);
            
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
        public static async ValueTask<CachedBlockState> CreateAsync(AssetManager assetManager, byte blockId, byte blockData)
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
        /// <param name="propertiesTag"></param>
        private static void ConvertBlockIdToBlockName(byte blockId, byte blockData, out string blockName,
            out CompoundTag? propertiesTag)
        {
            propertiesTag = null;
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
                    propertiesTag = new CompoundTag("data");
                    propertiesTag.Add(new StringTag("snowy", "false"));
                    break;
                case "minecraft:water" or "minecraft:lava":
                    propertiesTag = new CompoundTag("data");
                    propertiesTag.Add(new StringTag("level", "0"));
                    break;
                case "minecraft:flowing_water" or "minecraft:flowing_lava":
                    propertiesTag = new CompoundTag("data");
                    propertiesTag.Add(new StringTag("level", blockData.ToString()));
                    break;
                case "minecraft:log" or "minecraft:log2":
                    propertiesTag = new CompoundTag("data");
                    
                    GetBlockNameFromSubtype(ref blockName, blockData & 0b0011);
                    switch (blockData >> 2)
                    {
                        case 0:
                            propertiesTag.Add(new StringTag("axis", "y"));
                            break;
                        case 1:
                            propertiesTag.Add(new StringTag("axis", "x"));
                            break;
                        case 2:
                            propertiesTag.Add(new StringTag("axis", "z"));
                            break;
                    }
                    break;
                case "minecraft:leaves" or "minecraft:leaves2" or "minecraft:sapling":
                    GetBlockNameFromSubtype(ref blockName, blockData & 0b0111);
                    break;
                case "minecraft:double_plant" or "minecraft:tallgrass":
                    GetBlockNameFromSubtype(ref blockName, blockData & 0b0111);
                    var upper = (blockData & 0b1000) != 0;
                    propertiesTag = new CompoundTag("data");
                    propertiesTag.Add(new StringTag("half", upper ? "upper" : "lower"));
                    break;
                
                default:
                    GetBlockNameFromSubtype(ref blockName, blockData);
                    break;
            }
        }

        #endregion Static
    }
}