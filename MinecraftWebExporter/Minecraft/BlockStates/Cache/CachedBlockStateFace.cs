using System;
using MinecraftWebExporter.Minecraft.Models;
using MinecraftWebExporter.Minecraft.Models.Cache;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.BlockStates.Cache
{
    /// <summary>
    /// This is a copy of <see cref="CachedModelFace"/> but a few blockAsset properties are already calculated.
    /// </summary>
    public readonly struct CachedBlockStateFace
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
        /// Gets the tint type
        /// </summary>
        public ModelTintType TintType { get; init; }

        /// <summary>
        /// Gets the fluid type
        /// </summary>
        public ModelFluidType? FluidType { get; init; }

        /// <summary>
        /// Gets if this blockAsset is transparent
        /// </summary>
        public bool Transparent { get; init; }

        #region Bound
        
        /// <summary>
        /// Returns if this face is fully covered by the given blockAsset
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public bool IsCovered(CachedBlockState block)
        {
            if (!CullFace.HasValue)
                return false;

            // The other block doesn't have any geometry
            if (block.Variants.Length == 0 || block.DefaultVariant.Faces is null || block.DefaultVariant.Faces.Length == 0)
                return false;

            var direction = CullFace.Value;
            var invertDirection = InvertDirection(direction);
            var (minThis, maxThis) = GetBounds(direction);


            Vector2 minOthers = default;
            Vector2 maxOthers = default;
            var any = false;
            foreach (var other in block.DefaultVariant.Faces)
            {
                // Only check the opposite cull direction  
                if (other.CullFace == invertDirection)
                {
                    // Blocks are only covered by transparent blocks they use the same texture
                    if (!other.Transparent || Texture == other.Texture)
                    {
                        var (minOther, maxOther) = other.GetBounds(direction);

                        // Check if this face alone will cover the whole face alone
                        if (minThis.X >= minOther.X && maxThis.X <= maxOther.X && minThis.Y >= minOther.Y &&
                            maxThis.Y <= maxOther.Y)
                            return true;

                        if (!any)
                        {
                            minOthers = minOther;
                            maxOthers = maxOther;
                            any = true;
                        }
                        else
                        {
                            if (!ExtendBound(ref minOthers, ref maxOthers, minOther, maxOther))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (!any)
                return false;

            // Check if the combined area covered the face
            return minThis.X >= minOthers.X && maxThis.X <= maxOthers.X && minThis.Y >= minOthers.Y &&
                   maxThis.Y <= maxOthers.Y;
        }


        /// <summary>
        /// Returns the bounds of this face
        /// </summary>
        /// <returns></returns>
        public (Vector3, Vector3) GetBounds()
        {
            var min = new Vector3() {X = float.MaxValue, Y = float.MaxValue, Z = float.MaxValue};
            var max = new Vector3() {X = float.MinValue, Y = float.MinValue, Z = float.MinValue};

            EncapsulateBounds(ref min, ref max);

            return (min, max);
        }

        /// <summary>
        /// Returns the bounds of this face seen by the given <paramref name="direction"/>.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public (Vector2, Vector2) GetBounds(Direction direction)
        {
            var min = new Vector2() {X = float.MaxValue, Y = float.MaxValue};
            var max = new Vector2() {X = float.MinValue, Y = float.MinValue};

            EncapsulateBounds(direction, ref min, ref max);

            return (min, max);
        }

        /// <summary>
        /// Encapsulate the four vertices in the <paramref name="min"/> and <paramref name="max"/> bound.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void EncapsulateBounds(ref Vector3 min, ref Vector3 max)
        {
            if (VertexA.X < min.X) min.X = VertexA.X;
            if (VertexB.X < min.X) min.X = VertexB.X;
            if (VertexC.X < min.X) min.X = VertexC.X;
            if (VertexD.X < min.X) min.X = VertexD.X;

            if (VertexA.Y < min.Y) min.Y = VertexA.Y;
            if (VertexB.Y < min.Y) min.Y = VertexB.Y;
            if (VertexC.Y < min.Y) min.Y = VertexC.Y;
            if (VertexD.Y < min.Y) min.Y = VertexD.Y;

            if (VertexA.Z < min.Z) min.Z = VertexA.Z;
            if (VertexB.Z < min.Z) min.Z = VertexB.Z;
            if (VertexC.Z < min.Z) min.Z = VertexC.Z;
            if (VertexD.Z < min.Z) min.Z = VertexD.Z;

            if (VertexA.X > max.X) max.X = VertexA.X;
            if (VertexB.X > max.X) max.X = VertexB.X;
            if (VertexC.X > max.X) max.X = VertexC.X;
            if (VertexD.X > max.X) max.X = VertexD.X;

            if (VertexA.Y > max.Y) max.Y = VertexA.Y;
            if (VertexB.Y > max.Y) max.Y = VertexB.Y;
            if (VertexC.Y > max.Y) max.Y = VertexC.Y;
            if (VertexD.Y > max.Y) max.Y = VertexD.Y;

            if (VertexA.Z > max.Z) max.Z = VertexA.Z;
            if (VertexB.Z > max.Z) max.Z = VertexB.Z;
            if (VertexC.Z > max.Z) max.Z = VertexC.Z;
            if (VertexD.Z > max.Z) max.Z = VertexD.Z;
        }

        /// <summary>
        /// Encapsulate the four vertices in the <paramref name="min"/> and <paramref name="max"/> 2d bound seen by the given <paramref name="direction"/>.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void EncapsulateBounds(Direction direction, ref Vector2 min, ref Vector2 max)
        {
            switch (direction)
            {
                case Direction.East:
                case Direction.West:
                    EncapsulateBoundsAxisX(ref min, ref max);
                    break;
                case Direction.Down:
                case Direction.Up:
                    EncapsulateBoundsAxisY(ref min, ref max);
                    break;
                case Direction.North:
                case Direction.South:
                    EncapsulateBoundsAxisZ(ref min, ref max);
                    break;
            }
        }

        /// <summary>
        /// Encapsulate the four vertices in the <paramref name="min"/> and <paramref name="max"/> 2d bound seen by the x axis.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void EncapsulateBoundsAxisX(ref Vector2 min, ref Vector2 max)
        {
            if (VertexA.Z < min.X) min.X = VertexA.Z;
            if (VertexB.Z < min.X) min.X = VertexB.Z;
            if (VertexC.Z < min.X) min.X = VertexC.Z;
            if (VertexD.Z < min.X) min.X = VertexD.Z;

            if (VertexA.Y < min.Y) min.Y = VertexA.Y;
            if (VertexB.Y < min.Y) min.Y = VertexB.Y;
            if (VertexC.Y < min.Y) min.Y = VertexC.Y;
            if (VertexD.Y < min.Y) min.Y = VertexD.Y;

            if (VertexA.Z > max.X) max.X = VertexA.Z;
            if (VertexB.Z > max.X) max.X = VertexB.Z;
            if (VertexC.Z > max.X) max.X = VertexC.Z;
            if (VertexD.Z > max.X) max.X = VertexD.Z;

            if (VertexA.Y > max.Y) max.Y = VertexA.Y;
            if (VertexB.Y > max.Y) max.Y = VertexB.Y;
            if (VertexC.Y > max.Y) max.Y = VertexC.Y;
            if (VertexD.Y > max.Y) max.Y = VertexD.Y;
        }

        /// <summary>
        /// Encapsulate the four vertices in the <paramref name="min"/> and <paramref name="max"/> 2d bound seen by the y axis.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void EncapsulateBoundsAxisY(ref Vector2 min, ref Vector2 max)
        {
            if (VertexA.X < min.X) min.X = VertexA.X;
            if (VertexB.X < min.X) min.X = VertexB.X;
            if (VertexC.X < min.X) min.X = VertexC.X;
            if (VertexD.X < min.X) min.X = VertexD.X;

            if (VertexA.Z < min.Y) min.Y = VertexA.Z;
            if (VertexB.Z < min.Y) min.Y = VertexB.Z;
            if (VertexC.Z < min.Y) min.Y = VertexC.Z;
            if (VertexD.Z < min.Y) min.Y = VertexD.Z;

            if (VertexA.X > max.X) max.X = VertexA.X;
            if (VertexB.X > max.X) max.X = VertexB.X;
            if (VertexC.X > max.X) max.X = VertexC.X;
            if (VertexD.X > max.X) max.X = VertexD.X;

            if (VertexA.Z > max.Y) max.Y = VertexA.Z;
            if (VertexB.Z > max.Y) max.Y = VertexB.Z;
            if (VertexC.Z > max.Y) max.Y = VertexC.Z;
            if (VertexD.Z > max.Y) max.Y = VertexD.Z;
        }

        /// <summary>
        /// Encapsulate the four vertices in the <paramref name="min"/> and <paramref name="max"/> 2d bound seen by the z axis.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void EncapsulateBoundsAxisZ(ref Vector2 min, ref Vector2 max)
        {
            if (VertexA.X < min.X) min.X = VertexA.X;
            if (VertexB.X < min.X) min.X = VertexB.X;
            if (VertexC.X < min.X) min.X = VertexC.X;
            if (VertexD.X < min.X) min.X = VertexD.X;

            if (VertexA.Y < min.Y) min.Y = VertexA.Y;
            if (VertexB.Y < min.Y) min.Y = VertexB.Y;
            if (VertexC.Y < min.Y) min.Y = VertexC.Y;
            if (VertexD.Y < min.Y) min.Y = VertexD.Y;

            if (VertexA.X > max.X) max.X = VertexA.X;
            if (VertexB.X > max.X) max.X = VertexB.X;
            if (VertexC.X > max.X) max.X = VertexC.X;
            if (VertexD.X > max.X) max.X = VertexD.X;

            if (VertexA.Y > max.Y) max.Y = VertexA.Y;
            if (VertexB.Y > max.Y) max.Y = VertexB.Y;
            if (VertexC.Y > max.Y) max.Y = VertexC.Y;
            if (VertexD.Y > max.Y) max.Y = VertexD.Y;
        }

        /// <summary>
        /// Extends the bounding box (<paramref name="min"/> and <paramref name="max"/>) by <paramref name="maxOther"/>
        /// and <paramref name="maxOther"/> but only if the combined area is a rectangle.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="minOther"></param>
        /// <param name="maxOther"></param>
        /// <returns></returns>
        public static bool ExtendBound(ref Vector2 min, ref Vector2 max, Vector2 minOther, Vector2 maxOther)
        {
            var valid = false;
            if (min.X == minOther.X && max.X == maxOther.X)
            {
                if (minOther.Y < min.Y && maxOther.Y >= min.Y)
                {
                    min.Y = minOther.Y;
                    valid = true;
                }

                if (maxOther.Y > max.Y && minOther.Y <= max.Y)
                {
                    max.Y = maxOther.Y;
                    valid = true;
                }
            }
            else if (min.Y == minOther.Y && max.Y == maxOther.Y)
            {
                if (minOther.X < min.X && maxOther.X >= min.X)
                {
                    min.X = minOther.X;
                    valid = true;
                }

                if (maxOther.X > max.X && minOther.X <= max.X)
                {
                    max.X = maxOther.X;
                    valid = true;
                }
            }

            return valid;
        }
        
        #endregion Bound
        
        #region Static

        /// <summary>
        /// Creates the blockAsset state face by using the given block and the given model face.
        /// </summary>
        /// <param name="blockAsset"></param>
        /// <param name="face"></param>
        /// <returns></returns>
        public static CachedBlockStateFace Create(AssetIdentifier blockAsset, CachedModelFace face)
        {
            var tintType = ModelTintType.Default;

            // There is this thing 'tintIndex'. This allows one block to have multiple tinted faces but currently
            // Minecraft doesn't use it. I simply check if this value is set ot not, because I don't know the intended
            // order of the the indexes. Since it is not used in vanilla Minecraft, this should be fine. 
            // Also the tint-able blocks are hard-coded. Maybe I could block-tags in future.
            if (face.TintIndex.HasValue)
            {
                switch (blockAsset.Name)
                {
                    case "grass_block":
                        tintType = ModelTintType.Grass;
                        break;
                    case "vine" or "grass" or "tall_grass" or "fern" or "large_fern" or "blockAsset/fern" or "lily_pad"
                        or "melon_stem" or "attached_melon_stem" or "pumpkin_stem" or "attached_pumpkin_stem"
                        or "oak_leaves" or "acacia_leaves" or "birch_leaves" or "dark_oak_leaves" or "jungle_leaves" or "spruce_leaves":
                        tintType = ModelTintType.Foliage;
                        break;
                    case "water_cauldron":
                        tintType = ModelTintType.Water;
                        break;
                }
            }

            return new CachedBlockStateFace()
            {
                Direction = face.Direction,
                CullFace = face.CullFace,
                Normal = face.Normal,
                Texture = face.Texture,
                Transparent = face.Transparent,
                VertexA = face.VertexA,
                VertexB = face.VertexB,
                VertexC = face.VertexC,
                VertexD = face.VertexD,
                Uv = face.Uv,
                TintType = tintType,
            };
        }
        
        /// <summary>
        /// Returns a copy of the given <paramref name="face"/> with the given rotation in deg.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="uvLock"></param>
        /// <returns></returns>
        public static CachedBlockStateFace Rotate(CachedBlockStateFace face, float x, float y, bool uvLock)
        {
            var rx = (byte) (x / 90);
            var ry = (byte) (y / 90);
            return Rotate(face, rx, ry, uvLock);
        }

        /// <summary>
        /// Returns a copy of the given <paramref name="face"/> with the given rotation in 90°.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="uvLock"></param>
        /// <returns></returns>
        public static CachedBlockStateFace Rotate(CachedBlockStateFace face, byte x, byte y, bool uvLock)
        {
            // No rotation
            if (x == 0 && y == 0)
                return face;

            var direction = face.Direction;
            var normal = face.Normal;
            var cullFace = face.CullFace;
            
            var vertexA = face.VertexA;
            var vertexB = face.VertexB;
            var vertexC = face.VertexC;
            var vertexD = face.VertexD;

            var uv = face.Uv;

            // Rotate X
            switch (x)
            {
                case 1: // 90°
                    vertexA = new Vector3() { X = vertexA.X, Y = vertexA.Z, Z = 1 - vertexA.Y };
                    vertexB = new Vector3() { X = vertexB.X, Y = vertexB.Z, Z = 1 - vertexB.Y };
                    vertexC = new Vector3() { X = vertexC.X, Y = vertexC.Z, Z = 1 - vertexC.Y };
                    vertexD = new Vector3() { X = vertexD.X, Y = vertexD.Z, Z = 1 - vertexD.Y };
                    normal = new Vector3() { X = normal.X, Y = normal.Z, Z = -normal.Y };

                    switch (cullFace)
                    {
                        case Direction.North: 
                            cullFace = Direction.Down;
                            break;
                        case Direction.Up: 
                            cullFace = Direction.North;
                            break;
                        case Direction.South: 
                            cullFace = Direction.Up;
                            break;
                        case Direction.Down: 
                            cullFace = Direction.South;
                            break;
                    }
                    // Lock uv
                    if (uvLock && direction is Direction.East or Direction.West)
                    {
                        (vertexA, vertexB, vertexC, vertexD) = (vertexB, vertexC, vertexD, vertexA);
                        uv = new Vector4()
                        {
                            X = uv.Y,
                            Y = uv.Z,
                            Z = uv.W,
                            W = uv.X,
                        };
                    }
                    break;
                
                case 2: // 180°
                    vertexA = new Vector3() { X = vertexA.X, Y = 1 - vertexA.Y, Z = 1 - vertexA.Z };
                    vertexB = new Vector3() { X = vertexB.X, Y = 1 - vertexB.Y, Z = 1 - vertexB.Z };
                    vertexC = new Vector3() { X = vertexC.X, Y = 1 - vertexC.Y, Z = 1 - vertexC.Z };
                    vertexD = new Vector3() { X = vertexD.X, Y = 1 - vertexD.Y, Z = 1 - vertexD.Z };
                    normal = new Vector3() { X = normal.X, Y = -normal.Y, Z = -normal.Z };

                    switch (cullFace)
                    {
                        case Direction.North: 
                            cullFace = Direction.South;
                            break;
                        case Direction.Up: 
                            cullFace = Direction.Down;
                            break;
                        case Direction.South: 
                            cullFace = Direction.North;
                            break;
                        case Direction.Down: 
                            cullFace = Direction.Up;
                            break;
                    }
                    // Lock uv
                    if (uvLock && direction is Direction.East or Direction.West)
                    {
                        (vertexA, vertexB, vertexC, vertexD) = (vertexC, vertexD, vertexA, vertexB);
                    }
                    break;
                
                case 3: // 270°
                    vertexA = new Vector3() { X = vertexA.X, Y = 1 - vertexA.Z, Z = vertexA.Y };
                    vertexB = new Vector3() { X = vertexB.X, Y = 1 - vertexB.Z, Z = vertexB.Y };
                    vertexC = new Vector3() { X = vertexC.X, Y = 1 - vertexC.Z, Z = vertexC.Y };
                    vertexD = new Vector3() { X = vertexD.X, Y = 1 - vertexD.Z, Z = vertexD.Y };
                    normal = new Vector3() { X = normal.X, Y = -normal.Z, Z = normal.Y };

                    switch (cullFace)
                    {
                        case Direction.North: 
                            cullFace = Direction.Up;
                            break;
                        case Direction.Up: 
                            cullFace = Direction.South;
                            break;
                        case Direction.South: 
                            cullFace = Direction.Down;
                            break;
                        case Direction.Down: 
                            cullFace = Direction.North;
                            break;
                    }
                    // Lock uv
                    if (uvLock && direction is Direction.East or Direction.West)
                    {
                        (vertexA, vertexB, vertexC, vertexD) = (vertexD, vertexA, vertexB, vertexC);
                        uv = new Vector4()
                        {
                            X = uv.Y,
                            Y = uv.Z,
                            Z = uv.W,
                            W = uv.X,
                        };
                    }
                    break;
            }
            
            // Rotate Y
            switch (y)
            {
                case 1: // 90°
                    vertexA = new Vector3() { X = 1 - vertexA.Z, Y = vertexA.Y, Z = vertexA.X };
                    vertexB = new Vector3() { X = 1 - vertexB.Z, Y = vertexB.Y, Z = vertexB.X };
                    vertexC = new Vector3() { X = 1 - vertexC.Z, Y = vertexC.Y, Z = vertexC.X };
                    vertexD = new Vector3() { X = 1 - vertexD.Z, Y = vertexD.Y, Z = vertexD.X };
                    normal = new Vector3() { X = -normal.Z, Y = normal.Y, Z = normal.X };
                    
                    switch (cullFace)
                    {
                        case Direction.North: 
                            cullFace = Direction.East;
                            break;
                        case Direction.East: 
                            cullFace = Direction.South;
                            break;
                        case Direction.South: 
                            cullFace = Direction.West;
                            break;
                        case Direction.West: 
                            cullFace = Direction.North;
                            break;
                    }
                    
                    // Lock uv
                    if (uvLock && direction is Direction.Up or Direction.Down)
                    {
                        (vertexA, vertexB, vertexC, vertexD) = (vertexB, vertexC, vertexD, vertexA);
                        uv = new Vector4()
                        {
                            X = uv.Y,
                            Y = uv.Z,
                            Z = uv.W,
                            W = uv.X,
                        };
                    }
                    
                    break;
                
                case 2: // 180°
                    vertexA = new Vector3() { X = 1 - vertexA.X, Y = vertexA.Y, Z = 1 - vertexA.Z };
                    vertexB = new Vector3() { X = 1 - vertexB.X, Y = vertexB.Y, Z = 1 - vertexB.Z };
                    vertexC = new Vector3() { X = 1 - vertexC.X, Y = vertexC.Y, Z = 1 - vertexC.Z };
                    vertexD = new Vector3() { X = 1 - vertexD.X, Y = vertexD.Y, Z = 1 - vertexD.Z };
                    normal = new Vector3() { X = -normal.X, Y = normal.Y, Z = -normal.Z };

                    switch (cullFace)
                    {
                        case Direction.North: 
                            cullFace = Direction.South;
                            break;
                        case Direction.East: 
                            cullFace = Direction.West;
                            break;
                        case Direction.South: 
                            cullFace = Direction.North;
                            break;
                        case Direction.West: 
                            cullFace = Direction.East;
                            break;
                    }
                    
                    // Lock uv
                    if (uvLock && direction is Direction.Up or Direction.Down)
                    {
                        (vertexA, vertexB, vertexC, vertexD) = (vertexC, vertexD, vertexA, vertexB);
                    }
                    
                    break;

                case 3: // 270°
                    vertexA = new Vector3() { X = vertexA.Z, Y = vertexA.Y, Z = 1 - vertexA.X };
                    vertexB = new Vector3() { X = vertexB.Z, Y = vertexB.Y, Z = 1 - vertexB.X };
                    vertexC = new Vector3() { X = vertexC.Z, Y = vertexC.Y, Z = 1 - vertexC.X };
                    vertexD = new Vector3() { X = vertexD.Z, Y = vertexD.Y, Z = 1 - vertexD.X };
                    normal = new Vector3() { X = normal.Z, Y = normal.Y, Z = -normal.X };

                    switch (cullFace)
                    {
                        case Direction.North: 
                            cullFace = Direction.West;
                            break;
                        case Direction.East: 
                            cullFace = Direction.North;
                            break;
                        case Direction.South: 
                            cullFace = Direction.East;
                            break;
                        case Direction.West: 
                            cullFace = Direction.South;
                            break;
                    }
                    
                    // Lock uv
                    if (uvLock && direction is Direction.Up or Direction.Down)
                    {
                        (vertexA, vertexB, vertexC, vertexD) = (vertexD, vertexA, vertexB, vertexC);
                        uv = new Vector4()
                        {
                            X = uv.Y,
                            Y = uv.Z,
                            Z = uv.W,
                            W = uv.X,
                        };
                    }
                    
                    break;
            }

            // Creates a new face
            return new CachedBlockStateFace()
            {
                Direction = direction,
                Texture = face.Texture,
                TintType = face.TintType,
                Transparent = face.Transparent,

                Normal = normal,
                CullFace = cullFace,
                
                VertexA = vertexA,
                VertexB = vertexB,
                VertexC = vertexC,
                VertexD = vertexD,
                
                Uv = uv,
            };
        }
        
        /// <summary>
        /// Inverts the given direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static Direction InvertDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Down:
                    return Direction.Up;
                case Direction.Up:
                    return Direction.Down;
                case Direction.North:
                    return Direction.South;
                case Direction.South:
                    return Direction.North;
                case Direction.West:
                    return Direction.East;
                case Direction.East:
                    return Direction.West;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
        
        #endregion Static
    }
}