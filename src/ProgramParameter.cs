using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MinecraftWebExporter.Structs;
using MinecraftWebExporter.Utils;

namespace MinecraftWebExporter;

public struct ProgramParameter
{
    /// <summary>
    /// Gets and sets the path to the minecraft jar file
    /// </summary>
    public string PathToMinecraft { get; set; }
    
    /// <summary>
    /// Gets and sets the path to the export output directory
    /// </summary>
    public string PathToOutput { get; set; }
    
    /// <summary>
    /// Gets and sets the path to the world directory
    /// </summary>
    public string PathToWorld { get; set; }
    
    /// <summary>
    /// Gets and sets an array with paths to the resource packs
    /// </summary>
    public string[] PathToResourcePacks { get; set; }

    /// <summary>
    /// Gets and sets the number of export threads
    /// </summary>
    public int? NumberOfThreads { get; set; }
    
    /// <summary>
    /// Gets ands the world alias for the export.
    /// If set, the world in the output directory will be renamed to this property.
    /// If not set, the default world name will be used.
    /// </summary>
    public string? WorldAlias { get; set; }
    
    /// <summary>
    /// Gets and sets the minimum world coordinate to export.
    /// Blocks below this point will not be exported.
    /// </summary>
    public Vector3? WorldMin { get; set; }
    
    /// <summary>
    /// Gets and sets the maximum world coordinate to export.
    /// Blocks above this point will not be exported.
    /// </summary>
    public Vector3? WorldMax { get; set; }
    
    /// <summary>
    /// Gets and sets the home position of the world. This is where the viewer camera spawns.
    /// </summary>
    public Vector3? WorldHome { get; set; }

    /// <summary>
    /// Gets and sets the underground culling height
    /// </summary>
    public bool CaveCulling { get; set; }
    
    /// <summary>
    /// Gets and sets the underground culling height
    /// </summary>
    public int? CaveCullingHeight { get; set; }
    
    
    /// <summary>
    /// Parses the command line arguments and returns the result.
    /// </summary>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public static ProgramParameter Parse(string[] arguments)
    {
        string? pathToMinecraft = null;
        string? pathToOutput = null;
        string? pathToWorld = null;
        var pathToResourcePacks = new List<string>();
        var caveCulling = true;
        int? caveCullingHeight = null;
        string? worldAlias = null;
        Vector3? worldMin = null;
        Vector3? worldMax = null;
        Vector3? worldHome = null;
        int? numberOfThreads = null;
        
        var parser = new CmdLineParser(arguments);
        while (parser.Next())
        {
            switch (parser.ParameterName.ToLowerInvariant())
            {
                case "-m":
                case "--minecraft":
                    ThrowIfValueCountIsNotEqual(parser, 1);
                    pathToMinecraft = parser.GetValue(0);
                    // The path doesn't contain any path separators. 
                    // We will try to interpret this as version number and search in the default location.
                    if (pathToMinecraft.IndexOf('/') < 0 && pathToMinecraft.IndexOf('\\') < 0)
                    {
                        var minecraftVersion = pathToMinecraft;
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            var pathToAppData =
                                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                            pathToMinecraft = Path.Combine(pathToAppData, ".minecraft");
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            var pathToUserDirectory =
                                Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                            pathToMinecraft = Path.Combine(pathToUserDirectory, ".minecraft");
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        {
                            var pathToUserDirectory =
                                Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                            pathToMinecraft = Path.Combine(pathToUserDirectory, "Library", "Application Support",
                                "minecraft");
                        }
                        else
                            throw new NotSupportedException(
                                "Cannot detect the Minecraft directory for your OS! Please specify the full path to the Minecraft jar file.");

                        pathToMinecraft = Path.Combine(pathToMinecraft, "versions", minecraftVersion,
                            $"{minecraftVersion}.jar");
                    }

                    break;
                
                case "-o":
                case "--output":
                    ThrowIfValueCountIsNotEqual(parser, 1);
                    pathToOutput = parser.GetValue(0);
                    break;
                
                case "-w":
                case "--world":
                    ThrowIfValueCountIsNotEqual(parser, 1);
                    pathToWorld = parser.GetValue(0);
                    if (pathToWorld.EndsWith('/') || pathToWorld.EndsWith('\\'))
                    {
                        pathToWorld = pathToWorld.Substring(0, pathToWorld.Length - 1);
                    }
                    break;
                
                case "-a":
                case "--alias":
                    ThrowIfValueCountIsNotEqual(parser, 1);
                    worldAlias = parser.GetValue(0);
                    break;
                
                case "-r":
                case "--resourcepack":
                    foreach (var pathToResourcePack in parser.Values)
                    {
                        pathToResourcePacks.Add(pathToResourcePack);
                    }
                    break;
                
                case "-t":
                case "--threads":
                    ThrowIfValueCountIsNotEqual(parser, 1);
                    numberOfThreads = int.Parse(parser.GetValue(0));
                    break;
                
                case "--min":
                case "--from":
                    worldMin = ReadVector3WithOptionalYComponent(parser, -64);
                    break;
                
                case "--max":
                case "--to":
                    worldMax = ReadVector3WithOptionalYComponent(parser, 255);
                    break;
                
                case "--home":
                    worldHome = ReadVector3WithOptionalYComponent(parser, 0);
                    break;
                
                case "-c":
                case "--culling":
                    ThrowIfValueCountIsNotEqual(parser, 1);
                    caveCulling = bool.Parse(parser.GetValue(0));
                    break;
                    
                case "--culling-height":
                    ThrowIfValueCountIsNotEqual(parser, 1);
                    caveCullingHeight = int.Parse(parser.GetValue(0));
                    break;
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
        
        return new ProgramParameter()
        {
            PathToMinecraft = pathToMinecraft,
            PathToWorld = pathToWorld,
            PathToOutput = pathToOutput,
            PathToResourcePacks = pathToResourcePacks.ToArray(),
            CaveCulling = caveCulling,
            CaveCullingHeight = caveCullingHeight,
            WorldAlias = worldAlias,
            WorldMin = worldMin,
            WorldMax = worldMax,
            WorldHome = worldHome,
            NumberOfThreads = numberOfThreads,
        };
    }

    private static Vector3 ReadVector3WithOptionalYComponent(CmdLineParser parser, float y)
    {
        if (parser.ValueCount == 3)
        {
            return new Vector3(
                int.Parse(parser.GetValue(0)), 
                int.Parse(parser.GetValue(1)),
                int.Parse(parser.GetValue(2)));
        }
        if (parser.ValueCount == 2)
        {
            return new Vector3(
                int.Parse(parser.GetValue(0)), 
                y,
                int.Parse(parser.GetValue(1)));
        }
        throw new ArgumentException($"Invalid number of values for argument '{parser.ParameterName}'. Given: {parser.ValueCount} - Expected: 2 or 3");
    }
    private static void ThrowIfValueCountIsNotEqual(CmdLineParser parser, int expected)
    {
        if (parser.ValueCount != expected) throw new ArgumentException($"Invalid number of values for argument '{parser.ParameterName}'. Given: {parser.ValueCount} - Expected: {expected}");
    }
}