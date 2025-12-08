using System.Buffers;
using Art.Common;
using Art.Http.Resources;

namespace Art.Http;

public partial class HttpArtifactTool
{
    #region Direct downloads

    private static readonly Guid s_downloadOperation = Guid.ParseExact("c6d42b18f0ae452385f180aa74e9ef29", "N");

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestUri">Uri to download from.</param>
    /// <param name="stream">Target stream.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="useLogger">If true, use logger for progress details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public Task DownloadResourceAsync(
        string requestUri,
        Stream stream,
        HttpRequestConfig? httpRequestConfig = null,
        bool useLogger = true,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        HttpRequestMessage req = new(HttpMethod.Get, requestUri);
        ConfigureHttpRequest(req);
        return DownloadResourceAsync(req, stream, httpRequestConfig, useLogger, cancellationToken);
    }

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestUri">Uri to download from.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="useLogger">If true, use logger for progress details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task DownloadResourceAsync(
        string requestUri,
        ArtifactResourceKey key,
        HttpRequestConfig? httpRequestConfig = null,
        bool useLogger = true,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        HttpRequestMessage req = new(HttpMethod.Get, requestUri);
        ConfigureHttpRequest(req);
        await DownloadResourceInternalAsync(req, httpRequestConfig, key, useLogger, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestUri">Uri to download from.</param>
    /// <param name="file">Target filename.</param>
    /// <param name="key">Artifact key.</param>
    /// <param name="path">File path to prepend.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="useLogger">If true, use logger for progress details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public Task DownloadResourceAsync(
        string requestUri,
        string file,
        ArtifactKey key,
        string path = "",
        HttpRequestConfig? httpRequestConfig = null,
        bool useLogger = true,
        CancellationToken cancellationToken = default)
        => DownloadResourceAsync(requestUri, new ArtifactResourceKey(key, file, path), httpRequestConfig, useLogger, cancellationToken);

    /// <summary>
    /// Gets a download stream for a resource.
    /// </summary>
    /// <param name="requestUri"><see cref="Uri"/> to download from.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning stream.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public Task<Stream> GetResourceDownloadStreamAsync(
        Uri requestUri,
        HttpRequestConfig? httpRequestConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        HttpRequestMessage req = new(HttpMethod.Get, requestUri);
        ConfigureHttpRequest(req);
        return GetResourceDownloadStreamAsync(req, httpRequestConfig, cancellationToken);
    }

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestUri"><see cref="Uri"/> to download from.</param>
    /// <param name="stream">Target stream.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="useLogger">If true, use logger for progress details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public Task DownloadResourceAsync(
        Uri requestUri,
        Stream stream,
        HttpRequestConfig? httpRequestConfig = null,
        bool useLogger = true,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        HttpRequestMessage req = new(HttpMethod.Get, requestUri);
        ConfigureHttpRequest(req);
        return DownloadResourceAsync(req, stream, httpRequestConfig, useLogger, cancellationToken);
    }

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestUri"><see cref="Uri"/> to download from.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="useLogger">If true, use logger for progress details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task DownloadResourceAsync(
        Uri requestUri,
        ArtifactResourceKey key,
        HttpRequestConfig? httpRequestConfig = null,
        bool useLogger = true,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        HttpRequestMessage req = new(HttpMethod.Get, requestUri);
        ConfigureHttpRequest(req);
        await DownloadResourceInternalAsync(req, httpRequestConfig, key, useLogger, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestUri"><see cref="Uri"/> to download from.</param>
    /// <param name="file">Target filename.</param>
    /// <param name="key">Artifact key.</param>
    /// <param name="path">File path to prepend.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="useLogger">If true, use logger for progress details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public Task DownloadResourceAsync(
        Uri requestUri,
        string file,
        ArtifactKey key,
        string path = "",
        HttpRequestConfig? httpRequestConfig = null,
        bool useLogger = true,
        CancellationToken cancellationToken = default)
        => DownloadResourceAsync(requestUri, new ArtifactResourceKey(key, file, path), httpRequestConfig, useLogger, cancellationToken);

    /// <summary>
    /// Gets a download stream for a resource.
    /// </summary>
    /// <param name="requestMessage">Request to send.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning stream.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task<Stream> GetResourceDownloadStreamAsync(
        HttpRequestMessage requestMessage,
        HttpRequestConfig? httpRequestConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        // M3U behaviour depends on members always using this instance's HttpClient.
        HttpResponseMessage res = await HttpClient.SendAsync(requestMessage, DownloadCompletionOption, httpRequestConfig, cancellationToken).ConfigureAwait(false);
        ArtHttpResponseMessageException.EnsureSuccessStatusCode(res);
        var stream = await res.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return new DelegatingStreamWithDisposableContext(stream, res);
    }

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestMessage">Request to send.</param>
    /// <param name="stream">Target stream.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="useLogger">If true, use logger for progress details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task DownloadResourceAsync(
        HttpRequestMessage requestMessage,
        Stream stream,
        HttpRequestConfig? httpRequestConfig = null,
        bool useLogger = true,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        // M3U behaviour depends on members always using this instance's HttpClient.
        using HttpResponseMessage res = await HttpClient.SendAsync(requestMessage, DownloadCompletionOption, httpRequestConfig, cancellationToken).ConfigureAwait(false);
        ArtHttpResponseMessageException.EnsureSuccessStatusCode(res);
        await CopyStreamAsync(res, stream, useLogger, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestMessage">Request to send.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="useLogger">If true, use logger for progress details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task DownloadResourceAsync(
        HttpRequestMessage requestMessage,
        ArtifactResourceKey key,
        HttpRequestConfig? httpRequestConfig = null,
        bool useLogger = true,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        await DownloadResourceInternalAsync(requestMessage, httpRequestConfig, key, useLogger, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestMessage">Request to send.</param>
    /// <param name="file">Target filename.</param>
    /// <param name="key">Artifact key.</param>
    /// <param name="path">File path to prepend.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="useLogger">If true, use logger for progress details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public Task DownloadResourceAsync(
        HttpRequestMessage requestMessage,
        string file,
        ArtifactKey key,
        string path = "",
        HttpRequestConfig? httpRequestConfig = null,
        bool useLogger = true,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return DownloadResourceInternalAsync(requestMessage, httpRequestConfig, new ArtifactResourceKey(key, file, path), useLogger, cancellationToken);
    }

    /// <summary>
    /// Default <see cref="HttpCompletionOption"/> for download requests.
    /// </summary>
    public virtual HttpCompletionOption DownloadCompletionOption => HttpCompletionOption.ResponseHeadersRead;

    private async Task DownloadResourceInternalAsync(
        HttpRequestMessage requestMessage,
        HttpRequestConfig? httpRequestConfig,
        ArtifactResourceKey key,
        bool useLogger,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage res = await HttpClient.SendAsync(requestMessage, DownloadCompletionOption, httpRequestConfig, cancellationToken).ConfigureAwait(false);
        ArtHttpResponseMessageException.EnsureSuccessStatusCode(res);
        await StreamDownloadAsync(res, key, useLogger, cancellationToken).ConfigureAwait(false);
    }

    private async Task StreamDownloadAsync(HttpResponseMessage response, ArtifactResourceKey key, bool useLogger, CancellationToken cancellationToken)
    {
        OutputStreamOptions options = OutputStreamOptions.Default;
        if (response.Content.Headers.ContentLength is { } contentLength) options = options with { PreallocationSize = Math.Clamp(contentLength, 0, QueryBaseArtifactResourceInfo.MaxStreamDownloadPreallocationSize) };
        await using CommittableStream stream = await CreateOutputStreamAsync(key, options, cancellationToken).ConfigureAwait(false);
        await CopyStreamAsync(response, stream, useLogger, cancellationToken).ConfigureAwait(false);
        stream.ShouldCommit = true;
    }

    private async Task CopyStreamAsync(HttpResponseMessage response, Stream stream, bool useLogger, CancellationToken cancellationToken)
    {
        if (useLogger)
        {
            await CopyToWithLoggerAsync(response, stream, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await sourceStream.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task CopyToWithLoggerAsync(HttpResponseMessage sourceMessage, Stream targetStream, CancellationToken cancellationToken)
    {
        if (LogHandler is { } logHandler
            && sourceMessage.Content.Headers.ContentLength is { } contentLength
            && sourceMessage.RequestMessage is { } requestMessage
            && requestMessage.RequestUri is { } requestUri
            && requestUri.Segments is { Length: > 0 } segments)
        {
            string desc = $"{segments[^1]} ({DataSizes.GetSizeString(contentLength)})";
            if (logHandler.TryGetOperationProgressContext(desc, s_downloadOperation, out var context))
            {
                using var ctx = context;
                const int bufferSize = 8192;
                byte[] buf = ArrayPool<byte>.Shared.Rent(bufferSize);
                try
                {
                    var stream = await sourceMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                    var mem = buf.AsMemory(0, bufferSize);
                    long total = 0;
                    while (true)
                    {
                        int read;
                        if ((read = await stream.ReadAsync(mem, cancellationToken).ConfigureAwait(false)) != 0)
                        {
                            await targetStream.WriteAsync(mem[..read], cancellationToken).ConfigureAwait(false);
                            total += read;
                            ctx.Report(Math.Clamp((float)((double)total / contentLength), 0.0f, 1.0f));
                        }
                        else
                        {
                            ctx.Report(1.0f);
                            break;
                        }
                    }
                    return;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buf);
                }
            }
        }
        await sourceMessage.Content.CopyToAsync(targetStream, cancellationToken).ConfigureAwait(false);
    }

    #endregion
}
