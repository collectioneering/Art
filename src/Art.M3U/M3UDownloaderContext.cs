using System.Security.Cryptography;
using System.Text;

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
    }

    private static async Task<Uri> SelectSubStreamAsync(HttpArtifactTool tool, M3UDownloaderConfig config, CancellationToken cancellationToken = default)
    {
        StreamInfo highestStream = await SelectStreamAsync(tool, config, cancellationToken);
        long bw = highestStream.AverageBandwidth == 0 ? highestStream.Bandwidth : highestStream.AverageBandwidth;
        tool.LogInformation($"Selected {highestStream.Path} ({bw} b/s, {highestStream.ResolutionWidth}x{highestStream.ResolutionHeight})");
        return new Uri(new Uri(config.URL), highestStream.Path);
    }

    /// <summary>
    /// Sets configuration.
    /// </summary>
    /// <param name="config">Configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SetConfigAsync(M3UDownloaderConfig config, CancellationToken cancellationToken = default)
    {
        Uri mainUri = await SelectSubStreamAsync(Tool, config, cancellationToken);
        MainUri = mainUri;
        Config = config;
    }

    /// <summary>
    /// Creates a new downloader context.
    /// </summary>
    /// <param name="tool">Tool.</param>
    /// <param name="config">Configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning downloader context.</returns>
    public static async Task<M3UDownloaderContext> OpenContextAsync(HttpArtifactTool tool, M3UDownloaderConfig config, CancellationToken cancellationToken = default)
    {
        string? referrer = config.Referrer;
        tool.LogInformation("Getting stream info...");
        Uri mainUri = await SelectSubStreamAsync(tool, config, cancellationToken);
        tool.LogInformation("Getting sub stream info...");
        M3UFile m3;
        M3UEncryptionInfo? ei;
        using (var res = await tool.GetAsync(mainUri, referrer: referrer, cancellationToken: cancellationToken))
        {
            res.EnsureSuccessStatusCode();
            m3 = M3UReader.Parse(await res.Content.ReadAsStringAsync(cancellationToken));
            ei = m3.EncryptionInfo;
        }
        if (ei != null) tool.LogInformation($"Encrypted with {ei.Method}");
        if (ei?.Iv is not null) tool.LogInformation($"IV {Convert.ToHexString(ei.Iv)}");
        if (ei is { Uri: { } })
        {
            tool.LogInformation("Downloading enc key...");
            using var res = await tool.GetAsync(new Uri(mainUri, ei.Uri), referrer: referrer, cancellationToken: cancellationToken);
            res.EnsureSuccessStatusCode();
            ei.Key = await res.Content.ReadAsByteArrayAsync(cancellationToken);
            tool.LogInformation($"KEY {Convert.ToHexString(ei.Key)}");
        }
        return new M3UDownloaderContext(tool, config, mainUri, m3);
    }

    /// <summary>
    /// Writes encryption information to method.txt, key.bin, iv.bin according to <see cref="Tool"/>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task WriteKeyMaterialAsync(CancellationToken cancellationToken = default)
    {
        if (StreamInfo.EncryptionInfo is not { } ei) return;
        ArtifactResourceKey methodArk = new(Config.ArtifactKey, "method.txt", Config.ArtifactKey.Id);
        await using (Stream keyStream = await Tool.DataManager.CreateOutputStreamAsync(methodArk, cancellationToken)) keyStream.Write(Encoding.UTF8.GetBytes(ei.Method));
        if (ei.Key is not null)
        {
            ArtifactResourceKey keyArk = new(Config.ArtifactKey, "key.bin", Config.ArtifactKey.Id);
            await using Stream keyStream = await Tool.DataManager.CreateOutputStreamAsync(keyArk, cancellationToken);
            keyStream.Write(ei.Key);
        }
        if (ei.Iv is not null)
        {
            ArtifactResourceKey ivArk = new(Config.ArtifactKey, "iv.bin", Config.ArtifactKey.Id);
            await using Stream keyStream = await Tool.DataManager.CreateOutputStreamAsync(ivArk, cancellationToken);
            keyStream.Write(ei.Iv);
        }
    }

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
    /// <returns>Downloader.</returns>
    public M3UDownloaderContextSaver CreateSaver(bool oneOff, TimeSpan timeout) => new(this, oneOff, timeout);

    /// <summary>
    /// Downloads a segment.
    /// </summary>
    /// <param name="uri">URI to download.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="InvalidDataException">Thrown for unexpected encryption algorithm when set to decrypt.</exception>
    public async Task DownloadSegmentAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        ArtifactResourceKey ark = new(Config.ArtifactKey, Path.GetFileName(uri.Segments[^1]), Config.ArtifactKey.Id);
        if (await Tool.TryGetResourceAsync(ark, cancellationToken) != null)
        {
            if (Config.SkipExistingSegments) return;
            await Tool.RegistrationManager.RemoveResourceAsync(ark, cancellationToken);
        }
        ArtifactResourceInfo ari = new UriArtifactResourceInfo(Tool, uri, null, Config.Referrer, ark);
        if (StreamInfo.EncryptionInfo is { Key: { } } ei && Config.Decrypt)
        {
            byte[] key = ei.Key;
            ari = new EncryptedArtifactResourceInfo(ei.Method switch
            {
                "AES-128" => new EncryptionInfo(CryptoAlgorithm.Aes, key, CipherMode.CBC, EncIv: ei.Iv),
                "AES-192" => new EncryptionInfo(CryptoAlgorithm.Aes, key, CipherMode.CBC, EncIv: ei.Iv),
                "AES-256" => new EncryptionInfo(CryptoAlgorithm.Aes, key, CipherMode.CBC, EncIv: ei.Iv),
                _ => throw new InvalidDataException()
            }, ari);
        }
        await using (Stream oStream = await Tool.DataManager.CreateOutputStreamAsync(ark, cancellationToken))
            await ari.ExportStreamAsync(oStream, cancellationToken).ConfigureAwait(false);
        await Tool.AddResourceAsync(ari, cancellationToken);
    }

    /// <summary>
    /// Retrieves current stream file (<see cref="MainUri"/>).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning stream file from <see cref="MainUri"/>.</returns>
    public async Task<M3UFile> GetAsync(CancellationToken cancellationToken = default)
    {
        using var res = await Tool.GetAsync(MainUri, referrer: Config.Referrer, cancellationToken: cancellationToken);
        res.EnsureSuccessStatusCode();
        return M3UReader.Parse(await res.Content.ReadAsStringAsync(cancellationToken));
    }

    private static async Task<StreamInfo> SelectStreamAsync(HttpArtifactTool tool, M3UDownloaderConfig config, CancellationToken cancellationToken = default)
    {
        Uri liveUrlUri = new(config.URL);
        using var res = await tool.GetAsync(liveUrlUri, referrer: config.Referrer, cancellationToken: cancellationToken);
        res.EnsureSuccessStatusCode();
        var ff = M3UReader.Parse(await res.Content.ReadAsStringAsync(cancellationToken));
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
