using System.Diagnostics.CodeAnalysis;

namespace Art;

/// <summary>
/// Represents log handler for a tool.
/// </summary>
public interface IToolLogHandler : ILogHandler
{
    /// <summary>
    /// Logs a message.
    /// </summary>
    /// <param name="tool">Tool ID.</param>
    /// <param name="group">Tool group.</param>
    /// <param name="title">Log title.</param>
    /// <param name="body">Log body.</param>
    /// <param name="level">Log level.</param>
    void Log(string tool, string group, string? title, string? body, LogLevel level);

    /// <summary>
    /// Gets an operation progress context.
    /// </summary>
    /// <param name="operationName">A human-readable description for the operation.</param>
    /// <param name="operationGuid">GUID that identifies operation type.</param>
    /// <param name="operationProgressContext">Context that can be used to report progress.</param>
    /// <returns>True if successful.</returns>
    bool TryGetOperationProgressContext(string operationName, Guid operationGuid, [NotNullWhen(true)] out IOperationProgressContext? operationProgressContext)
    {
        operationProgressContext = null;
        return false;
    }

    /// <summary>
    /// Gets an operation progress context that operates concurrently with other operations.
    /// </summary>
    /// <param name="operationName">A human-readable description for the operation.</param>
    /// <param name="operationGuid">GUID that identifies operation type.</param>
    /// <param name="operationProgressContext">Context that can be used to report progress.</param>
    /// <returns>True if successful.</returns>
    bool TryGetConcurrentOperationProgressContext(string operationName, Guid operationGuid, [NotNullWhen(true)] out IOperationProgressContext? operationProgressContext)
    {
        operationProgressContext = null;
        return false;
    }
}
