using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftWebExporter.Structs;

/// <summary>
/// A vector with three components.
/// </summary>
[JsonConverter(typeof(Vector3JsonConverter))]
public struct Vector3 : IEquatable<Vector3>
{
    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Gets the x component
    /// </summary>
    [JsonPropertyName("x")]
    public float X { get; set; }
        
    /// <summary>
    /// Gets the y component
    /// </summary>
    [JsonPropertyName("y")]
    public float Y { get; set; }
        
    /// <summary>
    /// Gets the z component
    /// </summary>
    [JsonPropertyName("z")]
    public float Z { get; set; }

    /// <summary>
    /// Converts to string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }

    /// <summary>
    /// Gets the vector (0.0, 0.0, 0.0)
    /// </summary>
    public static Vector3 Zero { get; } = new() {X = 0.0f, Y = 0.0f, Z = 0.0f};
        
    /// <summary>
    /// Gets the vector (1.0, 1.0, 1.0)
    /// </summary>
    public static Vector3 One { get; } = new() {X = 1.0f, Y = 1.0f, Z = 1.0f};
        
    /// <summary>
    /// Gets the vector (1.0, 0.0, 0.0)
    /// </summary>
    public static Vector3 AxisX { get; } = new() {X = 1.0f, Y = 0.0f, Z = 0.0f};
        
    /// <summary>
    /// Gets the vector (0.0, 1.0, 0.0)
    /// </summary>
    public static Vector3 AxisY { get; } = new() {X = 0.0f, Y = 1.0f, Z = 0.0f};
        
    /// <summary>
    /// Gets the vector (0.0, 0.0, 1.0)
    /// </summary>
    public static Vector3 AxisZ { get; } = new() {X = 0.0f, Y = 0.0f, Z = 1.0f};

    public bool Equals(Vector3 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector3 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public static bool operator ==(Vector3 left, Vector3 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector3 left, Vector3 right)
    {
        return !left.Equals(right);
    }

    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
        return new Vector3() {X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z};
    }
        
    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
        return new Vector3() {X = a.X - b.X, Y = a.Y - b.Y, Z = a.Z - b.Z};
    }
        
    public static Vector3 operator -(Vector3 v)
    {
        return new Vector3() {X = -v.X, Y = -v.Y, Z = -v.Z};
    }
        
    public static Vector3 operator *(Vector3 a, float b)
    {
        return new Vector3() {X = a.X * b, Y = a.Y * b, Z = a.Z * b};
    }
        
    public static Vector3 operator /(Vector3 a, float b)
    {
        return new Vector3() {X = a.X / b, Y = a.Y / b, Z = a.Z / b};
    }
}

/// <summary>
/// The json converter for <see cref="Vector3"/>.
/// </summary>
public class Vector3JsonConverter : JsonConverter<Vector3>
{
    /// <summary>
    /// Reads the json element
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }
            
        var i = 0;
        Vector3 result = default;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                var value = reader.GetSingle();
                switch (i)
                {
                    case 0:
                        result.X = value;
                        break;
                    case 1:
                        result.Y = value;
                        break;
                    case 2:
                        result.Z = value;
                        break;
                    default:
                        throw new JsonException();
                }

                i++;
            }
            else if (reader.TokenType != JsonTokenType.Comment)
            {
                throw new JsonException();
            }
        }

        return result;
    }

    /// <summary>
    /// Writes the json element
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Z);
        writer.WriteEndArray();
    }
}