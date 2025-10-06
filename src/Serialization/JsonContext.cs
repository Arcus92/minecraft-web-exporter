using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using MinecraftWebExporter.Export;
using MinecraftWebExporter.Minecraft.BlockStates;
using MinecraftWebExporter.Minecraft.Models;
using MinecraftWebExporter.Minecraft.Textures;

namespace MinecraftWebExporter.Serialization;

[JsonSourceGenerationOptions(
    AllowTrailingCommas = true, 
    ReadCommentHandling = JsonCommentHandling.Skip, 
    Converters = [
        typeof(DictionaryStringStringConverter)
    ])
]
[JsonSerializable(typeof(WorldInfo))]
[JsonSerializable(typeof(RegionInfo))]
[JsonSerializable(typeof(Model))]
[JsonSerializable(typeof(BlockState))]
[JsonSerializable(typeof(TextureMeta))]
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class JsonContext : JsonSerializerContext
{
    
}