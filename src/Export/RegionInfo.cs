using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MinecraftWebExporter.Serialization;

namespace MinecraftWebExporter.Export;

/// <summary>
/// A cache file that contains the chunk timestamps for each <see cref="ExportDetailLevel"/>.
/// This is used to keep track of modified chunks between exports or even continue an export after the exporter
/// was closed mid converting.
/// </summary>
public class RegionInfo
{
    /// <summary>
    /// The collection of views
    /// </summary>
    public List<RegionInfoDetailLevel> Views { get; set; } = new();

    /// <summary>
    /// Gets the view. Creates it if there is no view with the given filename.
    /// </summary>
    /// <param name="view"></param>
    /// <returns></returns>
    private RegionInfoDetailLevel GetOrCreateByDetailLevel(ExportDetailLevel view)
    {
        var viewInfo = Views.FirstOrDefault(v => v.Filename == view.Filename);
        if (viewInfo == null)
        {
            viewInfo = new RegionInfoDetailLevel()
            {
                Filename = view.Filename,
                Timestamps = new long[view.ChunksInRegion * view.ChunksInRegion],
            };
            Views.Add(viewInfo);
        }

        return viewInfo;
    }
        
    /// <summary>
    /// Gets the last update value for the given chunk in this region
    /// </summary>
    /// <param name="view"></param>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public long GetChunkTimestamp(ExportDetailLevel view, byte x, byte z)
    {
        var viewInfo = GetOrCreateByDetailLevel(view);
        if (viewInfo.Timestamps is null) return 0;
        return viewInfo.Timestamps[x + z * view.ChunksInRegion];
    }

    /// <summary>
    /// Sets the last update value for the given chunk in this region
    /// </summary>
    /// <param name="view"></param>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="value"></param>
    public void SetChunkTimestamp(ExportDetailLevel view, byte x, byte z, long value)
    {
        var viewInfo = GetOrCreateByDetailLevel(view);
        if (viewInfo.Timestamps is null) return;
        viewInfo.Timestamps[x + z * view.ChunksInRegion] = value;
    }

    #region Static

    /// <summary>
    /// Loads the region info from the given path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async Task<RegionInfo> LoadAsync(string path)
    {
        // Return an empty region info if the file doesn't exist
        if (!File.Exists(path))
            return new RegionInfo();
            
        // Opens the file
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            
        var regionInfo = await JsonSerializer.DeserializeAsync(stream, JsonContext.Default.RegionInfo) ?? new RegionInfo();
        return regionInfo;
    }
        
    /// <summary>
    /// Saves the region info to the given path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="regionInfo"></param>
    /// <returns></returns>
    public static async Task SaveAsync(string path, RegionInfo regionInfo)
    {
        // Opens the file
        await using var stream = new FileStream(path, FileMode.Create);
            
        await JsonSerializer.SerializeAsync(stream, regionInfo, JsonContext.Default.RegionInfo);
    }
        
    #endregion Static
}