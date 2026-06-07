namespace Art.Common.Management;

/// <summary>
/// Represents a wrapper around a <see cref="MemoryStream"/>.
/// </summary>
public class CommittableMemoryStream : CommittableDelegatingStream
{
    /// <inheritdoc />
    protected override Stream InnerStream => MemoryStream;

    /// <summary>
    /// Target memory stream.
    /// </summary>
    public readonly MemoryStream MemoryStream;

    /// <summary>
    /// Creates a new instance of <see cref="CommittableMemoryStream"/>.
    /// </summary>
    public CommittableMemoryStream() : this(new MemoryStream())
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="CommittableMemoryStream"/>.
    /// </summary>
    /// <param name="capacity">Initial capacity.</param>
    public CommittableMemoryStream(int capacity) : this(new MemoryStream(capacity))
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="CommittableMemoryStream"/>.
    /// </summary>
    /// <param name="memoryStream">Stream to wrap.</param>
    public CommittableMemoryStream(MemoryStream memoryStream)
    {
        MemoryStream = memoryStream;
    }

    /// <inheritdoc />
    protected override void Commit(bool shouldCommit)
    {
    }

    /// <inheritdoc />
    protected override ValueTask CommitAsync(bool shouldCommit)
    {
        return ValueTask.CompletedTask;
    }
}
