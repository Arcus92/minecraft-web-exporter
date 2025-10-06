using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MinecraftWebExporter.Minecraft.BlockStates;
using MinecraftWebExporter.Minecraft.BlockStates.Cache;
using SharpNBT;

namespace MinecraftWebExporter.Minecraft.World
{
    /// <summary>
    /// A section is a rwo in <see cref="Chunk"/>. It contains 16x16x16 blocks.
    /// </summary>
    public class Section
    {
        public Section(Chunk chunk, sbyte y)
        {
            Chunk = chunk;
            Y = y;
        }
        
        /// <summary>
        /// Gets the parent chunk
        /// </summary>
        public Chunk Chunk { get; }

        /// <summary>
        /// Gets the y position in the <see cref="Chunk"/>
        /// </summary>
        public sbyte Y { get; }

        /// <summary>
        /// The loaded palette
        /// </summary>
        private CachedBlockState[]? m_BlockPalette;

        /// <summary>
        /// The block index
        /// </summary>
        private ushort[]? m_BlockStates;

        /// <summary>
        /// The block lights
        /// </summary>
        private byte[]? m_BlockLights;
        
        /// <summary>
        /// The sky lights
        /// </summary>
        private byte[]? m_SkyLights;
        
        /// <summary>
        /// Returns the block index in the block state
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static int GetBlockIndex(byte x, byte y, byte z)
        {
            return (y << 8) + (z << 4) + x;
        }
        
        /// <summary>
        /// Returns the block at the given position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public CachedBlockState GetBlock(byte x, byte y, byte z)
        {
            if (m_BlockPalette is null || m_BlockStates is null)
            {
                return default;
            }

            var index = m_BlockStates[GetBlockIndex(x, y, z)];
            return m_BlockPalette[index];
        }

        /// <summary>
        /// Returns the block light
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public byte GetBlockLight(byte x, byte y, byte z)
        {
            if (m_BlockLights is null)
                return 0;
            var i = GetBlockIndex(x, y, z);
            var b = m_BlockLights[i / 2];

            if (i % 2 == 0)
            {
                return (byte) (b & 0xF);
            }
            else
            {
                return (byte) ((b >> 4) & 0xF);
            }
        }
        
        /// <summary>
        /// Returns the sky light
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public byte GetSkyLight(byte x, byte y, byte z)
        {
            if (m_SkyLights is null)
                return (byte) (Y < 4 ? 0 : 15);
            var i = GetBlockIndex(x, y, z);
            var b = m_SkyLights[i / 2];

            if (i % 2 == 0)
            {
                return (byte) (b & 0xF);
            }
            else
            {
                return (byte) ((b >> 4) & 0xF);
            }
        }
        
        /// <summary>
        /// The biome palette
        /// </summary>
        private string?[]? m_BiomePalette;
        
        /// <summary>
        /// The biome index
        /// </summary>
        private ushort[]? m_Biomes;

        /// <summary>
        /// Gets the biome of the given block
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public string? GetBiome(byte x, byte z)
        {
            // Biomes are only defined per 4x4 area
            x = (byte) (x / 4);
            z = (byte) (z / 4);

            if (m_BiomePalette is null)
                return null;
            
            if (m_Biomes is null)
                return m_BiomePalette[0];

            return m_BiomePalette[m_Biomes[(z << 2) + x]];
        }

        /// <summary>
        /// Gets the number of block in a chunk section
        /// </summary>
        const int BlockCount = 16 * 16 * 16;
        
        /// <summary>
        /// Loads this section
        /// </summary>
        /// <param name="dataVersion"></param>
        /// <param name="sectionTag"></param>
        public async ValueTask LoadAsync(int dataVersion, CompoundTag sectionTag)
        {
            ListTag? paletteTag;
            LongArrayTag? blockStatesTag;
            
            // Loads the block lights
            if (sectionTag.TryGetValue<ByteArrayTag>("BlockLight", out var blockLightTag))
            {
                m_BlockLights = new byte[blockLightTag.Count];
                for (var i = 0; i < m_BlockLights.Length; i++)
                {
                    m_BlockLights[i] = blockLightTag[i];
                }
            }
            
            // Loads the skylights
            if (sectionTag.TryGetValue<ByteArrayTag>("SkyLight", out var skyLightTag))
            {
                m_SkyLights = new byte[skyLightTag.Count];
                for (var i = 0; i < m_SkyLights.Length; i++)
                {
                    m_SkyLights[i] = skyLightTag[i];
                }
            }
            
            // Minecraft 1.18
            if (dataVersion >= 2825)
            {
                if (!sectionTag.TryGetValue<CompoundTag>("block_states", out var dataTag))
                {
                    return;
                }
                dataTag.TryGetValue<ListTag>("palette", out paletteTag);
                dataTag.TryGetValue<LongArrayTag>("data", out blockStatesTag);
            }
            else
            {
                sectionTag.TryGetValue<ListTag>("Palette", out paletteTag);
                sectionTag.TryGetValue<LongArrayTag>("BlockStates", out blockStatesTag);
            }
            
            // Uses legacy block tag
            // TODO: Find the exact data version 
            if (sectionTag.TryGetValue<ByteArrayTag>("Blocks", out var blocksTag))
            {
                var dataTag = sectionTag.Get<ByteArrayTag>("Data");
                var idToPositionInPalette = new Dictionary<(byte, byte), ushort>();
                var blockPalette = new List<CachedBlockState>();
                m_BlockStates = new ushort[BlockCount];

                for (var i = 0; i < blocksTag.Count; i++)
                {
                    var blockId = blocksTag[i];
                    
                    var blockData = dataTag[i / 2];
                    if (blockData % 2 != 0)
                    {
                        blockData = (byte) (blockData & 0xF);
                    }
                    else
                    {
                        blockData = (byte) ((blockData >> 4) & 0xF);
                    }

                    if (!idToPositionInPalette.TryGetValue((blockId, blockData), out var positionInPalette))
                    {
                        var blockState =  await GetBlockAsync(blockId, blockData);
                        positionInPalette = (ushort) blockPalette.Count;
                        blockPalette.Add(blockState);
                        idToPositionInPalette.Add((blockId, blockData), positionInPalette);
                    }

                    m_BlockStates[i] = positionInPalette;
                }
                
                m_BlockPalette = blockPalette.ToArray();
                return;
            }
            
            
            if (paletteTag is null || blockStatesTag is null)
            {
                return;
            }
            
            // Create the palette with cached block
            m_BlockPalette = new CachedBlockState[paletteTag.Count];
            for (var p = 0; p < m_BlockPalette.Length; p++)
            {
                if (paletteTag[p] is CompoundTag blockData)
                {
                    m_BlockPalette[p] = await GetBlockAsync(blockData);
                }
            }
            
            var bits = (int) Math.Ceiling(Math.Log2(m_BlockPalette.Length));
            if (bits < 4) bits = 4;
            m_BlockStates = ChunkHelper.ReadLongArray(dataVersion, bits, BlockCount, blockStatesTag);

            // Loads the biomes
            if (sectionTag.TryGetValue<CompoundTag>("biomes", out var biomesTag))
            {
                if (biomesTag.TryGetValue<ListTag>("palette", out var biomesPaletteTag))
                {
                    m_BiomePalette = new string[biomesPaletteTag.Count];
                    for (var i = 0; i < m_BiomePalette.Length; i++)
                    {
                        var biomeTag = biomesPaletteTag[i] as StringTag;
                        m_BiomePalette[i] = biomeTag?.Value;
                    }
                }

                if (biomesTag.TryGetValue<LongArrayTag>("data", out var biomesDataTag) && m_BiomePalette is not null)
                {
                    bits = (int) Math.Ceiling(Math.Log2(m_BiomePalette.Length));
                    m_Biomes = ChunkHelper.ReadLongArray(dataVersion, bits, 64, biomesDataTag);
                }
            }
            
            
        }
        
        /// <summary>
        /// Gets the block model.
        /// </summary>
        /// <param name="blockData"></param>
        /// <returns></returns>
        private async ValueTask<CachedBlockState> GetBlockAsync(CompoundTag blockData)
        {
            if (!blockData.TryGetValue<StringTag>("Name", out var blockNameTag))
                return default;
            
            blockData.TryGetValue<CompoundTag>("Properties", out var blockPropertiesTag);
            var properties = IBlockStateProperties.Create(blockPropertiesTag);
            
            var blockName = blockNameTag.Value;
            return await GetBlockAsync(blockName, properties);
        }
        
        /// <summary>
        /// Gets the block model.
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        private async ValueTask<CachedBlockState> GetBlockAsync(string blockName, IBlockStateProperties? properties = null)
        {
            return await CachedBlockState.CreateAsync(Chunk.Region.World.Assets, blockName, properties);
        }
        
        /// <summary>
        /// Gets the block model.
        /// </summary>
        /// <param name="blockId"></param>
        /// <param name="blockData"></param>
        /// <returns></returns>
        private async ValueTask<CachedBlockState> GetBlockAsync(byte blockId, byte blockData)
        {
            return await CachedBlockState.CreateAsync(Chunk.Region.World.Assets, blockId, blockData);
        }
    }
}