using M3USharper;

namespace Art.M3U;

/// <summary>
/// Represents something that operates on playlist elements.
/// </summary>
public interface IPlaylistElementProcessor
{
    /// <summary>
    /// Processes a playlist element.
    /// </summary>
    /// <param name="uri">Full URI of segment.</param>
    /// <param name="file">M3U file this element is contained in.</param>
    /// <param name="mediaSequenceNumber">Media sequence number, if available.</param>
    /// <param name="segmentSettings">Optional per-segment settings, if available.</param>
    /// <param name="segmentName">Segment name, if available.</param>
    /// <param name="itemNo">Item number, if available.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    Task ProcessPlaylistElementAsync(Uri uri, M3UFile? file, long? mediaSequenceNumber = null, SegmentSettings? segmentSettings = null, string? segmentName = null, ItemNo? itemNo = null, CancellationToken cancellationToken = default);
}
