using Art.Extensions.BrowserCookies;

namespace Art.BrowserCookies.Chromium;

/// <summary>
/// Represents a context-specific keychain accessor.
/// </summary>
public interface IChromiumKeychain : IDisposable
{
    /// <summary>
    /// Gets the decrypted content for a buffer.
    /// </summary>
    /// <param name="domain">Domain cookie is associated with.</param>
    /// <param name="buffer">Buffer to decrypt.</param>
    /// <param name="logHandler">Tool log handler.</param>
    /// <returns>Decrypted content.</returns>
    string Unlock(string domain, byte[] buffer, LogHandler? logHandler);
}
