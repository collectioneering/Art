using Art.Http;

namespace Art.M3U;

/// <summary>
/// Represents a basic stream output saver.
/// </summary>
public class M3UDownloaderContextStreamOutputSaver : M3UDownloaderContextProcessor
{
    private readonly bool _oneOff;
    private readonly TimeSpan _timeout;
    private readonly SegmentFilterDelegate? _segmentFilter;

    internal M3UDownloaderContextStreamOutputSaver(M3UDownloaderContext context, bool oneOff, TimeSpan timeout, SegmentFilterDelegate? segmentFilter) : base(context)
    {
        _oneOff = oneOff;
        _timeout = timeout;
        _segmentFilter = segmentFilter;
    }

    /// <summary>
    /// Copies segments one at a time to target stream.
    /// </summary>
    /// <param name="stream">Stream to copy to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public Task ExportAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return ProcessPlaylistAsync(_oneOff, _timeout, new StreamOutputPlaylistElementProcessor(Context, stream), _segmentFilter, null, cancellationToken);
    }
}
