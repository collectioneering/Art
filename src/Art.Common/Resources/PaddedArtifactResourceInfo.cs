using Art.Common.CipherPadding;
using Art.Common.Crypto;

namespace Art.Common.Resources;

/// <summary>
/// Represents a resource with padding.
/// </summary>
/// <param name="ArtPaddingMode">Padding mode.</param>
/// <param name="BlockSize">Block size, in bits.</param>
/// <param name="BaseArtifactResourceInfo">Base resource.</param>
public record PaddedArtifactResourceInfo(ArtPaddingMode ArtPaddingMode, int? BlockSize, ArtifactResourceInfo BaseArtifactResourceInfo)
    : ArtifactResourceInfo(
        BaseArtifactResourceInfo.Key,
        BaseArtifactResourceInfo.ContentType,
        BaseArtifactResourceInfo.Updated,
        BaseArtifactResourceInfo.Retrieved,
        BaseArtifactResourceInfo.Version,
        BaseArtifactResourceInfo.Checksum)
{
    /// <inheritdoc/>
    public override bool CanExportStream => BaseArtifactResourceInfo.CanExportStream;

    /// <inheritdoc />
    public override bool CanGetStream => BaseArtifactResourceInfo.CanGetStream;

    /// <inheritdoc/>
    public override ValueTask ExportStreamAsync(Stream targetStream, ArtifactResourceExportOptions? exportOptions = null, CancellationToken cancellationToken = default)
    {
        var dp = GetDepaddingHandler();
        return ExportStreamWithDepaddingHandlerAsync(dp, targetStream, exportOptions, cancellationToken);
    }

    /// <inheritdoc />
    public override async ValueTask<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
    {
        var dp = GetDepaddingHandler();
        return new DepaddingReadStream(dp, await BaseArtifactResourceInfo.GetStreamAsync(cancellationToken).ConfigureAwait(false));
    }

    private DepaddingHandler GetDepaddingHandler()
    {
        GetParameters(out int? blockSizeBytesV);
        if (blockSizeBytesV is not { } blockSizeBytes) throw new InvalidOperationException("No block size provided");
        return ArtPaddingMode switch
        {
            ArtPaddingMode.Zero => new ZeroDepaddingHandler(blockSizeBytes),
            ArtPaddingMode.AnsiX9_23 => new AnsiX9_23DepaddingHandler(blockSizeBytes),
            ArtPaddingMode.Iso10126 => new Iso10126DepaddingHandler(blockSizeBytes),
            ArtPaddingMode.Pkcs7 => new Pkcs7DepaddingHandler(blockSizeBytes),
            ArtPaddingMode.Pkcs5 => new Pkcs5DepaddingHandler(blockSizeBytes),
            ArtPaddingMode.Iso_Iec_7816_4 => new Iso_Iec_7816_4DepaddingHandler(blockSizeBytes),
            _ => throw new InvalidOperationException("Invalid padding mode")
        };
    }

    private void GetParameters(out int? blockSizeBytes)
    {
        if (BlockSize is { } blockSize)
        {
            blockSizeBytes = GetBlockSizeBytes(blockSize);
        }
        else if (BaseArtifactResourceInfo is EncryptedArtifactResourceInfo({ PaddingMode: System.Security.Cryptography.PaddingMode.None } encryptionInfo, _))
        {
            // Fallback to trying to retrieve block size from base encrypted resource
            // Only do this if the base resource isn't configured to depad, it doesn't make sense to depad a depadded output
            // If nested depadding is *really* needed, explicitly set block size in the first place
            if (encryptionInfo.GetBlockSize() is { } encryptionInfoBlockSize)
            {
                blockSizeBytes = GetBlockSizeBytes(encryptionInfoBlockSize);
            }
            else
            {
                blockSizeBytes = null;
            }
        }
        else
        {
            blockSizeBytes = null;
        }
    }

    private static int GetBlockSizeBytes(int blockSizeBits)
    {
        if (blockSizeBits % 8 != 0)
        {
            throw new NotSupportedException($"Non-byte-aligned block size {blockSizeBits} is not supported");
        }
        return blockSizeBits / 8;
    }

    private async ValueTask ExportStreamWithDepaddingHandlerAsync(DepaddingHandler handler, Stream targetStream, ArtifactResourceExportOptions? exportOptions, CancellationToken cancellationToken)
    {
        await using DepaddingWriteStream ds = new(handler, targetStream, true);
        await BaseArtifactResourceInfo.ExportStreamAsync(ds, exportOptions, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override bool UsesMetadata => BaseArtifactResourceInfo.UsesMetadata;

    /// <inheritdoc/>
    public override async ValueTask<ArtifactResourceInfo> WithMetadataAsync(CancellationToken cancellationToken = default)
    {
        ArtifactResourceInfo b = await BaseArtifactResourceInfo.WithMetadataAsync(cancellationToken).ConfigureAwait(false);
        return this with { BaseArtifactResourceInfo = b, ContentType = b.ContentType, Updated = b.Updated, Version = b.Version };
    }
}
