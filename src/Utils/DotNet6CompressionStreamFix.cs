using System.IO;

namespace MinecraftWebExporter.Utils;

/// <summary>
/// There was a breaking change in .NET 6 where compression streams like <see cref="System.IO.Compression.DeflateStream"/> or
/// <see cref="System.IO.Compression.GZipStream"/> can return less bytes than requested by <see cref="Stream.Read(byte[],int,int)"/>.
/// See https://docs.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/partial-byte-reads-in-streams
/// 
/// This causes an issue in <see cref="SharpNBT"/> where they don't check the number of read bytes when parsing the NBT data.
/// This wrapper stream will restore the old behaviour from .NET 5 and prior again.
/// <see cref="Read"/> will now read the base stream until the exact number of requested bytes are read or
/// the base stream reached the end of the data and return 0 bytes.
/// </summary>
public class DotNet6CompressionStreamFix : Stream
{
    public DotNet6CompressionStreamFix(Stream stream)
    {
        m_BaseStream = stream;
    }

    /// <summary>
    /// Gets the base stream
    /// </summary>
    private readonly Stream m_BaseStream;

    #region Stream

    /// <inheritdoc cref="Stream.Read(byte[],int,int)"/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        var totalBytes = 0;
        while (totalBytes < count)
        {
            var readBytes = m_BaseStream.Read(buffer, totalBytes, count - totalBytes);
            if (readBytes == 0) break;
            totalBytes += readBytes;
        }
        
        return totalBytes;
    }
    
    /// <inheritdoc cref="Stream.Flush"/>
    public override void Flush()
    {
        m_BaseStream.Flush();
    }

    /// <inheritdoc cref="Stream.Seek"/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        return m_BaseStream.Seek(offset, origin);
    }

    /// <inheritdoc cref="Stream.SetLength"/>
    public override void SetLength(long value)
    {
        m_BaseStream.SetLength(value);
    }

    /// <inheritdoc cref="Stream.Write(byte[],int,int)"/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        m_BaseStream.Write(buffer, offset, count);
    }

    /// <inheritdoc cref="Stream.CanRead"/>
    public override bool CanRead => m_BaseStream.CanRead;

    /// <inheritdoc cref="Stream.CanSeek"/>
    public override bool CanSeek => m_BaseStream.CanSeek;

    /// <inheritdoc cref="Stream.CanWrite"/>
    public override bool CanWrite => m_BaseStream.CanWrite;

    /// <inheritdoc cref="Stream.Length"/>
    public override long Length => m_BaseStream.Length;

    /// <inheritdoc cref="Stream.Position"/>
    public override long Position
    {
        get => m_BaseStream.Position;
        set => m_BaseStream.Position = value;
    }
    
    #endregion Stream
}