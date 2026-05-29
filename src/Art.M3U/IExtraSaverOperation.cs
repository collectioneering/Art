using M3USharper;

namespace Art.M3U;

/// <summary>
/// Represents an extra operation that should be used when no new segments are immediately available in <see cref="M3UDownloaderContextProcessor"/>.
/// </summary>
/// <remarks>
/// The <see cref="TickAsync"/> method can be invoked multiple times, and
/// is intended to be used when no new segments are immediately available.
/// </remarks>
public interface IExtraSaverOperation
{
    /// <summary>
    /// Resets this operation.
    /// </summary>
    void Reset();

    /// <summary>
    /// Executes operation step.
    /// </summary>
    /// <param name="m3">Existing M3U file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task that returns false if this operation is no longer useful.</returns>
    Task<bool> TickAsync(M3UFile m3, CancellationToken cancellationToken = default);
}
