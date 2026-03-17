using System.Reflection;
using System.Security.Cryptography;

namespace Art.Common.Tests.Crypto;

public class HashProxyTests
{
    [Theory]
    [InlineData(typeof(SHA1), 0)]
    [InlineData(typeof(SHA1), 8)]
    [InlineData(typeof(SHA1), 3098 * 1024)]
    [InlineData(typeof(SHA256), 0)]
    [InlineData(typeof(SHA256), 8)]
    [InlineData(typeof(SHA256), 3098 * 1024)]
    [InlineData(typeof(SHA384), 0)]
    [InlineData(typeof(SHA384), 8)]
    [InlineData(typeof(SHA384), 3098 * 1024)]
    [InlineData(typeof(SHA512), 0)]
    [InlineData(typeof(SHA512), 8)]
    [InlineData(typeof(SHA512), 3098 * 1024)]
    [InlineData(typeof(MD5), 0)]
    [InlineData(typeof(MD5), 8)]
    [InlineData(typeof(MD5), 3098 * 1024)]
    public void TestHashProxy(Type hashType, int inputSize)
    {
        MethodInfo method = hashType.GetMethod("Create", BindingFlags.Static | BindingFlags.Public, []) ?? throw new InvalidOperationException($"Missing Create for {hashType}");
        HashAlgorithm hashAlgorithm = method.Invoke(null, []) as HashAlgorithm ?? throw new InvalidOperationException("Missing hash algorithm from return");
        TestHashProxy_Internal(inputSize, hashAlgorithm);
    }

    private static void TestHashProxy_Internal(int inputSize, HashAlgorithm hashAlgorithm)
    {
        byte[] arr = new byte[inputSize];
        Random.Shared.NextBytes(arr);
        byte[] expected = hashAlgorithm.ComputeHash(arr);
        HashProxyStream hps = new(new MemoryStream(arr), hashAlgorithm, false, true);
        hps.CopyTo(new MemoryStream());
        byte[] actual = hps.GetHash();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestHashProxyDisposal()
    {
        HashProxyStream hps = new(new MemoryStream(), SHA256.Create());
        _ = hps.Read(new byte[1], 0, 1);
        hps.Dispose();
        Assert.Throws<ObjectDisposedException>(() => hps.Read(new byte[1], 0, 1));
    }

    [Fact]
    public void TestHashProxyNoDisposalFromGetHash()
    {
        HashProxyStream hps = new(new MemoryStream(), SHA256.Create());
        _ = hps.Read(new byte[1], 0, 1);
        hps.GetHash();
        _ = hps.Read(new byte[1], 0, 1);
    }
}
