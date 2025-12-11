using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Art.Common.Logging;
using ConFormat;

namespace Art.Tesler;

public class ConsoleStyledToolLogHandlerProvider : ToolLogHandlerProviderBase
{
    private readonly Func<bool> _errorRedirectedFunc;
    private readonly Func<int> _widthFunc;
    private readonly Func<int> _heightFunc;
    private readonly Func<int> _initialRowFunc;

    public ConsoleStyledToolLogHandlerProvider(
        TextWriter outWriter,
        TextWriter warnWriter,
        TextWriter errorWriter,
        Func<bool> errorRedirectedFunc,
        Func<int> widthFunc,
        Func<int> heightFunc,
        Func<int> initialRowFunc,
        Func<Stream> outStreamAccessFunc)
        : base(outWriter, warnWriter, errorWriter, outStreamAccessFunc)
    {
        _errorRedirectedFunc = errorRedirectedFunc;
        _widthFunc = widthFunc;
        _heightFunc = heightFunc;
        _initialRowFunc = initialRowFunc;
    }

    public override IToolLogHandler GetStreamToolLogHandler()
    {
        return new ConsoleStyledLogHandler(
            Out,
            Warn,
            Error,
            true,
            _errorRedirectedFunc,
            _widthFunc,
            _heightFunc,
            _initialRowFunc,
            true);
    }

    public override IToolLogHandler GetDefaultToolLogHandler()
    {
        return new ConsoleStyledLogHandler(
            Out,
            Warn,
            Error,
            false,
            _errorRedirectedFunc,
            _widthFunc,
            _heightFunc,
            _initialRowFunc,
            false,
            OperatingSystem.IsMacOS());
    }
}

public class ConsoleStyledLogHandler : StyledLogHandler, IOperationsOwner
{
    private readonly bool _forceFallback;
    private readonly Func<bool> _errorRedirectedFunc;
    private readonly Func<int> _widthFunc;
    private readonly Func<int> _heightFunc;
    private readonly Func<int> _initialRowFunc;
    private static readonly Guid s_downloadOperation = Guid.ParseExact("c6d42b18f0ae452385f180aa74e9ef29", "N");
    private static readonly Guid s_operationWaitingForResult = Guid.ParseExact("4fd5c851a88c430c8f8da54dbcf70ab2", "N");
    private readonly Dictionary<object, Guid> _multiObjects = new();
    private readonly HashSet<Guid> _registeredMultiObjects = new();
    private MultiBarContext<Guid>? _multiBarContext;
    private readonly object _lock = new();

    public ConsoleStyledLogHandler(
        TextWriter outWriter,
        TextWriter warnWriter,
        TextWriter errorWriter,
        bool forceFallback,
        Func<bool> errorRedirectedFunc,
        Func<int> widthFunc,
        Func<int> heightFunc,
        Func<int> initialRowFunc,
        bool alwaysPrintToErrorStream,
        bool enableFancy = false)
        : base(outWriter, warnWriter, errorWriter, alwaysPrintToErrorStream, enableFancy)
    {
        _forceFallback = forceFallback;
        _errorRedirectedFunc = errorRedirectedFunc;
        _widthFunc = widthFunc;
        _heightFunc = heightFunc;
        _initialRowFunc = initialRowFunc;
    }

    private MultiBarContext<Guid> GetMultiBarContext()
    {
        lock (_lock)
        {
            if (_multiBarContext != null)
            {
                return _multiBarContext;
            }
            return _multiBarContext = MultiBarContext<Guid>.Create(
                Error,
                _forceFallback,
                _errorRedirectedFunc,
                _widthFunc,
                _heightFunc,
                _initialRowFunc());
        }
    }

    private Guid AllocateGuid()
    {
        Guid result;
        do
        {
            result = Guid.NewGuid();
        } while (!_registeredMultiObjects.Add(result));
        return result;
    }

    public override bool TryGetConcurrentOperationProgressContext(string operationName, Guid operationGuid, [NotNullWhen(true)] out IOperationProgressContext? operationProgressContext)
    {
        lock (_lock)
        {
            if (operationGuid.Equals(s_downloadOperation))
            {
                Guid guid = AllocateGuid();
                try
                {
                    operationProgressContext = new DownloadUpdateContextForMulti(
                        GetMultiBarContext(),
                        guid,
                        operationName,
                        this);
                    _multiObjects.Add(operationProgressContext, guid);
                }
                catch
                {
                    _registeredMultiObjects.Remove(guid);
                    throw;
                }
                return true;
            }
            if (operationGuid.Equals(s_operationWaitingForResult))
            {
                Guid guid = AllocateGuid();
                try
                {
                    operationProgressContext = new WaitUpdateContextForMulti(
                        GetMultiBarContext(),
                        guid,
                        operationName,
                        this);
                    _multiObjects.Add(operationProgressContext, guid);
                }
                catch
                {
                    _registeredMultiObjects.Remove(guid);
                    throw;
                }
                return true;
            }
            operationProgressContext = null;
            return false;
        }
    }

    public override bool TryGetOperationProgressContext(string operationName, Guid operationGuid, [NotNullWhen(true)] out IOperationProgressContext? operationProgressContext)
    {
        lock (_lock)
        {
            if (_multiObjects.Count > 0)
            {
                operationProgressContext = null;
                return false;
            }
            if (operationGuid.Equals(s_downloadOperation))
            {
                operationProgressContext = new DownloadUpdateContext(operationName, Error, _forceFallback, _errorRedirectedFunc, _widthFunc);
                return true;
            }
            if (operationGuid.Equals(s_operationWaitingForResult))
            {
                operationProgressContext = new WaitUpdateContext(operationName, Error, _forceFallback, _errorRedirectedFunc, _widthFunc);
                return true;
            }
            operationProgressContext = null;
            return false;
        }
    }

    void IOperationsOwner.Release(object self)
    {
        lock (_lock)
        {
            if (!_multiObjects.Remove(self, out var guid))
            {
                return;
            }
            _registeredMultiObjects.Remove(guid);
            if (_multiObjects.Count == 0 && _multiBarContext != null)
            {
                _multiBarContext.Dispose();
                _multiBarContext = null;
            }
        }
    }
}

internal interface IOperationsOwner
{
    void Release(object self);
}

internal class WaitUpdateContext : IOperationProgressContext
{
    private readonly BarContext _context;
    private EllipsisSuffixContentFiller _filler;

    public WaitUpdateContext(string name, TextWriter output, bool forceFallback, Func<bool> errorRedirectedFunc, Func<int> widthFunc)
    {
        _context = BarContext.Create(output, forceFallback, errorRedirectedFunc, widthFunc);
        _filler = new EllipsisSuffixContentFiller(name, 0);
        _context.Write(ref _filler);
    }

    public void Report(float value)
    {
        _context.Update(ref _filler);
    }

    public void Dispose()
    {
        _context.Clear();
        _context.Dispose();
    }
}

internal class WaitUpdateContextForMulti : IOperationProgressContext
{
    private readonly MultiBarContext<Guid> _context;
    private readonly Guid _key;
    private EllipsisSuffixContentFiller _filler;
    private readonly IOperationsOwner _operationsOwner;
    private bool _disposed;

    public WaitUpdateContextForMulti(MultiBarContext<Guid> context, Guid key, string name, IOperationsOwner owner)
    {
        _context = context;
        _key = key;
        _filler = new EllipsisSuffixContentFiller(name, 0);
        _context.Allocate(_key);
        _context.Write(_key, ref _filler);
        _operationsOwner = owner;
    }

    public void Report(float value)
    {
        EnsureNotDisposed();
        _context.Update(_key, ref _filler);
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _context.Clear(_key);
        _context.Remove(_key);
        _context.Dispose();
        _operationsOwner.Release(this);
    }
}

internal class DownloadUpdateContext : IOperationProgressContext
{
    private readonly BarContext _context;
    private readonly Stopwatch _stopwatch;
    private TimedDownloadPrefabContentFiller _filler;

    public DownloadUpdateContext(string name, TextWriter output, bool forceFallback, Func<bool> errorRedirectedFunc, Func<int> widthFunc)
    {
        _context = BarContext.Create(output, forceFallback, errorRedirectedFunc, widthFunc);
        _filler = TimedDownloadPrefabContentFiller.Create(name);
        _context.Write(ref _filler);
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    public void Report(float value)
    {
        _filler.SetDuration(_stopwatch.Elapsed);
        _filler.SetProgress(value);
        _context.Update(ref _filler);
    }

    public void Dispose()
    {
        _context.Clear();
        _context.Dispose();
    }
}

internal class DownloadUpdateContextForMulti : IOperationProgressContext
{
    private readonly MultiBarContext<Guid> _context;
    private readonly Guid _key;
    private readonly Stopwatch _stopwatch;
    private TimedDownloadPrefabContentFiller _filler;
    private readonly IOperationsOwner _operationsOwner;
    private bool _disposed;

    public DownloadUpdateContextForMulti(MultiBarContext<Guid> context, Guid key, string name, IOperationsOwner owner)
    {
        _context = context;
        _key = key;
        _context = context;
        _filler = TimedDownloadPrefabContentFiller.Create(name);
        _context.Allocate(_key);
        _context.Write(_key, ref _filler);
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
        _operationsOwner = owner;
    }

    public void Report(float value)
    {
        EnsureNotDisposed();
        _filler.SetDuration(_stopwatch.Elapsed);
        _filler.SetProgress(value);
        _context.Update(_key, ref _filler);
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _operationsOwner.Release(this);
        _context.Clear(_key);
        _context.Remove(_key);
        _context.Dispose();
    }
}
