namespace Art.Common.IO;

internal class NonDisposingStream : DelegatingStream
{
    protected override Stream InnerStream { get; }

    public NonDisposingStream(Stream innerStream)
    {
        InnerStream = innerStream;
    }

    protected override void Dispose(bool disposing)
    {
    }

    public override ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
