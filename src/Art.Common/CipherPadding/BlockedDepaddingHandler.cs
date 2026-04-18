using System.Security.Cryptography;

namespace Art.Common.CipherPadding;

/// <summary>
/// Base handler for blocked depadding.
/// </summary>
public abstract class BlockedDepaddingHandler : DepaddingHandler
{
    /// <summary>
    /// Block size.
    /// </summary>
    protected readonly int BlockSize;

    private readonly byte[][] _blockCaches;
    private byte[] _blockCache;
    private int _currentWritten;
    private int _blockCacheIdx;
    private bool _didFinal;

    /// <summary>
    /// Initializes a new instance of <see cref="BlockedDepaddingHandler"/>.
    /// </summary>
    /// <param name="supportedBlockSize">Supported block sizes.</param>
    /// <param name="blockSize">Block size, in bytes.</param>
    /// <exception cref="ArgumentException">Thrown for invalid <paramref name="blockSize"/> or illegally configured <paramref name="supportedBlockSize"/>.</exception>
    protected BlockedDepaddingHandler(KeySizes supportedBlockSize, int blockSize)
    {
        if (blockSize <= 0)
        {
            throw new ArgumentException("Invalid block size", nameof(blockSize));
        }
        if (!ValidateBlockSize(supportedBlockSize, blockSize))
        {
            throw new ArgumentException("Invalid block size", nameof(blockSize));
        }
        BlockSize = blockSize;
        _blockCaches = new byte[2][];
        _blockCaches[0] = new byte[blockSize];
        _blockCaches[1] = new byte[blockSize];
        _blockCache = _blockCaches[0];
        _blockCacheIdx = 0;
        _currentWritten = 0;
    }

    /// <inheritdoc />
    public sealed override bool TryUpdate(ReadOnlySpan<byte> data, out ReadOnlySpan<byte> a, out ReadOnlySpan<byte> b)
    {
        if (_didFinal)
        {
            throw new InvalidOperationException("Already performed final padding");
        }
        if (data.Length == 0)
        {
            a = ReadOnlySpan<byte>.Empty;
            b = ReadOnlySpan<byte>.Empty;
            return false;
        }
        if (_currentWritten == BlockSize)
        {
            // existing buffer was already populated from previous run, reuse since we have at least 1 more byte available after this
            a = _blockCache;
            FlipBuffer();
            // current cache now empty
        }
        else if (_currentWritten != 0)
        {
            // Try to populate existing cache
            int rem = BlockSize - _currentWritten;
            int av = Math.Min(data.Length, rem);
            data[..av].CopyTo(_blockCache.AsSpan(_currentWritten, av));
            data = data[av..];
            _currentWritten += av;
            if (data.Length != 0)
            {
                // buffer remaining after this
                // block size matches
                a = _blockCache;
                FlipBuffer();
                // current cache now empty
            }
            else
            {
                // no buffer remaining after this
                // even if block size matches, don't do anything in case data stream didn't actually end here
                a = ReadOnlySpan<byte>.Empty;
                b = ReadOnlySpan<byte>.Empty;
                return false;
            }
        }
        else
        {
            // cache was empty, so leave empty for now
            a = ReadOnlySpan<byte>.Empty;
            // current cache now empty
        }
        // Currently have empty cache
        int usedBytes = Math.Max((data.Length - 1) / BlockSize, 0) * BlockSize;
        b = data[..usedBytes];
        data = data[usedBytes..];
        // data.Length: [1, BlockSize]
        data.CopyTo(_blockCache);
        _currentWritten += data.Length;
        return true;
    }

    /// <inheritdoc />
    public sealed override bool TryUpdate(ReadOnlyMemory<byte> data, out ReadOnlyMemory<byte> a, out ReadOnlyMemory<byte> b)
    {
        if (_didFinal)
        {
            throw new InvalidOperationException("Already performed final padding");
        }
        if (data.Length == 0)
        {
            a = ReadOnlyMemory<byte>.Empty;
            b = ReadOnlyMemory<byte>.Empty;
            return false;
        }
        if (_currentWritten == BlockSize)
        {
            // existing buffer was already populated from previous run, reuse since we have at least 1 more byte available after this
            a = _blockCache;
            FlipBuffer();
            // current cache now empty
        }
        else if (_currentWritten != 0)
        {
            // Try to populate existing cache
            int rem = BlockSize - _currentWritten;
            int av = Math.Min(data.Length, rem);
            data[..av].Span.CopyTo(_blockCache.AsSpan(_currentWritten, av));
            data = data[av..];
            _currentWritten += av;
            if (data.Length != 0)
            {
                // buffer remaining after this
                // block size matches
                a = _blockCache;
                FlipBuffer();
                // current cache now empty
            }
            else
            {
                // no buffer remaining after this
                // even if block size matches, don't do anything in case data stream didn't actually end here
                a = ReadOnlyMemory<byte>.Empty;
                b = ReadOnlyMemory<byte>.Empty;
                return false;
            }
        }
        else
        {
            // cache was empty, so leave empty for now
            a = ReadOnlyMemory<byte>.Empty;
            // current cache now empty
        }
        // Currently have empty cache
        int usedBytes = Math.Max((data.Length - 1) / BlockSize, 0) * BlockSize;
        b = data[..usedBytes];
        data = data[usedBytes..];
        // data.Length: [1, BlockSize]
        data.CopyTo(_blockCache);
        _currentWritten += data.Length;
        return true;
    }

    /// <summary>
    /// Validates a data block and gets padding byte count.
    /// </summary>
    /// <param name="buffer">Buffer to validate.</param>
    /// <param name="b">Padding byte count.</param>
    /// <returns>If true, padding was successfully validated and <paramref name="b"/> stores number of padding bytes.</returns>
    protected abstract bool ValidateLastBlock(ReadOnlySpan<byte> buffer, out byte b);

    /// <inheritdoc />
    public sealed override void DoFinal(out ReadOnlyMemory<byte> buf)
    {
        if (_didFinal)
        {
            throw new InvalidOperationException("Already performed final padding");
        }
        // Should be the case that no data at all was written, so just ignore
        if (_currentWritten == 0)
        {
            buf = ReadOnlyMemory<byte>.Empty;
        }
        else
        {
            if (_currentWritten != BlockSize)
            {
                throw new InvalidDataException("Cannot perform final padding: current state indicates non-block-aligned data");
            }
            if (!ValidateLastBlock(_blockCache, out byte b))
            {
                throw new InvalidDataException("Failed to depad final block: invalid padding");
            }
            buf = new ReadOnlyMemory<byte>(_blockCache, 0, BlockSize - b);
        }
        _didFinal = true;
    }

    private void FlipBuffer()
    {
        if (_currentWritten != BlockSize)
        {
            throw new InvalidOperationException("Failed vibe check, current written != block size");
        }
        _blockCache = _blockCaches[_blockCacheIdx ^= 1];
        _currentWritten = 0;
    }
}
