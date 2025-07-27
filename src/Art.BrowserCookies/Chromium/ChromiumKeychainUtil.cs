using System.Diagnostics;
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

    public static IChromiumKeychain GetWindowsKeychain(ChromiumVariant chromiumVariant, string userDataPath, IToolLogHandler? toolLogHandler = null)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException();
        }
        string file = Path.Combine(userDataPath, "Local State");
        using var stream = File.OpenRead(file);
        return GetWindowsKeychainInternal(chromiumVariant, JsonSerializer.Deserialize(stream, SourceGenerationContext.Default.ChromiumWindowsLocalState) ?? throw new InvalidDataException(), toolLogHandler);
    }

    public static async Task<IChromiumKeychain> GetWindowsKeychainAsync(ChromiumVariant chromiumVariant, string userDataPath, IToolLogHandler? toolLogHandler = null, CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException();
        }
        string file = Path.Combine(userDataPath, "Local State");
        await using var stream = File.OpenRead(file);
        return GetWindowsKeychainInternal(chromiumVariant, await JsonSerializer.DeserializeAsync(stream, SourceGenerationContext.Default.ChromiumWindowsLocalState, cancellationToken: cancellationToken).ConfigureAwait(false) ?? throw new InvalidDataException(), toolLogHandler);
    }

    private static ChromiumWindowsKeychain GetWindowsKeychainInternal(ChromiumVariant chromiumVariant, ChromiumWindowsLocalState state, IToolLogHandler? toolLogHandler)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException();
        }
        byte[] data10 = Convert.FromBase64String(state.OsCrypt.EncryptedKey)[5..];
        byte[] res10 = ProtectedData.Unprotect(data10, null, DataProtectionScope.CurrentUser);
        data10.AsSpan().Clear();
        byte[] data20 = Convert.FromBase64String(state.OsCrypt.AppBoundEncryptedKey);
        if (!data20.AsSpan().StartsWith("APPB"u8))
        {
            throw new InvalidDataException();
        }
        byte[] data20Sub = data20[4..];
        data20.AsSpan().Clear();
        byte[] res20 = ProtectedData.Unprotect(ExecuteWCUnlockB("a", data20Sub, toolLogHandler), null, DataProtectionScope.CurrentUser);
        data20Sub.AsSpan().Clear();
        var keychain = new ChromiumWindowsKeychain(res10, res20, chromiumVariant);
        res10.AsSpan().Clear();
        res20.AsSpan().Clear();
        return keychain;
    }

    internal static byte[] ExecuteWCUnlockB(string variant, byte[] input, IToolLogHandler? toolLogHandler)
    {
        string tmpIn = Path.GetTempFileName();
        try
        {
            string tmpOut = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tmpIn, input);
                ProcessStartInfo psi = new() { FileName = "powershell", Verb = "runas", UseShellExecute = true };
                psi.ArgumentList.Add("-Command");
                psi.ArgumentList.Add(s_wcunlockB);
                psi.ArgumentList.Add(variant);
                psi.ArgumentList.Add(tmpIn);
                psi.ArgumentList.Add(tmpOut);
                toolLogHandler?.Log("Need Elevation", "Elevation is needed to decrypt keys. A UAC prompt may appear.", LogLevel.Information);
                var process = Process.Start(psi);
                toolLogHandler?.Log("Running cookie decryption helper...", null, LogLevel.Information);
                process?.WaitForExit();
                return File.ReadAllBytes(tmpOut);
            }
            finally
            {
                File.Delete(tmpOut);
            }
        }
        finally
        {
            File.Delete(tmpIn);
        }
    }

    private static string s_wcunlockB => s_wcunlockBValue ??= Encoding.UTF8.GetString(LoadResource("wcunlockB"));
    private static string? s_wcunlockBValue;

    private static byte[] LoadResource(string name)
    {
        using Stream s = typeof(ChromiumKeychainUtil).Assembly.GetManifestResourceStream(name) ?? throw new IOException($"Failed to load manifest resource [{name}]");
        if (s.CanSeek)
        {
            long l = s.Length;
            if (l > int.MaxValue)
            {
                throw new InvalidOperationException($"Manifest resource [{name}] has length {l} which exceeds supported length");
            }
            byte[] result = new byte[l];
            s.ReadExactly(result);
            return result;
        }
        var ms = new MemoryStream();
        s.CopyTo(ms);
        return ms.ToArray();
    }
}
