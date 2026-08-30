using System.Security.Cryptography;

namespace Art.Common;

/// <summary>
/// Identifies a checksum source.
/// </summary>
public class ChecksumSource
{
    /// <summary>
    /// Main instance of a SHA256 checksum source.
    /// </summary>
    public static readonly ChecksumSource SHA1 = new("SHA1", System.Security.Cryptography.SHA1.Create);

    /// <summary>
    /// Main instance of a SHA256 checksum source.
    /// </summary>
    public static readonly ChecksumSource SHA256 = new("SHA256", System.Security.Cryptography.SHA256.Create);

    /// <summary>
    /// Main instance of a SHA256 checksum source.
    /// </summary>
    public static readonly ChecksumSource SHA384 = new("SHA384", System.Security.Cryptography.SHA384.Create);

    /// <summary>
    /// Main instance of a SHA256 checksum source.
    /// </summary>
    public static readonly ChecksumSource SHA512 = new("SHA512", System.Security.Cryptography.SHA512.Create);

    /// <summary>
    /// Main instance of a SHA256 checksum source.
    /// </summary>
    public static readonly ChecksumSource MD5 = new("MD5", System.Security.Cryptography.MD5.Create);

    /// <summary>
    /// Default checksum sources.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, ChecksumSource> DefaultSources;

    static ChecksumSource()
    {
        DefaultSources = new[] { SHA1, SHA256, SHA384, SHA512, MD5 }.ToDictionary(source => source.Id, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Algorithm ID.</summary>
    public string Id { get; }

    /// <summary>Hash algorithm function.</summary>
    private Func<HashAlgorithm> HashAlgorithmFunc { get; }

    /// <summary>
    /// Identifies a checksum source.
    /// </summary>
    /// <param name="id">Algorithm ID.</param>
    /// <param name="hashAlgorithmFunc">Hash algorithm function, if available.</param>
    public ChecksumSource(string id, Func<HashAlgorithm> hashAlgorithmFunc)
    {
        Id = id;
        HashAlgorithmFunc = hashAlgorithmFunc;
    }

    /// <summary>
    /// Creates a <see cref="HashAlgorithm"/> for this checksum type.
    /// </summary>
    /// <returns><see cref="HashAlgorithm"/>.</returns>
    public HashAlgorithm CreateHashAlgorithm() => HashAlgorithmFunc();
}
