using System.Collections.Generic;
using System.Threading.Tasks;

namespace MinecraftWebExporter.Minecraft.Models.Cache;

/// <summary>
/// The calculated block faces
/// </summary>
public readonly struct CachedModel
{
    /// <summary>
    /// Gets and sets all faces for this model
    /// </summary>
    public CachedModelFace[]? Faces { get; init; }
        
    #region Static

    /// <summary>
    /// Calculate all faces
    /// </summary>
    /// <param name="assetManager"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async ValueTask<CachedModel> CreateAsync(IAssetManager assetManager, Model? model)
    {
        if (model?.Elements is null || model.Textures is null)
        {
            return default;
        }
            
        var faces = new List<CachedModelFace>();
        foreach (var element in model.Elements)
        {
            await CachedModelFace.CreateFacesAsync(assetManager, model, element, faces);
        }

        return new CachedModel()
        {
            Faces = faces.ToArray(),
        };
    }
        
    #endregion Static
}