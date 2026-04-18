using System.Security.Cryptography;

namespace Art.Common.CipherPadding;

/// <summary>
/// Handler for null-pad depadding.
/// </summary>
public class ZeroDepaddingHandler : BlockedDepaddingHandler
{
    private static readonly KeySizes s_supportedBlockSize = new(1, int.MaxValue, 1);

    /// <summary>
    /// Initializes a new instance of <see cref="ZeroDepaddingHandler"/>.
    /// </summary>
    /// <param name="blockSize">Block size, in bytes.</param>
    /// <exception cref="ArgumentException">Thrown for invalid <paramref name="blockSize"/>.</exception>
    public ZeroDepaddingHandler(int blockSize) : base(s_supportedBlockSize, blockSize)
    {
    }

    /// <inheritdoc />
    protected override bool ValidateLastBlock(ReadOnlySpan<byte> buffer, out byte b)
    {
        b = 0;
        for (int i = buffer.Length - 1; i >= 0; i--)
        {
            if (buffer[i] == 0)
            {
                b++;
            }
        }
        return true;
    }
}
