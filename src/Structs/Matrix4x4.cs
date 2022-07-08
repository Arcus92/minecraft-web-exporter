using System;

namespace MinecraftWebExporter.Structs;

/// <summary>
/// A 4x4 matrix
/// </summary>
public struct Matrix4x4
{
    public float M00 { get; set; }
    public float M01 { get; set; }
    public float M02 { get; set; }
    public float M03 { get; set; }
    public float M10 { get; set; }
    public float M11 { get; set; }
    public float M12 { get; set; }
    public float M13 { get; set; }
    public float M20 { get; set; }
    public float M21 { get; set; }
    public float M22 { get; set; }
    public float M23 { get; set; }
    public float M30 { get; set; }
    public float M31 { get; set; }
    public float M32 { get; set; }
    public float M33 { get; set; }

    /// <summary>
    /// Translate this matrix
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void Translate(float x, float y, float z)
    {
        Multiply(Offset(x, y, z));
    }
    
    /// <summary>
    /// Translate this matrix
    /// </summary>
    /// <param name="offset"></param>
    public void Translate(Vector3 offset)
    {
        Translate(offset.X, offset.Y, offset.Z);
    }

    /// <summary>
    /// Scales this matrix by the given vector
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void Scale(float x, float y, float z)
    {
        Multiply(Scaling(x, y, z));
    }
    
    /// <summary>
    /// Scales this matrix by the given vector
    /// </summary>
    /// <param name="scale"></param>
    public void Scale(Vector3 scale)
    {
        Scale(scale.X, scale.Y, scale.Z);
    }
    
    /// <summary>
    /// Rotates this matrix by the given euler rotation
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void Rotate(float x, float y, float z)
    {
        Multiply(Rotation(x, y, z));
    }
    
    /// <summary>
    /// Rotates this matrix by the given euler rotation
    /// </summary>
    /// <param name="euler"></param>
    public void Rotate(Vector3 euler)
    {
        Rotate(euler.X, euler.Y, euler.Z);
    }
    
    /// <summary>
    /// Multiplies the given point with this matrix
    /// </summary>
    /// <param name="point">Returns the transformed point</param>
    /// <returns></returns>
    public Vector3 MultiplyPoint(Vector3 point)
    {
        return MultiplyPoint(point.X, point.Y, point.Z);
    }
    
    /// <summary>
    /// Multiplies the given point with this matrix
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns>Returns the transformed point</returns>
    public Vector3 MultiplyPoint(float x, float y, float z)
    {
        return new Vector3()
        {
            X = M00 * x + M01 * y + M02 * z + M03,
            Y = M10 * x + M11 * y + M12 * z + M13,
            Z = M20 * x + M21 * y + M22 * z + M23,
        };
    }
    
    /// <summary>
    /// Multiplies the given vector with this matrix
    /// </summary>
    /// <param name="point">Returns the transformed vector</param>
    /// <returns></returns>
    public Vector3 MultiplyVector(Vector3 point)
    {
        return MultiplyVector(point.X, point.Y, point.Z);
    }
    
    /// <summary>
    /// Multiplies the given vector with this matrix
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns>Returns the transformed vector</returns>
    public Vector3 MultiplyVector(float x, float y, float z)
    {
        return new Vector3()
        {
            X = M00 * x + M01 * y + M02 * z,
            Y = M10 * x + M11 * y + M12 * z,
            Z = M20 * x + M21 * y + M22 * z,
        };
    }

    /// <summary>
    /// Multiplies the given matrix to this matrix
    /// </summary>
    /// <param name="l"></param>
    public void Multiply(Matrix4x4 r)
    {
        var l = this;
        M00 = l.M00 * r.M00 + l.M01 * r.M10 + l.M02 * r.M20 + l.M03 * r.M30;
        M01 = l.M00 * r.M01 + l.M01 * r.M11 + l.M02 * r.M21 + l.M03 * r.M31;
        M02 = l.M00 * r.M02 + l.M01 * r.M12 + l.M02 * r.M22 + l.M03 * r.M32;
        M03 = l.M00 * r.M03 + l.M01 * r.M13 + l.M02 * r.M23 + l.M03 * r.M33;

        M10 = l.M10 * r.M00 + l.M11 * r.M10 + l.M12 * r.M20 + l.M13 * r.M30;
        M11 = l.M10 * r.M01 + l.M11 * r.M11 + l.M12 * r.M21 + l.M13 * r.M31;
        M12 = l.M10 * r.M02 + l.M11 * r.M12 + l.M12 * r.M22 + l.M13 * r.M32;
        M13 = l.M10 * r.M03 + l.M11 * r.M13 + l.M12 * r.M23 + l.M13 * r.M33;

        M20 = l.M20 * r.M00 + l.M21 * r.M10 + l.M22 * r.M20 + l.M23 * r.M30;
        M21 = l.M20 * r.M01 + l.M21 * r.M11 + l.M22 * r.M21 + l.M23 * r.M31;
        M22 = l.M20 * r.M02 + l.M21 * r.M12 + l.M22 * r.M22 + l.M23 * r.M32;
        M23 = l.M20 * r.M03 + l.M21 * r.M13 + l.M22 * r.M23 + l.M23 * r.M33;

        M30 = l.M30 * r.M00 + l.M31 * r.M10 + l.M32 * r.M20 + l.M33 * r.M30;
        M31 = l.M30 * r.M01 + l.M31 * r.M11 + l.M32 * r.M21 + l.M33 * r.M31;
        M32 = l.M30 * r.M02 + l.M31 * r.M12 + l.M32 * r.M22 + l.M33 * r.M32;
        M33 = l.M30 * r.M03 + l.M31 * r.M13 + l.M32 * r.M23 + l.M33 * r.M33;
    }
    
    /// <summary>
    /// Multiplies the two matrices and returns the result
    /// </summary>
    /// <param name="l"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static Matrix4x4 operator *(Matrix4x4 l, Matrix4x4 r)
    {
        return new Matrix4x4()
        {
            M00 = l.M00 * r.M00 + l.M01 * r.M10 + l.M02 * r.M20 + l.M03 * r.M30,
            M01 = l.M00 * r.M01 + l.M01 * r.M11 + l.M02 * r.M21 + l.M03 * r.M31,
            M02 = l.M00 * r.M02 + l.M01 * r.M12 + l.M02 * r.M22 + l.M03 * r.M32,
            M03 = l.M00 * r.M03 + l.M01 * r.M13 + l.M02 * r.M23 + l.M03 * r.M33,

            M10 = l.M10 * r.M00 + l.M11 * r.M10 + l.M12 * r.M20 + l.M13 * r.M30,
            M11 = l.M10 * r.M01 + l.M11 * r.M11 + l.M12 * r.M21 + l.M13 * r.M31,
            M12 = l.M10 * r.M02 + l.M11 * r.M12 + l.M12 * r.M22 + l.M13 * r.M32,
            M13 = l.M10 * r.M03 + l.M11 * r.M13 + l.M12 * r.M23 + l.M13 * r.M33,

            M20 = l.M20 * r.M00 + l.M21 * r.M10 + l.M22 * r.M20 + l.M23 * r.M30,
            M21 = l.M20 * r.M01 + l.M21 * r.M11 + l.M22 * r.M21 + l.M23 * r.M31,
            M22 = l.M20 * r.M02 + l.M21 * r.M12 + l.M22 * r.M22 + l.M23 * r.M32,
            M23 = l.M20 * r.M03 + l.M21 * r.M13 + l.M22 * r.M23 + l.M23 * r.M33,

            M30 = l.M30 * r.M00 + l.M31 * r.M10 + l.M32 * r.M20 + l.M33 * r.M30,
            M31 = l.M30 * r.M01 + l.M31 * r.M11 + l.M32 * r.M21 + l.M33 * r.M31,
            M32 = l.M30 * r.M02 + l.M31 * r.M12 + l.M32 * r.M22 + l.M33 * r.M32,
            M33 = l.M30 * r.M03 + l.M31 * r.M13 + l.M32 * r.M23 + l.M33 * r.M33,
        };
    }
    
    /// <summary>
    /// Returns a transformation matrix with the given offset.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static Matrix4x4 Offset(float x, float y, float z)
    {
        return new Matrix4x4()
        {
            M00 = 1f, M03 = x,
            M11 = 1f, M13 = y,
            M22 = 1f, M23 = z,
            M33 = 1f,
        };
    }
    
    /// <summary>
    /// Returns a transformation matrix with the given offset.
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static Matrix4x4 Offset(Vector3 offset)
    {
        return Offset(offset.X, offset.Y, offset.Z);
    }

    /// <summary>
    /// Returns a transformation matrix with the given scale.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static Matrix4x4 Scaling(float x, float y, float z)
    {
        return new Matrix4x4()
        {
            M00 = x,
            M11 = y,
            M22 = z,
            M33 = 1f,
        };
    }
    
    /// <summary>
    /// Returns a transformation matrix with the given scale.
    /// </summary>
    /// <param name="scale"></param>
    /// <returns></returns>
    public static Matrix4x4 Scaling(Vector3 scale)
    {
        return Scaling(scale.X, scale.Y, scale.Z);
    }
    
    /// <summary>
    /// Returns a transformation matrix with the given euler rotation 
    /// </summary>
    /// <param name="x">The rotation around the x axis (in rad)</param>
    /// <param name="y">The rotation around the y axis (in rad)</param>
    /// <param name="z">The rotation around the z axis (in rad)</param>
    /// <returns></returns>
    public static Matrix4x4 Rotation(float x, float y, float z)
    {
        var cosX = MathF.Cos(x);
        var sinX = MathF.Sin(x);
        var cosY = MathF.Cos(y);
        var sinY = MathF.Sin(y);
        var cosZ = MathF.Cos(z);
        var sinZ = MathF.Sin(z);
        
        return new Matrix4x4()
        {
            M00 = cosY * cosZ, M01 = sinX * sinY * cosZ - cosX * sinZ, M02 = cosX * sinY * cosZ + sinX * sinZ,
            M10 = cosY * sinZ, M11 = sinX * sinY * sinZ + cosX * cosZ, M12 = cosX * sinY * sinZ - sinX * cosZ,
            M20 = -sinY, M21 = sinX * cosY, M22 = cosX * cosY, 
            M33 = 1,
        };
    }
    
    /// <summary>
    /// Returns the identity matrix:
    /// <code>
    /// 1 0 0 0
    /// 0 1 0 0
    /// 0 0 1 0
    /// 0 0 0 1
    /// </code>
    /// </summary>
    public static Matrix4x4 Identity { get; } = new()
    {
        M00 = 1f, M11 = 1f, M22 = 1f, M33 = 1f,
    };
}