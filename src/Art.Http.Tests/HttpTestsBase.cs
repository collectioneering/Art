using System;
using System.IO;
using System.Security.Cryptography;
using Art.Common;
using RichardSzalay.MockHttp;

namespace Art.Http.Tests;

public class HttpTestsBase : IDisposable
{
    protected class TestHttpArtifactTool : HttpArtifactTool
    {
    }

    protected HttpArtifactTool Tool;
    protected MockHttpMessageHandler MockHandler;
    protected ArtifactData Data;

    public HttpTestsBase()
    {
        Tool = new TestHttpArtifactTool();
        Tool.InitializeAsync().Wait();
        MockHandler = new MockHttpMessageHandler();
        Tool.HttpClient = MockHandler.ToHttpClient();
        Data = Tool.CreateData("default");
    }

    protected static void PreludeRandomContent(int length, out byte[] originalData, out MemoryStream source)
    {
        originalData = new byte[length];
        Random.Shared.NextBytes(originalData);
        source = new MemoryStream(originalData);
    }

    protected static void PreludeRandomEncryptedContent(Func<SymmetricAlgorithm> saf, CipherMode mode, PaddingMode paddingMode, int length, int keyBits, int ivBits, out byte[] originalData, out byte[] key, out byte[] iv, out MemoryStream encrypted)
    {
        originalData = new byte[length];
        key = new byte[keyBits / 8];
        iv = new byte[ivBits / 8];
        Random.Shared.NextBytes(originalData);
        Random.Shared.NextBytes(key);
        Random.Shared.NextBytes(iv);
        encrypted = new MemoryStream();
        using SymmetricAlgorithm sa = saf();
        sa.Mode = mode;
        sa.Padding = paddingMode;
        using ICryptoTransform enc = sa.CreateEncryptor(key, iv);
        using CryptoStream cs = new(encrypted, enc, CryptoStreamMode.Write, true);
        cs.Write(originalData);
        cs.FlushFinalBlock();
        encrypted.Position = 0;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Tool.Dispose();
            MockHandler.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
