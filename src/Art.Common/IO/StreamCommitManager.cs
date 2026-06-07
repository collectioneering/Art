namespace Art.Common.IO;

internal class StreamCommitManager : ICommittable<Stream>
{
    internal Stream? _stream;

    /// <inheritdoc />
    public void Dispose() => _stream?.Dispose();

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return _stream?.DisposeAsync() ?? ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public bool ShouldCommit { get; set; }

    public Stream Value => _stream ?? throw new InvalidOperationException();
}
