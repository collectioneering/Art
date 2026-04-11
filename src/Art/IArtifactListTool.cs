using System.Runtime.CompilerServices;

namespace Art;

/// <summary>
/// Represents an artifact tool that lists.
/// </summary>
public interface IArtifactListTool : IArtifactTool
{
    /// <summary>
    /// Lists artifacts.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async-enumerable artifacts.</returns>
    IAsyncEnumerable<IArtifactData> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists artifacts.
    /// </summary>
    /// <param name="filterDelegate">Delegate to filter potential artifacts.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async-enumerable artifacts.</returns>
    async IAsyncEnumerable<IArtifactData> ListAsync(FilterDelegate<ArtifactKey, ListFilterCommand> filterDelegate, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var entry in ListAsync(cancellationToken).ConfigureAwait(false))
        {
            switch (await filterDelegate(entry.Info.Key, cancellationToken).ConfigureAwait(false))
            {
                case ListFilterCommand.Accept:
                    yield return entry;
                    break;
                case ListFilterCommand.Reject:
                    break;
                case ListFilterCommand.Abort:
                    yield break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
