namespace Art.Common.CipherPadding;

/// <summary>
/// Represents a stream that uses a <see cref="DepaddingHandler"/> to write de-padded content to a target stream.
/// </summary>
public class DepaddingWriteStream : Stream
{
    private readonly DepaddingHandler _handler;
    private readonly Stream _targetStream;
    private readonly bool _keepOpen;
    private readonly object __lock = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="DepaddingWriteStream"/>.
    /// </summary>
    /// <param name="handler">Depadding handler.</param>
    /// <param name="targetStream">Target stream.</param>
    /// <param name="keepOpen">If true, keeps <paramref name="targetStream"/> open after disposal.</param>
    public DepaddingWriteStream(DepaddingHandler handler, Stream targetStream, bool keepOpen = false)
    {
        _handler = handler;
        _targetStream = targetStream;
        _keepOpen = keepOpen;
    }

    /// <inheritdoc />
    public override void Flush() => _targetStream.Flush();

    /// <inheritdoc />
    public override Task FlushAsync(CancellationToken cancellationToken) => _targetStream.FlushAsync(cancellationToken);

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <inheritdoc />
    public override int Read(Span<byte> buffer) => throw new NotSupportedException();

    /// <inheritdoc />
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException();

    /// <inheritdoc />
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
        if (_handler.TryUpdate(buffer.AsMemory(offset, count), out var a, out var b))
        {
            if (a.Length != 0)
            {
                _targetStream.Write(a.Span);
            }
            if (b.Length != 0)
            {
                _targetStream.Write(b.Span);
            }
        }
    }

    /// <inheritdoc />
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if (_handler.TryUpdate(buffer, out var a, out var b))
        {
            if (a.Length != 0)
            {
                _targetStream.Write(a);
            }
            if (b.Length != 0)
            {
                _targetStream.Write(b);
            }
        }
        base.Write(buffer);
    }

    /// <inheritdoc />
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (_handler.TryUpdate(buffer.AsMemory(offset, count), out var a, out var b))
        {
            if (a.Length != 0)
            {
                await _targetStream.WriteAsync(a, cancellationToken).ConfigureAwait(false);
            }
            if (b.Length != 0)
            {
                await _targetStream.WriteAsync(b, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_handler.TryUpdate(buffer, out var a, out var b))
        {
            if (a.Length != 0)
            {
                await _targetStream.WriteAsync(a, cancellationToken).ConfigureAwait(false);
            }
            if (b.Length != 0)
            {
                await _targetStream.WriteAsync(b, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private void DisposeCore()
    {
        try
        {
            _handler.DoFinal(out var buf);
            if (buf.Length != 0)
            {
                _targetStream.Write(buf.Span);
            }
        }
        finally
        {
            if (!_keepOpen)
            {
                _targetStream.Dispose();
            }
        }
    }

    private async ValueTask DisposeCoreAsync()
    {
        try
        {
            _handler.DoFinal(out var buf);
            if (buf.Length != 0)
            {
                await _targetStream.WriteAsync(buf).ConfigureAwait(false);
            }
        }
        finally
        {
            if (!_keepOpen)
            {
                await _targetStream.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        lock (__lock)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
        }
        DisposeCore();
    }

    /// <inheritdoc />
    public override ValueTask DisposeAsync()
    {
        lock (__lock)
        {
            if (_disposed)
            {
                return ValueTask.CompletedTask;
            }
            _disposed = true;
        }
        return DisposeCoreAsync();
    }

    /// <inheritdoc />
    public override bool CanRead => false;

    /// <inheritdoc />
    public override bool CanSeek => false;

    /// <inheritdoc />
    public override bool CanWrite => true;

    /// <inheritdoc />
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc />
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }
}
