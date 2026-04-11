namespace Art;

/// <summary>
/// Represents a command for a <see cref="IArtifactListTool"/> to take as a result of a filter operation.
/// </summary>
public enum ListFilterCommand
{
    /// <summary>
    /// Output the current item.
    /// </summary>
    Accept,
    
    /// <summary>
    /// Skip the current item.
    /// </summary>
    Reject,
    
    /// <summary>
    /// Do not list more items, exit immediately.
    /// </summary>
    Abort,
}
