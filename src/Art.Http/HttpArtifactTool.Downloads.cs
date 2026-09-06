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
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="exportOptions">Options to use for export operation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public Task DownloadResourceAsync(
        string requestUri,
        Stream stream,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        ArtifactResourceExportOptions? exportOptions = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return DownloadResourceAsync(CreateRequest, stream, httpRequestMetaConfig, exportOptions, cancellationToken);

        HttpRequestEx CreateRequest()
        {
            HttpRequestMessage req = new(HttpMethod.Get, requestUri);
            ConfigureHttpRequest(req);
            return new HttpRequestEx(req, httpRequestConfigDelegate?.Invoke());
        }
    }

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestUri">Uri to download from.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="exportOptions">Options to use for export operation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task DownloadResourceAsync(
        string requestUri,
        ArtifactResourceKey key,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        ArtifactResourceExportOptions? exportOptions = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        await DownloadResourceInternalAsync(CreateRequest, httpRequestMetaConfig, key, exportOptions, cancellationToken).ConfigureAwait(false);
        return;

        HttpRequestEx CreateRequest()
        {
            HttpRequestMessage req = new(HttpMethod.Get, requestUri);
            ConfigureHttpRequest(req);
            return new HttpRequestEx(req, httpRequestConfigDelegate?.Invoke());
        }
    }

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestUri">Uri to download from.</param>
    /// <param name="file">Target filename.</param>
    /// <param name="key">Artifact key.</param>
    /// <param name="path">File path to prepend.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="exportOptions">Options to use for export operation.</param>
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
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        ArtifactResourceExportOptions? exportOptions = null,
        CancellationToken cancellationToken = default)
        => DownloadResourceAsync(requestUri, new ArtifactResourceKey(key, file, path), httpRequestConfigDelegate, httpRequestMetaConfig, exportOptions, cancellationToken);

    /// <summary>
    /// Gets a download stream for a resource.
    /// </summary>
    /// <param name="requestUri"><see cref="Uri"/> to download from.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning stream.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public Task<Stream> GetResourceDownloadStreamAsync(
        Uri requestUri,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return GetResourceDownloadStreamAsync(CreateRequest, httpRequestMetaConfig, cancellationToken);

        HttpRequestEx CreateRequest()
        {
            HttpRequestMessage req = new(HttpMethod.Get, requestUri);
            ConfigureHttpRequest(req);
            return new HttpRequestEx(req, httpRequestConfigDelegate?.Invoke());
        }
    }

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestUri"><see cref="Uri"/> to download from.</param>
    /// <param name="stream">Target stream.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="exportOptions">Options to use for export operation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public Task DownloadResourceAsync(
        Uri requestUri,
        Stream stream,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        ArtifactResourceExportOptions? exportOptions = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return DownloadResourceAsync(CreateRequest, stream, httpRequestMetaConfig, exportOptions, cancellationToken);

        HttpRequestEx CreateRequest()
        {
            HttpRequestMessage req = new(HttpMethod.Get, requestUri);
            ConfigureHttpRequest(req);
            return new HttpRequestEx(req, httpRequestConfigDelegate?.Invoke());
        }
    }

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestUri"><see cref="Uri"/> to download from.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="exportOptions">Options to use for export operation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task DownloadResourceAsync(
        Uri requestUri,
        ArtifactResourceKey key,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        ArtifactResourceExportOptions? exportOptions = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        await DownloadResourceInternalAsync(CreateRequest, httpRequestMetaConfig, key, exportOptions, cancellationToken).ConfigureAwait(false);
        return;

        HttpRequestEx CreateRequest()
        {
            HttpRequestMessage req = new(HttpMethod.Get, requestUri);
            ConfigureHttpRequest(req);
            return new HttpRequestEx(req, httpRequestConfigDelegate?.Invoke());
        }
    }

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestUri"><see cref="Uri"/> to download from.</param>
    /// <param name="file">Target filename.</param>
    /// <param name="key">Artifact key.</param>
    /// <param name="path">File path to prepend.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="exportOptions">Options to use for export operation.</param>
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
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        ArtifactResourceExportOptions? exportOptions = null,
        CancellationToken cancellationToken = default)
        => DownloadResourceAsync(requestUri, new ArtifactResourceKey(key, file, path), httpRequestConfigDelegate, httpRequestMetaConfig, exportOptions, cancellationToken);

    /// <summary>
    /// Gets a download stream for a resource.
    /// </summary>
    /// <param name="requestDelegate">A delegate that creates a new instance of <see cref="HttpRequestEx"/>.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning stream.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task<Stream> GetResourceDownloadStreamAsync(
        Func<HttpRequestEx> requestDelegate,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        // M3U behaviour depends on members always using this instance's HttpClient.
        HttpResponseMessage res = await HttpClient.SendAsync(requestDelegate, DownloadCompletionOption, httpRequestMetaConfig, cancellationToken).ConfigureAwait(false);
        ArtHttpResponseMessageException.EnsureSuccessStatusCode(res);
        var stream = await res.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return new DelegatingStreamWithDisposableContext(stream, res);
    }

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestDelegate">A delegate that creates a new instance of <see cref="HttpRequestEx"/>.</param>
    /// <param name="stream">Target stream.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="exportOptions">Options to use for export operation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task DownloadResourceAsync(
        Func<HttpRequestEx> requestDelegate,
        Stream stream,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        ArtifactResourceExportOptions? exportOptions = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        // M3U behaviour depends on members always using this instance's HttpClient.
        using HttpResponseMessage res = await HttpClient.SendAsync(requestDelegate, DownloadCompletionOption, httpRequestMetaConfig, cancellationToken).ConfigureAwait(false);
        ArtHttpResponseMessageException.EnsureSuccessStatusCode(res);
        await CopyStreamAsync(res, stream, exportOptions, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestDelegate">A delegate that creates a new instance of <see cref="HttpRequestEx"/>.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="exportOptions">Options to use for export operation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task DownloadResourceAsync(
        Func<HttpRequestEx> requestDelegate,
        ArtifactResourceKey key,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        ArtifactResourceExportOptions? exportOptions = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        await DownloadResourceInternalAsync(requestDelegate, httpRequestMetaConfig, key, exportOptions, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Downloads a resource.
    /// </summary>
    /// <param name="requestDelegate">A delegate that creates a new instance of <see cref="HttpRequestEx"/>.</param>
    /// <param name="file">Target filename.</param>
    /// <param name="key">Artifact key.</param>
    /// <param name="path">File path to prepend.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="exportOptions">Options to use for export operation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public Task DownloadResourceAsync(
        Func<HttpRequestEx> requestDelegate,
        string file,
        ArtifactKey key,
        string path = "",
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        ArtifactResourceExportOptions? exportOptions = null,
        CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return DownloadResourceInternalAsync(requestDelegate, httpRequestMetaConfig, new ArtifactResourceKey(key, file, path), exportOptions, cancellationToken);
    }

    /// <summary>
    /// Default <see cref="HttpCompletionOption"/> for download requests.
    /// </summary>
    public virtual HttpCompletionOption DownloadCompletionOption => HttpCompletionOption.ResponseHeadersRead;

    private async Task DownloadResourceInternalAsync(
        Func<HttpRequestEx> requestDelegate,
        HttpRequestMetaConfig? httpRequestMetaConfig,
        ArtifactResourceKey key,
        ArtifactResourceExportOptions? exportOptions,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage res = await HttpClient.SendAsync(requestDelegate, DownloadCompletionOption, httpRequestMetaConfig, cancellationToken).ConfigureAwait(false);
        ArtHttpResponseMessageException.EnsureSuccessStatusCode(res);
        await StreamDownloadAsync(res, key, exportOptions, cancellationToken).ConfigureAwait(false);
    }

    private async Task StreamDownloadAsync(HttpResponseMessage response, ArtifactResourceKey key, ArtifactResourceExportOptions? exportOptions, CancellationToken cancellationToken)
    {
        OutputStreamOptions options = OutputStreamOptions.Default;
        if (response.Content.Headers.ContentLength is { } contentLength)
        {
            options = options with { PreallocationSize = Math.Clamp(contentLength, 0, QueryBaseArtifactResourceInfo.MaxStreamDownloadPreallocationSize) };
        }
        await using ICommittable<Stream> stream = await CreateOutputStreamAsync(key, options, cancellationToken).ConfigureAwait(false);
        await CopyStreamAsync(response, stream.Value, exportOptions, cancellationToken).ConfigureAwait(false);
        stream.ShouldCommit = true;
    }

    private async Task CopyStreamAsync(HttpResponseMessage sourceMessage, Stream targetStream, ArtifactResourceExportOptions? exportOptions, CancellationToken cancellationToken)
    {
        if (LogHandler is { } logHandler
            && sourceMessage.Content.Headers.ContentLength is { } contentLength
            && sourceMessage.RequestMessage is { RequestUri: { } requestUri })
        {
            var logPreferences = logHandler.LogPreferences;
            string sizeString = DataSizes.GetSizeString(contentLength, logPreferences.DataUnits, logPreferences.DataUnitFormat);
            string desc = $"{(requestUri.Segments is { Length: > 0 } segments ? segments[^1] : "Incoming file")} ({sizeString})";
            IOperationProgressContext? context;
            if (exportOptions?.IsConcurrent ?? false
                    ? logHandler.TryGetConcurrentOperationProgressContext(desc, s_downloadOperation, out context)
                    : logHandler.TryGetOperationProgressContext(desc, s_downloadOperation, out context))
            {
                using var ctx = context;
                var stream = await sourceMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                await ReportUtility.CopyToWithReportAsync(stream, targetStream, context, null, contentLength, cancellationToken).ConfigureAwait(false);
                ctx.MarkSafe();
            }
        }
        // fall back to copy without logging
        var sourceStream = await sourceMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await sourceStream.CopyToAsync(targetStream, cancellationToken).ConfigureAwait(false);
    }

    #endregion
}
