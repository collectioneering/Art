using System.Security.Cryptography;

namespace Art.Common.Crypto;

internal class XorSymmetricAlgorithm : SymmetricAlgorithm
{
    public const int DefaultKeySize = 16 * 8;

    public XorSymmetricAlgorithm(int keySizeValue = DefaultKeySize)
    {
        LegalKeySizesValue = [new(8, int.MaxValue, 8)];
        LegalBlockSizesValue = [new(8, int.MaxValue, 8)];
        KeySizeValue = keySizeValue;
    }

    public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[]? rgbIV) => new XorCryptoTransform(rgbKey);

    public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[]? rgbIV) => new XorCryptoTransform(rgbKey);

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

    private class XorCryptoTransform : ICryptoTransform
    {
        private readonly byte[] _key;

        public XorCryptoTransform(byte[] key) => _key = key;

        public bool CanReuseTransform => true;

        public bool CanTransformMultipleBlocks => false;

        public int InputBlockSize => _key.Length;

        public int OutputBlockSize => _key.Length;

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
            if (srcBuffer.Length > _key.Length) throw new ArgumentException("Cannot process block greater than key size");
            if (resBuffer.Length != srcBuffer.Length) throw new ArgumentException("Result buffer size must be equal to source buffer size");
            for (int i = 0; i < resBuffer.Length; i++)
                resBuffer[i] = (byte)(srcBuffer[i] ^ _key[i]);
        }
    }
}
