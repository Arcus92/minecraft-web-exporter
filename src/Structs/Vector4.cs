using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftWebExporter.Structs;

/// <summary>
/// A vector with four components.
/// </summary>
[JsonConverter(typeof(Vector4JsonConverter))]
public struct Vector4 : IEquatable<Vector4>
{
    public Vector4(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
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
    /// Gets the w component
    /// </summary>
    [JsonPropertyName("w")]
    public float W { get; set; }
        
    /// <summary>
    /// Converts to string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"({X}, {Y}, {Z}, {W})";
    }
        
    /// <summary>
    /// Gets the vector (0.0, 0.0, 0.0, 0.0)
    /// </summary>
    public static Vector4 Zero { get; } = new() {X = 0.0f, Y = 0.0f, Z = 0.0f, W = 0.0f};
        
    /// <summary>
    /// Gets the vector (1.0, 1.0, 1.0, 1.0)
    /// </summary>
    public static Vector4 One { get; } = new() {X = 1.0f, Y = 1.0f, Z = 1.0f, W = 1.0f};

    public bool Equals(Vector4 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector4 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z, W);
    }

    public static bool operator ==(Vector4 left, Vector4 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector4 left, Vector4 right)
    {
        return !left.Equals(right);
    }

    public static Vector4 operator +(Vector4 a, Vector4 b)
    {
        return new Vector4() {X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z, W = a.W + b.W};
    }
        
    public static Vector4 operator -(Vector4 a, Vector4 b)
    {
        return new Vector4() {X = a.X - b.X, Y = a.Y - b.Y, Z = a.Z - b.Z, W = a.W - b.W};
    }
        
    public static Vector4 operator -(Vector4 v)
    {
        return new Vector4() {X = -v.X, Y = -v.Y, Z = -v.Z, W = -v.W};
    }
}

/// <summary>
/// The json converter for <see cref="Vector4"/>.
/// </summary>
public class Vector4JsonConverter : JsonConverter<Vector4>
{
    /// <summary>
    /// Reads the json element
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }
            
        var i = 0;
        Vector4 result = default;
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
                    case 3:
                        result.W = value;
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
    public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Z);
        writer.WriteNumberValue(value.W);
        writer.WriteEndArray();
    }
}