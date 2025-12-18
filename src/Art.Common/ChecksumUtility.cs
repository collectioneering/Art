using System.Security.Cryptography;

namespace Art.Common;

/// <summary>
/// Utility for <see cref="Checksum"/>.
/// </summary>
public static class ChecksumUtility
{
    /// <summary>
    /// Initializes a new instance of <see cref="Checksum"/>.
    /// </summary>
    /// <param name="id">ID.</param>
    /// <param name="value">Hex string containing checksum value.</param>
    public static Checksum CreateChecksum(string id, string value)
    {
        return new Checksum(id, Dehex(value));
    }

    /// <summary>
    /// Gets checksum of a resource.
    /// </summary>
    /// <param name="artifactDataManager"><see cref="IArtifactDataManager"/>.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="checksumId">Checksum algorithm ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Checksum for resource.</returns>
    /// <exception cref="KeyNotFoundException">Thrown for missing resource.</exception>
    /// <exception cref="ArgumentException">Thrown for a bad <paramref name="checksumId"/> value.</exception>
    public static async ValueTask<Checksum> ComputeChecksumAsync(this IArtifactDataManager artifactDataManager, ArtifactResourceKey key, string checksumId, CancellationToken cancellationToken = default)
    {
        if (!ChecksumSource.DefaultSources.TryGetValue(checksumId, out var checksumSource))
        {
            throw new ArgumentException("Unknown checksum ID", nameof(checksumId));
        }
        using HashAlgorithm hashAlgorithm = checksumSource.CreateHashAlgorithm();
        await using Stream sourceStream = await artifactDataManager.OpenInputStreamAsync(key, cancellationToken).ConfigureAwait(false);
        await using HashProxyStream hps = new(sourceStream, hashAlgorithm, true, true);
        await using MemoryStream ms = new();
        await hps.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        return new Checksum(checksumId, hps.GetHash());
    }

    /// <summary>
    /// Gets checksum of a resource.
    /// </summary>
    /// <param name="artifactDataManager"><see cref="INamespacedArtifactDataManager"/>.</param>
    /// <param name="checksumId">Checksum algorithm ID.</param>
    /// <param name="name">File name.</param>
    /// <param name="path">File path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Checksum for resource.</returns>
    /// <exception cref="KeyNotFoundException">Thrown for missing resource.</exception>
    /// <exception cref="ArgumentException">Thrown for a bad <paramref name="checksumId"/> value.</exception>
    public static async ValueTask<Checksum> ComputeChecksumAsync(this INamespacedArtifactDataManager artifactDataManager, string checksumId, string name, string path = "", CancellationToken cancellationToken = default)
    {
        if (!ChecksumSource.DefaultSources.TryGetValue(checksumId, out var checksumSource))
        {
            throw new ArgumentException("Unknown checksum ID", nameof(checksumId));
        }
        using HashAlgorithm hashAlgorithm = checksumSource.CreateHashAlgorithm();
        await using Stream sourceStream = await artifactDataManager.OpenInputStreamAsync(name, path, cancellationToken).ConfigureAwait(false);
        await using HashProxyStream hps = new(sourceStream, hashAlgorithm, true, true);
        await using MemoryStream ms = new();
        await hps.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        return new Checksum(checksumId, hps.GetHash());
    }

    /// <summary>
    /// Compares two values for data equality.
    /// </summary>
    /// <param name="first">First value.</param>
    /// <param name="second">Other value.</param>
    /// <returns>True if equal.</returns>
    public static bool DatawiseEquals(Checksum? first, Checksum? second)
    {
        if (first != null && second == null || first == null && second != null) return false;
        if (first != null && second != null)
            return string.Equals(first.Id, second.Id, StringComparison.InvariantCultureIgnoreCase) && first.Value.AsSpan().SequenceEqual(second.Value);
        return true;
    }

    private static byte[] Dehex(ReadOnlySpan<char> hex)
    {
        if (hex.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase)) hex = hex[2..];
        return Convert.FromHexString(hex);
    }
}
