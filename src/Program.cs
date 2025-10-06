using System.Threading.Tasks;
using MinecraftWebExporter.Export;
using MinecraftWebExporter.Minecraft;
using MinecraftWebExporter.Minecraft.World;

namespace MinecraftWebExporter;

/// <summary>
/// A tool to export Minecraft worlds into a binary data format that can be read by the Minecraft web viewer.
/// The main entrypoint will handle the command line parameters. The actual export is done in <see cref="WorldExporter"/>.
/// </summary>
public static class Program
{
    static async Task Main(string[] args)
    {
        var parameter = ProgramParameter.Parse(args);
        await RunWorldExport(parameter);
    }
        
    private static async Task RunWorldExport(ProgramParameter parameter)
    {
        // Collects all resource packs
        var resourcePacks = new ResourcePacks();
        resourcePacks.AddRange(parameter.PathToResourcePacks);
        resourcePacks.Add(parameter.PathToMinecraft);

        // Creates an asset manager
        var assets = new AssetManager(resourcePacks);

        // Loads the world
        var world = new World(assets, parameter.PathToWorld);
            
        // Create the exporter instance
        var exporter = new WorldExporter(assets, world, parameter.PathToOutput)
        {
            WorldAlias = parameter.WorldAlias,
            WorldBorderMin = parameter.WorldMin,
            WorldBorderMax = parameter.WorldMax,
            WorldHome = parameter.WorldHome,
            CaveCulling = parameter.CaveCulling,
        };

        if (parameter.CaveCullingHeight.HasValue)
        {
            exporter.CaveCullingHeight = parameter.CaveCullingHeight.Value;
        }
            
        if (parameter.NumberOfThreads.HasValue)
        {
            exporter.NumberOfThreads = parameter.NumberOfThreads.Value;
        }
            
        await exporter.ExportAsync();

        assets.Dispose();
    }
}