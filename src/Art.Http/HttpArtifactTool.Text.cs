using System.Net.Http.Headers;

namespace Art.Http;

public partial class HttpArtifactTool
{
    #region Text

    /// <summary>
    /// Retrieves text using a uri.
    /// </summary>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning text.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task<string> GetHttpTextAsync(
        string requestUri,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        using HttpResponseMessage res = await HttpClient.SendAsync(CreateRequest, TextCompletionOption, httpRequestMetaConfig, cancellationToken).ConfigureAwait(false);
        ArtHttpResponseMessageException.EnsureSuccessStatusCode(res);
        return await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        HttpRequestEx CreateRequest()
        {
            HttpRequestMessage req = new(HttpMethod.Get, requestUri);
            ConfigureTextRequest(req);
            return new HttpRequestEx(req, httpRequestConfigDelegate?.Invoke());
        }
    }

    /// <summary>
    /// Retrieves text using a <see cref="Uri"/>.
    /// </summary>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning text.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task<string> GetHttpTextAsync(
        Uri requestUri,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        using HttpResponseMessage res = await HttpClient.SendAsync(CreateRequest, TextCompletionOption, httpRequestMetaConfig, cancellationToken).ConfigureAwait(false);
        ArtHttpResponseMessageException.EnsureSuccessStatusCode(res);
        return await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        HttpRequestEx CreateRequest()
        {
            HttpRequestMessage req = new(HttpMethod.Get, requestUri);
            ConfigureTextRequest(req);
            return new HttpRequestEx(req, httpRequestConfigDelegate?.Invoke());
        }
    }

    /// <summary>
    /// Retrieves text using a <see cref="HttpRequestEx"/>.
    /// </summary>
    /// <param name="requestDelegate">A delegate that creates a new instance of <see cref="HttpRequestEx"/>.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning text.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task<string> RetrieveHttpTextAsync(
        Func<HttpRequestEx> requestDelegate,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        using HttpResponseMessage res = await HttpClient.SendAsync(requestDelegate, TextCompletionOption, httpRequestMetaConfig, cancellationToken).ConfigureAwait(false);
        ArtHttpResponseMessageException.EnsureSuccessStatusCode(res);
        return await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Configures a text request.
    /// </summary>
    /// <param name="request">Request to configure.</param>
    public virtual void ConfigureTextRequest(HttpRequestMessage request)
    {
        ConfigureHttpRequest(request);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US"));
        request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en", 0.9));
    }

    /// <summary>
    /// Default <see cref="HttpCompletionOption"/> for text requests.
    /// </summary>
    public virtual HttpCompletionOption TextCompletionOption => HttpCompletionOption.ResponseContentRead;

    #endregion
}
