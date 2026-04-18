using System.Buffers;

namespace Art.Common.CipherPadding;

/// <summary>
/// Represents a stream that uses a <see cref="DepaddingHandler"/> to read de-padded content from a source stream.
/// </summary>
public class DepaddingReadStream : Stream
{
    private const int TempBufferSize = 4096;
    private readonly DepaddingHandler _handler;
    private readonly Stream _sourceStream;
    private readonly bool _keepOpen;
    private readonly object __lock = new();
    private bool _didFinal;
    private ReadOnlyMemory<byte> _final;
    private MemoryStream _cache;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="DepaddingReadStream"/>.
    /// </summary>
    /// <param name="handler">Depadding handler.</param>
    /// <param name="sourceStream">Source stream.</param>
    /// <param name="keepOpen">If true, keeps <paramref name="sourceStream"/> open after disposal.</param>
    public DepaddingReadStream(DepaddingHandler handler, Stream sourceStream, bool keepOpen = false)
    {
        _handler = handler;
        _sourceStream = sourceStream;
        _keepOpen = keepOpen;
        _didFinal = false;
        _final = ReadOnlyMemory<byte>.Empty;
        _cache = new MemoryStream();
    }

    /// <inheritdoc />
    public override void Flush() => _sourceStream.Flush();

    /// <inheritdoc />
    public override Task FlushAsync(CancellationToken cancellationToken) => _sourceStream.FlushAsync(cancellationToken);

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);
        if (_didFinal)
        {
            return CopyRemainingFinal(buffer.AsSpan(offset, count));
        }
        byte[] tempBuffer = ArrayPool<byte>.Shared.Rent(TempBufferSize);
        try
        {
            int total = 0;
            while (count > total)
            {
                int start = offset + total;
                var target = buffer.AsSpan(start, count - start);
                if (_cache.Length != 0)
                {
                    total += FillFromCache(target);
                }
                else
                {
                    int read = _sourceStream.Read(tempBuffer, 0, Math.Min(TempBufferSize, target.Length));
                    if (read == 0)
                    {
                        return total + StartCopyFinal(target);
                    }
                    AddToCache(tempBuffer.AsMemory(0, read));
                }
            }
            return total;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(tempBuffer);
        }
    }

    /// <inheritdoc />
    public override int Read(Span<byte> buffer)
    {
        if (_didFinal)
        {
            return CopyRemainingFinal(buffer);
        }
        byte[] tempBuffer = ArrayPool<byte>.Shared.Rent(TempBufferSize);
        try
        {
            int total = 0;
            while (buffer.Length > total)
            {
                var target = buffer[total..];
                if (_cache.Length != 0)
                {
                    total += FillFromCache(target);
                }
                else
                {
                    int read = _sourceStream.Read(tempBuffer, 0, Math.Min(TempBufferSize, target.Length));
                    if (read == 0)
                    {
                        return total + StartCopyFinal(target);
                    }
                    AddToCache(tempBuffer.AsMemory(0, read));
                }
            }
            return total;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(tempBuffer);
        }
    }

    /// <inheritdoc />
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ValidateBufferArguments(buffer, offset, count);
        if (_didFinal)
        {
            return CopyRemainingFinal(buffer.AsSpan(offset, count));
        }
        byte[] tempBuffer = ArrayPool<byte>.Shared.Rent(TempBufferSize);
        try
        {
            int total = 0;
            while (count > total)
            {
                int start = offset + total;
                var target = buffer.AsMemory(start, count - start);
                if (_cache.Length != 0)
                {
                    total += FillFromCache(target.Span);
                }
                else
                {
                    int read = await _sourceStream.ReadAsync(tempBuffer.AsMemory(0, Math.Min(TempBufferSize, target.Length)), cancellationToken).ConfigureAwait(false);
                    if (read == 0)
                    {
                        return total + StartCopyFinal(target.Span);
                    }
                    AddToCache(tempBuffer.AsMemory(0, read));
                }
            }
            return total;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(tempBuffer);
        }
    }

    /// <inheritdoc />
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_didFinal)
        {
            return CopyRemainingFinal(buffer.Span);
        }
        byte[] tempBuffer = ArrayPool<byte>.Shared.Rent(TempBufferSize);
        try
        {
            int total = 0;
            while (buffer.Length > total)
            {
                var target = buffer[total..];
                if (_cache.Length != 0)
                {
                    total += FillFromCache(target.Span);
                }
                else
                {
                    int read = await _sourceStream.ReadAsync(tempBuffer.AsMemory(0, Math.Min(TempBufferSize, target.Length)), cancellationToken).ConfigureAwait(false);
                    if (read == 0)
                    {
                        return total + StartCopyFinal(target.Span);
                    }
                    AddToCache(tempBuffer.AsMemory(0, read));
                }
            }
            return total;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(tempBuffer);
        }
    }

    private void AddToCache(ReadOnlyMemory<byte> readBuffer)
    {
        if (_handler.TryUpdate(readBuffer, out var a, out var b))
        {
            if (a.Length != 0)
            {
                _cache.Write(a.Span);
            }
            if (b.Length != 0)
            {
                _cache.Write(b.Span);
            }
            _cache.Position = 0;
        }
    }

    private int FillFromCache(Span<byte> target)
    {
        int commonLength = (int)Math.Min(_cache.Length - _cache.Position, target.Length);
        _cache.ReadExactly(target[..commonLength]);
        if (_cache.Position == _cache.Length)
        {
            _cache.Position = 0;
            _cache.SetLength(0);
        }
        return commonLength;
    }

    private int StartCopyFinal(Span<byte> target)
    {
        _didFinal = true;
        _handler.DoFinal(out _final);
        return CopyRemainingFinal(target);
    }

    private int CopyRemainingFinal(Span<byte> target)
    {
        int commonLength = Math.Min(_final.Length, target.Length);
        if (commonLength == 0)
        {
            return commonLength;
        }
        _final.Span[..commonLength].CopyTo(target[..commonLength]);
        _final = _final[commonLength..];
        return commonLength;
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();

    /// <inheritdoc />
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException();

    /// <inheritdoc />
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    private void DisposeCore()
    {
        if (!_keepOpen)
        {
            _sourceStream.Dispose();
        }
    }

    private async ValueTask DisposeCoreAsync()
    {
        if (!_keepOpen)
        {
            await _sourceStream.DisposeAsync().ConfigureAwait(false);
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
    public override bool CanRead => true;

    /// <inheritdoc />
    public override bool CanSeek => false;

    /// <inheritdoc />
    public override bool CanWrite => false;

    /// <inheritdoc />
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc />
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }
}
