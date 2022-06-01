using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.Models.Cache
{
    /// <summary>
    /// The calculated face
    /// </summary>
    public readonly struct CachedModelFace
    {
        /// <summary>
        /// Gets the vertex a
        /// </summary>
        public Vector3 VertexA { get; init; }
        
        /// <summary>
        /// Gets the vertex b
        /// </summary>
        public Vector3 VertexB { get; init; }
        
        /// <summary>
        /// Gets the vertex c
        /// </summary>
        public Vector3 VertexC { get; init; }
        
        /// <summary>
        /// Gets the vertex d
        /// </summary>
        public Vector3 VertexD { get; init; }
        
        /// <summary>
        /// Gets the uv
        /// </summary>
        public Vector4 Uv { get; init; }

        /// <summary>
        /// Gets the texture rotation
        /// </summary>
        public int? Rotation { get; init; }

        /// <summary>
        /// Gets the normal of the face
        /// </summary>
        public Vector3 Normal { get; init; }

        /// <summary>
        /// Gets the texture name
        /// </summary>
        public AssetIdentifier Texture { get; init; }

        /// <summary>
        /// Gets the original face direction
        /// </summary>
        public Direction Direction { get; init; }
        
        /// <summary>
        /// Gets the culling direction
        /// </summary>
        public Direction? CullFace { get; init; }

        /// <summary>
        /// Gets the tint index
        /// </summary>
        public int? TintIndex { get; init; }
        
        /// <summary>
        /// Gets if this block is transparent
        /// </summary>
        public bool Transparent { get; init; }
        
        #region Static
        
        /// <summary>
        /// Returns the default uv
        /// </summary>
        /// <param name="element"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static Vector4 GetDefaultUv(ModelElement element, Direction direction)
        {
            switch (direction)
            {
                case Direction.Down:
                    return new Vector4()
                    {
                        X = element.From.X,
                        Y = element.From.Z,
                        Z = element.To.X,
                        W = element.To.Z,
                    };
                case Direction.Up:
                    return new Vector4()
                    {
                        X = element.From.X,
                        Y = element.From.Z,
                        Z = element.To.X,
                        W = element.To.Z,
                    };
                case Direction.North:
                    return new Vector4()
                    {
                        X = element.From.X,
                        Y = element.From.Y,
                        Z = element.To.X,
                        W = element.To.Y,
                    };
                case Direction.South:
                    return new Vector4()
                    {
                        X = element.From.X,
                        Y = element.From.Y,
                        Z = element.To.X,
                        W = element.To.Y,
                    };
                case Direction.West:
                    return new Vector4()
                    {
                        X = element.From.Z,
                        Y = element.From.Y,
                        Z = element.To.Z,
                        W = element.To.Y,
                    };
                case Direction.East:
                    return new Vector4()
                    {
                        X = element.From.Z,
                        Y = element.From.Y,
                        Z = element.To.Z,
                        W = element.To.Y,
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
        
        /// <summary>
        /// Creates the face
        /// </summary>
        /// <param name="assetManager"></param>
        /// <param name="model"></param>
        /// <param name="element"></param>
        /// <param name="direction"></param>
        /// <param name="face"></param>
        /// <returns></returns>
        public static async ValueTask<CachedModelFace> CreateAsync(AssetManager assetManager, Model model, ModelElement element, 
            Direction direction, ModelElementFace face)
        {
            Vector4 uv;
            if (face.Uv.HasValue)
            {
                uv = face.Uv.Value;
            }
            else
            {
                uv = GetDefaultUv(element, direction);
            }
            
            // Rotate the position
            Vector3 RotatePosition(Vector3 position)
            {
                position = element.Rotation?.TransformPoint(position) ?? position;

                return new Vector3()
                {
                    X = position.X / 16f,
                    Y = position.Y / 16f,
                    Z = position.Z / 16f,
                };
            }
            
            // Rotate the normal
            Vector3 RotateNormal(Vector3 normal)
            {
                return element.Rotation?.TransformVector(normal) ?? normal;
            }

            var textureAsset = model.Textures?.Get(face.Texture) ?? default;
            var texture = await assetManager.TextureCache.GetAsync(textureAsset);
            var transparent = texture.IsAreaTransparent(uv);

            Vector3 vertexA, vertexB, vertexC, vertexD, normal;
            switch (direction)
            {
                case Direction.Down:
                    normal = RotateNormal(new Vector3() {X = 0.0f, Y = -1.0f, Z = 0.0f});
                    vertexA = RotatePosition(new Vector3() {X = element.From.X, Y = element.From.Y, Z = element.To.Z});
                    vertexB = RotatePosition(new Vector3() {X = element.From.X, Y = element.From.Y, Z = element.From.Z});
                    vertexC = RotatePosition(new Vector3() {X = element.To.X, Y = element.From.Y, Z = element.From.Z});
                    vertexD = RotatePosition(new Vector3() {X = element.To.X, Y = element.From.Y, Z = element.To.Z});
                    break;
                case Direction.Up:
                    normal = RotateNormal(new Vector3() {X = 0.0f, Y = 1.0f, Z = 0.0f});
                    vertexA = RotatePosition(new Vector3() {X = element.From.X, Y = element.To.Y, Z = element.From.Z});
                    vertexB = RotatePosition(new Vector3() {X = element.From.X, Y = element.To.Y, Z = element.To.Z});
                    vertexC = RotatePosition(new Vector3() {X = element.To.X, Y = element.To.Y, Z = element.To.Z});
                    vertexD = RotatePosition(new Vector3() {X = element.To.X, Y = element.To.Y, Z = element.From.Z});
                    break;
                case Direction.North:
                    normal = RotateNormal(new Vector3() {X = 0.0f, Y = 0.0f, Z = -1.0f});
                    vertexA = RotatePosition(new Vector3() {X = element.To.X, Y = element.To.Y, Z = element.From.Z});
                    vertexB = RotatePosition(new Vector3() {X = element.To.X, Y = element.From.Y, Z = element.From.Z});
                    vertexC = RotatePosition(new Vector3() {X = element.From.X, Y = element.From.Y, Z = element.From.Z});
                    vertexD = RotatePosition(new Vector3() {X = element.From.X, Y = element.To.Y, Z = element.From.Z });
                    break;
                case Direction.South:
                    normal = RotateNormal(new Vector3() {X = 0.0f, Y = 0.0f, Z = 1.0f});
                    vertexA = RotatePosition(new Vector3() {X = element.From.X, Y = element.To.Y, Z = element.To.Z});
                    vertexB = RotatePosition(new Vector3() {X = element.From.X, Y = element.From.Y, Z = element.To.Z});
                    vertexC = RotatePosition(new Vector3() {X = element.To.X, Y = element.From.Y, Z = element.To.Z});
                    vertexD = RotatePosition(new Vector3() {X = element.To.X, Y = element.To.Y, Z = element.To.Z});
                    break; 
                case Direction.West:
                    normal = RotateNormal(new Vector3() {X = -1.0f, Y = 0.0f, Z = 0.0f});
                    vertexA = RotatePosition(new Vector3() {X = element.From.X, Y = element.To.Y, Z = element.From.Z });
                    vertexB = RotatePosition(new Vector3() {X = element.From.X, Y = element.From.Y, Z = element.From.Z});
                    vertexC = RotatePosition(new Vector3() {X = element.From.X, Y = element.From.Y, Z = element.To.Z});
                    vertexD = RotatePosition(new Vector3() {X = element.From.X, Y = element.To.Y, Z = element.To.Z});
                    break;
                case Direction.East:
                    normal = RotateNormal(new Vector3() {X = 1.0f, Y = 0.0f, Z = 0.0f});
                    vertexA = RotatePosition(new Vector3() {X = element.To.X, Y = element.To.Y, Z = element.To.Z});
                    vertexB = RotatePosition(new Vector3() {X = element.To.X, Y = element.From.Y, Z = element.To.Z});
                    vertexC = RotatePosition(new Vector3() {X = element.To.X, Y = element.From.Y, Z = element.From.Z});
                    vertexD = RotatePosition(new Vector3() {X = element.To.X, Y = element.To.Y, Z = element.From.Z});
                    break; 
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unknown direction!");
            }
            
            // Rotate the texture uvs by switching the vertex order
            if (face.Rotation.HasValue)
            {
                var rotation = face.Rotation.Value / 90;
                switch (rotation)
                {
                    case 1:
                        (vertexA, vertexB, vertexC, vertexD) = (vertexB, vertexC, vertexD, vertexA);
                        break;
                    case 2:
                        (vertexA, vertexB, vertexC, vertexD) = (vertexC, vertexD, vertexA, vertexB);
                        break;
                    case 3:
                        (vertexA, vertexB, vertexC, vertexD) = (vertexD, vertexA, vertexB, vertexC);
                        break;
                }
            }
            
            return new CachedModelFace()
            {
                Direction = direction,
                Texture = textureAsset,
                Normal = normal,
                CullFace = face.CullFace,
                TintIndex = face.TintIndex,
                Transparent = transparent,
                Rotation = face.Rotation,

                VertexA = vertexA,
                VertexB = vertexB,
                VertexC = vertexC,
                VertexD = vertexD,
                
                Uv = uv,
            };
        }

        /// <summary>
        /// Returns all faces from the element
        /// </summary>
        /// <param name="assetManager"></param>
        /// <param name="model"></param>
        /// <param name="element"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static async ValueTask CreateFacesAsync(AssetManager assetManager, Model model, ModelElement element, List<CachedModelFace> list)
        {
            if (element.Faces is null)
                return;
            foreach (var (direction, face) in element.Faces.GetFaces())
            {
                list.Add(await CreateAsync(assetManager, model, element, direction, face));
            }
        }

        #endregion Static
    }
}