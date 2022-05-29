using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MinecraftWebExporter.Utils;
using MinecraftWebExporter.Minecraft;
using MinecraftWebExporter.Minecraft.BlockStates;
using MinecraftWebExporter.Minecraft.BlockStates.Cache;
using MinecraftWebExporter.Minecraft.Models;
using MinecraftWebExporter.Minecraft.Textures;
using MinecraftWebExporter.Minecraft.World;
using MinecraftWebExporter.Structs;
using Region = MinecraftWebExporter.Minecraft.World.Region;

namespace MinecraftWebExporter.Export
{
    /// <summary>
    /// The exporter class
    /// </summary>
    public class MapExporter
    {
        /// <summary>
        /// Gets and sets the output path
        /// </summary>
        public string? Output { get; set; }

        /// <summary>
        /// Get and sets the minecraft asset manager
        /// </summary>
        public AssetManager Assets { get; }
        
        /// <summary>
        /// Gets and sets the number of threads
        /// </summary>
        public int NumberOfThreads { get; set; } = 16;

        /// <summary>
        /// Gets and sets the minimum world coordinate to export.
        /// Blocks below this point will not be exported.
        /// </summary>
        public Vector3? WorldBorderMin { get; set; }

        /// <summary>
        /// Gets and sets the maximum world coordinate to export.
        /// Blocks above this point will not be exported.
        /// </summary>
        public Vector3? WorldBorderMax { get; set; }

        /// <summary>
        /// Gets and sets the home position of the world. This is where the viewer camera spawns.
        /// </summary>
        public Vector3? WorldHome { get; set; }

        /// <summary>
        /// Gets and sets if the world border defined through <see cref="WorldBorderMin"/> and <see cref="WorldBorderMax"/>
        /// is culled out. If set to <c>false</c> all edge faces at the border will be drawn.
        /// </summary>
        public bool WorldBorderCulled { get; set; } = true;

        /// <summary>
        /// Gets and sets if the underground is culled out
        /// </summary>
        public bool UndergroundCulling { get; set; } = true;

        /// <summary>
        /// Gets and sets the underground culling height
        /// </summary>
        public int UndergroundCullingHeight { get; set; } = 64;

        /// <summary>
        /// Creates the map exporter using the given assets
        /// </summary>
        /// <param name="assets"></param>
        public MapExporter(AssetManager assets)
        {
            Assets = assets;
            m_World = new World(assets);
        }
        
        /// <summary>
        /// The current exported world
        /// </summary>
        private World m_World;

        private static readonly string[] MaterialPrefixes = new[] {"block", "entity", "environment"};
        
        /// <summary>
        /// Exports the materials to <see cref="Output"/>
        /// </summary>
        public async Task ExportMaterials()
        {
            if (Output is null) throw new ArgumentNullException(nameof(Output));
            
            // Writes the materials
            Console.WriteLine("Exporting materials...");
            foreach (var texturePrefix in MaterialPrefixes)
            {
                var mtlFileName = texturePrefix + ".mats";
                var mtlFilePath = Path.Combine(Output, mtlFileName);

                Console.WriteLine($"Exporting {mtlFileName}...");
                await ExportMaterialsAsync(mtlFilePath, texturePrefix + "/");
            }
        }

        /// <summary>
        /// Exports the world for <paramref name="world"/> to <see cref="Output"/>.
        /// </summary>
        /// <param name="exportSettings"></param>
        /// <param name="world"></param>
        /// <param name="worldName"></param>
        public async Task ExportWorld(ExportSettings exportSettings, World world, string worldName)
        {
            if (Output is null) throw new ArgumentNullException(nameof(Output));
            
            m_World = world;
            
            // Create the output directory
            var worldPath = Path.Combine(Output, worldName);
            Directory.CreateDirectory(worldPath);
            
            Console.WriteLine("Exporting world info.json...");
            
            var worldInfo = new WorldInfo();
            worldInfo.Views = exportSettings.Views;
            worldInfo.Materials = MaterialPrefixes;
            if (WorldHome.HasValue)
            {
                worldInfo.Home = WorldHome.Value;
            }

            var worldInfoPath = Path.Combine(Output, worldName, "info.json");
            await WorldInfo.SaveAsync(worldInfoPath, worldInfo);
            
            
            Console.WriteLine("Exporting regions...");
            var regions = m_World.GetRegions();

            var centerX = 0;
            var centerZ = 0;
            
            // Handle min world border
            if (WorldBorderMin.HasValue)
            {
                var regionMinX = (int)MathF.Floor(WorldBorderMin.Value.X / 32 / 16);
                var regionMinZ = (int)MathF.Floor(WorldBorderMin.Value.Z / 32 / 16);
                regions = regions.Where(r => r.X >= regionMinX && r.Z >= regionMinZ);
            }
            
            // Handle max world border
            if (WorldBorderMax.HasValue)
            {
                var regionMaxX = (int)MathF.Floor(WorldBorderMax.Value.X / 32 / 16);
                var regionMaxZ = (int)MathF.Floor(WorldBorderMax.Value.Z / 32 / 16);
                regions = regions.Where(r => r.X <= regionMaxX && r.Z <= regionMaxZ);
            }
            
            
            var regionList = regions.OrderBy(r => Math.Abs(r.X - centerX) + Math.Abs(r.Z - centerZ)).ToList();
            await regionList.ParallelForEachAsync(async (region) =>
            {
                await region.LoadAsync();
                
                var regionName = $"r.{region.X}.{region.Z}";
                var regionPath = Path.Combine(Output, worldName, regionName);
                
                Directory.CreateDirectory(regionPath);
                
                var regionInfoPath = Path.Combine(regionPath, "info.json");
                var regionInfo = await RegionInfo.LoadAsync(regionInfoPath);
                
                Console.WriteLine($"Exporting region '{regionName}'...");

                // Count the updates, so we don't write the region info file for each chunk.
                var updates = 0;
                foreach (var view in exportSettings.Views)
                {
                    var chunks = view.ChunksInRegion;
                    for (byte chunkX = 0; chunkX < chunks; chunkX++)
                    for (byte chunkZ = 0; chunkZ < chunks; chunkZ++)
                    {
                        // Checks the chunk timestamp
                        var timestamp = GetChunkTimestamp(view, region, chunkX, chunkZ);
                        if (regionInfo.GetChunkTimestamp(view, chunkX, chunkZ) >= timestamp)
                            continue;

                        var chunkMeshPath = Path.Combine(regionPath, $"{view.Filename}.{chunkX}.{chunkZ}.m");

                        // Exports the chunk and all blocks
                        await ExportChunkToFile(view, chunkMeshPath, region, chunkX, chunkZ);

                        // Sets the update timestamp
                        regionInfo.SetChunkTimestamp(view, chunkX, chunkZ, timestamp);

                        updates++;
                        if (updates > 16)
                        {
                            updates = 0;
                            await RegionInfo.SaveAsync(regionInfoPath, regionInfo);
                        }
                    }

                    // One final write
                    if (updates > 0)
                    {
                        await RegionInfo.SaveAsync(regionInfoPath, regionInfo);
                    }
                }
            }, NumberOfThreads);
        }

        /// <summary>
        /// Gets the timestamp from the given chunk
        /// </summary>
        /// <param name="view"></param>
        /// <param name="region"></param>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <returns></returns>
        private long GetChunkTimestamp(ExportDetailLevel view, Minecraft.World.Region region, byte chunkX, byte chunkZ)
        {
            var timestamp = 0;

            var chunks = view.ChunkSpan;
            for (byte x = 0; x < chunks; x++)
            for (byte z = 0; z < chunks; z++)
            {
                var t = region.GetChunkTimestamp((byte) (chunkX * chunks + x), (byte) (chunkZ * chunks + z));
                if (t > timestamp)
                    timestamp = t;
            }
            
            return timestamp;
        }
        
        #region Export
        
        /// <summary>
        /// Exports the given chunk position to <paramref name="path"/>.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="path"></param>
        /// <param name="region"></param>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        private async Task ExportChunkToFile(ExportDetailLevel view, string path, Minecraft.World.Region region, byte chunkX, byte chunkZ)
        {
            // Defines the export region
            var blocksInChunk =  16 / view.BlockSpan * view.ChunkSpan;
            var chunksInRegion =  32 / view.ChunkSpan;
            var minX = region.X * chunksInRegion * blocksInChunk + chunkX * blocksInChunk;
            var minY = -64 / view.BlockSpan;
            var minZ = region.Z * chunksInRegion * blocksInChunk + chunkZ * blocksInChunk;
            var maxX = minX + blocksInChunk;
            var maxY = 255 / view.BlockSpan;
            var maxZ = minZ + blocksInChunk;
            var originX = minX;
            var originZ = minZ;

            if (WorldBorderMin.HasValue)
            {
                var min = WorldBorderMin.Value / view.BlockSpan;
                if (min.X > minX) minX = (int)min.X;
                if (min.Y > minY) minY = (int)min.Y;
                if (min.Z > minZ) minZ = (int)min.Z;
            }
            
            if (WorldBorderMax.HasValue)
            {
                var max = WorldBorderMax.Value / view.BlockSpan;
                if (max.X < maxX) maxX = (int)max.X;
                if (max.Y < maxY) maxY = (int)max.Y;
                if (max.Z < maxZ) maxZ = (int)max.Z;
            }

            // Writes the file
            await ExportToFile(view, path, originX, originZ, minX, minY, minZ, maxX, maxY, maxZ);
        }

        /// <summary>
        /// Writes all blocks in the given area into <paramref name="path"/>.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="path"></param>
        /// <param name="originX"></param>
        /// <param name="originZ"></param>
        /// <param name="minX"></param>
        /// <param name="minY"></param>
        /// <param name="minZ"></param>
        /// <param name="maxX"></param>
        /// <param name="maxY"></param>
        /// <param name="maxZ"></param>
        private async Task ExportToFile(ExportDetailLevel view, string path, int originX, int originZ, int minX, int minY, int minZ, int maxX, int maxY,
            int maxZ)
        {
            // Creates a new mesh file
            var meshFile = new MeshFile();
            if (view.Type == ExportDetailLevelType.Heightmap)
            {
                await ExportHeightmap(view, meshFile, originX, originZ, minX, minZ, maxX, maxZ);
            }
            else
            {
                await ExportBlocks(view, meshFile, originX, originZ, minX, minY, minZ, maxX, maxY, maxZ);
            }

            // Skip empty
            if (meshFile.Geometries.Count == 0)
                return;
            
            // Writes the mesh to the tmp file
            var pathTmp = path + ".tmp";
            await meshFile.WriteToFileAsync(pathTmp);
            
            // Delete the existing file
            if (File.Exists(path))
                File.Delete(path);
            
            File.Move(pathTmp, path);
        }

        /// <summary>
        /// Writes all blocks in the given area into <paramref name="meshFile"/>.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="meshFile"></param>
        /// <param name="originX"></param>
        /// <param name="originZ"></param>
        /// <param name="minX"></param>
        /// <param name="minY"></param>
        /// <param name="minZ"></param>
        /// <param name="maxX"></param>
        /// <param name="maxY"></param>
        /// <param name="maxZ"></param>
        private async Task ExportBlocks(ExportDetailLevel view, MeshFile meshFile,  int originX, int originZ, int minX, int minY, int minZ, int maxX, int maxY,
            int maxZ)
        {
            for (var x = minX; x < maxX; x++)
            for (var y = minY; y < maxY; y++)
            for (var z = minZ; z < maxZ; z++)
            {
                var block = await m_World.GetBlockAsync(x, y, z);
                var blockVariant = block.GetRandomVariant();
                if (blockVariant.Faces is null)
                {
                    continue;
                }

                // Removes non surface blocks
                if (UndergroundCulling && !await IsVisibleFromWorldSurfaceAsync(view, x, y, z))
                    continue;
                
                var offset = new Vector3(x - originX , y, z - originZ);
                IEnumerable<CachedBlockStateFace> faces = blockVariant.Faces;
                
                // Handle water
                if (block.WaterLevel.HasValue)
                {
                    var heights = await GetFluidHeights(ModelFluidType.Water, x, y, z);
                    var model = Assets.ModelCache.GetFluidModel(ModelFluidType.Water, heights);
                    if (model.Faces is not null)
                    {
                        faces = faces.Concat(model.Faces);
                    }
                }
                
                // Handle lava
                if (block.LavaLevel.HasValue)
                {
                    var heights = await GetFluidHeights(ModelFluidType.Lava, x, y, z);
                    var model = Assets.ModelCache.GetFluidModel(ModelFluidType.Lava, heights);
                    if (model.Faces is not null)
                    {
                        faces = faces.Concat(model.Faces);
                    }
                }
                
                // Export all faces
                foreach (var face in faces)
                {
                    if (await CheckFaceCullingAsync(view, x, y, z, face))
                    {
                        continue;
                    }

                    await ExportFace(x, y, z, face, offset, meshFile);
                }
            }
        }

        /// <summary>
        /// Returns the joined fluid level of this corner coordinate.
        /// </summary>
        /// <param name="fluidType"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private async ValueTask<(byte, byte, byte, byte)> GetFluidHeights(ModelFluidType fluidType, int x, int y, int z)
        {
            return (
                await GetFluidHeight(fluidType, x, y, z),
                await GetFluidHeight(fluidType, x + 1, y, z),
                await GetFluidHeight(fluidType, x + 1, y, z + 1),
                await GetFluidHeight(fluidType, x, y, z + 1)
            );
        }

        /// <summary>
        /// Returns the joined fluid level of this corner coordinate.
        /// </summary>
        /// <param name="fluidType"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private async ValueTask<byte> GetFluidHeight(ModelFluidType fluidType, int x, int y, int z)
        {
            // If there is any block above, the level is always full (8).
            var levelNE = await m_World.GetFluidLevelAsync(fluidType, x, y + 1, z - 1);
            var levelNW = await m_World.GetFluidLevelAsync(fluidType, x - 1, y + 1, z - 1);
            var levelSE = await m_World.GetFluidLevelAsync(fluidType, x, y + 1, z);
            var levelSW = await m_World.GetFluidLevelAsync(fluidType, x - 1, y + 1, z);
            if (levelNE.HasValue || levelNW.HasValue || levelSE.HasValue || levelSW.HasValue)
            {
                return 8;
            }
            
            levelNE = await m_World.GetFluidLevelAsync(fluidType, x, y, z - 1);
            levelNW = await m_World.GetFluidLevelAsync(fluidType, x - 1, y, z - 1);
            levelSE = await m_World.GetFluidLevelAsync(fluidType, x, y, z);
            levelSW = await m_World.GetFluidLevelAsync(fluidType, x - 1, y, z);
            
            var count = 0;
            var max = 0;
            if (levelNE.HasValue)
            {
                count++;
                max = Math.Max(max, GetFluidHeight(levelNE.Value));
            }
            if (levelNW.HasValue)
            {
                count++;
                max = Math.Max(max, GetFluidHeight(levelNW.Value));
            }
            if (levelSE.HasValue)
            {
                count++;
                max = Math.Max(max, GetFluidHeight(levelSE.Value));
            }
            if (levelSW.HasValue)
            {
                count++;
                max = Math.Max(max, GetFluidHeight(levelSW.Value));
            }

            if (count == 0) return 0;
            
            return (byte) (max - 1);
        }
        
        /// <summary>
        /// Gets the height of a fluid corner
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private static byte GetFluidHeight(byte level)
        {
            if (level < 8)
            {
                level = (byte) (8 - level);
            }

            return level;
        }

        /// <summary>
        /// Writes the mesh heightmap in the given area into <paramref name="meshFile"/>.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="meshFile"></param>
        /// <param name="originX"></param>
        /// <param name="originZ"></param>
        /// <param name="minX"></param>
        /// <param name="minZ"></param>
        /// <param name="maxX"></param>
        /// <param name="maxZ"></param>
        private async Task ExportHeightmap(ExportDetailLevel view, MeshFile meshFile, int originX, int originZ, int minX, int minZ, int maxX,
            int maxZ)
        {
            for (var x = minX; x < maxX; x++)
            for (var z = minZ; z < maxZ; z++)
            {
                var offset = new Vector3((x - originX) * view.BlockSpan , 0f, (z - originZ) * view.BlockSpan);
                
                var height = await GetHeightInChunkAsync(view, HeightmapType.MotionBlocking, x,  z, 0);
                var heightNoLeaves = await GetHeightInChunkAsync(view, HeightmapType.MotionBlockingNoLeaves, x,  z, 0);

                await ExportHeightmapBlock(view, meshFile, x, z, offset, HeightmapType.MotionBlockingNoLeaves);

                if (height != heightNoLeaves)
                {
                    await ExportHeightmapBlock(view, meshFile, x, z, offset, HeightmapType.MotionBlocking);
                }
            }
        }

        /// <summary>
        /// Gets the average height of the given chunk position 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="type"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private async ValueTask<int> GetHeightInChunkAsync(ExportDetailLevel view, HeightmapType type, int x, int z,
            int defaultValue)
        {
            var blocks = view.BlockSpan;
            
            return await m_World.GetHeightAsync(type, x * blocks,  z * blocks, defaultValue);
        }

        private async ValueTask<CachedBlockState> GetBlockInChunkAsync(ExportDetailLevel view, int x, int y, int z)
        {
            var blocks = view.BlockSpan;
            
            return await m_World.GetBlockAsync(x * blocks, y, z * blocks);
        }
        
        private async ValueTask<byte> GetBlockLightInChunkAsync(ExportDetailLevel view, int x, int y, int z)
        {
            var blocks = view.BlockSpan;
            
            return await m_World.GetBlockLightAsync(x * blocks, y, z * blocks);
        }
        
        private async ValueTask<byte> GetSkyLightInChunkAsync(ExportDetailLevel view, int x, int y, int z)
        {
            var blocks = view.BlockSpan;
            
            return await m_World.GetSkyLightAsync(x * blocks, y, z * blocks);
        }
        
        private async ValueTask<Vector3> GetTintColorInChunkAsync(ExportDetailLevel view, int x, int y, int z, ModelTintType tintType)
        {
            var blocks = view.BlockSpan;
            
            return await m_World.GetSmoothTintColorAsync(x * blocks + blocks / 2, y, z * blocks + blocks / 2, tintType);
        }

        /// <summary>
        /// Exports a heightmap block
        /// </summary>
        /// <param name="view"></param>
        /// <param name="meshFile"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="offset"></param>
        /// <param name="type"></param>
        private async Task ExportHeightmapBlock(ExportDetailLevel view, MeshFile meshFile, int x, int z, Vector3 offset, HeightmapType type)
        {
            var blocks = view.BlockSpan;
            var height = await GetHeightInChunkAsync(view, type, x,  z, 0);
            var heightNE = await GetHeightInChunkAsync(view, type, x + 1,  z + 1, height);
            var heightNW = await GetHeightInChunkAsync(view, type, x - 1,  z + 1, height);
            var heightSE = await GetHeightInChunkAsync(view, type, x + 1,  z - 1, height);
            var heightSW = await GetHeightInChunkAsync(view, type, x - 1,  z - 1, height);
            var heightN = await GetHeightInChunkAsync(view, type, x,  z + 1, height);
            var heightE = await GetHeightInChunkAsync(view, type, x + 1,  z, height);
            var heightS = await GetHeightInChunkAsync(view, type, x,  z - 1, height);
            var heightW = await GetHeightInChunkAsync(view, type, x - 1,  z, height);

            AssetIdentifier textureAsset = default;
            ModelTintType tintType = default;
            for (var y = 1; y <= 3; y++)
            {
                var block = await GetBlockInChunkAsync(view, x, height - y, z);
                var blockVariant = block.GetRandomVariant();
                
                // Force water texture
                if (block.WaterLevel.HasValue)
                {
                    textureAsset = World.TextureWaterStill;
                    tintType = ModelTintType.Water;
                    break;
                }
                
                // Force lava texture
                if (block.LavaLevel.HasValue)
                {
                    textureAsset = World.TextureLavaStill;
                    break;
                }
                
                // There are no faces / no block
                if (blockVariant.Faces == null || blockVariant.Faces.Length == 0)
                {
                    continue;
                }
                
                // Finds the most upper block
                var face = blockVariant.Faces.FirstOrDefault(f => f.CullFace is Direction.Up);

                // Gets the texture
                textureAsset = face.Texture;
                tintType = face.TintType;
                if (textureAsset.Name != null) break;
            }
            if (textureAsset.Name == null) return;

            var textureName = GetMaterialNameByAsset(textureAsset);
            
            // Creates the geometry
            var geometry = meshFile.GetOrCreateGeometryByMaterial(textureName);
            var vertexCount = geometry.Vertices.Count;

            var normal = Vector3.AxisY;

            var tintColor = await GetTintColorInChunkAsync(view, x, height, z, tintType);
            var blockLight = await GetBlockLightInChunkAsync(view, x, height + 1, z);
            var skyLight = await GetSkyLightInChunkAsync(view, x, height + 1, z);
            var light = skyLight > blockLight ? skyLight : blockLight;
            var lightValue = light / 15f;
            var color = new Vector3() { X = tintColor.X * lightValue, Y = tintColor.Y * lightValue, Z = tintColor.Z * lightValue};
            
            
            geometry.Vertices.Add(new MeshVertex()
            {
                Position = new Vector3() { X = offset.X + blocks, Y = (height + heightSE + heightS + heightE) / 4f, Z = offset.Z + 0f },
                Uv = new Vector2() { X = 0f, Y = 0f },
                Normal = normal,
                Color = color,
            });
            geometry.Vertices.Add(new MeshVertex()
            {
                Position = new Vector3() { X = offset.X + 0f, Y = (height + heightSW + heightS + heightW) / 4f, Z = offset.Z + 0f },
                Uv = new Vector2() { X = 0f, Y = 1f },
                Normal = normal,
                Color = color,
            });
            geometry.Vertices.Add(new MeshVertex()
            {
                Position = new Vector3() { X = offset.X + 0f, Y = (height + heightNW + heightN + heightW) / 4f, Z = offset.Z + blocks },
                Uv = new Vector2() { X = 1f, Y = 1f },
                Normal = normal,
                Color = color,
            });
            geometry.Vertices.Add(new MeshVertex()
            {
                Position = new Vector3() { X = offset.X + blocks, Y = (height + heightNE + heightN + heightE) / 4f, Z = offset.Z + blocks },
                Uv = new Vector2() { X = 1f, Y = 0f },
                Normal = normal,
                Color = color,
            });
        
            geometry.Triangles.Add(vertexCount + 0);
            geometry.Triangles.Add(vertexCount + 1);
            geometry.Triangles.Add(vertexCount + 2);

            geometry.Triangles.Add(vertexCount + 0);
            geometry.Triangles.Add(vertexCount + 2);
            geometry.Triangles.Add(vertexCount + 3);
        }
        
        /// <summary>
        /// Exports the given block with the given offset to the given scene
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="face"></param>
        /// <param name="offset"></param>
        /// <param name="meshFile"></param>
        private async Task ExportFace(int x, int y, int z, CachedBlockStateFace face, Vector3 offset, MeshFile meshFile)
        {
            // Gets the texture
            var textureAsset = face.Texture;
            if (textureAsset.Name == null) return;
            var textureName = GetMaterialNameByAsset(textureAsset);

            // Creates the geometry
            var geometry = meshFile.GetOrCreateGeometryByMaterial(textureName);
            var vertexCount = geometry.Vertices.Count;

            GetCullDirection(face.CullFace, x, y, z, out var lx, out var ly, out var lz);
            var tintColor = await m_World.GetSmoothTintColorAsync(x, y, z, face.TintType);
            var blockLight = await m_World.GetBlockLightAsync(lx, ly, lz);
            var skyLight = await m_World.GetSkyLightAsync(lx, ly, lz);
            var light = skyLight > blockLight ? skyLight : blockLight;
            var lightValue = light / 15f;
            var color = new Vector3() { X = tintColor.X * lightValue, Y = tintColor.Y * lightValue, Z = tintColor.Z * lightValue};


            var uvA = new Vector2() {X = face.Uv.X / 16f, Y = 1f - face.Uv.Y / 16f};
            var uvB = new Vector2() {X = face.Uv.X / 16f, Y = 1f - face.Uv.W / 16f};
            var uvC = new Vector2() {X = face.Uv.Z / 16f, Y = 1f - face.Uv.W / 16f};
            var uvD = new Vector2() {X = face.Uv.Z / 16f, Y = 1f - face.Uv.Y / 16f};

            geometry.Vertices.Add(new MeshVertex()
            {
                Position = offset + face.VertexA,
                Uv = uvA,
                Normal = face.Normal,
                Color = color,
            });
            geometry.Vertices.Add(new MeshVertex()
            {
                Position = offset + face.VertexB,
                Uv = uvB,
                Normal = face.Normal,
                Color = color,
            });
            geometry.Vertices.Add(new MeshVertex()
            {
                Position = offset + face.VertexC,
                Uv = uvC,
                Normal = face.Normal,
                Color = color,
            });
            geometry.Vertices.Add(new MeshVertex()
            {
                Position = offset + face.VertexD,
                Uv = uvD,
                Normal = face.Normal,
                Color = color,
            });
            
            geometry.Triangles.Add(vertexCount + 0);
            geometry.Triangles.Add(vertexCount + 1);
            geometry.Triangles.Add(vertexCount + 2);

            geometry.Triangles.Add(vertexCount + 0);
            geometry.Triangles.Add(vertexCount + 2);
            geometry.Triangles.Add(vertexCount + 3);
        }
        
        
        private async ValueTask ExportFluidBlock(int x, int y, int z, Vector3 offset, float heightNW, float heightNE, float heightSE, float heightSW, AssetIdentifier textureStillAsset, AssetIdentifier textureFlowAsset, MeshFile meshFile)
        {
            var textureStillName = GetMaterialNameByAsset(textureStillAsset);

            // Creates the geometry
            var geometry = meshFile.GetOrCreateGeometryByMaterial(textureStillName);
            var vertexCount = geometry.Vertices.Count;
            
            var tintColor = await m_World.GetSmoothTintColorAsync(x, y, z, ModelTintType.Water);
            var blockLight = await m_World.GetBlockLightAsync(x, y, z);
            var skyLight = await m_World.GetSkyLightAsync(x, y, z);
            var light = skyLight > blockLight ? skyLight : blockLight;
            var lightValue = light / 15f;
            var color = new Vector3() { X = tintColor.X * lightValue, Y = tintColor.Y * lightValue, Z = tintColor.Z * lightValue};
            
            var normal = Vector3.AxisY;
            
            geometry.Vertices.Add(new MeshVertex()
            {
                Position = new Vector3() { X = offset.X + 1f, Y = offset.Y + heightNE, Z = offset.Z + 0f },
                Uv = new Vector2() { X = 0f, Y = 0f },
                Normal = normal,
                Color = color,
            });
            geometry.Vertices.Add(new MeshVertex()
            {
                Position = new Vector3() { X = offset.X + 0f, Y = offset.Y + heightNE, Z = offset.Z + 0f },
                Uv = new Vector2() { X = 0f, Y = 1f },
                Normal = normal,
                Color = color,
            });
            geometry.Vertices.Add(new MeshVertex()
            {
                Position = new Vector3() { X = offset.X + 0f, Y = offset.Y + heightNE, Z = offset.Z + 1f },
                Uv = new Vector2() { X = 1f, Y = 1f },
                Normal = normal,
                Color = color,
            });
            geometry.Vertices.Add(new MeshVertex()
            {
                Position = new Vector3() { X = offset.X + 1f, Y = offset.Y + heightNE, Z = offset.Z + 1f },
                Uv = new Vector2() { X = 1f, Y = 0f },
                Normal = normal,
                Color = color,
            });
        
            geometry.Triangles.Add(vertexCount + 0);
            geometry.Triangles.Add(vertexCount + 1);
            geometry.Triangles.Add(vertexCount + 2);

            geometry.Triangles.Add(vertexCount + 0);
            geometry.Triangles.Add(vertexCount + 2);
            geometry.Triangles.Add(vertexCount + 3);
        }

        #endregion Export
        
        #region Culling

        /// <summary>
        /// Returns the coordinate of the cull direction.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="rx"></param>
        /// <param name="ry"></param>
        /// <param name="rz"></param>
        private static void GetCullDirection(Direction? direction, int x, int y, int z, out int rx, out int ry,
            out int rz)
        {
            rx = x;
            ry = y;
            rz = z;
            if (!direction.HasValue)
            {
                return;
            }
            
            World.MoveInDirection(ref rx, ref ry, ref rz, direction.Value);
        }
        
        /// <summary>
        /// The height map cache per chunk
        /// </summary>
        private readonly ConcurrentDictionary<(int, int), int[]> m_HeightmapCache = new();
        
        /// <summary>
        /// Checks if the given position is invisible
        /// </summary>
        /// <param name="view"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private async ValueTask<bool> IsVisibleFromWorldSurfaceAsync(ExportDetailLevel view, int x, int y, int z)
        {
            if (y >= UndergroundCullingHeight) return true;
            
            var blockX = (byte) (x & 0x0F);
            var blockZ = (byte) (z & 0x0F);

            var chunkX = (x - blockX) / 16;
            var chunkZ = (z - blockZ) / 16;

            var heightmap = await GetOrCreateHeightmapCacheByChunkAsync(chunkX, chunkZ);

            var height = heightmap[blockX + blockZ * 16];
            return y >= height - 1;
        }

        /// <summary>
        /// Gets or creates the heightmap cache for the given chunk coordinate
        /// </summary>
        /// <param name="chunkX"></param>
        /// <param name="chunkZ"></param>
        /// <returns></returns>
        private async ValueTask<int[]> GetOrCreateHeightmapCacheByChunkAsync(int chunkX, int chunkZ)
        {
            if (m_HeightmapCache.TryGetValue((chunkX, chunkZ), out var heightmap))
            {
                return heightmap;
            }

            const int range = 8;
            const HeightmapType heightMapType = HeightmapType.OceanFloor;
            
            heightmap = new int[16 * 16];
            for (byte x = 0; x < 16; x++)
            for (byte z = 0; z < 16; z++)
            {
                var height = 255;
                
                for (var hx = -range; hx <= range; hx++)
                for (var hz = -range; hz <= range; hz++)
                {
                    var h = await m_World.GetHeightAsync(heightMapType, chunkX * 16 + x + hx, chunkZ * 16 + z + hz);
                    if (h < height)
                        height = h;
                }

                heightmap[x + z * 16] = height;
            }

            m_HeightmapCache.TryAdd((chunkX, chunkZ), heightmap);
            
            return heightmap;
        }
        
        /// <summary>
        /// Checks if the given face at the given position is culled out.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="face"></param>
        /// <returns></returns>
        private async Task<bool> CheckFaceCullingAsync(ExportDetailLevel view, int x, int y, int z, CachedBlockStateFace face)
        {
            // Ignore world floor
            if (face.CullFace == Direction.Down && y == -64)
                return true;

            // Checking world border
            if (!WorldBorderCulled && face.CullFace.HasValue)
            {
                var direction = face.CullFace.Value;
                var nextX = x;
                var nextY = y;
                var nextZ = z;
                World.MoveInDirection(ref nextX, ref nextY, ref nextZ, direction);
                
                // Don't cull the edges of the world border. We want a fancy hard cut.
                if (WorldBorderMin.HasValue && (nextX < WorldBorderMin.Value.X || nextY < WorldBorderMin.Value.Y ||
                                                nextZ < WorldBorderMin.Value.Z))
                {
                    return false;
                }
                if (WorldBorderMax.HasValue && (nextX >= WorldBorderMax.Value.X || nextY >= WorldBorderMax.Value.Y ||
                                                nextZ >= WorldBorderMax.Value.Z))
                {
                    return false;
                }
            }

            return await m_World.CheckFaceCullingAsync(x, y, z, face);
        }
        
        #endregion Culling
        
        #region Materials

        /// <summary>
        /// Returns the material name from the given texture asset
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        private static string GetMaterialNameByAsset(AssetIdentifier asset)
        {
            return asset.Name;
        }

        /// <summary>
        /// Exports all textures and writes the mtl file
        /// </summary>
        /// <param name="mtlPath"></param>
        /// <param name="texturePrefix"></param>
        /// <param name="textureSubDirectory"></param>
        private async Task<MaterialFile> ExportMaterialsAsync(string mtlPath, string texturePrefix, string textureSubDirectory = "textures")
        {
            var mtlDirectory = Path.GetDirectoryName(mtlPath);
            if (mtlDirectory == null)
                throw new ArgumentException($"Could not get parent directory of '{mtlPath}'!", nameof(mtlPath));
            
            
            var mtlFile = new MaterialFile();
            foreach (var textureAsset in Assets.Source.GetAssets(AssetType.Texture, "minecraft"))
            {
                if (!textureAsset.Name.StartsWith(texturePrefix))
                    continue;
                
                // Gets the material
                var textureInfo = await Assets.TextureCache.GetAsync(textureAsset);

                // Gets the name
                var name = GetMaterialNameByAsset(textureAsset);
                var textureFileName = textureSubDirectory + "/" + name + ".png";
                
                var textureFilePath = Path.Combine(mtlDirectory, textureFileName);
                if (!File.Exists(textureFilePath))
                {
                    var texturePath = Path.GetDirectoryName(textureFilePath);
                    Debug.Assert(texturePath != null, nameof(texturePath) + " != null");
                    Directory.CreateDirectory(texturePath);

                    if (!Assets.Source.TryOpenAsset(textureAsset, out var textureStream) || textureStream is null)
                        continue;
                    
                    await using (textureStream)
                    {
                        await using var textureOutput = new FileStream(textureFilePath, FileMode.Create);
                        await textureStream.CopyToAsync(textureOutput);
                    }
                    
                }

                var animationFrameTime = 0;
                var animationFrameCount = 0;
                int[]? animationFrames = null;
                if (textureInfo.Animation.HasValue)
                {
                    var animation = textureInfo.Animation.Value;
                    animationFrameTime = animation.FrameTime;
                    animationFrameCount = textureInfo.Height / textureInfo.Width;
                    animationFrames = animation.Frames;
                }
                
                var material = new MeshMaterial()
                {
                    Name = name, 
                    Texture = textureFileName,
                    Transparent = textureInfo.Transparency == TextureTransparency.Semi,
                    AnimationFrameTime = animationFrameTime,
                    AnimationFrameCount = animationFrameCount,
                    AnimationFrames = animationFrames,
                };
                mtlFile.Materials.Add(material);
            }

            await mtlFile.WriteToFileAsync(mtlPath);
            return mtlFile;
        }
        
        #endregion Materials
    }
}