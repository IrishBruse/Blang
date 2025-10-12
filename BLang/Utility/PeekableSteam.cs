namespace BLang.Utility;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


/// <summary>
/// A stream wrapper that adds the ability to peek ahead at the underlying stream's data
/// without consuming it.
/// </summary>
public class PeekableStream : Stream
{
    private readonly Stream baseStream;
    private readonly Queue<byte> buffer;
    private readonly Encoding encoding;
    private const int MaxPeekSize = 3;

    public PeekableStream(Stream baseStream, Encoding? encoding = null)
    {
        ArgumentNullException.ThrowIfNull(baseStream);

        if (!baseStream.CanRead)
        {
            throw new ArgumentException("Stream must be readable.", nameof(baseStream));
        }

        this.baseStream = baseStream;
        buffer = new Queue<byte>(MaxPeekSize);
        this.encoding = encoding ?? Encoding.UTF8;
    }

    public override bool CanRead => baseStream.CanRead;
    public override bool CanSeek => baseStream.CanSeek;
    public override bool CanWrite => false;
    public override long Length => baseStream.Length;

    // If buffer is empty and baseStream returns -1, we're at the end
    public bool EndOfStream => buffer.Count == 0 && baseStream.Position >= baseStream.Length;

    public override long Position
    {
        get
        {
            return baseStream.Position - buffer.Count;
        }

        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);

            buffer.Clear();
            baseStream.Position = value;
        }
    }

    /// <summary>
    /// Peeks at the next bytes in the stream without advancing the position.
    /// </summary>
    /// <param name="buffer">The buffer to write the peeked data into.</param>
    /// <param name="offset">The byte offset in buffer at which to begin writing.</param>
    /// <param name="count">The maximum number of bytes to peek.</param>
    /// <returns>The total number of bytes peeked into the buffer.</returns>
    public int Peek(byte[] buffer, int offset, int count)
    {
        if (count > MaxPeekSize)
            throw new ArgumentOutOfRangeException(nameof(count), $"Cannot peek more than {MaxPeekSize} bytes.");

        EnsureBufferFilled(count);

        int peekCount = Math.Min(count, this.buffer.Count);
        this.buffer.ToArray().CopyTo(buffer, offset);

        return peekCount;
    }

    /// <summary>
    /// Peeks at the next character in the stream using the specified encoding,
    /// without advancing the position.
    /// </summary>
    /// <returns>
    /// The next character as an integer, or -1 if the end of the stream has been reached.
    /// </returns>
    public int PeekChar(int offset = 0)
    {
        Decoder decoder = encoding.GetDecoder();

        byte[] byteBuffer = new byte[MaxPeekSize];
        int peekedByteCount = Peek(byteBuffer, 0, MaxPeekSize);

        if (peekedByteCount == 0)
        {
            return -1; // End of stream
        }

        // Determine the max number of chars that could be produced from the peeked bytes
        int maxCharCount = encoding.GetMaxCharCount(peekedByteCount);
        char[] charBuffer = new char[maxCharCount];
        int charCount = decoder.GetChars(byteBuffer, 0, peekedByteCount, charBuffer, 0);

        if (charCount > 0)
        {
            return charBuffer[offset];
        }

        return -1;
    }

    private void EnsureBufferFilled(int requiredCount)
    {
        while (buffer.Count < requiredCount)
        {
            int byteRead = baseStream.ReadByte();
            if (byteRead == -1)
            {
                // End of stream
                break;
            }
            buffer.Enqueue((byte)byteRead);
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int bytesRead = 0;

        // First, read from our internal buffer
        while (this.buffer.Count > 0 && bytesRead < count)
        {
            buffer[offset + bytesRead] = this.buffer.Dequeue();
            bytesRead++;
        }

        // Then, read from the base stream if more bytes are needed
        if (bytesRead < count)
        {
            bytesRead += baseStream.Read(buffer, offset + bytesRead, count - bytesRead);
        }

        return bytesRead;
    }

    /// <summary>
    /// Reads the next character from the stream, advancing the position.
    /// </summary>
    /// <returns>
    /// The next character as an integer, or -1 if the end of the stream has been reached.
    /// </returns>
    public int ReadChar()
    {
        // 1. Peek to find out what the next char is. This is the simplest way to
        //    determine how many bytes to consume without managing a complex decoder state.
        int peekedChar = PeekChar();
        if (peekedChar == -1)
        {
            return -1;
        }

        // 2. Encode that single character back into bytes to know how many bytes to read.
        char[] singleChar = { (char)peekedChar };
        int byteCount = encoding.GetByteCount(singleChar, 0, 1);

        // 3. Read exactly that many bytes from the stream to consume the character.
        byte[] readBuffer = new byte[byteCount];
        _ = Read(readBuffer, 0, byteCount);

        return peekedChar;
    }

    public override int ReadByte()
    {
        if (buffer.Count > 0)
        {
            return buffer.Dequeue();
        }
        return baseStream.ReadByte();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        buffer.Clear();
        return baseStream.Seek(offset, origin);
    }

    public override void Flush()
    {
        baseStream.Flush();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            baseStream.Dispose();
        }
        base.Dispose(disposing);
    }
}
