using System.Diagnostics.CodeAnalysis;

namespace Art.Common;

/// <summary>
/// Represents an <see cref="IExtensionsContext"/> that uses a type map for registration.
/// </summary>
public class MappedExtensionsContext : IExtensionsContext
{
    private readonly Dictionary<Type, Entry> _map = new();

    private record struct Entry(Type InstanceType, Func<object> CreationDelegate);

    /// <inheritdoc />
    public bool TryGetExtension<T>([NotNullWhen(true)] out T? extension) where T : class
    {
        if (_map.TryGetValue(typeof(T), out var entry))
        {
            extension = entry.CreationDelegate() as T;
            return extension != null;
        }
        extension = null;
        return false;
    }

    /// <summary>
    /// Registers an extension type that has a parameterless constructor.
    /// </summary>
    public void Register<TInterfaceExtensionType, TInstantiatedExtensionType>() where TInstantiatedExtensionType : TInterfaceExtensionType, new()
    {
        if (!TryRegister<TInterfaceExtensionType, TInstantiatedExtensionType>())
        {
            var existing = _map.TryGetValue(typeof(TInterfaceExtensionType), out var instance) ? instance.InstanceType : null;
            throw new InvalidOperationException($"Could not register extension type {typeof(TInstantiatedExtensionType)}: another type {existing} is already registered");
        }
    }

    /// <summary>
    /// Registers an extension type that has a parameterless constructor.
    /// </summary>
    /// <typeparam name="TInterfaceExtensionType">Extension interface type to register under.</typeparam>
    /// <typeparam name="TInstantiatedExtensionType">Instantiated extension type/</typeparam>
    /// <returns>True if successful.</returns>
    public bool TryRegister<TInterfaceExtensionType, TInstantiatedExtensionType>() where TInstantiatedExtensionType : TInterfaceExtensionType, new()
    {
        if (_map.ContainsKey(typeof(TInterfaceExtensionType)))
        {
            return false;
        }
        _map.Add(typeof(TInterfaceExtensionType), new Entry(typeof(TInstantiatedExtensionType), static () => new TInstantiatedExtensionType()));
        return true;
    }
}
