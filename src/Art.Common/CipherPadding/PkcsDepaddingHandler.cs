using System.Security.Cryptography;

namespace Art.Common.CipherPadding;

/// <summary>
/// Base handler for PKCS#5/PKCS#7 depadding.
/// </summary>
public abstract class PkcsDepaddingHandler : BlockedDepaddingHandler
{
    /// <summary>
    /// Initializes a new instance of <see cref="PkcsDepaddingHandler"/>.
    /// </summary>
    /// <param name="supportedBlockSize">Supported block sizes.</param>
    /// <param name="blockSize">Block size, in bytes.</param>
    /// <exception cref="ArgumentException">Thrown for invalid <paramref name="blockSize"/> or illegally configured <paramref name="supportedBlockSize"/>.</exception>
    protected PkcsDepaddingHandler(KeySizes supportedBlockSize, int blockSize) : base(supportedBlockSize, blockSize)
    {
    }

    /// <inheritdoc />
    protected override bool ValidateLastBlock(ReadOnlySpan<byte> buffer, out byte b)
    {
        if (buffer.Length == 0)
        {
            b = 0;
            return true;
        }
        b = buffer[^1];
        if (b > buffer.Length)
        {
            return false;
        }
        for (int i = buffer.Length - 1, c = 0; i >= 0 && c < b; i--, c++)
        {
            if (buffer[i] != b)
            {
                return false;
            }
        }
        return true;
    }
}
