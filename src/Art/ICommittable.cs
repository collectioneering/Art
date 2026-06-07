namespace Art;

/// <summary>
/// Represents an object that may be committed.
/// </summary>
public interface ICommittable : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// If true, commit the object upon disposal.
    /// </summary>
    public bool ShouldCommit { get; set; }
}

/// <summary>
/// Represents an object that may be committed.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
public interface ICommittable<out T> : ICommittable
{
    /// <summary>
    /// Retrieves the value of this instance.
    /// </summary>
    public T Value { get; }
}
