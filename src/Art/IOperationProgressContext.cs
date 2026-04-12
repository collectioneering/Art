namespace Art;

/// <summary>
/// Represents a target for updates on an operation.
/// </summary>
public interface IOperationProgressContext : IProgress<float>, IRefreshContext, IDisposable
{
    /// <summary>
    /// Report a status update with a specified name.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <param name="name">Updated name.</param>
    void ReportNamed(float value, string name)
    {
        Report(value);
    }

    /// <summary>
    /// Call this method to indicate that the operation has completed in its entirety.
    /// </summary>
    /// <remarks>
    /// This method is to be used as a signal for cleanup to proceed normally.
    /// A missing call should have the context recognize that there was an error.
    /// </remarks>
    void MarkSafe();
}
