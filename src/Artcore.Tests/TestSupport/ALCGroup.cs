using System.Runtime.Loader;

namespace Artcore.Tests.TestSupport;

internal abstract class ALCGroup : IDisposable, IAsyncDisposable
{
    public abstract void RegisterAssemblyLoadContext(AssemblyLoadContext assemblyLoadContext);

    public abstract void Dispose();

    public abstract ValueTask DisposeAsync();

    public static ALCGroup Create(int capacity)
    {
        return new FixedCapacityALCGroup(capacity);
    }

    protected static void AddReferences(List<WeakReference> weakReferences, IList<AssemblyLoadContext> contexts)
    {
        for (int i = contexts.Count - 1; i >= 0; i--)
        {
            var context = contexts[i];
            contexts[i] = null!;
            if (ReferenceEquals(context, null))
            {
                continue;
            }
            weakReferences.Add(new WeakReference(context));
            context.Unload();
        }
    }

    private const int s_stepCount = 10;
    private static readonly TimeSpan s_stepWait = TimeSpan.FromSeconds(0.3);

    protected static void WaitForWeakReferences(List<WeakReference> weakReferences)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        for (int i = 0; i < s_stepCount; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            weakReferences.RemoveAll(static v => !v.IsAlive);
            if (weakReferences.Count == 0)
            {
                return;
            }
            Thread.Sleep(s_stepWait);
        }
        throw new Exception("Failed to unload all assembly load contexts");
    }

    protected static async Task WaitForWeakReferencesAsync(List<WeakReference> weakReferences)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        for (int i = 0; i < s_stepCount; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            weakReferences.RemoveAll(static v => !v.IsAlive);
            if (weakReferences.Count == 0)
            {
                return;
            }
            await Task.Delay(s_stepWait);
        }
        throw new Exception("Failed to unload all assembly load contexts");
    }
}

internal class DynamicALCGroup : ALCGroup
{
    private readonly List<AssemblyLoadContext> _contexts;

    public DynamicALCGroup(int capacity)
    {
        _contexts = new List<AssemblyLoadContext>(capacity);
    }

    public override void RegisterAssemblyLoadContext(AssemblyLoadContext assemblyLoadContext)
    {
        if (!assemblyLoadContext.IsCollectible)
        {
            throw new ArgumentException($"{nameof(ALCGroup)} is not applicable to non-collectible ALC");
        }
        _contexts.Add(assemblyLoadContext);
    }

    public override void Dispose()
    {
        var weakReferences = new List<WeakReference>();
        AddReferences(weakReferences, _contexts);
        _contexts.Clear();
        WaitForWeakReferences(weakReferences);
    }

    public override async ValueTask DisposeAsync()
    {
        var weakReferences = new List<WeakReference>();
        AddReferences(weakReferences, _contexts);
        _contexts.Clear();
        await WaitForWeakReferencesAsync(weakReferences);
    }
}

internal class FixedCapacityALCGroup : ALCGroup
{
    private readonly AssemblyLoadContext[] _contexts;
    private int _count;

    public FixedCapacityALCGroup(int maxCapacity)
    {
        _contexts = new AssemblyLoadContext[maxCapacity];
    }

    public override void RegisterAssemblyLoadContext(AssemblyLoadContext assemblyLoadContext)
    {
        if (!assemblyLoadContext.IsCollectible)
        {
            throw new ArgumentException($"{nameof(ALCGroup)} is not applicable to non-collectible ALC");
        }
        if ((uint)(_count + 1) > _contexts.Length)
        {
            throw new InvalidOperationException($"Group is full with {_contexts.Length} capacity");
        }
        _contexts[_count++] = assemblyLoadContext;
    }

    public override void Dispose()
    {
        var weakReferences = new List<WeakReference>();
        AddReferences(weakReferences, _contexts);
        WaitForWeakReferences(weakReferences);
    }

    public override async ValueTask DisposeAsync()
    {
        var weakReferences = new List<WeakReference>();
        AddReferences(weakReferences, _contexts);
        await WaitForWeakReferencesAsync(weakReferences);
    }
}
