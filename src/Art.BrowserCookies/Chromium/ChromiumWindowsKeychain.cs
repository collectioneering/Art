using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Art.BrowserCookies.Chromium;

internal class ChromiumWindowsKeychain : IChromiumKeychain
{
    // https://chromium.googlesource.com/chromium/src/+/refs/heads/main/components/os_crypt/os_crypt_win.cc
    // https://armmbed.github.io/mbed-crypto/html/api/ops/aead.html
    // https://github.com/runassu/chrome_v20_decryption/blob/main/decrypt_chrome_v20_cookie.py
    // https://stackoverflow.com/a/79216440
    private const int AeadTagBytes = 16;
    private const int AeadKeyBytes = 256 / 8;
    private const int AeadNonceBytes = 96 / 8;

    private static readonly byte[] s_v20EKey1 =
        Convert.FromHexString("b31c6e241ac846728da9c1fac4936651cffb944d143ab816276bcc6da0284787");

    private static readonly byte[] s_v20EKey2 =
        Convert.FromHexString("e98f37d7f4e1fa433d19304dc2258042090e2d1d7eea7670d41f738d08729660");

    private static readonly byte[] s_v20EKey3 =
        Convert.FromHexString("ccf8a1cec56605b8517552ba1a2d061c03a29e90274fb2fcf59ba4b75c392390");

    private readonly AesGcm _aes10;
    private readonly AesGcm _aes20;
    private bool _disposed;

    public ChromiumWindowsKeychain(byte[] keyV10, byte[] keyV20, ChromiumVariant chromiumVariant)
    {
        if (keyV10.Length != AeadKeyBytes)
        {
            throw new ArgumentException();
        }

        _aes10 = new AesGcm(keyV10, AeadTagBytes);
        switch (chromiumVariant)
        {
            case ChromiumVariant.Edge:
                byte[] keyV20Sub = keyV20[^32..];
                _aes20 = new AesGcm(keyV20Sub, AeadTagBytes);
                break;
            case ChromiumVariant.Chrome:
            case ChromiumVariant.Unspecified:
            default:
                ReadOnlySpan<byte> keySpan = keyV20;
                int len1 = BinaryPrimitives.ReadInt32LittleEndian(keySpan);
                keySpan = keySpan[4..];
                keySpan = keySpan[len1..];
                int len2 = BinaryPrimitives.ReadInt32LittleEndian(keySpan);
                keySpan = keySpan[4..];
                var main = keySpan;
                keySpan = keySpan[len2..];
                Debug.Assert(keySpan.Length == 0);
                switch (main[0])
                {
                    case 1:
                        {
                            byte[] target = new byte[32];
                            DecryptGcm(keyV20.AsSpan()[^60..], target, new AesGcm(s_v20EKey1, AeadTagBytes));
                            _aes20 = new AesGcm(target, AeadTagBytes);
                            break;
                        }
                    case 2:
                        {
                            byte[] target = new byte[32];
                            DecryptGcm(keyV20.AsSpan()[^60..], target, new ChaCha20Poly1305(s_v20EKey2));
                            _aes20 = new AesGcm(target, AeadTagBytes);
                            break;
                        }
                    case 3:
                        {
                            byte[] xKey = ChromiumKeychainUtil.ExecuteWCUnlockB("b", main[1..][..32].ToArray(), null);
                            if (xKey.Length != 32)
                            {
                                throw new IOException("Failed to decrypt");
                            }
                            for (int i = 0; i < xKey.Length; i++)
                            {
                                xKey[i] ^= s_v20EKey3[i];
                            }
                            byte[] target = new byte[32];
                            DecryptGcm(keyV20.AsSpan()[^60..], target, new AesGcm(xKey, AeadTagBytes));
                            _aes20 = new AesGcm(target, AeadTagBytes);
                            break;
                        }
                    default:
                        throw new NotSupportedException();
                }

                break;
        }
    }

    private static int GetGcmDecryptLength(int size)
    {
        return size - AeadNonceBytes - AeadTagBytes;
    }

    private static void DecryptGcm(ReadOnlySpan<byte> source, Span<byte> target, AesGcm aes)
    {
        var nonce = source[..AeadNonceBytes];
        source = source[AeadNonceBytes..];
        var tag = source[^AeadTagBytes..];
        source = source[..^AeadTagBytes];
        aes.Decrypt(nonce, source, tag, target);
    }

    private static void DecryptGcm(ReadOnlySpan<byte> source, Span<byte> target, ChaCha20Poly1305 chaCha20Poly1305)
    {
        var nonce = source[..AeadNonceBytes];
        source = source[AeadNonceBytes..];
        var tag = source[^AeadTagBytes..];
        source = source[..^AeadTagBytes];
        chaCha20Poly1305.Decrypt(nonce, source, tag, target);
    }

    public string Unlock(string domain, byte[] buffer, IToolLogHandler? toolLogHandler)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException();
        }

        EnsureNotDisposed();
        ReadOnlySpan<byte> src = buffer;
        var prefix = src[..3];
        if (prefix.SequenceEqual("v10"u8))
        {
            src = src[3..];
            Span<byte> target = stackalloc byte[GetGcmDecryptLength(src.Length)];
            DecryptGcm(src, target, _aes10);
            return Encoding.UTF8.GetString(target);
        }

        if (prefix.SequenceEqual("v20"u8))
        {
            src = src[3..];
            Span<byte> target = stackalloc byte[GetGcmDecryptLength(src.Length)];
            DecryptGcm(src, target, _aes20);
            return Encoding.UTF8.GetString(target[32..]);
        }

        // fallback
        return Encoding.UTF8.GetString(ProtectedData.Unprotect(buffer, null, DataProtectionScope.CurrentUser));
    }

    public void Dispose()
    {
        _disposed = true;
        _aes10.Dispose();
        _aes20.Dispose();
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ChromiumWindowsKeychain));
        }
    }
}
