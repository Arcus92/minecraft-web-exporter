using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftWebExporter.Structs
{
    /// <summary>
    /// A vector with two components.
    /// </summary>
    [JsonConverter(typeof(Vector2JsonConverter))]
    public struct Vector2 : IEquatable<Vector2>
    {
        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
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
        /// Converts to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"({X}, {Y})";
        }
        
        /// <summary>
        /// Gets the vector (0.0, 0.0)
        /// </summary>
        public static Vector2 Zero { get; } = new() {X = 0.0f, Y = 0.0f};
        
        /// <summary>
        /// Gets the vector (1.0, 1.0)
        /// </summary>
        public static Vector2 One { get; } = new() {X = 1.0f, Y = 1.0f};

        public bool Equals(Vector2 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object? obj)
        {
            return obj is Vector2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(Vector2 left, Vector2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2 left, Vector2 right)
        {
            return !left.Equals(right);
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2() {X = a.X + b.X, Y = a.Y + b.Y};
        }
        
        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2() {X = a.X - b.X, Y = a.Y - b.Y};
        }
        
        public static Vector2 operator -(Vector2 v)
        {
            return new Vector2() {X = -v.X, Y = -v.Y};
        }
        
        public static Vector2 operator *(Vector2 a, float b)
        {
            return new Vector2() {X = a.X * b, Y = a.Y * b};
        }
        
        public static Vector2 operator /(Vector2 a, float b)
        {
            return new Vector2() {X = a.X / b, Y = a.Y / b};
        }
    }

    /// <summary>
    /// The json converter for <see cref="Vector2"/>.
    /// </summary>
    public class Vector2JsonConverter : JsonConverter<Vector2>
    {
        /// <summary>
        /// Reads the json element
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException();
            }
            
            var i = 0;
            Vector2 result = default;
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
        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteEndArray();
        }
    }
}