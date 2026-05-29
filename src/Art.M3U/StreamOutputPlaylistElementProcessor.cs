using M3USharper;

namespace Art.M3U;

internal class StreamOutputPlaylistElementProcessor : IPlaylistElementProcessor
{
    private readonly M3UDownloaderContext _context;
    private readonly Stream _targetStream;

    public StreamOutputPlaylistElementProcessor(M3UDownloaderContext context, Stream targetStream)
    {
        _context = context;
        _targetStream = targetStream;
    }

    public Task ProcessPlaylistElementAsync(Uri uri, M3UFile? file, long? mediaSequenceNumber = null, SegmentSettings? segmentSettings = null, string? segmentName = null, ItemNo? itemNo = null, CancellationToken cancellationToken = default)
    {
        if (itemNo?.GetMessage() is { } itemNumberText)
        {
            _context.Tool.LogInformation($"Streaming segment {itemNumberText} {segmentName ?? "???"}");
        }
        else
        {
            _context.Tool.LogInformation($"Streaming segment {segmentName ?? "???"}");
        }
        return _context.StreamSegmentAsync(_targetStream, uri, file, mediaSequenceNumber, segmentSettings, cancellationToken);
    }
}
