using System.Net.Http.Headers;

namespace Art.Http;

public partial class HttpArtifactTool
{
    #region Raw http requests

    /// <summary>
    /// Sends an HTTP HEAD request.
    /// </summary>
    /// <param name="requestUri">Request.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning response (status left unchecked).</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    public async Task<HttpResponseMessage> HeadAsync(
        string requestUri,
        HttpRequestConfig? httpRequestConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return await HttpClient.SendAsync(CreateRequestMessage, GenericCompletionOption, httpRequestConfig, cancellationToken).ConfigureAwait(false);

        HttpRequestMessage CreateRequestMessage()
        {
            HttpRequestMessage req = new(HttpMethod.Head, requestUri);
            ConfigureHttpRequest(req);
            return req;
        }
    }

    /// <summary>
    /// Sends an HTTP HEAD request.
    /// </summary>
    /// <param name="requestUri">Request.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning response (status left unchecked).</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    public async Task<HttpResponseMessage> HeadAsync(
        Uri requestUri,
        HttpRequestConfig? httpRequestConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return await HttpClient.SendAsync(CreateRequestMessage, GenericCompletionOption, httpRequestConfig, cancellationToken).ConfigureAwait(false);

        HttpRequestMessage CreateRequestMessage()
        {
            HttpRequestMessage req = new(HttpMethod.Head, requestUri);
            ConfigureHttpRequest(req);
            return req;
        }
    }

    /// <summary>
    /// Sends an HTTP GET request.
    /// </summary>
    /// <param name="requestUri">Request.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning response (status left unchecked).</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    public async Task<HttpResponseMessage> GetAsync(
        string requestUri,
        HttpRequestConfig? httpRequestConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return await HttpClient.SendAsync(CreateRequestMessage, GenericCompletionOption, httpRequestConfig, cancellationToken).ConfigureAwait(false);

        HttpRequestMessage CreateRequestMessage()
        {
            HttpRequestMessage req = new(HttpMethod.Get, requestUri);
            ConfigureHttpRequest(req);
            return req;
        }
    }

    /// <summary>
    /// Sends an HTTP GET request.
    /// </summary>
    /// <param name="requestUri">Request.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning response (status left unchecked).</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    public async Task<HttpResponseMessage> GetAsync(
        Uri requestUri,
        HttpRequestConfig? httpRequestConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return await HttpClient.SendAsync(CreateRequestMessage, GenericCompletionOption, httpRequestConfig, cancellationToken).ConfigureAwait(false);

        HttpRequestMessage CreateRequestMessage()
        {
            HttpRequestMessage req = new(HttpMethod.Get, requestUri);
            ConfigureHttpRequest(req);
            return req;
        }
    }

    /// <summary>
    /// Sends an HTTP request.
    /// </summary>
    /// <param name="requestMessageDelegate">A delegate that creates a new instance of <see cref="System.Net.Http.HttpRequestMessage"/>.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning response (status left unchecked).</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    public async Task<HttpResponseMessage> SendAsync(
        Func<HttpRequestMessage> requestMessageDelegate,
        HttpRequestConfig? httpRequestConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return await HttpClient.SendAsync(requestMessageDelegate, GenericCompletionOption, httpRequestConfig, cancellationToken).ConfigureAwait(false);
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
