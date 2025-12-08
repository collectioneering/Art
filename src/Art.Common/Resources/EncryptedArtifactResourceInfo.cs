using System.Security.Cryptography;
using Art.Common.Crypto;

namespace Art.Common.Resources;

/// <summary>
/// Represents an encrypted resource.
/// </summary>
/// <param name="EncryptionInfo">Encryption information.</param>
/// <param name="BaseArtifactResourceInfo">Base resource.</param>
public record EncryptedArtifactResourceInfo(EncryptionInfo EncryptionInfo, ArtifactResourceInfo BaseArtifactResourceInfo)
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
    public override async ValueTask ExportStreamAsync(Stream targetStream, ArtifactResourceExportOptions? exportOptions = null, CancellationToken cancellationToken = default)
    {
        using SymmetricAlgorithm algorithm = EncryptionInfo.CreateSymmetricAlgorithm();
        await using CryptoStream cs = new(targetStream, algorithm.CreateDecryptor(), CryptoStreamMode.Write, true);
        await BaseArtifactResourceInfo.ExportStreamAsync(cs, exportOptions, cancellationToken).ConfigureAwait(false);
        // crypto stream flushes on dispose, but do this too to be like... more sure
        if (!cs.HasFlushedFinalBlock) await cs.FlushFinalBlockAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async ValueTask<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
    {
        SymmetricAlgorithm algorithm = EncryptionInfo.CreateSymmetricAlgorithm();
        return new CryptoStream(await BaseArtifactResourceInfo.GetStreamAsync(cancellationToken).ConfigureAwait(false), algorithm.CreateDecryptor(), CryptoStreamMode.Read, false);
    }

    /// <inheritdoc/>
    public override bool UsesMetadata => BaseArtifactResourceInfo.UsesMetadata;

    /// <inheritdoc/>
    public override async ValueTask<ArtifactResourceInfo> WithMetadataAsync(CancellationToken cancellationToken = default)
    {
        ArtifactResourceInfo b = await BaseArtifactResourceInfo.WithMetadataAsync(cancellationToken).ConfigureAwait(false);
        return this with
        {
            BaseArtifactResourceInfo = b,
            ContentType = b.ContentType,
            Updated = b.Updated,
            Version = b.Version,
            Checksum = b.Checksum
        };
    }

    /// <inheritdoc />
    public override void AugmentOutputStreamOptions(ref OutputStreamOptions options)
        => BaseArtifactResourceInfo.AugmentOutputStreamOptions(ref options);
}
