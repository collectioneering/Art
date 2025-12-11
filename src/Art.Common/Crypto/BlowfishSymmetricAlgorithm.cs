using System.Security.Cryptography;

namespace Art.Common.Crypto;

internal class BlowfishSymmetricAlgorithm : SymmetricAlgorithm
{
    public const int DefaultKeySize = 56 * 8;

    public override CipherMode Mode
    {
        get => ModeValue;
        set => ModeValue = value switch
        {
            CipherMode.CBC => value,
            CipherMode.ECB => value,
            _ => throw new NotSupportedException()
        };
    }

    public BlowfishSymmetricAlgorithm(int keySizeValue = DefaultKeySize)
    {
        LegalKeySizesValue = [new(4 * 8, 56 * 8, 8)];
        LegalBlockSizesValue = [new(8 * 8, 8 * 8, 8)];
        KeySizeValue = keySizeValue;
        ModeValue = CipherMode.ECB;
    }

    public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[]? rgbIV)
    {
        Blowfish bf = rgbIV != null ? new Blowfish(rgbKey, rgbIV) : new Blowfish(rgbKey);
        return new BlowfishCryptoTransform(bf, ModeValue, false);
    }

    public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[]? rgbIV)
    {
        Blowfish bf = rgbIV != null ? new Blowfish(rgbKey, rgbIV) : new Blowfish(rgbKey);
        return new BlowfishCryptoTransform(bf, ModeValue, true);
    }

    public override void GenerateIV()
    {
    }

    public override void GenerateKey()
    {
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        DoFill(KeyValue);
        KeyValue = new byte[KeySize / 8];
        rng.GetBytes(KeyValue);
    }

    protected override void Dispose(bool disposing)
    {
        DoFill(KeyValue);
        DoFill(IVValue);
        base.Dispose(disposing);
    }

    private static void DoFill(byte[]? v)
    {
        if (v == null) return;
        Array.Fill<byte>(v, 0);
    }

    private class BlowfishCryptoTransform : ICryptoTransform
    {
        private Blowfish _b;
        private readonly CipherMode _mode;
        private readonly bool _e;

        public BlowfishCryptoTransform(Blowfish blowfish, CipherMode mode, bool encrypt)
            => (_b, _mode, _e) = (blowfish, mode, encrypt);

        public bool CanReuseTransform => true;

        public bool CanTransformMultipleBlocks => false;

        public int InputBlockSize => 8 * 8;

        public int OutputBlockSize => 8 * 8;

        public void Dispose()
        {
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            Decode(inputBuffer.AsSpan(inputOffset, inputCount), outputBuffer.AsSpan(outputOffset, inputCount));
            return inputCount;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            byte[] res = new byte[inputCount];
            Decode(inputBuffer.AsSpan(inputOffset, inputCount), res);
            return res;
        }

        private void Decode(ReadOnlySpan<byte> srcBuffer, Span<byte> resBuffer)
        {
            if (srcBuffer.Length % 8 != 0) throw new ArgumentException("Cannot process data not matching block size");
            if (resBuffer.Length != srcBuffer.Length) throw new ArgumentException("Result buffer size must be equal to source buffer size");
            srcBuffer.CopyTo(resBuffer);
            switch (_mode)
            {
                case CipherMode.CBC:
                    if (_e) _b.EncryptCbc(resBuffer);
                    else _b.DecryptCbc(resBuffer);
                    break;
                case CipherMode.ECB:
                    if (_e) _b.EncryptEcb(resBuffer);
                    else _b.DecryptEcb(resBuffer);
                    break;
                case CipherMode.OFB:
                case CipherMode.CFB:
                case CipherMode.CTS:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
