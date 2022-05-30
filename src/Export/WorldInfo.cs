using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Export
{
    /// <summary>
    /// The world info file contains information for the viewer.
    /// </summary>
    public class WorldInfo
    {
        /// <summary>
        /// Gets and sets the home position
        /// </summary>
        [JsonPropertyName("home")]
        public Vector3 Home { get; set; }
        
        /// <summary>
        /// Gets and sets the list of material files to load
        /// </summary>
        [JsonPropertyName("materials")]
        public string[]? Materials { get; set; }

        /// <summary>
        /// Gets and sets the detail levels for this world
        /// </summary>
        [JsonPropertyName("views")]
        public ExportDetailLevel[]? Views { get; set; }

        #region Static

        /// <summary>
        /// Loads the world info from the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<WorldInfo> LoadAsync(string path)
        {
            // Return an empty region info if the file doesn't exist
            if (!File.Exists(path))
                return new WorldInfo();
            
            // Opens the file
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);

            var option = new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };
            
            var worldInfo = await JsonSerializer.DeserializeAsync<WorldInfo>(stream, option) ?? new WorldInfo();
            return worldInfo;
        }
        
        /// <summary>
        /// Saves the world info to the given path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="worldInfo"></param>
        /// <returns></returns>
        public static async Task SaveAsync(string path, WorldInfo worldInfo)
        {
            // Opens the file
            await using var stream = new FileStream(path, FileMode.Create);
            
            await JsonSerializer.SerializeAsync(stream, worldInfo);
        }
        
        #endregion Static
    }
}