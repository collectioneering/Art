using System.Net.Http.Headers;

namespace Art.Http;

public partial class HttpArtifactTool
{
    #region Raw http requests

    /// <summary>
    /// Sends an HTTP HEAD request.
    /// </summary>
    /// <param name="requestUri">Request.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning response (status left unchecked).</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    public async Task<HttpResponseMessage> HeadAsync(
        string requestUri,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return await HttpClient.SendAsync(CreateRequest, GenericCompletionOption, httpRequestMetaConfig, cancellationToken).ConfigureAwait(false);

        HttpRequestEx CreateRequest()
        {
            HttpRequestMessage req = new(HttpMethod.Head, requestUri);
            ConfigureHttpRequest(req);
            return new HttpRequestEx(req, httpRequestConfigDelegate?.Invoke());
        }
    }

    /// <summary>
    /// Sends an HTTP HEAD request.
    /// </summary>
    /// <param name="requestUri">Request.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning response (status left unchecked).</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    public async Task<HttpResponseMessage> HeadAsync(
        Uri requestUri,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return await HttpClient.SendAsync(CreateRequest, GenericCompletionOption, httpRequestMetaConfig, cancellationToken).ConfigureAwait(false);

        HttpRequestEx CreateRequest()
        {
            HttpRequestMessage req = new(HttpMethod.Head, requestUri);
            ConfigureHttpRequest(req);
            return new HttpRequestEx(req, httpRequestConfigDelegate?.Invoke());
        }
    }

    /// <summary>
    /// Sends an HTTP GET request.
    /// </summary>
    /// <param name="requestUri">Request.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning response (status left unchecked).</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    public async Task<HttpResponseMessage> GetAsync(
        string requestUri,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return await HttpClient.SendAsync(CreateRequest, GenericCompletionOption, httpRequestMetaConfig, cancellationToken).ConfigureAwait(false);

        HttpRequestEx CreateRequest()
        {
            HttpRequestMessage req = new(HttpMethod.Get, requestUri);
            ConfigureHttpRequest(req);
            return new HttpRequestEx(req, httpRequestConfigDelegate?.Invoke());
        }
    }

    /// <summary>
    /// Sends an HTTP GET request.
    /// </summary>
    /// <param name="requestUri">Request.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning response (status left unchecked).</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    public async Task<HttpResponseMessage> GetAsync(
        Uri requestUri,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return await HttpClient.SendAsync(CreateRequest, GenericCompletionOption, httpRequestMetaConfig, cancellationToken).ConfigureAwait(false);

        HttpRequestEx CreateRequest()
        {
            HttpRequestMessage req = new(HttpMethod.Get, requestUri);
            ConfigureHttpRequest(req);
            return new HttpRequestEx(req, httpRequestConfigDelegate?.Invoke());
        }
    }

    /// <summary>
    /// Sends an HTTP request.
    /// </summary>
    /// <param name="requestDelegate">A delegate that creates a new instance of <see cref="HttpRequestEx"/>.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning response (status left unchecked).</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    public async Task<HttpResponseMessage> SendAsync(
        Func<HttpRequestEx> requestDelegate,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return await HttpClient.SendAsync(requestDelegate, GenericCompletionOption, httpRequestMetaConfig, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Configures an HTTP request.
    /// </summary>
    /// <param name="request">Request to configure.</param>
    public virtual void ConfigureHttpRequest(HttpRequestMessage request)
    {
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
    }

    /// <summary>
    /// Default <see cref="HttpCompletionOption"/> for generic requests.
    /// </summary>
    public virtual HttpCompletionOption GenericCompletionOption => HttpCompletionOption.ResponseContentRead;

    #endregion
}
