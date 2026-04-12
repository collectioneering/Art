using System.Diagnostics.CodeAnalysis;
using System.Net;
using Art.Extensions.BrowserCookies;

namespace Art.BrowserCookies;

/// <summary>
/// Provides shared implementation of a <see cref="ICookieProviderExtension"/>.
/// </summary>
public class CookieProviderExtension : ICookieProviderExtension
{
    /// <inheritdoc />
    public string[] GetSupportedBrowserNames() => CookieSource.GetSupportedBrowserNames();

    /// <inheritdoc />
    public bool TryGetBrowserFromName(string browserName, [NotNullWhen(true)] out ICookieSource? cookieSource, string? profile = null)
    {
        if (CookieSource.TryGetBrowserFromName(browserName, out var localCookieSource, profile))
        {
            cookieSource = localCookieSource;
            return true;
        }
        cookieSource = null;
        return false;
    }

    /// <inheritdoc />
    public void LoadCookies(CookieContainer cookieContainer, CookieFilter domain, string browserName, string? profile = null, LogHandler? logHandler = null)
    {
        CookieSource.LoadCookies(cookieContainer, domain, browserName, profile, logHandler);
    }

    /// <inheritdoc />
    public Task LoadCookiesAsync(CookieContainer cookieContainer, CookieFilter domain, string browserName, string? profile = null, LogHandler? logHandler = null, CancellationToken cancellationToken = default)
    {
        return CookieSource.LoadCookiesAsync(cookieContainer, domain, browserName, profile, logHandler, cancellationToken);
    }

    /// <inheritdoc />
    public void LoadCookies(CookieContainer cookieContainer, IReadOnlyCollection<CookieFilter> domains, string browserName, string? profile = null, LogHandler? logHandler = null)
    {
        CookieSource.LoadCookies(cookieContainer, domains, browserName, profile, logHandler);
    }

    /// <inheritdoc />
    public Task LoadCookiesAsync(CookieContainer cookieContainer, IReadOnlyCollection<CookieFilter> domains, string browserName, string? profile = null, LogHandler? logHandler = null, CancellationToken cancellationToken = default)
    {
        return CookieSource.LoadCookiesAsync(cookieContainer, domains, browserName, profile, logHandler, cancellationToken);
    }
}
