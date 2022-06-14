using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using MinecraftWebExporter.Utils;
using SharpNBT;

namespace MinecraftWebExporter.Minecraft.World
{
    /// <summary>
    /// A region is a collection of 32x32 <see cref="Chunk"/>s.
    /// </summary>
    public class Region : IAsyncDisposable
    {
        public Region(World world, int x, int z)
        {
            World = world;
            X = x;
            Z = z;
        }
        
        /// <summary>
        /// Gets the parent world
        /// </summary>
        public World World { get; }

        /// <summary>
        /// Gets the x position in the <see cref="World"/>
        /// </summary>
        public int X { get; }
        
        /// <summary>
        /// Gets the Z position in the <see cref="World"/>
        /// </summary>
        public int Z { get; }

        /// <summary>
        /// Gets if this region file was loaded
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// The internal file stream of the region file
        /// </summary>
        private Stream? m_Stream;

        /// <summary>
        /// The chunk offsets
        /// </summary>
        private RegionChunkOffset[]? m_ChunkOffsets;

        /// <summary>
        /// The chunk time stamps
        /// </summary>
        private int[]? m_ChunkTimestamps;
        
        /// <summary>
        /// Loads the region file
        /// </summary>
        /// <returns></returns>
        public async ValueTask<bool> LoadAsync()
        {
            if (IsLoaded)
            {
                return true;
            }
            
            var path = GetRegionFilePath();
            if (!File.Exists(path))
            {
                return true;
            }

            return await Task.Run(() => {
                const int chunkCount = 32 * 32;
                m_Stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                m_ChunkOffsets = new RegionChunkOffset[chunkCount];
                
                // Read the offset
                var reader = new BinaryReader(m_Stream);
                for (var i = 0; i < chunkCount; i++)
                {
                    var b0 = reader.ReadByte();
                    var b1 = reader.ReadByte();
                    var b2 = reader.ReadByte();
                    var b3 = reader.ReadByte();
                        
                    m_ChunkOffsets[i] = new RegionChunkOffset((b0 << 16) | (b1 << 8) | b2, b3);
                }

                // Read the timestamps
                m_ChunkTimestamps = new int[chunkCount];
                for (var i = 0; i < chunkCount; i++)
                {
                    var b0 = reader.ReadByte();
                    var b1 = reader.ReadByte();
                    var b2 = reader.ReadByte();
                    var b3 = reader.ReadByte();
                        
                    m_ChunkTimestamps[i] = (b0 << 24) | (b1 << 16) | (b2 << 8) | b3;
                }
                
                IsLoaded = true;
                return true;
            });
        }

        /// <summary>
        /// UnloadAsync the chunk
        /// </summary>
        public async ValueTask UnloadAsync()
        {
            IsLoaded = false;
            if (m_Stream != null)
            {
                await m_Stream.DisposeAsync();
                m_Stream = null;
            }
        }
        
        /// <summary>
        /// Returns the path to the region file
        /// </summary>
        /// <returns></returns>
        private string GetRegionFilePath()
        {
            return Path.Combine(World.Path, $"region/r.{X}.{Z}.mca");
        }
        
        /// <summary>
        /// Returns the chunk stream
        /// </summary>
        /// <param name="input"></param>
        /// <param name="compressionMode"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static Stream GetChunkDataStream(Stream input, byte compressionMode)
        {
            switch (compressionMode)
            {
                case 0:
                    return input;
                case 1:
                    return new DotNet6CompressionStreamFix(
                        new GZipStream(input, CompressionMode.Decompress, true));
                case 2:
                    return new DotNet6CompressionStreamFix(
                        new SharpNBT.ZLib.ZLibStream(input, CompressionMode.Decompress, true));
                default:
                    throw new ArgumentException("Invalid compression mode!", nameof(compressionMode));
            }
        }
        
        #region Chunks

        /// <summary>
        /// The list of all chunks
        /// </summary>
        private readonly ConcurrentDictionary<(byte, byte), Chunk> m_Chunks = new();

        /// <summary>
        /// Returns the last update timestamp
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <returns></returns>
        public int GetChunkTimestamp(byte chunkX, byte chunkZ)
        {
            if (m_ChunkTimestamps is null) return 0;
            return m_ChunkTimestamps[chunkX + chunkZ * 32];
        }

        #endregion Chunks

        /// <summary>
        /// Dispose this region
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async ValueTask DisposeAsync()
        {
            if (IsLoaded)
            {
                await UnloadAsync();
            }
        }

        /// <summary>
        /// Gets the chunk at the given location.
        /// Returns <c>null</c> if the chunk doesn't exists.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public async ValueTask<Chunk?> GetOrLoadChunkAsync(byte x, byte z)
        {
            if (m_Chunks.TryGetValue((x, z), out var chunk))
            {
                return chunk;
            }

            // The region isn't loaded anymore
            if (!IsLoaded)
            {
                return null;
            }

            var chunkOffset = GetChunkOffset(x, z);
            if (chunkOffset.Size == 0)
            {
                return null;
            }

            if (m_Stream is null) throw new InvalidOperationException("The region file stream is closed!");
            
            byte compressionMode;
            byte[] data;
            lock (m_Stream)
            {
                // Seeks to the chunk offset and reads the chunk header
                m_Stream.Seek((long) chunkOffset.Offset * 4096, SeekOrigin.Begin);
                var b0 = (byte) m_Stream.ReadByte();
                var b1 = (byte) m_Stream.ReadByte();
                var b2 = (byte) m_Stream.ReadByte();
                var b3 = (byte) m_Stream.ReadByte();
                var size = b0 << 24 | b1 << 16 | b2 << 8 | b3;
                compressionMode = (byte) m_Stream.ReadByte();

                // Create a temporary memory stream to decrypt the data 
                data = new byte[size - 1];
                var bytes = m_Stream.Read(data, 0, data.Length);
                if (bytes != data.Length) throw new IOException($"Chunk data is incomplete: Only {bytes} of {data.Length} could be read!");
            }

            await using var ms = new MemoryStream(data);
            
            var compressionStream = GetChunkDataStream(ms, compressionMode);
            
            // Reads the tag
            await using var tagReader = new TagReader(compressionStream, FormatOptions.Java);
            var tag = await tagReader.ReadTagAsync<CompoundTag>();
            
            // Creates and loads the chunk
            chunk = new Chunk(this, x, z);
            await chunk.LoadAsync(tag);
            m_Chunks.TryAdd((x, z), chunk);
            return chunk;
        }
        
        /// <summary>
        /// Returns the loaded chunk.
        /// Returns <c>null</c> if the chunk wasn't loaded yet.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public Chunk? GetChunk(byte x, byte z)
        {
            if (m_Chunks is null) throw new InvalidOperationException("The region file wasn't loaded yet!");
            
            if (m_Chunks.TryGetValue((x, z), out var chunk))
            {
                return chunk;
            }

            return null;
        }
        
        /// <summary>
        /// Returns the chunk info
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private RegionChunkOffset GetChunkOffset(byte x, byte z)
        {
            if (m_ChunkOffsets is null) throw new InvalidOperationException("The region file wasn't loaded yet!");
            
            return m_ChunkOffsets[x + z * 32];
        }
    }
}