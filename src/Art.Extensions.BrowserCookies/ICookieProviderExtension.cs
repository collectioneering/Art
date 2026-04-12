using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Art.Extensions.BrowserCookies;

/// <summary>
/// Provides an interface for a cookie provider extension.
/// </summary>
public interface ICookieProviderExtension
{
    /// <summary>
    /// Gets supported browser names.
    /// </summary>
    /// <returns>Supported browser names.</returns>
    string[] GetSupportedBrowserNames();

    /// <summary>
    /// Attempts to get a <see cref="ICookieSource"/> for the specified browser.
    /// </summary>
    /// <param name="browserName">Browser name.</param>
    /// <param name="cookieSource">Retrieved <see cref="ICookieSource"/>.</param>
    /// <param name="profile">Optional browser profile name to initialize with.</param>
    /// <returns>True if <paramref name="cookieSource"/> was found.</returns>
    /// <remarks>
    /// This method only checks if the supplied browser name corresponds to a supported type.
    /// This method does not check if the browser is installed.
    /// </remarks>
    bool TryGetBrowserFromName(string browserName, [NotNullWhen(true)] out ICookieSource? cookieSource, string? profile = null);

    /// <summary>
    /// Loads cookies for a domain into the specified <see cref="CookieContainer"/>.
    /// </summary>
    /// <param name="cookieContainer">Container to populate.</param>
    /// <param name="domain">Target domain.</param>
    /// <param name="browserName">Browser name.</param>
    /// <param name="profile">Optional browser profile name.</param>
    /// <param name="logHandler">Tool log handler.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArgumentException">Thrown for invalid domain specification.</exception>
    /// <exception cref="BrowserNotFoundException">Thrown for unknown browser name.</exception>
    /// <exception cref="BrowserProfileNotFoundException">Thrown for unknown browser profile name.</exception>
    void LoadCookies(CookieContainer cookieContainer, CookieFilter domain, string browserName, string? profile = null, LogHandler? logHandler = null);

    /// <summary>
    /// Loads cookies for a domain into the specified <see cref="CookieContainer"/>.
    /// </summary>
    /// <param name="cookieContainer">Container to populate.</param>
    /// <param name="domain">Target domain.</param>
    /// <param name="browserName">Browser name.</param>
    /// <param name="profile">Optional browser profile name.</param>
    /// <param name="logHandler">Tool log handler.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArgumentException">Thrown for invalid domain specification.</exception>
    /// <exception cref="BrowserNotFoundException">Thrown for unknown browser name.</exception>
    /// <exception cref="BrowserProfileNotFoundException">Thrown for unknown browser profile name.</exception>
    Task LoadCookiesAsync(CookieContainer cookieContainer, CookieFilter domain, string browserName, string? profile = null, LogHandler? logHandler = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads cookies for a domain into the specified <see cref="CookieContainer"/>.
    /// </summary>
    /// <param name="cookieContainer">Container to populate.</param>
    /// <param name="domains">Target domains.</param>
    /// <param name="browserName">Browser name.</param>
    /// <param name="profile">Optional browser profile name.</param>
    /// <param name="logHandler">Tool log handler.</param>
    /// <exception cref="ArgumentException">Thrown for invalid domain specification.</exception>
    /// <exception cref="BrowserNotFoundException">Thrown for unknown browser name.</exception>
    /// <exception cref="BrowserProfileNotFoundException">Thrown for unknown browser profile name.</exception>
    void LoadCookies(CookieContainer cookieContainer, IReadOnlyCollection<CookieFilter> domains, string browserName, string? profile = null, LogHandler? logHandler = null);

    /// <summary>
    /// Loads cookies for a domain into the specified <see cref="CookieContainer"/>.
    /// </summary>
    /// <param name="cookieContainer">Container to populate.</param>
    /// <param name="domains">Target domains.</param>
    /// <param name="browserName">Browser name.</param>
    /// <param name="profile">Optional browser profile name.</param>
    /// <param name="logHandler">Tool log handler.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArgumentException">Thrown for invalid domain specification.</exception>
    /// <exception cref="BrowserNotFoundException">Thrown for unknown browser name.</exception>
    /// <exception cref="BrowserProfileNotFoundException">Thrown for unknown browser profile name.</exception>
    Task LoadCookiesAsync(CookieContainer cookieContainer, IReadOnlyCollection<CookieFilter> domains, string browserName, string? profile = null, LogHandler? logHandler = null, CancellationToken cancellationToken = default);
}
