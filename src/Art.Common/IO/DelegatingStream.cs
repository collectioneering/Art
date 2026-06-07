namespace Art.Common.IO;

/// <summary>
/// Base type for <see cref="Stream"/>s that wrap another.
/// </summary>
public abstract class DelegatingStream : Stream
{
    /// <summary>
    /// Wrapped stream.
    /// </summary>
    protected abstract Stream InnerStream { get; }

    /// <inheritdoc />
    public override bool CanRead
    {
        get { return InnerStream.CanRead; }
    }

    /// <inheritdoc />
    public override bool CanSeek
    {
        get { return InnerStream.CanSeek; }
    }

    /// <inheritdoc />
    public override bool CanWrite
    {
        get { return InnerStream.CanWrite; }
    }

    /// <inheritdoc />
    public override long Length
    {
        get { return InnerStream.Length; }
    }

    /// <inheritdoc />
    public override long Position
    {
        get { return InnerStream.Position; }
        set
        {
            ValidateSeekState();
            InnerStream.Position = value;
        }
    }

    /// <inheritdoc />
    public override int ReadTimeout
    {
        get { return InnerStream.ReadTimeout; }
        set { InnerStream.ReadTimeout = value; }
    }

    /// <inheritdoc />
    public override bool CanTimeout
    {
        get { return InnerStream.CanTimeout; }
    }

    /// <inheritdoc />
    public override int WriteTimeout
    {
        get { return InnerStream.WriteTimeout; }
        set { InnerStream.WriteTimeout = value; }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            InnerStream.Dispose();
        }
        base.Dispose(disposing);
    }

    /// <inheritdoc />
    public override ValueTask DisposeAsync()
    {
        return InnerStream.DisposeAsync();
    }

    /// <summary>
    /// Checks that this instance is allowed to seek the underlying stream.
    /// </summary>
    protected virtual void ValidateSeekState()
    {
    }

    /// <summary>
    /// Checks that this instance is allowed to flush the underlying stream.
    /// </summary>
    protected virtual void ValidateFlushState()
    {
    }

    /// <summary>
    /// Checks that this instance is allowed to set the length of the underlying stream.
    /// </summary>
    protected virtual void ValidateSetLengthState()
    {
    }

    /// <summary>
    /// Checks that this instance is allowed to read from the underlying stream.
    /// </summary>
    protected virtual void ValidateReadState()
    {
    }

    /// <summary>
    /// Checks that this instance is allowed to write to the underlying stream.
    /// </summary>
    protected virtual void ValidateWriteState()
    {
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        ValidateSeekState();
        return InnerStream.Seek(offset, origin);
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        ValidateReadState();
        return InnerStream.Read(buffer, offset, count);
    }

    /// <inheritdoc />
    public override int Read(Span<byte> buffer)
    {
        ValidateReadState();
        return InnerStream.Read(buffer);
    }

    /// <inheritdoc />
    public override int ReadByte()
    {
        ValidateReadState();
        return InnerStream.ReadByte();
    }

    /// <inheritdoc />
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ValidateReadState();
        return InnerStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    /// <inheritdoc />
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ValidateReadState();
        return InnerStream.ReadAsync(buffer, cancellationToken);
    }

    /// <inheritdoc />
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
    {
        ValidateReadState();
        return InnerStream.BeginRead(buffer, offset, count, callback, state);
    }

    /// <inheritdoc />
    public override int EndRead(IAsyncResult asyncResult)
    {
        ValidateReadState();
        return InnerStream.EndRead(asyncResult);
    }

    /// <inheritdoc />
    public override void CopyTo(Stream destination, int bufferSize)
    {
        ValidateReadState();
        InnerStream.CopyTo(destination, bufferSize);
    }

    /// <inheritdoc />
    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        ValidateReadState();
        return InnerStream.CopyToAsync(destination, bufferSize, cancellationToken);
    }

    /// <inheritdoc />
    public override void Flush()
    {
        ValidateFlushState();
        InnerStream.Flush();
    }

    /// <inheritdoc />
    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        ValidateFlushState();
        return InnerStream.FlushAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override void SetLength(long value)
    {
        ValidateSetLengthState();
        InnerStream.SetLength(value);
    }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
        ValidateWriteState();
        InnerStream.Write(buffer, offset, count);
    }

    /// <inheritdoc />
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        ValidateWriteState();
        InnerStream.Write(buffer);
    }

    /// <inheritdoc />
    public override void WriteByte(byte value)
    {
        ValidateWriteState();
        InnerStream.WriteByte(value);
    }

    /// <inheritdoc />
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ValidateWriteState();
        return InnerStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    /// <inheritdoc />
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ValidateWriteState();
        return InnerStream.WriteAsync(buffer, cancellationToken);
    }

    /// <inheritdoc />
    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
    {
        ValidateWriteState();
        return InnerStream.BeginWrite(buffer, offset, count, callback, state);
    }

    /// <inheritdoc />
    public override void EndWrite(IAsyncResult asyncResult)
    {
        ValidateWriteState();
        InnerStream.EndWrite(asyncResult);
    }
}
