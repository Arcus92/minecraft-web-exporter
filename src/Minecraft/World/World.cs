using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MinecraftWebExporter.Minecraft.BlockStates;
using MinecraftWebExporter.Minecraft.BlockStates.Cache;
using MinecraftWebExporter.Minecraft.Models;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.World;

/// <summary>
/// The minecraft world
/// </summary>
public class World : IAsyncDisposable
{
    /// <summary>
    /// Opens the world at the given path
    /// </summary>
    /// <param name="assets"></param>
    /// <param name="path"></param>
    public World(AssetManager assets, string path)
    {
        Assets = assets;
        Path = path;
        Name = System.IO.Path.GetFileName(path);
    }
        
    /// <summary>
    /// Creates an empty world
    /// </summary>
    /// <param name="assets"></param>
    public World(AssetManager assets)
    {
        Assets = assets;
        Path = string.Empty;
        Name = string.Empty;
    }

    /// <summary>
    /// Gets the assets for this world
    /// </summary>
    public AssetManager Assets { get; }

    /// <summary>
    /// Gets the directory of the world files
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the name of the world
    /// </summary>
    public string Name { get; }

    #region Regions

    /// <summary>
    /// The list of all regions
    /// </summary>
    private readonly ConcurrentDictionary<(int, int), Region> m_Regions = new();
        
    /// <summary>
    /// Loads the given region
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public async ValueTask<Region?> GetOrLoadRegionAsync(int x, int z)
    {
        if (m_Regions.TryGetValue((x, z), out var region))
        {
            return region;
        }

        region = new Region(this, x, z);
        await region.LoadAsync();
        m_Regions.TryAdd((x, z), region);
        return region;
    }

    /// <summary>
    /// Returns the loaded region
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public Region? GetRegion(int x, int z)
    {
        if (m_Regions.TryGetValue((x, z), out var region))
        {
            return region;
        }

        return null;
    }

    /// <summary>
    /// Returns all regions from this world.
    /// The regions are unloaded.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Region> GetRegions()
    {
        var regionPath = System.IO.Path.Combine(Path, "region");
        foreach (var file in Directory.GetFiles(regionPath, "*.mca"))
        {
            var filename = System.IO.Path.GetFileName(file);
            var i1 = filename.IndexOf('.');
            var i2 = filename.IndexOf('.', i1 + 1);
            var i3 = filename.IndexOf('.', i2 + 2);

            var strX = filename.Substring(i1 + 1, i2 - i1 - 1);
            var strZ = filename.Substring(i2 + 1, i3 - i2 - 1);
            var x = int.Parse(strX);
            var z = int.Parse(strZ);
            yield return new Region(this, x, z);
        }
    }

    /// <summary>
    /// Gets the block at this absolute coordinate
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public CachedBlockState? GetBlock(int x, int y, int z)
    {
        DecomposeCoordinate(x, y, z, out var regionX, out var regionZ, out var chunkX, out var chunkY,
            out var chunkZ, out var blockX, out var blockY, out var blockZ);
            
        var region = GetRegion(regionX, regionZ);
        if (region is null)
        {
            return null;
        }

        var chunk = region.GetChunk(chunkX, chunkZ);
        if (chunk is null)
        {
            return null;
        }

        var section = chunk.GetSection(chunkY);
        if (section is null)
        {
            return null;
        }
            
        return section.GetBlock(blockX, blockY, blockZ);
    }
        
    /// <summary>
    /// Gets the block at this absolute coordinate but loads the chunk if needed
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public async ValueTask<CachedBlockState> GetBlockAsync(int x, int y, int z)
    {
        DecomposeCoordinate(x, y, z, out var regionX, out var regionZ, out var chunkX, out var chunkY,
            out var chunkZ, out var blockX, out var blockY, out var blockZ);
            
        var region = await GetOrLoadRegionAsync(regionX, regionZ);
        if (region is null)
        {
            return default;
        }

        var chunk = await region.GetOrLoadChunkAsync(chunkX, chunkZ);
        if (chunk is null)
        {
            return default;
        }

        var section = chunk.GetSection(chunkY);
        if (section is null)
        {
            return default;
        }
            
        return section.GetBlock(blockX, blockY, blockZ);
    }
        
    /// <summary>
    /// Gets the light level of the given block
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public async ValueTask<byte> GetBlockLightAsync(int x, int y, int z)
    {
        DecomposeCoordinate(x, y, z, out var regionX, out var regionZ, out var chunkX, out var chunkY,
            out var chunkZ, out var blockX, out var blockY, out var blockZ);
            
        var region = await GetOrLoadRegionAsync(regionX, regionZ);
        if (region is null)
        {
            return 0;
        }

        var chunk = await region.GetOrLoadChunkAsync(chunkX, chunkZ);
        if (chunk is null)
        {
            return 0;
        }

        var section = chunk.GetSection(chunkY);
        if (section is null)
        {
            return 0;
        }
            
        return section.GetBlockLight(blockX, blockY, blockZ);
    }
        
    /// <summary>
    /// Gets the sky light level of the given block
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public async ValueTask<byte> GetSkyLightAsync(int x, int y, int z)
    {
        DecomposeCoordinate(x, y, z, out var regionX, out var regionZ, out var chunkX, out var chunkY,
            out var chunkZ, out var blockX, out var blockY, out var blockZ);
            
        var region = await GetOrLoadRegionAsync(regionX, regionZ);
        if (region is null)
        {
            return 16;
        }

        var chunk = await region.GetOrLoadChunkAsync(chunkX, chunkZ);
        if (chunk is null)
        {
            return 16;
        }

        var section = chunk.GetSection(chunkY);
        if (section is null)
        {
            return 16;
        }
            
        return section.GetSkyLight(blockX, blockY, blockZ);
    }
        
    /// <summary>
    /// Gets the biome at this absolute coordinate but loads the chunk if needed
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public async ValueTask<string?> GetBiomeAsync(int x, int y, int z)
    {
        DecomposeCoordinate(x, y, z, out var regionX, out var regionZ, out var chunkX, out var chunkY,
            out var chunkZ, out var blockX, out var _, out var blockZ);
            
        var region = await GetOrLoadRegionAsync(regionX, regionZ);
        if (region is null)
        {
            return null;
        }

        var chunk = await region.GetOrLoadChunkAsync(chunkX, chunkZ);
        if (chunk is null)
        {
            return null;
        }

        var section = chunk.GetSection(chunkY);
        if (section is null)
        {
            return null;
        }
            
        return section.GetBiome(blockX, blockZ);
    }

    /// <summary>
    /// Gets the height information but loads the chunk if needed
    /// </summary>
    /// <param name="type"></param>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public async ValueTask<int> GetHeightAsync(HeightmapType type, int x, int z, int defaultValue = 256)
    {
        DecomposeCoordinate(x, 0, z, out var regionX, out var regionZ, out var chunkX, out var _,
            out var chunkZ, out var blockX, out var _, out var blockZ);
            
        var region = await GetOrLoadRegionAsync(regionX, regionZ);
        if (region is null)
        {
            return 0;
        }

        var chunk = await region.GetOrLoadChunkAsync(chunkX, chunkZ);
        if (chunk is null)
        {
            return 0;
        }

        if (chunk.TryGetHeightmap(type, out var heightmap))
        {
            return heightmap.GetHeight(blockX, blockZ);
        }
            
        return defaultValue;
    }
        
    /// <summary>
    /// Gets the water level of this block (0 = full - 7 = lowest, 8 = falling)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public async ValueTask<byte?> GetWaterLevelAsync(int x, int y, int z)
    {
        var block = await GetBlockAsync(x, y, z);
            
        return block.WaterLevel;
    }
        
    /// <summary>
    /// Gets the lava level of this block (0 = full - 7 = lowest, 8 = falling)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public async ValueTask<byte?> GetLavaLevelAsync(int x, int y, int z)
    {
        var block = await GetBlockAsync(x, y, z);
            
        return block.LavaLevel;
    }
        
    /// <summary>
    /// Gets the fluid level of this block (0 = full - 7 = lowest, 8 = falling)
    /// </summary>
    /// <param name="fluidType"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public async ValueTask<byte?> GetFluidLevelAsync(ModelFluidType fluidType, int x, int y, int z)
    {
        switch (fluidType)
        {
            case ModelFluidType.Water:
                return await GetWaterLevelAsync(x, y, z);
            case ModelFluidType.Lava:
                return await GetLavaLevelAsync(x, y, z);
            default:
                return null;
        }
    }

    /// <summary>
    /// The texture for still water
    /// </summary>
    public static readonly AssetIdentifier TextureWaterStill = new(AssetType.Texture, "minecraft", "block/water_still");
        
    /// <summary>
    /// The texture for flow water
    /// </summary>
    public static readonly AssetIdentifier TextureWaterFlow = new(AssetType.Texture, "minecraft", "block/water_flow");
        
    /// <summary>
    /// The texture for still lava
    /// </summary>
    public static readonly AssetIdentifier TextureLavaStill = new(AssetType.Texture, "minecraft", "block/lava_still");
        
    /// <summary>
    /// The texture for flow lava
    /// </summary>
    public static readonly AssetIdentifier TextureLavaFlow = new(AssetType.Texture, "minecraft", "block/lava_flow");
        
    /// <summary>
    /// A struct for a tint color map
    /// </summary>
    public readonly struct ColorMap
    {
        /// <summary>
        /// The lower left color
        /// </summary>
        public Vector3 Uv00 { get; init; }
            
        /// <summary>
        /// The lower right color
        /// </summary>
        public Vector3 Uv10 { get; init; }
            
        /// <summary>
        /// The upper left color
        /// </summary>
        public Vector3 Uv01 { get; init; }

        /// <summary>
        /// Gets the value
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector3 GetValue(float u, float v)
        {
            var x = u - v;
            var y = 1f - u;
            var z = v;
                
            return new Vector3()
            {
                X = x * Uv00.X + y * Uv10.X + z * Uv01.X, 
                Y = x * Uv00.Y + y * Uv10.Y + z * Uv01.Y, 
                Z = x * Uv00.Z + y * Uv10.Z + z * Uv01.Z,
            };
        }
    }

    /// <summary>
    /// The grass tint color map
    /// </summary>
    public static readonly ColorMap TintGrass = new()
    {
        Uv00 = new() {X = 191f / 255f, Y = 183f / 255f, Z = 85f / 255f},
        Uv10 = new() {X = 128f / 255f, Y = 180f / 255f, Z = 151f / 255f},
        Uv01 = new() {X = 71f / 255f, Y = 205f / 255f, Z = 51f / 255f},
    };
        
    /// <summary>
    /// The foliage tint color map
    /// </summary>
    public static readonly ColorMap TintFoliage = new()
    {
        Uv00 = new() {X = 174f / 255f, Y = 164f / 255f, Z = 42f / 255f},
        Uv10 = new() {X = 96f / 255f, Y = 161f / 255f, Z = 123f / 255f},
        Uv01 = new() {X = 26f / 255f, Y = 191f / 255f, Z = 0f / 255f},
    };

    /// <summary>
    /// The redstone write color
    /// </summary>
    public static readonly Vector3 ColorRedstoneWire = new Vector3() {X = 252f / 255f, Y = 49f / 255f, Z = 0f / 255f};
        
        
    /// <summary>
    /// Gets the tint color based on the biome and <see cref="tintType"/>.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="tintType"></param>
    /// <returns></returns>
    public async ValueTask<Vector3> GetTintColorAsync(int x, int y, int z, ModelTintType tintType)
    {
        // This block isn't tinted
        if (tintType == ModelTintType.Default)
            return Vector3.One;

        if (tintType == ModelTintType.Redstone)
            return ColorRedstoneWire;
            
        // Fetches the biome info
        var biomeName = await GetBiomeAsync(x, y, z);
        var biome = Biome.Get(biomeName);

        if (tintType == ModelTintType.Water)
            return biome.WaterSurfaceColor;

        var rainfall = biome.Rainfall;
        var temperature = biome.Temperature;
        if (y > 64) temperature -= 0.00166667f * (y - 64);

        var adjTemperature = Math.Clamp(temperature, 0f, 1f);
        var adjRainfall = Math.Clamp(rainfall, 0f, 1f) * adjTemperature;
            
        switch (tintType)
        {
            case ModelTintType.Grass:
                return TintGrass.GetValue(adjTemperature, adjRainfall);
            case ModelTintType.Foliage:
                return TintFoliage.GetValue(adjTemperature, adjRainfall);
            default:
                return Vector3.One;
        }
    }

    /// <summary>
    /// Gets the tint color based on the biome and <see cref="tintType"/> smooth with the surrounding biomes.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="tintType"></param>
    /// <returns></returns>
    public async ValueTask<Vector3> GetSmoothTintColorAsync(int x, int y, int z, ModelTintType tintType)
    {
        // This block isn't tinted
        if (tintType == ModelTintType.Default)
            return Vector3.One;

        var r = 0f;
        var g = 0f;
        var b = 0f;
        const int s = 4;
        const int c = (s + s + 1) * (s + s + 1);
        for (var xo = -s; xo <= s; xo++)
        for (var zo = -s; zo <= s; zo++)
        {
            var tc = await GetTintColorAsync(x + xo, y, z + zo, tintType);
            r += tc.X;
            g += tc.Y;
            b += tc.Z;
        }

        return new Vector3() {X = r / c, Y = g / c, Z = b / c};
    }

    /// <summary>
    /// Returns true if the <paramref name="face"/> at the given position is culled.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="face"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async ValueTask<bool> CheckFaceCullingAsync(int x, int y, int z, CachedBlockStateFace face)
    {
        if (!face.CullFace.HasValue)
            return false;
            
        var direction = face.CullFace.Value;
        MoveInDirection(ref x, ref y, ref z, direction);

        var block = await GetBlockAsync(x, y, z);
            
        // Check water
        if (face.FluidType == ModelFluidType.Water && block.WaterLevel.HasValue)
        {
            return true;
        }
        // Check lava
        if (face.FluidType == ModelFluidType.Lava && block.LavaLevel.HasValue)
        {
            return true;
        }
            
        return face.IsCovered(block);
    }
        
    /// <summary>
    /// Moves the given coordinates into the given direction
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="direction"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void MoveInDirection(ref int x, ref int y, ref int z, Direction direction)
    {
        switch (direction)
        {
            case Direction.Down:
                y--;
                break;
            case Direction.Up:
                y++;
                break;
            case Direction.North:
                z--;
                break;
            case Direction.South:
                z++;
                break;
            case Direction.West:
                x--;
                break;
            case Direction.East:
                x++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }
        
    #endregion Regions
        
    #region Coordinates

    /// <summary>
    /// Composes the absolute world coordinate by the given region, chunk and local coordinates.
    /// </summary>
    /// <param name="regionX"></param>
    /// <param name="regionZ"></param>
    /// <param name="chunkX"></param>
    /// <param name="chunkY"></param>
    /// <param name="chunkZ"></param>
    /// <param name="blockX"></param>
    /// <param name="blockY"></param>
    /// <param name="blockZ"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public static void ComposeCoordinate(int regionX, int regionZ, byte chunkX, sbyte chunkY, byte chunkZ,
        byte blockX, byte blockY, byte blockZ, out int x, out int y, out int z)
    {
        x = (regionX * 32 + chunkX) * 16 + blockX;
        y = chunkY * 16 + blockY;
        z = (regionZ * 32 + chunkZ) * 16 + blockZ;
    }
        
    /// <summary>
    /// Decomposes the absolute world coordinate into the given region, chunk and local coordinates. 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="regionX"></param>
    /// <param name="regionZ"></param>
    /// <param name="chunkX"></param>
    /// <param name="chunkY"></param>
    /// <param name="chunkZ"></param>
    /// <param name="blockX"></param>
    /// <param name="blockY"></param>
    /// <param name="blockZ"></param>
    public static void DecomposeCoordinate(int x, int y, int z, out int regionX, out int regionZ, out byte chunkX,
        out sbyte chunkY, out byte chunkZ, out byte blockX, out byte blockY, out byte blockZ)
    {
        blockX = (byte)(x & 0x0F);
        blockY = (byte)(y & 0x0F);
        blockZ = (byte)(z & 0x0F);
        chunkX = (byte)(((x - blockX) / 16) & 0x1F);
        chunkY = (sbyte)((y - blockY) / 16);
        chunkZ = (byte)(((z - blockZ) / 16) & 0x1F);
        regionX = (x - blockX - chunkX * 16) / 16 / 32;
        regionZ = (z - blockZ - chunkZ * 16) / 16 / 32;
    }
        
        
        
    #endregion Coordinates
        
    /// <summary>
    /// Dispose the regions
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        foreach (var region in m_Regions.Values)
        {
            await region.DisposeAsync();
        }
    }
}