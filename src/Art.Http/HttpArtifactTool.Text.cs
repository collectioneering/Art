﻿using System.Net.Http.Headers;

namespace Art.Http;

public partial class HttpArtifactTool
{
    #region Text

    /// <summary>
    /// Retrieves text using a uri.
    /// </summary>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="requestAction">Custom configuration callback for the <see cref="HttpRequestMessage"/> created.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning text.</returns>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task<string> GetHttpTextAsync(string requestUri, Action<HttpRequestMessage>? requestAction = null, CancellationToken cancellationToken = default)
    {
        NotDisposed();
        HttpRequestMessage req = new(HttpMethod.Get, requestUri);
        ConfigureTextRequest(req);
        requestAction?.Invoke(req);
        using HttpResponseMessage res = await HttpClient.SendAsync(req, JsonCompletionOption, cancellationToken).ConfigureAwait(false);
        ArtHttpResponseMessageException.EnsureSuccessStatusCode(res);
        return await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves text using a <see cref="Uri"/>.
    /// </summary>
    /// <param name="requestUri">Request URI.</param>
    /// <param name="requestAction">Custom configuration callback for the <see cref="HttpRequestMessage"/> created.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning text.</returns>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task<string> GetHttpTextAsync(Uri requestUri, Action<HttpRequestMessage>? requestAction = null, CancellationToken cancellationToken = default)
    {
        NotDisposed();
        HttpRequestMessage req = new(HttpMethod.Get, requestUri);
        ConfigureTextRequest(req);
        requestAction?.Invoke(req);
        using HttpResponseMessage res = await HttpClient.SendAsync(req, JsonCompletionOption, cancellationToken).ConfigureAwait(false);
        ArtHttpResponseMessageException.EnsureSuccessStatusCode(res);
        return await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves text using a <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <param name="requestMessage">Request to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning text.</returns>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task<string> RetrieveHttpTextAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
    {
        NotDisposed();
        using HttpResponseMessage res = await HttpClient.SendAsync(requestMessage, JsonCompletionOption, cancellationToken).ConfigureAwait(false);
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
