using System.Diagnostics;
using Art.Http;

namespace Art.M3U;

/// <summary>
/// Represents a basic saver.
/// </summary>
public class M3UDownloaderContextStandardSaver : M3UDownloaderContextSaver
{
    private readonly bool _oneOff;
    private readonly TimeSpan _timeout;

    internal M3UDownloaderContextStandardSaver(M3UDownloaderContext context, bool oneOff, TimeSpan timeout) : base(context)
    {
        _oneOff = oneOff;
        _timeout = timeout;
    }

    /// <inheritdoc />
    public override Task RunAsync(CancellationToken cancellationToken = default)
    {
        return OperateAsync(null, cancellationToken);
    }

    /// <inheritdoc />
    public override Task ExportAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return OperateAsync(stream, cancellationToken);
    }

    private async Task OperateAsync(Stream? targetStream = null, CancellationToken cancellationToken = default)
    {
        FailCounter = 0;
        HashSet<string> hs = new();
        Stopwatch sw = new();
        while (true)
        {
            try
            {
                if (HeartbeatCallback != null) await HeartbeatCallback().ConfigureAwait(false);
                Context.Tool.LogInformation("Reading main...");
                List<string> entries = new();
                M3UFile m3 = await Context.GetAsync(cancellationToken).ConfigureAwait(false);
                if (Context.StreamInfo.EncryptionInfo is { Encrypted: true } ei && m3.EncryptionInfo is { Encrypted: true } ei2 && ei.Method == ei2.Method)
                {
                    ei2.Key ??= ei.Key; // assume key kept if it was supplied in the first place
                    ei2.Iv ??= ei.Iv; // assume IV kept if it was supplied in the first place
                }
                entries.AddRange(m3.DataLines);
                entries.RemoveAll(v => hs.Contains(v));
                Context.Tool.LogInformation($"{entries.Count} new segments...");
                if (entries.Count != 0)
                {
                    sw.Restart();
                }
                else if (sw.IsRunning && sw.Elapsed > _timeout)
                {
                    Context.Tool.LogInformation($"No new entries for timeout {_timeout}, stopping");
                    return;
                }
                int i = 0;
                foreach (string entry in entries)
                {
                    Context.Tool.LogInformation($"Downloading segment {entry}...");
                    if (targetStream != null)
                    {
                        await Context.StreamSegmentAsync(targetStream, new Uri(Context.MainUri, entry), m3, m3.FirstMediaSequenceNumber + i, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        await Context.DownloadSegmentAsync(new Uri(Context.MainUri, entry), m3, m3.FirstMediaSequenceNumber + i, cancellationToken).ConfigureAwait(false);
                    }
                    i++;
                }
                hs.UnionWith(entries);
                if (_oneOff) break;
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                FailCounter = 0;
            }
            catch (ArtHttpResponseMessageException e)
            {
                await HandleRequestExceptionAsync(e, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
