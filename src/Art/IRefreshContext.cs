namespace Art;

/// <summary>
/// Represents a context that can be refreshed.
/// </summary>
/// <seealso cref="IOperationProgressContext"/>
public interface IRefreshContext
{
    /// <summary>
    /// Refreshes the output if applicable.
    /// </summary>
    /// <remarks>
    /// This method may be used to signal the context to update its display (e.g. for elapsed time ticking) without submitting new data.
    /// </remarks>
    void Refresh();
}
