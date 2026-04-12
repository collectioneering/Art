using System.Net;

namespace Art.Extensions.BrowserCookies;

/// <summary>
/// Represents a source for cookies.
/// </summary>
public interface ICookieSource
{
    /// <summary>
    /// Attempts to resolve the provided source to a valid configuration.
    /// </summary>
    /// <exception cref="BrowserProfileNotFoundException">Thrown for unknown browser profile name.</exception>
    /// <remarks>Implementations can, for example, attempt to resolve a user-configured profile name to the profile directory name on the filesystem.</remarks>
    ICookieSource Resolve();

    /// <summary>
    /// Synchronously loads cookies for a domain into the specified <see cref="CookieContainer"/>.
    /// </summary>
    /// <param name="cookieContainer">Container to populate.</param>
    /// <param name="domain">Target domain.</param>
    /// <param name="logHandler">Tool log handler.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArgumentException">Thrown for invalid domain specification.</exception>
    void LoadCookies(CookieContainer cookieContainer, CookieFilter domain, LogHandler? logHandler);

    /// <summary>
    /// Loads cookies for a domain into the specified <see cref="CookieContainer"/>.
    /// </summary>
    /// <param name="cookieContainer">Container to populate.</param>
    /// <param name="domain">Target domain.</param>
    /// <param name="logHandler">Tool log handler.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArgumentException">Thrown for invalid domain specification.</exception>
    Task LoadCookiesAsync(CookieContainer cookieContainer, CookieFilter domain, LogHandler? logHandler = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronously loads cookies for a domain into the specified <see cref="CookieContainer"/>.
    /// </summary>
    /// <param name="cookieContainer">Container to populate.</param>
    /// <param name="domains">Target domains.</param>
    /// <param name="logHandler">Tool log handler.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArgumentException">Thrown for invalid domain specification.</exception>
    void LoadCookies(CookieContainer cookieContainer, IReadOnlyCollection<CookieFilter> domains, LogHandler? logHandler);

    /// <summary>
    /// Loads cookies for a domain into the specified <see cref="CookieContainer"/>.
    /// </summary>
    /// <param name="cookieContainer">Container to populate.</param>
    /// <param name="domains">Target domains.</param>
    /// <param name="logHandler">Tool log handler.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArgumentException">Thrown for invalid domain specification.</exception>
    Task LoadCookiesAsync(CookieContainer cookieContainer, IReadOnlyCollection<CookieFilter> domains, LogHandler? logHandler = null, CancellationToken cancellationToken = default);

}
