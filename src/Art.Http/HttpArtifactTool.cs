using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.WebSockets;
using Art.BrowserCookies;
using Art.Common;

namespace Art.Http;

/// <summary>
/// Represents an instance of an artifact tool that depends on an <see cref="System.Net.Http.HttpClient"/>.
/// </summary>
public abstract partial class HttpArtifactTool : ArtifactTool
{
    #region Fields

    /// <summary>
    /// Option used to specify browser to extract cookies from.
    /// </summary>
    public const string OptCookieBrowser = "cookieBrowser";

    /// <summary>
    /// Optional option used to specify browser profile to get cookies from if applicable.
    /// </summary>
    public const string OptCookieBrowserProfile = "cookieBrowserProfile";

    /// <summary>
    /// Option used to specify one domain to filter when extracting browser cookies.
    /// </summary>
    public const string OptCookieBrowserDomain = "cookieBrowserDomain";

    /// <summary>
    /// Option used to specify domains to filter when extracting browser cookies.
    /// </summary>
    public const string OptCookieBrowserDomains = "cookieBrowserDomains";

    /// <summary>
    /// Option used to specify path to http client cookie file.
    /// </summary>
    public const string OptCookieFile = "cookieFile";

    /// <summary>
    /// Option used to specify custom user agent string.
    /// </summary>
    public const string OptUserAgent = "userAgent";

    /// <summary>
    /// HTTP cookie container used by this instance.
    /// </summary>
    public CookieContainer CookieContainer
    {
        get
        {
            NotDisposed();
            return _cookieContainer;
        }
        set
        {
            NotDisposed();
            _cookieContainer = value;
        }
    }

    /// <summary>
    /// HTTP client used by this instance.
    /// </summary>
    public HttpClient HttpClient
    {
        get
        {
            NotDisposed();
            return _httpClient;
        }
        set
        {
            NotDisposed();
            _httpClient = value;
        }
    }

    /// <summary>
    /// Http message handler for <see cref="HttpClient"/>.
    /// </summary>
    public HttpMessageHandler HttpMessageHandler
    {
        get
        {
            NotDisposed();
            return _httpMessageHandler;
        }
        set
        {
            NotDisposed();
            _httpMessageHandler = value;
        }
    }

    /// <summary>
    /// Dummy default user agent string.
    /// </summary>
    public const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.85 Safari/537.36 Edg/90.0.818.46";

    #endregion

    #region Private fields

    private CookieContainer _cookieContainer = null!;

    private HttpClient _httpClient = null!;

    private HttpMessageHandler _httpMessageHandler = null!;

    private bool _disposed;

    #endregion

    #region Configuration

    /// <inheritdoc/>
    public override async Task ConfigureAsync(CancellationToken cancellationToken = default)
    {
        _cookieContainer = await CreateCookieContainerAsync(cancellationToken).ConfigureAwait(false);
        _httpMessageHandler = CreateHttpMessageHandler();
        _httpClient = CreateHttpClient(_httpMessageHandler);
        string userAgent = TryGetOption(OptUserAgent, out string? customUserAgent, SourceGenerationContext.Default.String) ? customUserAgent : DefaultUserAgent;
        _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
    }

    #endregion

    #region Http client configuration

    /// <summary>
    /// Creates a cookie container.
    /// </summary>
    /// <returns>A cookie container.</returns>
    /// <remarks>
    /// By default, this uses the <see cref="OptCookieBrowser"/>, <see cref="OptCookieBrowserDomain"/>, <see cref="OptCookieBrowserDomains"/>, <see cref="OptCookieBrowserProfile"/>, and <see cref="OptCookieFile"/> configuration options.
    /// </remarks>
    public virtual CookieContainer CreateCookieContainer()
    {
        CookieContainer cookies = new();
        TryLoadBrowserCookiesFromOption(cookies, OptCookieBrowser, OptCookieBrowserDomain, OptCookieBrowserDomains, OptCookieBrowserProfile);
        TryLoadCookieFileFromOption(cookies, OptCookieFile);
        return cookies;
    }

    /// <summary>
    /// Creates a cookie container.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning a cookie container.</returns>
    /// <remarks>
    /// By default, this uses the <see cref="OptCookieBrowser"/>, <see cref="OptCookieBrowserDomain"/>, <see cref="OptCookieBrowserDomains"/>, <see cref="OptCookieBrowserProfile"/>, and <see cref="OptCookieFile"/> configuration options.
    /// </remarks>
    public virtual async Task<CookieContainer> CreateCookieContainerAsync(CancellationToken cancellationToken = default)
    {
        CookieContainer cookies = new();
        await TryLoadBrowserCookiesFromOptionAsync(cookies, OptCookieBrowser, OptCookieBrowserDomain, OptCookieBrowserDomains, OptCookieBrowserProfile, cancellationToken).ConfigureAwait(false);
        await TryLoadCookieFileFromOptionAsync(cookies, OptCookieFile, cancellationToken).ConfigureAwait(false);
        return cookies;
    }

    /// <summary>
    /// Attempts to load cookies from a browser based on the specified option keys.
    /// </summary>
    /// <param name="cookies">Cookie container to populate.</param>
    /// <param name="optKeyBrowserName">Option key for browser name.</param>
    /// <param name="optKeyBrowserDomain">Option key for domains to filter by.</param>
    /// <param name="optKeyBrowserDomains">Option key for domains to filter by.</param>
    /// <param name="optKeyProfile">Optional option key for browser profile name.</param>
    /// <returns>True if necessary keys were found.</returns>
    public bool TryLoadBrowserCookiesFromOption(CookieContainer cookies, string optKeyBrowserName, string optKeyBrowserDomain, string optKeyBrowserDomains, string? optKeyProfile)
    {
        if (TryPrepareCookiesFromOption(optKeyBrowserName, optKeyBrowserDomain, optKeyBrowserDomains, optKeyProfile, out List<CookieFilter>? mappedDomains, out string? browserName, out string? profile))
        {
            CookieSource.LoadCookies(cookies, mappedDomains, browserName, profile, LogHandler);
        }
        return false;
    }

    /// <summary>
    /// Attempts to load cookies from a browser based on the specified option keys.
    /// </summary>
    /// <param name="cookies">Cookie container to populate.</param>
    /// <param name="optKeyBrowserName">Option key for browser name.</param>
    /// <param name="optKeyBrowserDomain">Option key for domains to filter by.</param>
    /// <param name="optKeyBrowserDomains">Option key for domains to filter by.</param>
    /// <param name="optKeyProfile">Optional option key for browser profile name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning if necessary keys were found.</returns>
    public async Task<bool> TryLoadBrowserCookiesFromOptionAsync(CookieContainer cookies, string optKeyBrowserName, string optKeyBrowserDomain, string optKeyBrowserDomains, string? optKeyProfile, CancellationToken cancellationToken = default)
    {
        if (TryPrepareCookiesFromOption(optKeyBrowserName, optKeyBrowserDomain, optKeyBrowserDomains, optKeyProfile, out List<CookieFilter>? mappedDomains, out string? browserName, out string? profile))
        {
            await CookieSource.LoadCookiesAsync(cookies, mappedDomains, browserName, profile, LogHandler, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        return false;
    }

    private bool TryPrepareCookiesFromOption(
        string optKeyBrowserName,
        string optKeyBrowserDomain,
        string optKeyBrowserDomains,
        string? optKeyProfile,
        [NotNullWhen(true)] out List<CookieFilter>? mappedDomains,
        [NotNullWhen(true)] out string? browserName,
        out string? profile)
    {
        if (TryGetOption(optKeyBrowserName, out browserName, SourceGenerationContext.Default.String))
        {
            mappedDomains = [];
            if (TryGetOption(optKeyBrowserDomain, out string? domain, SourceGenerationContext.Default.String))
            {
                mappedDomains.Add(new CookieFilter(domain));
            }
            if (TryGetOption(optKeyBrowserDomains, out string[]? domains, SourceGenerationContext.Default.StringArray))
            {
                mappedDomains.AddRange(domains.Select(v => new CookieFilter(v)));
            }
            profile = optKeyProfile != null && TryGetOption(optKeyProfile, out string? profileValue, SourceGenerationContext.Default.String) ? profileValue : null;
            return true;
        }
        mappedDomains = null;
        browserName = null;
        profile = null;
        return false;
    }

    /// <summary>
    /// Attempts to load cookies from a file based on the specified option key.
    /// </summary>
    /// <param name="cookies">Cookie container to populate.</param>
    /// <param name="optKey">Option key.</param>
    /// <returns>True if option key was found.</returns>
    public bool TryLoadCookieFileFromOption(CookieContainer cookies, string optKey)
    {
        if (TryGetOption(optKey, out string? cookieFile, SourceGenerationContext.Default.String))
        {
            using StreamReader f = File.OpenText(cookieFile);
            cookies.LoadCookieFile(f);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Attempts to load cookies from a file based on the specified option key.
    /// </summary>
    /// <param name="cookies">Cookie container to populate.</param>
    /// <param name="optKey">Option key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning if option key was found.</returns>
    public async Task<bool> TryLoadCookieFileFromOptionAsync(CookieContainer cookies, string optKey, CancellationToken cancellationToken = default)
    {
        if (TryGetOption(optKey, out string? cookieFile, SourceGenerationContext.Default.String))
        {
            using StreamReader f = File.OpenText(cookieFile);
            await cookies.LoadCookieFileAsync(f, cancellationToken).ConfigureAwait(false);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Creates an <see cref="HttpMessageHandler"/> instance.
    /// </summary>
    /// <returns>Cookie container.</returns>
    public virtual HttpMessageHandler CreateHttpMessageHandler() => new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All, CookieContainer = _cookieContainer };

    /// <summary>
    /// Creates an <see cref="System.Net.Http.HttpClient"/> instance configured to use the specified message handler.
    /// </summary>
    /// <param name="httpMessageHandler">HTTP message handler.</param>
    /// <returns>Configured HTTP client.</returns>
    public virtual HttpClient CreateHttpClient(HttpMessageHandler httpMessageHandler) => new(httpMessageHandler);

    /// <summary>
    /// Creates a <see cref="ClientWebSocket"/> using this instance's <see cref="CookieContainer"/>.
    /// </summary>
    /// <returns>New instance of <see cref="ClientWebSocket"/>.</returns>
    public ClientWebSocket CreateClientWebSocket()
    {
        ClientWebSocket cws = new();
        cws.Options.Cookies = CookieContainer;
        return cws;
    }

    #endregion

    #region IDisposable

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!_disposed)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                try
                {
                    _httpClient.Dispose();
                }
                catch
                {
                    // ignored
                }
                try
                {
                    _httpMessageHandler.Dispose();
                }
                catch
                {
                    // ignored
                }
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            _httpClient = null!;
            _httpMessageHandler = null!;
            _disposed = true;
        }
    }

    private void NotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    #endregion
}
