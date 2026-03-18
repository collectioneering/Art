namespace Art;

/// <summary>
/// Marks the version for a tool.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ToolVersionAttribute : Attribute
{
    /// <summary>
    /// Initializes an instance of <see cref="ToolVersionAttribute"/>.
    /// </summary>
    /// <param name="version">The version as a string.</param>
    public ToolVersionAttribute(string version)
    {
        Version = version;
    }

    /// <summary>
    /// The version as a string.
    /// </summary>
    public string Version { get; }
}
