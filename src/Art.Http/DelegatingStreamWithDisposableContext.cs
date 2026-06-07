using Art.Common.IO;

namespace Art.Http;

internal class DelegatingStreamWithDisposableContext : DelegatingStream
{
    /// <inheritdoc />
    protected override Stream InnerStream { get; }

    private readonly IDisposable _disposable;

    public DelegatingStreamWithDisposableContext(Stream innerStream, IDisposable disposable)
    {
        InnerStream = innerStream;
        _disposable = disposable ?? throw new ArgumentNullException(nameof(disposable));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        // may be safer to dispose the context after the stream
        if (disposing)
        {
            _disposable.Dispose();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync().ConfigureAwait(false);
        // may be safer to dispose the context after the stream
        if (_disposable is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            _disposable.Dispose();
        }
    }
}
