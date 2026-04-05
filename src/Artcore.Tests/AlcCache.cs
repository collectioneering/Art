using System.Runtime.Loader;

namespace Artcore.Tests;

internal class AlcCache : IDisposable, IAsyncDisposable
{
    private readonly List<AssemblyLoadContext> _contexts = new();

    public void RegisterAssemblyLoadContext(AssemblyLoadContext assemblyLoadContext)
    {
        if (!assemblyLoadContext.IsCollectible)
        {
            throw new ArgumentException($"{nameof(AlcCache)} is not applicable to non-collectible ALC");
        }
        _contexts.Add(assemblyLoadContext);
    }

    private static void AddReferences(List<WeakReference> weakReferences, List<AssemblyLoadContext> contexts)
    {
        for (int i = contexts.Count - 1; i >= 0; i--)
        {
            var context = contexts[i];
            contexts[i] = null!;
            weakReferences.Add(new WeakReference(context));
            context.Unload();
        }
        contexts.Clear();
    }

    public void Dispose()
    {
        var weakReferences = new List<WeakReference>();
        AddReferences(weakReferences, _contexts);
        for (int i = 0; i < 10; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            weakReferences.RemoveAll(static v => !v.IsAlive);
            if (weakReferences.Count == 0)
            {
                return;
            }
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
        }
        throw new Exception("Failed to unload all assembly load contexts");
    }

    public async ValueTask DisposeAsync()
    {
        var weakReferences = new List<WeakReference>();
        AddReferences(weakReferences, _contexts);
        for (int i = 0; i < 10; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            weakReferences.RemoveAll(static v => !v.IsAlive);
            if (weakReferences.Count == 0)
            {
                return;
            }
            await Task.Delay(TimeSpan.FromSeconds(0.5));
        }
        throw new Exception("Failed to unload all assembly load contexts");
    }
}
