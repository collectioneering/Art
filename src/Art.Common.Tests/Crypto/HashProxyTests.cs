using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography;
using NUnit.Framework;

namespace Art.Common.Tests.Crypto;

public class HashProxyTests
{
    [Test, Combinatorial]
    public void TestHashProxy(
        [Values(0, 8, 3098 * 1024)] int inputSize,
        [Values(typeof(SHA1), typeof(SHA256), typeof(SHA384), typeof(SHA512), typeof(MD5))]
        Type hashType)
    {
        MethodInfo method = hashType.GetMethod("Create", BindingFlags.Static | BindingFlags.Public, Array.Empty<Type>())!;
        HashAlgorithm hashAlgorithm = (HashAlgorithm)method.Invoke(null, Array.Empty<object?>())!;
        TestHashProxy(inputSize, hashAlgorithm);
    }

    private static void TestHashProxy(int inputSize, HashAlgorithm hashAlgorithm)
    {
        byte[] arr = new byte[inputSize];
        Random.Shared.NextBytes(arr);
        byte[] expected = hashAlgorithm.ComputeHash(arr);
        HashProxyStream hps = new(new MemoryStream(arr), hashAlgorithm, false, true);
        hps.CopyTo(new MemoryStream());
        byte[] actual = hps.GetHash();
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public void TestHashProxyDisposal()
    {
        HashProxyStream hps = new(new MemoryStream(), SHA256.Create());
        Assert.That(() => hps.Read(new byte[1], 0, 1), Throws.Nothing);
        hps.Dispose();
        Assert.That(() => hps.Read(new byte[1], 0, 1), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public void TestHashProxyNoDisposalFromGetHash()
    {
        HashProxyStream hps = new(new MemoryStream(), SHA256.Create());
        Assert.That(() => hps.Read(new byte[1], 0, 1), Throws.Nothing);
        hps.GetHash();
        Assert.That(() => hps.Read(new byte[1], 0, 1), Throws.Nothing);
    }
}
