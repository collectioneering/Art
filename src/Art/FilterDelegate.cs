namespace Art;

/// <summary>
/// Represents a filter that may execute asynchronously.
/// </summary>
/// <param name="value">Value to compare.</param>
/// <param name="cancellationToken">Optional cancellation token.</param>
/// <typeparam name="TValue">The type of object to compare.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public delegate ValueTask<TResult> FilterDelegate<in TValue, TResult>(TValue value, CancellationToken cancellationToken = default);
