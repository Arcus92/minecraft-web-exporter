using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MinecraftWebExporter.Export;
using MinecraftWebExporter.Minecraft;
using MinecraftWebExporter.Minecraft.World;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter
{
    /// <summary>
    /// A tool to export Minecraft worlds into a binary data format that can be read by the Minecraft web viewer.
    /// The main entrypoint will handle the command line parameters. The actual export is done in <see cref="MapExporter"/>.
    /// </summary>
    public static class Program
    {
        static async Task Main(string[] args)
        {
            // TODO: Clean up argument parser and optimize usage for 'non-developers':
            // - Auto-detect latest installed Minecraft version.
            // - Allow 2D --from and --to coordinates. In may cases you don't need to specify the y component.
            // - Specify material and texture subdirectory to support multiple Minecraft versions / texture packs.
            // - Validate the inputs and return better error messages.
            // - Show errors / warning if unsupported chunks (pre 1.13 or without height / lightmap) were detected.
            // - Add parameter to disable block lighting.
            // - Proper export progress bar. 
            // - Add help text and how-to-use.

            string? pathToMinecraft = null;
            string? pathToOutput = null;
            string? pathToWorld = null;
            string? worldAlias = null;
            int? numberOfThreads = default;
            Vector3? worldMin = default;
            Vector3? worldMax = default;
            Vector3? worldHome = default;
            var culling = true;
            int? cullingHeight = default;
            var pathToResourcePacks = new List<string>();

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                switch (arg.ToLowerInvariant())
                {
                    case "-m":
                    case "--minecraft":
                        if (i < args.Length - 1)
                        {
                            pathToMinecraft = args[++i];

                            // The path doesn't contain any path separators. 
                            // We will try to interpret this as version number and search in the default location.
                            if (pathToMinecraft.IndexOf('/') < 0 && pathToMinecraft.IndexOf('\\') < 0)
                            {
                                var pathToAppData =
                                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                                pathToMinecraft = Path.Combine(pathToAppData, ".minecraft", "versions", pathToMinecraft,
                                    $"{pathToMinecraft}.jar");
                            }
                        }
                        break;
                    
                    case "-o":
                    case "--output":
                        if (i < args.Length - 1)
                        {
                            pathToOutput = args[++i];
                        }
                        break;
                    
                    case "-w":
                    case "--world":
                        if (i < args.Length - 1)
                        {
                            pathToWorld = args[++i];
                            if (pathToWorld.EndsWith('/') || pathToWorld.EndsWith('\\'))
                            {
                                pathToWorld = pathToWorld.Substring(0, pathToWorld.Length - 1);
                            }
                        }
                        break;
                    
                    case "-a":
                    case "--alias":
                        if (i < args.Length - 1)
                        {
                            worldAlias = args[++i];
                        }
                        break;
                    
                    case "-r":
                    case "--resourcepack":
                        if (i < args.Length - 1)
                        {
                            var pathToResourcePack = args[++i];
                            pathToResourcePacks.Add(pathToResourcePack);
                        }
                        break;
                    
                    case "-t":
                    case "--threads":
                        if (i < args.Length - 1)
                        {
                            numberOfThreads = int.Parse(args[++i]);
                        }
                        break;
                    
                    case "--min":
                    case "--from":
                        if (i < args.Length - 3)
                        {
                            var v = new Vector3();
                            v.X = int.Parse(args[++i]);
                            v.Y = int.Parse(args[++i]);
                            v.Z = int.Parse(args[++i]);
                            worldMin = v;
                            
                        }
                        break;
                    case "--max":
                    case "--to":
                        if (i < args.Length - 3)
                        {
                            var v = new Vector3();
                            v.X = int.Parse(args[++i]);
                            v.Y = int.Parse(args[++i]);
                            v.Z = int.Parse(args[++i]);
                            worldMax = v;
                            
                        }
                        break;
                    case "--home":
                        if (i < args.Length - 3)
                        {
                            var v = new Vector3();
                            v.X = int.Parse(args[++i]);
                            v.Y = int.Parse(args[++i]);
                            v.Z = int.Parse(args[++i]);
                            worldHome = v;
                            
                        }
                        break;
                    
                    case "-c":
                    case "--culling":
                        if (i < args.Length - 1)
                        {
                            culling = bool.Parse(args[++i]);
                        }
                        break;
                    
                    case "--cullingheight":
                        if (i < args.Length - 1)
                        {
                            cullingHeight = int.Parse(args[++i]);
                        }
                        break;

                    default:
                        throw new ArgumentException($"Unknown parameter '{arg}'");
                }
            }

            if (string.IsNullOrEmpty(pathToMinecraft))
            {
                throw new ArgumentException("Minecraft path is not set. Use `--minecraft <path/version>`");
            }
            
            if (string.IsNullOrEmpty(pathToWorld))
            {
                throw new ArgumentException("World path is not set. Use `--world <path>`");
            }
            
            if (string.IsNullOrEmpty(pathToOutput))
            {
                throw new ArgumentException("Output path is not set. Use `--output <path>`");
            }

            // Calculate the center of the world bounds as home
            if (!worldHome.HasValue && worldMin.HasValue && worldMax.HasValue)
            {
                worldHome = (worldMin.Value + worldMax.Value) / 2;
            }
            
            // Collects all resource packs
            var resourcePacks = new ResourcePacks();
            resourcePacks.AddRange(pathToResourcePacks);
            resourcePacks.Add(pathToMinecraft);

            // Creates an asset manager
            var assets = new AssetManager(resourcePacks);

            // Loads the world
            var world = new World(assets, pathToWorld);

            var mapSettings = new ExportSettings()
            {
                Views = new []
                {
                    new ExportDetailLevel()
                    {
                        Filename = "h0",
                        Type = ExportDetailLevelType.Heightmap,
                        BlockSpan = 1,
                        ChunkSpan = 4,
                        Distance = 100,
                    },
                    new ExportDetailLevel()
                    {
                        Filename = "h1",
                        Type = ExportDetailLevelType.Heightmap,
                        BlockSpan = 2,
                        ChunkSpan = 8,
                        Distance = 250,
                    },
                    new ExportDetailLevel()
                    {
                        Filename = "h2",
                        Type = ExportDetailLevelType.Heightmap,
                        BlockSpan = 4,
                        ChunkSpan = 16,
                        Distance = 400,
                    },
                    new ExportDetailLevel()
                    {
                        Filename = "h3",
                        Type = ExportDetailLevelType.Heightmap,
                        BlockSpan = 8,
                        ChunkSpan = 32,
                        Distance = 800,
                    },
                    new ExportDetailLevel()
                    {
                        Filename = "b",
                        Type = ExportDetailLevelType.Blocks,
                        BlockSpan = 1,
                        ChunkSpan = 2,
                        Distance = 0,
                    }
                }
            };
            
            // Create the exporter
            var exporter = new MapExporter(assets);
            exporter.Output = pathToOutput;
            if (numberOfThreads.HasValue)
            {
                exporter.NumberOfThreads = numberOfThreads.Value;
            }

            if (cullingHeight.HasValue)
            {
                exporter.UndergroundCullingHeight = cullingHeight.Value;
            }
            
            exporter.WorldBorderMin = worldMin;
            exporter.WorldBorderMax = worldMax;
            exporter.WorldHome = worldHome;
            exporter.UndergroundCulling = culling;

            await exporter.ExportMaterials();
            await exporter.ExportWorld(mapSettings, world, worldAlias ?? world.Name);

            assets.Dispose();
        }

        
    }
}