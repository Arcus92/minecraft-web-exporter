using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                return default;
            
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
            if (sectionTag["BlockLight"] is ByteArrayTag blockLightTag)
            {
                m_BlockLights = new byte[blockLightTag.Count];
                for (var i = 0; i < m_BlockLights.Length; i++)
                {
                    m_BlockLights[i] = blockLightTag[i];
                }
            }
            
            // Loads the sky lights
            if (sectionTag["SkyLight"] is ByteArrayTag skyLightTag)
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
                var dataTag = sectionTag["block_states"] as CompoundTag;
                if (dataTag is null)
                {
                    return;
                }
                paletteTag = dataTag["palette"] as ListTag;
                blockStatesTag = dataTag["data"] as LongArrayTag;
            }
            else
            {
                paletteTag = sectionTag["Palette"] as ListTag;
                blockStatesTag = sectionTag["BlockStates"] as LongArrayTag;
            }
            
            // Uses legacy block tag
            // TODO: Find the exact data version 
            if (sectionTag["Blocks"] is ByteArrayTag blocksTag)
            {
                var dataTag = sectionTag["Data"] as ByteArrayTag;
                Debug.Assert(dataTag != null, nameof(dataTag) + " != null");
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
            if (sectionTag["biomes"] is CompoundTag biomesTag)
            {
                if (biomesTag["palette"] is ListTag biomesPaletteTag)
                {
                    m_BiomePalette = new string[biomesPaletteTag.Count];
                    for (var i = 0; i < m_BiomePalette.Length; i++)
                    {
                        var biomeTag = biomesPaletteTag[i] as StringTag;
                        m_BiomePalette[i] = biomeTag?.Value;
                    }
                }

                if (biomesTag["data"] is LongArrayTag biomesDataTag && m_BiomePalette is not null)
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
            var blockNameTag = blockData["Name"] as StringTag;
            if (blockNameTag is null)
                return default;
            var blockPropertiesTag = blockData["Properties"] as CompoundTag;
            var blockName = blockNameTag.Value;
            var properties = IBlockStateProperties.Create(blockPropertiesTag);
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