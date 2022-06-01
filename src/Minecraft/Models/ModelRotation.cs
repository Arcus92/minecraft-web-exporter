using System;
using System.Text.Json.Serialization;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.Models
{
    /// <summary>
    /// The rotation used for <see cref="ModelElement"/>.
    /// </summary>
    public class ModelRotation
    {
        /// <summary>
        /// Gets the origin of the rotation
        /// </summary>
        [JsonPropertyName("origin")] public Vector3 Origin { get; set; }
        
        /// <summary>
        /// Gets the rotation axis
        /// </summary>
        [JsonPropertyName("axis")] public Axis Axis { get; set; }
        
        /// <summary>
        /// Gets the rotation angle in degrees
        /// </summary>
        [JsonPropertyName("angle")] public float Angle { get; set; }

        /// <summary>
        /// Gets if the mesh should be rescaled
        /// </summary>
        [JsonPropertyName("rescale")] public bool Rescale { get; set; }
        
        
        /// <summary>
        /// Rotates the given point round <see cref="Origin"/> on the <see cref="Axis"/> with the <see cref="Angle"/>.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Vector3 TransformPoint(Vector3 p)
        {
            var angleInRad = Angle / 180.0f * MathF.PI;

            var sin = MathF.Sin(angleInRad);
            var cos = MathF.Cos(angleInRad);

            if (Rescale)
            {
                var scale = 1f / cos;
                sin *= scale;
                cos *= scale;
            }

            var x = p.X;
            var y = p.Y;
            var z = p.Z;
            // Relative from origin
            var rx = p.X - Origin.X;
            var ry = p.Y - Origin.Y;
            var rz = p.Z - Origin.Z;
            switch (Axis)
            {
                case Axis.X:
                    y = Origin.Y + ry * cos - rz * sin;
                    z = Origin.Z + rz * cos + ry * sin;
                    break;
                case Axis.Y:
                    x = Origin.X + rx * cos + rz * sin;
                    z = Origin.Z + rz * cos - rx * sin;
                    break;
                case Axis.Z:
                    x = Origin.X + rx * cos - ry * sin;
                    y = Origin.Y + ry * cos + rx * sin;
                    break;
            }

            return new Vector3() {X = x, Y = y, Z = z};
        }
        
        /// <summary>
        /// Rotates the given vector
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Vector3 TransformVector(Vector3 p)
        {
            var angleInRad = Angle / 180.0f * MathF.PI;

            var sin = MathF.Sin(angleInRad);
            var cos = MathF.Cos(angleInRad);
            
            var x = p.X;
            var y = p.Y;
            var z = p.Z;
            switch (Axis)
            {
                case Axis.X:
                    y = p.Y * cos - p.Z * sin;
                    z = p.Z * cos + p.Y * sin;
                    break;
                case Axis.Y:
                    x = p.X * cos + p.Z * sin;
                    z = p.Z * cos - p.X * sin;
                    break;
                case Axis.Z:
                    x = p.X * cos - p.Y * sin;
                    y = p.Y * cos + p.X * sin;
                    break;
            }

            return new Vector3() {X = x, Y = y, Z = z};
        }
        
        #region Static

        /// <summary>
        /// Returns a vector for the current rotation
        /// </summary>
        /// <returns></returns>
        public Vector3 GetAxisVector()
        {
            return GetAxisVector(Axis);
        }
        
        /// <summary>
        /// Returns a vector for the given axis
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Vector3 GetAxisVector(Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return Vector3.AxisX;
                case Axis.Y:
                    return Vector3.AxisY;
                case Axis.Z:
                    return Vector3.AxisZ;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }
        
        #endregion Static
    }
}