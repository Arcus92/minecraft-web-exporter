using System;
using System.Globalization;
using System.IO;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Wavefront;

/// <summary>
/// The obj file writer used to export .obj 3d models
/// </summary>
public class ObjWriter : IDisposable
{
    /// <summary>
    /// Opens the writer
    /// </summary>
    /// <param name="path"></param>
    public ObjWriter(string path) : this(new FileStream(path, FileMode.Create))
    {
    }
        
    /// <summary>
    /// Opens the writer
    /// </summary>
    /// <param name="stream"></param>
    public ObjWriter(Stream stream)
    {
        m_TextWriter = new StreamWriter(stream);
    }
        
    /// <summary>
    /// The internal text writer
    /// </summary>
    private readonly TextWriter m_TextWriter;
        
    /// <summary>
    /// Writes the vector to the text writer
    /// </summary>
    /// <param name="op"></param>
    /// <param name="vector"></param>
    public void Write(string op, Vector2 vector)
    {
        m_TextWriter.Write(op);
        m_TextWriter.Write(" ");
        m_TextWriter.Write(vector.X.ToString(CultureInfo.InvariantCulture));
        m_TextWriter.Write(" ");
        m_TextWriter.Write(vector.Y.ToString(CultureInfo.InvariantCulture));
        m_TextWriter.WriteLine();
    }
        
    /// <summary>
    /// Writes the vector to the text writer
    /// </summary>
    /// <param name="op"></param>
    /// <param name="vector"></param>
    public void Write(string op, Vector3 vector)
    {
        m_TextWriter.Write(op);
        m_TextWriter.Write(" ");
        m_TextWriter.Write(vector.X.ToString(CultureInfo.InvariantCulture));
        m_TextWriter.Write(" ");
        m_TextWriter.Write(vector.Y.ToString(CultureInfo.InvariantCulture));
        m_TextWriter.Write(" ");
        m_TextWriter.Write(vector.Z.ToString(CultureInfo.InvariantCulture));
        m_TextWriter.WriteLine();
    }
        
    /// <summary>
    /// Writes the vector to the text writer
    /// </summary>
    /// <param name="op"></param>
    /// <param name="vector"></param>
    public void Write(string op, Vector4 vector)
    {
        m_TextWriter.Write(op);
        m_TextWriter.Write(" ");
        m_TextWriter.Write(vector.X.ToString(CultureInfo.InvariantCulture));
        m_TextWriter.Write(" ");
        m_TextWriter.Write(vector.Y.ToString(CultureInfo.InvariantCulture));
        m_TextWriter.Write(" ");
        m_TextWriter.Write(vector.Z.ToString(CultureInfo.InvariantCulture));
        m_TextWriter.Write(" ");
        m_TextWriter.Write(vector.W.ToString(CultureInfo.InvariantCulture));
        m_TextWriter.WriteLine();
    }
        
    /// <summary>
    /// Writes the string to the text writer
    /// </summary>
    /// <param name="op"></param>
    /// <param name="value"></param>
    public void Write(string op, string value)
    {
        m_TextWriter.Write(op);
        m_TextWriter.Write(" ");
        m_TextWriter.Write(value);
        m_TextWriter.WriteLine();
    }
        
    /// <summary>
    /// Writes the double value to the text writer
    /// </summary>
    /// <param name="op"></param>
    /// <param name="value"></param>
    public void Write(string op, double value)
    {
        m_TextWriter.Write(op);
        m_TextWriter.Write(" ");
        m_TextWriter.Write(value.ToString(CultureInfo.InvariantCulture));
        m_TextWriter.WriteLine();
    }

    /// <summary>
    /// Writes the quad to the text writer
    /// </summary>
    /// <param name="op"></param>
    /// <param name="quad"></param>
    /// <param name="positionOffset"></param>
    /// <param name="uvOffset"></param>
    /// <param name="normalOffset"></param>
    public void Write(string op, ObjQuad quad, int positionOffset, int uvOffset, int normalOffset)
    {
        m_TextWriter.Write(op);

        Write(quad.Vertex1, positionOffset, uvOffset, normalOffset);
        Write(quad.Vertex2, positionOffset, uvOffset, normalOffset);
        Write(quad.Vertex3, positionOffset, uvOffset, normalOffset);
        Write(quad.Vertex4, positionOffset, uvOffset, normalOffset);
            
        m_TextWriter.WriteLine();
    }
        
    /// <summary>
    /// Writes the quad to the text writer
    /// </summary>
    /// <param name="quadVertex"></param>
    /// <param name="positionOffset"></param>
    /// <param name="uvOffset"></param>
    /// <param name="normalOffset"></param>
    public void Write(ObjQuadVertex quadVertex, int positionOffset, int uvOffset, int normalOffset)
    {
        m_TextWriter.Write(" ");
        if (quadVertex.Position.HasValue) m_TextWriter.Write(quadVertex.Position.Value + positionOffset);
        m_TextWriter.Write("/");
        if (quadVertex.Uv.HasValue) m_TextWriter.Write(quadVertex.Uv.Value + uvOffset);
        m_TextWriter.Write("/");
        if (quadVertex.Normal.HasValue) m_TextWriter.Write(quadVertex.Normal.Value + normalOffset);
    }
        
    /// <summary>
    /// Writes a comment to the text writer
    /// </summary>
    /// <param name="text"></param>
    public void Comment(string text)
    {
        m_TextWriter.WriteLine($"# {text}");
    }
        
    /// <summary>
    /// Dispose the writer
    /// </summary>
    public void Dispose()
    {
        m_TextWriter.Dispose();
    }
}