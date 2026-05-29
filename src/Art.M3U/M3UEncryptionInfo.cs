using System.Buffers.Binary;
using System.Security.Cryptography;
using Art.Common.Crypto;
using M3USharper;
using PaddingMode = System.Security.Cryptography.PaddingMode;

namespace Art.M3U;

/// <summary>
/// Extension methods for <see cref="M3UEncryptionInfo"/>.
/// </summary>
public static class M3UEncryptionInfoExtensions
{
    /// <summary>
    /// Creates an <see cref="EncryptionInfo"/> from this object.
    /// </summary>
    /// <param name="m3UEncryptionInfo">M3U encryption info.</param>
    /// <param name="mediaSequenceNumber">Current media sequence number.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">State is invalid for direct decryption, or no IV is present in this instance and no media sequence number was provided.</exception>
    /// <exception cref="InvalidDataException">Unexpected <see cref="M3UEncryptionInfo.Method"/> value.</exception>
    public static EncryptionInfo ToEncryptionInfo(this M3UEncryptionInfo m3UEncryptionInfo, long? mediaSequenceNumber = null)
    {
        if (m3UEncryptionInfo.Key == null)
        {
            throw new InvalidOperationException($"Key not present in this instance of {nameof(M3UEncryptionInfo)}");
        }
        byte[]? iv = m3UEncryptionInfo.Iv;
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
        return ToEncryptionInfo(m3UEncryptionInfo.Method, m3UEncryptionInfo.Key, iv);
    }

    private static EncryptionInfo ToEncryptionInfo(string method, byte[] key, byte[] iv) =>
        method switch
        {
            "AES-128" => new EncryptionInfo(CryptoAlgorithm.Aes, key, CipherMode.CBC, EncIv: iv, PaddingMode: PaddingMode.PKCS7),
            "SAMPLE-AES" => throw new InvalidOperationException("Encryption type SAMPLE-AES is not supported for direct decryption"),
            _ => throw new InvalidDataException()
        };
}
