using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinecraftWebExporter.Minecraft.BlockStates.Cache;
using SharpNBT;

namespace MinecraftWebExporter.Minecraft.World
{
    /// <summary>
    /// A chunk is the column of multiple 
    /// </summary>
    public class Chunk
    {
        public Chunk(Region region, byte x, byte z)
        {
            Region = region;
            X = x;
            Z = z;
        }

        /// <summary>
        /// Gets the data version
        /// </summary>
        public int DataVersion { get; private set; }

        /// <summary>
        /// Gets the parent region
        /// </summary>
        public Region Region { get; }

        /// <summary>
        /// Gets the x position in the <see cref="Region"/>
        /// </summary>
        public byte X { get; }

        /// <summary>
        /// Gets the Z position in the <see cref="Region"/>
        /// </summary>
        public byte Z { get; }
        
        /// <summary>
        /// Gets the tick of the last update
        /// </summary>
        public long LastUpdate { get; private set; }

        /// <summary>
        /// The height maps
        /// </summary>
        private readonly Dictionary<HeightmapType, Heightmap> m_Heightmaps = new();

        /// <summary>
        /// Returns the height map by the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="heightmap"></param>
        /// <returns></returns>
        public bool TryGetHeightmap(HeightmapType type, out Heightmap heightmap)
        {
            return m_Heightmaps.TryGetValue(type, out heightmap);
        }

        /// <summary>
        /// Returns the height map by the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Heightmap? GetHeightmap(HeightmapType type)
        {
            if (m_Heightmaps.TryGetValue(type, out var heightmap))
            {
                return heightmap;
            }

            return null;
        }

        /// <summary>
        /// Loads the chunk data
        /// </summary>
        /// <param name="tag"></param>
        public async Task LoadAsync(CompoundTag tag)
        {
            // Read the data version
            if (!tag.TryGetValue<IntTag>("DataVersion", out var dataVersionTag))
            {
                throw new ArgumentException("Chunk file does not contain 'DataVersion' tag.", nameof(tag));
            }
            
            DataVersion = dataVersionTag.Value;
            
            
            CompoundTag? levelTag;

            // Minecraft 1.18
            if (DataVersion >= 2825)
            {
                levelTag = tag;
            }
            else
            {
                // Read the level
                levelTag = tag.Get<CompoundTag>("Level");
                if (levelTag is null)
                {
                    throw new ArgumentException("Chunk file does not contain 'Level' tag.", nameof(tag));
                }
            }
            
            // Sets the last update
            if (levelTag.TryGetValue<LongTag>("LastUpdate", out var lastUpdateTag))
            {
                LastUpdate = lastUpdateTag.Value;
            }

            // Read the height maps
            if (levelTag.TryGetValue<CompoundTag>("Heightmaps", out var heightmapsTag))
            {
                foreach (var t in heightmapsTag)
                {
                    if (t.Name is null) continue;
                    
                    var heightmapType = Heightmap.GetHeightmapType(t.Name);
                    var heightmapTag = (LongArrayTag)t;

                    var heights = ChunkHelper.ReadLongArray(DataVersion, 9, 16 * 16, heightmapTag);
                    m_Heightmaps[heightmapType] = new Heightmap(this, heights);
                }
            }
            
            // Read the legacy height map
            if (levelTag.TryGetValue<IntArrayTag>("HeightMap", out var legacyHeightmapTag))
            {
                var heights = legacyHeightmapTag.Select(h => (ushort) h).ToArray();
                m_Heightmaps[HeightmapType.MotionBlocking] = new Heightmap(this, heights);
                m_Heightmaps[HeightmapType.MotionBlockingNoLeaves] = new Heightmap(this, heights);
                m_Heightmaps[HeightmapType.OceanFloor] = new Heightmap(this, heights);
                m_Heightmaps[HeightmapType.OceanFloorWorldGen] = new Heightmap(this, heights);
            }
            
            // Read the sections
            if (!levelTag.TryGetValue<ListTag>("sections", out var sectionsTag) && 
                !levelTag.TryGetValue<ListTag>("Sections", out sectionsTag))
            {
                throw new ArgumentException("Chunk file does not contain 'Sections' tag.", nameof(tag));
            }

            // Read all sections
            foreach (var sectionTagRaw in sectionsTag)
            {
                var sectionTag = (CompoundTag) sectionTagRaw;
                var yTag = sectionTag["Y"] as ByteTag;
                if (yTag is null)
                {
                    continue;
                }

                var y = yTag.SignedValue;
                var section = new Section(this, y);
                await section.LoadAsync(DataVersion, sectionTag);
                m_Sections.TryAdd(y, section);
            }
        }

        #region Sections

        /// <summary>
        /// The list of all sections
        /// </summary>
        private readonly ConcurrentDictionary<sbyte, Section> m_Sections = new();
        
        /// <summary>
        /// Returns the section with the <paramref name="y"/> coordinate.
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public Section? GetSection(sbyte y)
        {
            return m_Sections.GetValueOrDefault(y);
        }

        /// <summary>
        /// Gets a block at this given chunk position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public CachedBlockState GetBlock(int x, int y, int z)
        {
            var blockX = (byte)(x & 0x0F);
            var blockY = (byte)(y & 0x0F);
            var blockZ = (byte)(z & 0x0F);
            var chunkY = (sbyte)((y - blockY) / 16);
            var section = GetSection(chunkY);
            if (section is null)
            {
                return default;
            }
            
            return section.GetBlock(blockX, blockY, blockZ);
        }
        
        #endregion Sections
    }
}