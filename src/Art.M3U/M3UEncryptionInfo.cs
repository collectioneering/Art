using System.Buffers.Binary;
using System.Security.Cryptography;
using Art.Common.Crypto;
using PaddingMode = System.Security.Cryptography.PaddingMode;

namespace Art.M3U;

/// <summary>
/// Stores encryption information for a stream.
/// </summary>
public class M3UEncryptionInfo
{
    /// <summary>
    /// True if <see cref="Method"/> indicates entries are encrypted (anything but "NONE").
    /// </summary>
    public bool Encrypted => Method != "NONE";

    /// <summary>
    /// Key format.
    /// </summary>
    public string KeyFormat { get; set; }

    /// <summary>
    /// Encryption method.
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// Key URI.
    /// </summary>
    public string? Uri { get; set; }

    /// <summary>
    /// Key.
    /// </summary>
    public byte[]? Key { get; set; }

    /// <summary>
    /// IV.
    /// </summary>
    public byte[]? Iv { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="M3UEncryptionInfo"/>.
    /// </summary>
    /// <param name="keyFormat">Key format.</param>
    /// <param name="method">Method.</param>
    /// <param name="uri">URI.</param>
    /// <param name="key">Key.</param>
    /// <param name="iv">IV.</param>
    public M3UEncryptionInfo(string keyFormat, string method, string? uri = null, byte[]? key = null, byte[]? iv = null)
    {
        KeyFormat = keyFormat;
        Method = method;
        Uri = uri;
        Key = key;
        Iv = iv;
    }

    /// <summary>
    /// Creates an <see cref="EncryptionInfo"/> from this object.
    /// </summary>
    /// <param name="mediaSequenceNumber">Current media sequence number.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">State is invalid for direct decryption, or no IV is present in this instance and no media sequence number was provided.</exception>
    /// <exception cref="InvalidDataException">Unexpected <see cref="Method"/> value.</exception>
    public EncryptionInfo ToEncryptionInfo(long? mediaSequenceNumber = null)
    {
        if (Key == null)
        {
            throw new InvalidOperationException($"Key not present in this instance of {nameof(M3UEncryptionInfo)}");
        }
        byte[]? iv = Iv;
        if (iv == null)
        {
            if (mediaSequenceNumber is { } msn)
            {
                BinaryPrimitives.WriteInt64BigEndian(iv = new byte[16], msn);
            }
            else
            {
                throw new InvalidOperationException($"IV not present in this instance of {nameof(M3UEncryptionInfo)}, and no media sequence number provided to synthesize IV");
            }
        }
        return ToEncryptionInfo(Method, Key, iv);
    }

    private static EncryptionInfo ToEncryptionInfo(string method, byte[] key, byte[] iv) =>
        method switch
        {
            "AES-128" => new EncryptionInfo(CryptoAlgorithm.Aes, key, CipherMode.CBC, EncIv: iv, PaddingMode: PaddingMode.PKCS7),
            "SAMPLE-AES" => throw new InvalidOperationException("Encryption type SAMPLE-AES is not supported for direct decryption"),
            _ => throw new InvalidDataException()
        };
}
