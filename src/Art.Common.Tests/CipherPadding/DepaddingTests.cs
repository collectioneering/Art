using Art.Common.CipherPadding;

namespace Art.Common.Tests.CipherPadding;

public class DepaddingTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1024)]
    [InlineData(64 * 1024 - 1)]
    [InlineData(64 * 1024)]
    public async Task DepaddingWriteStream_CopyToAsync_Pkcs5Blocks_Valid(int blockCount)
    {
        byte[] buf = new byte[blockCount * 8];
        Random.Shared.NextBytes(buf);
        byte pad = 4;
        PadRepeatEnd(buf, pad);
        ArraySegment<byte> expected = GetDepadded(buf, pad);
        ArraySegment<byte> actual = await CopyViaWriteStreamAsync(buf, new Pkcs5DepaddingHandler(8));
        Assert.True(expected.AsSpan().SequenceEqual(actual));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1024)]
    [InlineData(64 * 1024 - 1)]
    [InlineData(64 * 1024)]
    public async Task DepaddingReadStream_CopyToAsync_Pkcs5Blocks_Valid(int blockCount)
    {
        byte[] buf = new byte[blockCount * 8];
        Random.Shared.NextBytes(buf);
        byte pad = 4;
        PadRepeatEnd(buf, pad);
        ArraySegment<byte> expected = GetDepadded(buf, pad);
        ArraySegment<byte> actual = await CopyViaReadStreamAsync(buf, new Pkcs5DepaddingHandler(8));
        Assert.True(expected.AsSpan().SequenceEqual(actual));
    }

    [Theory]
    [InlineData(1,0)]
    [InlineData(1,1)]
    [InlineData(1,1024)]
    [InlineData(1,64 * 1024 - 1)]
    [InlineData(1,64 * 1024)]
    [InlineData(2,0)]
    [InlineData(2,1)]
    [InlineData(2,1024)]
    [InlineData(2,64 * 1024 - 1)]
    [InlineData(2,64 * 1024)]
    [InlineData(127,0)]
    [InlineData(127,1)]
    [InlineData(127,1024)]
    [InlineData(127,64 * 1024 - 1)]
    [InlineData(127,64 * 1024)]
    [InlineData(128,0)]
    [InlineData(128,1)]
    [InlineData(128,1024)]
    [InlineData(128,64 * 1024 - 1)]
    [InlineData(128,64 * 1024)]
    public async Task DepaddingWriteStream_CopyToAsync_Pkcs7Blocks_Valid( int blockSize, int blockCount)
    {
        byte[] buf = new byte[blockCount * blockSize];
        Random.Shared.NextBytes(buf);
        byte pad = (byte)Math.Max(1, blockSize / 2);
        PadRepeatEnd(buf, pad);
        ArraySegment<byte> expected = GetDepadded(buf, pad);
        ArraySegment<byte> actual = await CopyViaWriteStreamAsync(buf, new Pkcs7DepaddingHandler(blockSize));
        Assert.True(expected.AsSpan().SequenceEqual(actual));
    }

    [Theory]
    [InlineData(1,0)]
    [InlineData(1,1)]
    [InlineData(1,1024)]
    [InlineData(1,64 * 1024 - 1)]
    [InlineData(1,64 * 1024)]
    [InlineData(2,0)]
    [InlineData(2,1)]
    [InlineData(2,1024)]
    [InlineData(2,64 * 1024 - 1)]
    [InlineData(2,64 * 1024)]
    [InlineData(127,0)]
    [InlineData(127,1)]
    [InlineData(127,1024)]
    [InlineData(127,64 * 1024 - 1)]
    [InlineData(127,64 * 1024)]
    [InlineData(128,0)]
    [InlineData(128,1)]
    [InlineData(128,1024)]
    [InlineData(128,64 * 1024 - 1)]
    [InlineData(128,64 * 1024)]
    public async Task DepaddingReadStream_CopyToAsync_Pkcs7Blocks_Valid(int blockSize, int blockCount)
    {
        byte[] buf = new byte[blockCount * blockSize];
        Random.Shared.NextBytes(buf);
        byte pad = (byte)Math.Max(1, blockSize / 2);
        PadRepeatEnd(buf, pad);
        ArraySegment<byte> expected = GetDepadded(buf, pad);
        ArraySegment<byte> actual = await CopyViaReadStreamAsync(buf, new Pkcs7DepaddingHandler(blockSize));
        Assert.True(expected.AsSpan().SequenceEqual(actual));
    }

    [Fact]
    public async Task DepaddingWriteStream_CopyToAsync_Pkcs5BadPad_InvalidDataException()
    {
        byte[] buf = new byte[10 * 8];
        Random.Shared.NextBytes(buf);
        PadRepeatEnd(buf, 4);
        buf[^2] = 0x66;
        await Assert.ThrowsAsync<InvalidDataException>(async () => await CopyViaWriteStreamAsync(buf, new Pkcs5DepaddingHandler(8)));
    }

    [Fact]
    public async Task DepaddingReadStream_CopyToAsync_Pkcs5BadPad_InvalidDataException()
    {
        byte[] buf = new byte[10 * 8];
        Random.Shared.NextBytes(buf);
        PadRepeatEnd(buf, 4);
        buf[^2] = 0x66;
        await Assert.ThrowsAsync<InvalidDataException>(async () => await CopyViaReadStreamAsync(buf, new Pkcs5DepaddingHandler(8)));
    }

    [Fact]
    public async Task DepaddingWriteStream_CopyToAsync_Pkcs7BadPad_InvalidDataException()
    {
        byte[] buf = new byte[10 * 17];
        Random.Shared.NextBytes(buf);
        PadRepeatEnd(buf, 10);
        buf[^4] = 0x66;
        await Assert.ThrowsAsync<InvalidDataException>(async () => await CopyViaWriteStreamAsync(buf, new Pkcs7DepaddingHandler(17)));
    }

    [Fact]
    public async Task DepaddingReadStream_CopyToAsync_Pkcs7BadPad_InvalidDataException()
    {
        byte[] buf = new byte[10 * 17];
        Random.Shared.NextBytes(buf);
        PadRepeatEnd(buf, 10);
        buf[^4] = 0x66;
        await Assert.ThrowsAsync<InvalidDataException>(async () => await CopyViaReadStreamAsync(buf, new Pkcs7DepaddingHandler(17)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public async Task DepaddingWriteStream_CopyToAsync_Pkcs5BadLength_InvalidDataException(int sub)
    {
        byte[] buf = new byte[10 * 8 - sub];
        Random.Shared.NextBytes(buf);
        PadRepeatEnd(buf, 10);
        await Assert.ThrowsAsync<InvalidDataException>(async () => await CopyViaWriteStreamAsync(buf, new Pkcs7DepaddingHandler(17)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public async Task DepaddingReadStream_CopyToAsync_Pkcs5BadLength_InvalidDataException(int sub)
    {
        byte[] buf = new byte[10 * 8 - sub];
        Random.Shared.NextBytes(buf);
        PadRepeatEnd(buf, 10);
        await Assert.ThrowsAsync<InvalidDataException>(async () => await CopyViaReadStreamAsync(buf, new Pkcs7DepaddingHandler(17)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    public async Task DepaddingWriteStream_CopyToAsync_Pkcs7BadLength_InvalidDataException(int sub)
    {
        byte[] buf = new byte[10 * 17 - sub];
        Random.Shared.NextBytes(buf);
        PadRepeatEnd(buf, 10);
        await Assert.ThrowsAsync<InvalidDataException>(async () => await CopyViaWriteStreamAsync(buf, new Pkcs7DepaddingHandler(17)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    public async Task DepaddingReadStream_CopyToAsync_Pkcs7BadLength_InvalidDataException(int sub)
    {
        byte[] buf = new byte[10 * 17 - sub];
        Random.Shared.NextBytes(buf);
        PadRepeatEnd(buf, 10);
        await Assert.ThrowsAsync<InvalidDataException>(async () => await CopyViaReadStreamAsync(buf, new Pkcs7DepaddingHandler(17)));
    }

    private static void PadRepeatEnd(byte[] buf, byte v)
    {
        for (int i = buf.Length - 1, c = 0; i >= 0 && c < v; i--, c++)
        {
            buf[i] = v;
        }
    }

    private static async Task<ArraySegment<byte>> CopyViaWriteStreamAsync(byte[] buf, DepaddingHandler handler)
    {
        MemoryStream ms = new();
        using (DepaddingWriteStream ds = new(handler, ms, true))
        {
            await new MemoryStream(buf).CopyToAsync(ds);
        }
        ms.TryGetBuffer(out var bb);
        return bb;
    }

    private static async Task<ArraySegment<byte>> CopyViaReadStreamAsync(byte[] buf, DepaddingHandler handler)
    {
        MemoryStream ms = new();
        using (DepaddingReadStream ds = new(handler, new MemoryStream(buf), true))
        {
            await ds.CopyToAsync(ms);
        }
        ms.TryGetBuffer(out var bb);
        return bb;
    }

    private static ArraySegment<byte> GetDepadded(byte[] buf, int paddingBytes) => new(buf, 0, Math.Max(0, buf.Length - paddingBytes));
}
