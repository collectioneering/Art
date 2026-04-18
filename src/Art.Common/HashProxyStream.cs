using System.Security.Cryptography;

namespace Art.Common;

/// <summary>
/// Represents a non-seekable proxy to another stream, sending data to a hash algorithm as data is read or written.
/// </summary>
public sealed class HashProxyStream : Stream
{
    /// <summary>
    /// If true, hash value has been computed.
    /// </summary>
    public bool HashComputed => _hashComputed;

    private readonly Stream _stream;
    private readonly HashAlgorithm _hashAlgorithm;
    private readonly long _initPos;
    private readonly bool _leaveStreamOpen;
    private readonly bool _leaveHashAlgorithmOpen;
    private byte[]? _hash;
    private bool _hashComputed;
    private long _pos;
    private bool _disposed;
    private HashMode _mode = HashMode.Unassigned;

    /// <summary>
    /// Creates a new instance of <see cref="HashProxyStream"/>.
    /// </summary>
    /// <param name="stream">Stream to wrap.</param>
    /// <param name="hashAlgorithm">Hash algorithm.</param>
    /// <param name="leaveStreamOpen">If true, leaves <paramref name="stream"/> open after disposal.</param>
    /// <param name="leaveHashAlgorithmOpen">If true, leaves <paramref name="hashAlgorithm"/> open after disposal.</param>
    public HashProxyStream(Stream stream, HashAlgorithm hashAlgorithm, bool leaveStreamOpen = false, bool leaveHashAlgorithmOpen = false)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _hashAlgorithm = hashAlgorithm ?? throw new ArgumentNullException(nameof(hashAlgorithm));
        if (_stream.CanSeek)
        {
            try
            {
                _initPos = _stream.Position;
            }
            catch
            {
                // ignored
            }
        }
        _leaveStreamOpen = leaveStreamOpen;
        _leaveHashAlgorithmOpen = leaveHashAlgorithmOpen;
    }

    /// <inheritdoc />
    public override void Flush() => _stream.Flush();

    /// <inheritdoc />
    public override Task FlushAsync(CancellationToken cancellationToken) => _stream.FlushAsync(cancellationToken);

    private void UpdateHash(byte[] buffer, int offset, int count)
    {
        if (_hashComputed)
        {
            return;
        }
        _hashAlgorithm.TransformBlock(buffer, offset, count, null, 0);
    }

    private void FinishHash()
    {
        if (_hashComputed)
        {
            return;
        }
        // ensure TransformBlock is called at least once
        _hashAlgorithm.TransformBlock([], 0, 0, null, 0);
        _hashAlgorithm.TransformFinalBlock([], 0, 0);
        _hash = _hashAlgorithm.Hash ?? throw new InvalidOperationException("Hash function did not return hash");
        _hashComputed = true;
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        AssignMode(HashMode.Read);
        int read = _stream.Read(buffer, offset, count);
        _pos += read;
        if (_hashComputed)
        {
            return read;
        }
        if (read != 0)
        {
            UpdateHash(buffer, offset, read);
        }
        else
        {
            FinishHash();
        }
        return read;
    }

    /// <inheritdoc />
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        AssignMode(HashMode.Read);
        int read = await _stream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        _pos += read;
        if (_hashComputed)
        {
            return read;
        }
        if (read != 0)
        {
            UpdateHash(buffer, offset, read);
        }
        else
        {
            FinishHash();
        }
        return read;
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
        AssignMode(HashMode.Write);
        UpdateHash(buffer, offset, count);
        _stream.Write(buffer, offset, count);
    }

    /// <inheritdoc />
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        AssignMode(HashMode.Write);
        UpdateHash(buffer, offset, count);
        return _stream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    /// <inheritdoc />
    public override bool CanRead => _stream.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => false;

    /// <inheritdoc />
    public override bool CanWrite => _stream.CanWrite;

    /// <inheritdoc />
    public override long Length => _stream.CanSeek ? _stream.Length - _initPos : throw new NotSupportedException();

    /// <inheritdoc />
    public override long Position
    {
        get => _pos;
        set => throw new NotSupportedException();
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        FinishHash();
        if (disposing)
        {
            if (!_leaveStreamOpen)
            {
                _stream.Dispose();
            }
            if (!_leaveHashAlgorithmOpen)
            {
                _hashAlgorithm.Dispose();
            }
        }
        _disposed = true;
    }

    /// <summary>
    /// Gets the computed hash value.
    /// </summary>
    /// <returns>Hash value.</returns>
    /// <remarks>
    /// This triggers an immediate hash calculation (if not already computed).
    /// If called before the underlying stream has been fully processed by calls to any Read[Async] overloads,
    /// then the returned hash value will only account for the portion read during this stream's lifespan.
    /// </remarks>
    public byte[] GetHash()
    {
        FinishHash();
        return _hash!;
    }

    /// <summary>
    /// Assigns hash mode.
    /// </summary>
    /// <param name="mode">Hash mode.</param>
    /// <exception cref="InvalidOperationException">Thrown when mixing read/write operations, which is not supported.</exception>
    private void AssignMode(HashMode mode)
    {
        if (_mode == HashMode.Unassigned)
        {
            _mode = mode;
        }
        else if (_mode != mode)
        {
            throw new InvalidOperationException("Cannot mix read and write operations on this stream.");
        }
    }

    private enum HashMode
    {
        Unassigned,
        Read,
        Write
    }
}
