using System.Globalization;
using System.Runtime.CompilerServices;
using Art.Common.Async;

namespace Art.Common;

/// <summary>
/// Provides utility functions.
/// </summary>
public static class ArtCommonExtensions
{
    private static readonly char[] s_invalid = Path.GetInvalidFileNameChars().Append(':').ToArray();

    /// <summary>
    /// Remove invalid filename characters, based on <see cref="Path.GetInvalidFileNameChars()"/>.
    /// </summary>
    /// <param name="name">String.</param>
    /// <returns>Better filename.</returns>
    public static string SafeifyFileName(this string name) => s_invalid.Aggregate(Path.GetFileName(name), (f, v) => f.Contains(v) ? f.Replace(v, '-') : f);

    /// <summary>
    /// Copies all elements to a list asynchronously.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="enumerable">Enumerable to convert to list.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning list.</returns>
#if NET10_0_OR_GREATER
    [Obsolete("Use System.Linq.AsyncEnumerable.ToListAsync<T> from System.Linq.Async instead")]
#endif
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken = default)
    {
        List<T> list = new();
        await foreach (T value in enumerable.WithCancellation(cancellationToken).ConfigureAwait(false))
            list.Add(value);
        return list;
    }

    /// <summary>
    /// Simplifies a numeric key string.
    /// </summary>
    /// <param name="key">Number key.</param>
    /// <returns>Normalized number key (e.g. 1, 616, 333018).</returns>
    public static string SimplifyNumericKey(this string key)
        => long.TryParse(key, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result) ? result.ToString(CultureInfo.InvariantCulture) : key;

    /// <summary>
    /// Lists artifacts as key-value pairs of ID to artifact data.
    /// </summary>
    /// <param name="artifactListTool">Artifact tool.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning created dictionary.</returns>
    public static async ValueTask<Dictionary<string, IArtifactData>> ListDictionaryAsync(this IArtifactListTool artifactListTool, CancellationToken cancellationToken = default)
    {
        Dictionary<string, IArtifactData> res = new();
        await foreach (IArtifactData artifactData in artifactListTool.ListAsync(cancellationToken).ConfigureAwait(false))
            res[artifactData.Info.Key.Id] = artifactData;
        return res;
    }

    /// <summary>
    /// Takes up to a specified number of elements from an enumerator.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="enumerator">Enumerator.</param>
    /// <param name="collection">Collection to populate.</param>
    /// <param name="max">Maximum number of elements to take.</param>
    /// <returns>True if any elements were taken (false if no more enumerator elements remained).</returns>
    public static bool DoTake<T>(this IEnumerator<T> enumerator, ICollection<T> collection, int max)
    {
        bool any = false;
        while (max-- > 0 && enumerator.MoveNext())
        {
            collection.Add(enumerator.Current);
            any = true;
        }

        return any;
    }

    /// <summary>
    /// Fallback empty enumerable.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="enumerable">Enumerable.</param>
    /// <returns>Nonnull enumerable.</returns>
    public static IEnumerable<T> FallbackEmpty<T>(this IEnumerable<T>? enumerable) => enumerable ?? Array.Empty<T>();

    /// <summary>
    /// Produces task returning true if any elements are contained in this enumerable.
    /// </summary>
    /// <param name="enumerable">Enumerable.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Task returning true if any elements are contained.</returns>
    public static async ValueTask<bool> AnyAsync<T>(this IAsyncEnumerable<T> enumerable)
    {
        await foreach (var _ in enumerable.ConfigureAwait(false))
            return true;
        return false;
    }

    /// <summary>
    /// Produces task returning count of elements.
    /// </summary>
    /// <param name="enumerable">Enumerable.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Task returning count of elements.</returns>
    public static async ValueTask<int> CountAsync<T>(this IAsyncEnumerable<T> enumerable)
    {
        int sum = 0;
        await foreach (var _ in enumerable.ConfigureAwait(false)) sum++;
        return sum;
    }

    /// <summary>
    /// Produces enumerable filtering elements.
    /// </summary>
    /// <param name="enumerable">Enumerable.</param>
    /// <param name="predicate">Predicate.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Task returning count of elements.</returns>
    public static async IAsyncEnumerable<T> WhereAsync<T>(this IAsyncEnumerable<T> enumerable, Predicate<T> predicate)
    {
        await foreach (T v in enumerable.ConfigureAwait(false))
            if (predicate(v))
                yield return v;
    }

    /// <summary>
    /// Produces enumerable transforming elements.
    /// </summary>
    /// <param name="enumerable">Enumerable.</param>
    /// <param name="func">Transform.</param>
    /// <typeparam name="TFrom">Source value type.</typeparam>
    /// <typeparam name="TTo">Target type.</typeparam>
    /// <returns>Task returning count of elements.</returns>
    public static async IAsyncEnumerable<TTo> SelectAsync<TFrom, TTo>(this IAsyncEnumerable<TFrom> enumerable, Func<TFrom, TTo> func)
    {
        await foreach (TFrom from in enumerable.ConfigureAwait(false))
            yield return func(from);
    }

    /// <summary>
    /// Gets only element or default value.
    /// </summary>
    /// <param name="enumerable">Enumerable.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Task returning only element or default.</returns>
    public static async ValueTask<T?> SingleOrDefaultAsync<T>(this IAsyncEnumerable<T> enumerable)
    {
        await using ConfiguredCancelableAsyncEnumerable<T>.Enumerator enumerator = enumerable.ConfigureAwait(false).GetAsyncEnumerator();
        if (await enumerator.MoveNextAsync())
        {
            T value = enumerator.Current;
            if (await enumerator.MoveNextAsync()) throw new InvalidOperationException("More than one element contained in sequence");
            return value;
        }

        return default;
    }

    /// <summary>
    /// Gets only element or default value.
    /// </summary>
    /// <param name="enumerable">Enumerable.</param>
    /// <param name="default">Default value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Task returning only element or default.</returns>
    public static async ValueTask<T?> SingleOrDefaultAsync<T>(this IAsyncEnumerable<T> enumerable, T @default)
    {
        await using ConfiguredCancelableAsyncEnumerable<T>.Enumerator enumerator = enumerable.ConfigureAwait(false).GetAsyncEnumerator();
        if (await enumerator.MoveNextAsync())
        {
            T value = enumerator.Current;
            if (await enumerator.MoveNextAsync()) throw new InvalidOperationException("More than one element contained in sequence");
            return value;
        }

        return @default;
    }

    /// <summary>
    /// Gets first element or default value.
    /// </summary>
    /// <param name="enumerable">Enumerable.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Task returning first element or default.</returns>
    public static async ValueTask<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> enumerable)
    {
        await foreach (T v in enumerable.ConfigureAwait(false))
            return v;
        return default;
    }

    /// <summary>
    /// Gets first element or default value.
    /// </summary>
    /// <param name="enumerable">Enumerable.</param>
    /// <param name="default">Default value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Task returning first element or default.</returns>
    public static async ValueTask<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> enumerable, T @default)
    {
        await foreach (T v in enumerable.ConfigureAwait(false))
            return v;
        return @default;
    }

    /// <summary>
    /// Executes enumerable's enumerator independently of move-next calls.
    /// </summary>
    /// <param name="asyncEnumerable">Enumerable.</param>
    /// <param name="maxPreemptiveAccesses">Maximum number of preemptive accesses (-1 for infinite).</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Enumerable.</returns>
    public static IAsyncEnumerable<T> EagerAsync<T>(this IAsyncEnumerable<T> asyncEnumerable, int maxPreemptiveAccesses = -1)
    {
        return new EagerAsyncEnumerable<T>(asyncEnumerable, maxPreemptiveAccesses);
    }
}
