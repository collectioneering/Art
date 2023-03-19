using System.Globalization;
using System.Text;
using Art.Common.Resources;
using Art.Http;
using Art.Http.Resources;

namespace Art.M3U;

/// <summary>
/// Represents a downloader context.
/// </summary>
public class M3UDownloaderContext
{
    /// <summary>
    /// Main stream URI.
    /// </summary>
    public Uri MainUri { get; private set; }

    /// <summary>
    /// Stream info.
    /// </summary>
    public M3UFile StreamInfo { get; }

    /// <summary>
    /// Configuration.
    /// </summary>
    public M3UDownloaderConfig Config { get; private set; }

    /// <summary>
    /// Current tool.
    /// </summary>
    public HttpArtifactTool Tool { get; }

    private M3UDownloaderContext(HttpArtifactTool tool, M3UDownloaderConfig config, Uri mainUri, M3UFile streamInfo)
    {
        Tool = tool;
        Config = config;
        MainUri = mainUri;
        StreamInfo = streamInfo;
        ValidateConfig();
    }

    private static async Task<Uri> SelectSubStreamAsync(HttpArtifactTool tool, M3UDownloaderConfig config, CancellationToken cancellationToken = default)
    {
        StreamInfo highestStream = await SelectStreamAsync(tool, config, cancellationToken).ConfigureAwait(false);
        long bw = highestStream.AverageBandwidth == 0 ? highestStream.Bandwidth : highestStream.AverageBandwidth;
        tool.LogInformation($"Selected {highestStream.Path} ({bw} b/s, {highestStream.ResolutionWidth}x{highestStream.ResolutionHeight})");
        return new Uri(new Uri(config.URL), highestStream.Path);
    }

    /// <summary>
    /// Sets configuration.
    /// </summary>
    /// <param name="config">Configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task SetConfigAsync(M3UDownloaderConfig config, CancellationToken cancellationToken = default)
    {
        Uri mainUri = await SelectSubStreamAsync(Tool, config, cancellationToken).ConfigureAwait(false);
        MainUri = mainUri;
        Config = config;
        ValidateConfig();
    }

    private void ValidateConfig()
    {
        if (Config.MaxFails < 0)
        {
            Config = Config with { MaxFails = 0 };
        }
        if (Config.RequestTimeoutRetries < 0)
        {
            Config = Config with { RequestTimeoutRetries = 0 };
        }
        if (Config.RequestTimeout <= 0)
        {
            Config = Config with { RequestTimeout = 1 };
        }
    }

    /// <summary>
    /// Creates a new downloader context.
    /// </summary>
    /// <param name="tool">Tool.</param>
    /// <param name="config">Configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning downloader context.</returns>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public static async Task<M3UDownloaderContext> OpenContextAsync(HttpArtifactTool tool, M3UDownloaderConfig config, CancellationToken cancellationToken = default)
    {
        string? referrer = config.Referrer;
        tool.LogInformation("Getting stream info...");
        Uri mainUri = await SelectSubStreamAsync(tool, config, cancellationToken).ConfigureAwait(false);
        tool.LogInformation("Getting sub stream info...");
        M3UFile m3;
        M3UEncryptionInfo? ei;
        using (var res = await tool.GetAsync(mainUri, new HttpRequestConfig(Referrer: referrer), cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            ArtHttpResponseMessageException.EnsureSuccessStatusCode(res);
            m3 = M3UReader.Parse(await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
            ei = m3.EncryptionInfo;
        }
        if (ei != null) tool.LogInformation($"Encrypted with {ei.Method}");
        if (ei?.Iv is not null) tool.LogInformation($"IV {Convert.ToHexString(ei.Iv)}");
        if (ei is { Uri: { } })
        {
            tool.LogInformation("Downloading enc key...");
            using var res = await tool.GetAsync(new Uri(mainUri, ei.Uri), new HttpRequestConfig(Referrer: referrer), cancellationToken: cancellationToken).ConfigureAwait(false);
            ArtHttpResponseMessageException.EnsureSuccessStatusCode(res);
            ei.Key = await res.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            tool.LogInformation($"KEY {Convert.ToHexString(ei.Key)}");
        }
        return new M3UDownloaderContext(tool, config, mainUri, m3);
    }

    /// <summary>
    /// Writes encryption information to keyformat.txt, method.txt, key.bin, iv.bin according to <see cref="Tool"/>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task WriteKeyMaterialAsync(CancellationToken cancellationToken = default)
    {
        if (StreamInfo.EncryptionInfo is not { } ei) return;
        await WriteAncillaryFileAsync("keyformat.txt", Encoding.UTF8.GetBytes(ei.KeyFormat), cancellationToken).ConfigureAwait(false);
        await WriteAncillaryFileAsync("method.txt", Encoding.UTF8.GetBytes(ei.Method), cancellationToken).ConfigureAwait(false);
        if (ei.Key is not null) await WriteAncillaryFileAsync("key.bin", ei.Key, cancellationToken).ConfigureAwait(false);
        if (ei.Iv is not null) await WriteAncillaryFileAsync("iv.bin", ei.Iv, cancellationToken).ConfigureAwait(false);
    }

    private async Task WriteAncillaryFileAsync(string file, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        ArtifactResourceKey keyFormatArk = new(Config.ArtifactKey, file, Config.ArtifactKey.Id);
        await using CommittableStream keyFormatStream = await Tool.DataManager.CreateOutputStreamAsync(keyFormatArk, OutputStreamOptions.Default, cancellationToken).ConfigureAwait(false);
        await keyFormatStream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
        keyFormatStream.ShouldCommit = true;
    }

    /// <summary>
    /// Creates a top-down (ID-based) downloader with default name translation function.
    /// </summary>
    /// <param name="top">Top ID.</param>
    /// <returns>Downloader.</returns>
    public M3UDownloaderContextTopDownSaver CreateTopDownSaver(long top) => new(this, top);

    /// <summary>
    /// Creates a top-down (ID-based) downloader.
    /// </summary>
    /// <param name="top">Top ID.</param>
    /// <param name="idFormatter">ID string formatter.</param>
    /// <returns>Downloader.</returns>
    public M3UDownloaderContextTopDownSaver CreateTopDownSaver(long top, Func<long, string> idFormatter) => new(this, top, idFormatter);

    /// <summary>
    /// Creates a top-down (ID-based) downloader.
    /// </summary>
    /// <param name="top">Top ID.</param>
    /// <param name="nameTransform">Name transformation function (for received IDs).</param>
    /// <returns>Downloader.</returns>
    public M3UDownloaderContextTopDownSaver CreateTopDownSaver(long top, Func<string, long, string> nameTransform) => new(this, top, nameTransform);

    /// <summary>
    /// Creates a basic downloader.
    /// </summary>
    /// <param name="oneOff">If true, only request target once.</param>
    /// <param name="timeout">Timeout when waiting for new entries.</param>
    /// <param name="extraOperation">Extra operation to invoke during down time.</param>
    /// <returns>Downloader.</returns>
    public M3UDownloaderContextStandardSaver CreateStandardSaver(bool oneOff, TimeSpan timeout, IExtraSaverOperation? extraOperation = null) => new(this, oneOff, timeout, extraOperation);

    /// <summary>
    /// Creates a basic stream output downloader.
    /// </summary>
    /// <param name="oneOff">If true, only request target once.</param>
    /// <param name="timeout">Timeout when waiting for new entries.</param>
    /// <returns>Downloader.</returns>
    public M3UDownloaderContextStreamOutputSaver CreateStreamOutputSaver(bool oneOff, TimeSpan timeout) => new(this, oneOff, timeout);

    /// <summary>
    /// Downloads a segment with an associated media sequence number.
    /// </summary>
    /// <param name="uri">URI to download.</param>
    /// <param name="file">Optional specific file to use (defaults to <see cref="StreamInfo"/>).</param>
    /// <param name="mediaSequenceNumber">Media sequence number, if available.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    /// <exception cref="InvalidDataException">Thrown for unexpected encryption algorithm when set to decrypt.</exception>
    /// <remarks>
    /// <paramref name="mediaSequenceNumber"/> is meant to support scenarios for decrypting media segments without an explicit IV,
    /// as the media sequence number determines the IV instead.
    /// </remarks>
    public Task DownloadSegmentAsync(Uri uri, M3UFile? file, long? mediaSequenceNumber = null, CancellationToken cancellationToken = default)
    {
        return DownloadSegmentInternalAsync(uri, file ?? StreamInfo, mediaSequenceNumber, cancellationToken);
    }

    /// <summary>
    /// Outputs a segment to a target stream.
    /// </summary>
    /// <param name="targetStream">Target stream</param>
    /// <param name="uri">URI to download.</param>
    /// <param name="file">Optional specific file to use (defaults to <see cref="StreamInfo"/>).</param>
    /// <param name="mediaSequenceNumber">Media sequence number, if available.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    /// <exception cref="InvalidDataException">Thrown for unexpected encryption algorithm when set to decrypt.</exception>
    /// <remarks>
    /// <paramref name="mediaSequenceNumber"/> is meant to support scenarios for decrypting media segments without an explicit IV,
    /// as the media sequence number determines the IV instead.
    /// </remarks>
    public Task StreamSegmentAsync(Stream targetStream, Uri uri, M3UFile? file, long? mediaSequenceNumber = null, CancellationToken cancellationToken = default)
    {
        return StreamSegmentInternalAsync(targetStream, uri, file ?? StreamInfo, mediaSequenceNumber, cancellationToken);
    }

    private async Task DownloadSegmentInternalAsync(Uri uri, M3UFile file, long? mediaSequenceNumber, CancellationToken cancellationToken)
    {
        string fn = GetFileName(uri);
        ArtifactResourceKey ark = new(Config.ArtifactKey, fn, Config.ArtifactKey.Id);
        if (await Tool.RegistrationManager.TryGetResourceAsync(ark, cancellationToken).ConfigureAwait(false) != null)
        {
            if (Config.SkipExistingSegments) return;
            await Tool.RegistrationManager.RemoveResourceAsync(ark, cancellationToken).ConfigureAwait(false);
        }
        // don't need to always write msn (only necessary for later dec) but do it anyway...
        if (mediaSequenceNumber is { } msn) await WriteAncillaryFileAsync($"{fn}.msn.txt", Encoding.UTF8.GetBytes(msn.ToString(CultureInfo.InvariantCulture)), cancellationToken).ConfigureAwait(false);
        ArtifactResourceInfo ari = GetResourceInternal(ark, uri, file, mediaSequenceNumber);
        await using (CommittableStream oStream = await Tool.DataManager.CreateOutputStreamAsync(ari.Key, OutputStreamOptions.Default, cancellationToken).ConfigureAwait(false))
        {
            await StreamSegmentInternalAsync(ari, oStream, cancellationToken).ConfigureAwait(false);
            oStream.ShouldCommit = true;
        }
        await Tool.RegistrationManager.AddResourceAsync(ari, cancellationToken).ConfigureAwait(false);
    }

    private async Task StreamSegmentInternalAsync(Stream targetStream, Uri uri, M3UFile file, long? mediaSequenceNumber, CancellationToken cancellationToken)
    {
        string fn = GetFileName(uri);
        ArtifactResourceKey ark = new(Config.ArtifactKey, fn, Config.ArtifactKey.Id);
        ArtifactResourceInfo ari = GetResourceInternal(ark, uri, file, mediaSequenceNumber);
        var ms = new MemoryStream();
        await StreamSegmentInternalAsync(ari, ms, cancellationToken).ConfigureAwait(false);
        ms.Position = 0;
        await ms.CopyToAsync(targetStream, cancellationToken).ConfigureAwait(false);
    }

    private ArtifactResourceInfo GetResourceInternal(ArtifactResourceKey artifactResourceKey, Uri uri, M3UFile file, long? mediaSequenceNumber = null)
    {
        // Use ResponseHeadersRead to make timeout only count up to headers
        var httpRequestConfig = new HttpRequestConfig(Referrer: Config.Referrer, HttpCompletionOption: HttpCompletionOption.ResponseHeadersRead, Timeout: TimeSpan.FromMilliseconds(Config.RequestTimeout));
        ArtifactResourceInfo ari = new UriArtifactResourceInfo(Tool, uri, httpRequestConfig, artifactResourceKey);
        if (file.EncryptionInfo is not { Encrypted: true } ei)
        {
            return ari;
        }
        if (Config.Decrypt)
        {
            ari = new EncryptedArtifactResourceInfo(ei.ToEncryptionInfo(mediaSequenceNumber), ari);
        }
        return ari;
    }

    private async Task StreamSegmentInternalAsync(ArtifactResourceInfo artifactResourceInfo, Stream targetStream, CancellationToken cancellationToken)
    {
        int retries = 0;
        long? initPosition = targetStream.CanSeek ? targetStream.Position : null;
        while (true)
        {
            try
            {
                await artifactResourceInfo.ExportStreamAsync(targetStream, cancellationToken).ConfigureAwait(false);
                break;
            }
            catch (TaskCanceledException e)
            {
                // .NET 5 throws TaskCanceledException with TimeoutException inner exception
                // Added semantics, our timeout from HttpRequestConfig does the same
                if (e.InnerException is not TimeoutException)
                {
                    throw;
                }
                if (retries >= Config.RequestTimeoutRetries)
                {
                    throw;
                }
                retries++;
                Tool.LogWarning($"timeout, retrying {retries}/{Config.RequestTimeoutRetries}");
                if (initPosition is not { } initPositionV)
                {
                    throw new IOException("Cannot retry operation: output stream is not seekable.");
                }
                targetStream.Position = initPositionV;
                targetStream.SetLength(initPositionV);
            }
        }
    }

    /// <summary>
    /// Gets file name for URI.
    /// </summary>
    /// <param name="uri">URI.</param>
    /// <returns>File name.</returns>
    public static string GetFileName(Uri uri) => Path.GetFileName(uri.Segments[^1]);

    /// <summary>
    /// Retrieves current stream file (<see cref="MainUri"/>).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning stream file from <see cref="MainUri"/>.</returns>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public async Task<M3UFile> GetAsync(CancellationToken cancellationToken = default)
    {
        using var res = await Tool.GetAsync(MainUri, new HttpRequestConfig(Referrer: Config.Referrer), cancellationToken: cancellationToken).ConfigureAwait(false);
        ArtHttpResponseMessageException.EnsureSuccessStatusCode(res);
        return M3UReader.Parse(await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
    }

    private static async Task<StreamInfo> SelectStreamAsync(HttpArtifactTool tool, M3UDownloaderConfig config, CancellationToken cancellationToken = default)
    {
        Uri liveUrlUri = new(config.URL);
        using var res = await tool.GetAsync(liveUrlUri, new HttpRequestConfig(Referrer: config.Referrer), cancellationToken: cancellationToken).ConfigureAwait(false);
        ArtHttpResponseMessageException.EnsureSuccessStatusCode(res);
        var ff = M3UReader.Parse(await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
        if (ff.Streams.All(v => v.AverageBandwidth != 0))
            return config.PrioritizeResolution
                ? ff.Streams.OrderByDescending(v => (v.ResolutionWidth * v.ResolutionHeight, v.AverageBandwidth)).First()
                : ff.Streams.OrderByDescending(v => (v.AverageBandwidth, v.ResolutionWidth * v.ResolutionHeight)).First();
        if (ff.Streams.All(v => v.Bandwidth != 0))
            return config.PrioritizeResolution
                ? ff.Streams.OrderByDescending(v => (v.ResolutionWidth * v.ResolutionHeight, v.Bandwidth)).First()
                : ff.Streams.OrderByDescending(v => (v.Bandwidth, v.ResolutionWidth * v.ResolutionHeight)).First();
        throw new InvalidDataException("Failed to choose best stream");
    }
}
