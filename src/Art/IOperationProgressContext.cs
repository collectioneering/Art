namespace Art;

/// <summary>
/// Represents a target for updates on an operation.
/// </summary>
public interface IOperationProgressContext : IProgress<float>, IDisposable
{
    /// <summary>
    /// Call this method to indicate that the operation has completed in its entirety.
    /// </summary>
    /// <remarks>
    /// This method is to be used as a signal for cleanup to proceed normally.
    /// A missing call should have the context recognize that there was an error.
    /// </remarks>
    void MarkSafe();
}
