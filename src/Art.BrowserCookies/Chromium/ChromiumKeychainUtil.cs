using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Art.BrowserCookies.Chromium;

internal static class ChromiumKeychainUtil
{
    private const int MaxPasswordSize = 1024;

    public static IChromiumKeychain GetMacosKeychain(string service)
    {
        var process = new Process { StartInfo = { FileName = "security", ArgumentList = { "find-generic-password", "-ws", service }, RedirectStandardOutput = true, UseShellExecute = false } };
        process.Start();
        Span<char> buf = new char[MaxPasswordSize];
        try
        {
            int n = process.StandardOutput.ReadBlock(buf);
            if (n == MaxPasswordSize && process.StandardOutput.Peek() != -1)
            {
                throw new InvalidDataException("Password exceeds max length");
            }
            process.WaitForExit();
            return new ChromiumMacosKeychain(buf[..n].Trim());
        }
        finally
        {
            buf.Clear();
        }
    }

    public static async Task<IChromiumKeychain> GetMacosKeychainAsync(string service, CancellationToken cancellationToken = default)
    {
        var process = new Process { StartInfo = { FileName = "security", ArgumentList = { "find-generic-password", "-ws", service }, RedirectStandardOutput = true, UseShellExecute = false } };
        process.Start();
        Memory<char> buf = new char[MaxPasswordSize];
        try
        {
            int n = await process.StandardOutput.ReadBlockAsync(buf, cancellationToken).ConfigureAwait(false);
            if (n == MaxPasswordSize && process.StandardOutput.Peek() != -1)
            {
                throw new InvalidDataException("Password exceeds max length");
            }
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            return new ChromiumMacosKeychain(buf.Span[..n].Trim());
        }
        finally
        {
            buf.Span.Clear();
        }
    }

    [SupportedOSPlatform("windows5.1.2600")]
    public static IChromiumKeychain GetWindowsKeychain(ChromiumVariant chromiumVariant, string userDataPath, IToolLogHandler? toolLogHandler = null)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException();
        }
        string file = Path.Join(userDataPath, "Local State");
        using var stream = File.OpenRead(file);
        return GetWindowsKeychainInternal(chromiumVariant, JsonSerializer.Deserialize(stream, SourceGenerationContext.Default.ChromiumWindowsLocalState) ?? throw new InvalidDataException(), toolLogHandler);
    }

    [SupportedOSPlatform("windows5.1.2600")]
    public static async Task<IChromiumKeychain> GetWindowsKeychainAsync(ChromiumVariant chromiumVariant, string userDataPath, IToolLogHandler? toolLogHandler = null, CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException();
        }
        string file = Path.Join(userDataPath, "Local State");
        await using var stream = File.OpenRead(file);
        return GetWindowsKeychainInternal(chromiumVariant, await JsonSerializer.DeserializeAsync(stream, SourceGenerationContext.Default.ChromiumWindowsLocalState, cancellationToken: cancellationToken).ConfigureAwait(false) ?? throw new InvalidDataException(), toolLogHandler);
    }

    [SupportedOSPlatform("windows5.1.2600")]
    private static ChromiumWindowsKeychain GetWindowsKeychainInternal(ChromiumVariant chromiumVariant, ChromiumWindowsLocalState state, IToolLogHandler? toolLogHandler)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException();
        }
        byte[] data10 = Convert.FromBase64String(state.OsCrypt.EncryptedKey)[5..];
        byte[] res10 = ProtectedDataLite.Unprotect(data10, null, ProtectedDataLite.Scope.CurrentUser);
        data10.AsSpan().Clear();
        byte[] data20 = Convert.FromBase64String(state.OsCrypt.AppBoundEncryptedKey);
        if (!data20.AsSpan().StartsWith("APPB"u8))
        {
            throw new InvalidDataException();
        }
        byte[] data20Sub = data20[4..];
        data20.AsSpan().Clear();
        ProtectedDataLite.LogHandler? protectedDataLogHandler = toolLogHandler != null ? (title, body) => toolLogHandler.Log(title, body, LogLevel.Information) : null;
        byte[] res20 = ProtectedDataLite.Unprotect(ProtectedDataLite.Unprotect(data20Sub, null, ProtectedDataLite.Scope.System, protectedDataLogHandler), null, ProtectedDataLite.Scope.CurrentUser);
        data20Sub.AsSpan().Clear();
        var keychain = new ChromiumWindowsKeychain(res10, res20, chromiumVariant);
        res10.AsSpan().Clear();
        res20.AsSpan().Clear();
        return keychain;
    }
}
