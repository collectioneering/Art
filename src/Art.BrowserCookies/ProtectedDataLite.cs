using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Windows.Win32;
using Windows.Win32.Security.Cryptography;
using Art.Extensions.BrowserCookies;

namespace Art.BrowserCookies;

internal static class ProtectedDataLite
{
    public enum Scope
    {
        CurrentUser = 0x0,
        LocalMachine = 0x1,
        System = 0x2,
    }

    [SupportedOSPlatform("windows5.1.2600")]
    public static byte[] Unprotect(byte[] input, byte[]? entropy, Scope scope, LogHandler? logHandler = null)
    {
        return Unprotect(input, entropy ?? ReadOnlySpan<byte>.Empty, scope, logHandler);
    }

    [SupportedOSPlatform("windows5.1.2600")]
    public static unsafe byte[] Unprotect(ReadOnlySpan<byte> input, ReadOnlySpan<byte> entropy, Scope scope, LogHandler? logHandler = null)
    {
        switch (scope)
        {
            case Scope.CurrentUser:
            case Scope.LocalMachine:
                {
                    CRYPT_INTEGER_BLOB blobIn = default;
                    CRYPT_INTEGER_BLOB blobEntropy = default;
                    CRYPT_INTEGER_BLOB blobOut = default;
                    fixed (byte* srcPtr = input, entropyPtr = entropy)
                    {
                        blobIn.pbData = srcPtr;
                        blobIn.cbData = (uint)input.Length;
                        if (entropy.Length > 0)
                        {
                            blobOut.pbData = entropyPtr;
                            blobOut.cbData = (uint)entropy.Length;
                        }
                        uint dwFlags = PInvoke.CRYPTPROTECT_UI_FORBIDDEN;
                        if (scope == Scope.LocalMachine)
                        {
                            dwFlags |= PInvoke.CRYPTPROTECT_LOCAL_MACHINE;
                        }
                        Span<byte> decrypted = Span<byte>.Empty;
                        try
                        {
                            bool result = PInvoke.CryptUnprotectData(
                                &blobIn,
                                null,
                                entropy.Length > 0 ? &blobEntropy : null,
                                null,
                                null,
                                dwFlags,
                                &blobOut
                            );
                            if (!result)
                            {
                                throw new CryptographicException(Marshal.GetLastPInvokeError());
                            }
                            if (blobOut.pbData == null)
                            {
                                throw new OutOfMemoryException();
                            }
                            decrypted = new Span<byte>(blobOut.pbData, (int)blobOut.cbData);
                            return decrypted.ToArray();
                        }
                        finally
                        {
                            if (blobOut.pbData != null)
                            {
                                decrypted.Clear();
                                Marshal.FreeHGlobal(new IntPtr(blobOut.pbData));
                            }
                        }
                    }
                }
            case Scope.System:
                return UnprotectSystem(
                    input,
                    entropy,
                    logHandler);
            default:
                throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
        }
    }

    private static string s_wcunlockB => s_wcunlockBValue ??= BuildScriptBundle();
    private static string? s_wcunlockBValue;

    [SupportedOSPlatform("windows5.1.2600")]
    internal static byte[] UnprotectSystem(
        ReadOnlySpan<byte> input,
        ReadOnlySpan<byte> entropy,
        LogHandler? logHandler)
    {
        string tmpIn = Path.GetTempFileName();
        try
        {
            string tmpEntropy = Path.GetTempFileName();
            try
            {
                string tmpOut = Path.GetTempFileName();
                try
                {
                    using (var fs = File.Create(tmpIn))
                    {
                        fs.Write(input);
                    }
                    if (entropy is { Length: > 0 })
                    {
                        using (var fs = File.Create(tmpEntropy))
                        {
                            fs.Write(entropy);
                        }
                    }
                    ProcessStartInfo psi = new() { FileName = "powershell", Verb = "runas", UseShellExecute = true };
                    psi.ArgumentList.Add("-Command");
                    psi.ArgumentList.Add(s_wcunlockB);
                    psi.ArgumentList.Add("Invoke-UnprotectSystem");
                    psi.ArgumentList.Add($"'{tmpIn}'");
                    psi.ArgumentList.Add($"'{tmpEntropy}'");
                    psi.ArgumentList.Add($"'{tmpOut}'");
                    logHandler?.Invoke("Need Elevation", "Elevation is needed to decrypt keys. A UAC prompt may appear.");
                    var process = Process.Start(psi);
                    logHandler?.Invoke("Running cookie decryption helper...", null);
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
                File.Delete(tmpEntropy);
            }
        }
        finally
        {
            File.Delete(tmpIn);
        }
    }

    [SupportedOSPlatform("windows5.1.2600")]
    internal static byte[] DecryptBufferWithKey(
        ReadOnlySpan<byte> input,
        string keyName,
        LogHandler? logHandler)
    {
        string tmpIn = Path.GetTempFileName();
        try
        {
            string tmpOut = Path.GetTempFileName();
            try
            {
                using (var fs = File.Create(tmpIn))
                {
                    fs.Write(input);
                }
                ProcessStartInfo psi = new() { FileName = "powershell", Verb = "runas", UseShellExecute = true };
                psi.ArgumentList.Add("-Command");
                psi.ArgumentList.Add(s_wcunlockB);
                psi.ArgumentList.Add("Invoke-DecryptBufferWithKey");
                psi.ArgumentList.Add($"'{tmpIn}'");
                psi.ArgumentList.Add($"'{keyName}'");
                psi.ArgumentList.Add($"'{tmpOut}'");
                logHandler?.Invoke("Need Elevation", "Elevation is needed to decrypt keys. A UAC prompt may appear.");
                var process = Process.Start(psi);
                logHandler?.Invoke("Running cookie decryption helper...", null);
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

    private static string BuildScriptBundle()
    {
        var sb = new StringBuilder();
        PsScriptUtil.AppendSimplifiedPsScript(sb, Encoding.UTF8.GetString(LoadResource("Invoke-CommandAs_ps1")));
        sb.AppendLine();
        PsScriptUtil.AppendSimplifiedPsScript(sb, Encoding.UTF8.GetString(LoadResource("Invoke-ScheduledTask_ps1")));
        sb.AppendLine();
        PsScriptUtil.AppendSimplifiedPsScript(sb, Encoding.UTF8.GetString(LoadResource("wcunlockB_ps1")));
        sb.AppendLine();
        return sb.ToString();
    }

    private static byte[] LoadResource(string name)
    {
        using Stream s = typeof(ProtectedDataLite).Assembly.GetManifestResourceStream(name) ?? throw new IOException($"Failed to load manifest resource [{name}]");
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
