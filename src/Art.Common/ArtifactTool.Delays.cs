namespace Art.Common;

public partial class ArtifactTool
{
    #region Delays

    #region Defaults

    /// <summary>
    /// Delays this operation for <see cref="DelaySeconds"/> seconds.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public Task DelayAsync(CancellationToken cancellationToken = default)
        => DelayAsync(DelaySeconds, cancellationToken);

    /// <summary>
    /// Delays this operation for <see cref="DelaySeconds"/> seconds, after the first call to this method or one of its overloads.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public Task DelayAfterFirstAsync(CancellationToken cancellationToken = default)
        => DelayAfterFirstAsync(DelaySeconds, cancellationToken);

    #endregion

    #region Seconds

    /// <summary>
    /// Delays this operation for the specified amount of time.
    /// </summary>
    /// <param name="delaySeconds">Delay in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public static Task DelayAsync(double delaySeconds, CancellationToken cancellationToken = default)
        => Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);

    /// <summary>
    /// Delays this operation for the specified amount of time.
    /// </summary>
    /// <param name="delaySeconds">Delay in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public async Task LoggedDelayAsync(double delaySeconds, CancellationToken cancellationToken = default)
    {
        LogInformation($"Delay {delaySeconds}s...");
        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Delays this operation for the specified amount of time, after the first call to this method or one of its overloads.
    /// </summary>
    /// <param name="delaySeconds">Delay in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public Task DelayAfterFirstAsync(double delaySeconds, CancellationToken cancellationToken = default)
    {
        if (_delayFirstCalled)
        {
            return DelayAsync(delaySeconds, cancellationToken);
        }
        _delayFirstCalled = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Delays this operation for the specified amount of time, after the first call to this method or one of its overloads.
    /// </summary>
    /// <param name="delaySeconds">Delay in seconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public Task LoggedDelayAfterFirstAsync(double delaySeconds, CancellationToken cancellationToken = default)
    {
        if (_delayFirstCalled)
        {
            return LoggedDelayAsync(delaySeconds, cancellationToken);
        }
        _delayFirstCalled = true;
        return Task.CompletedTask;
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Delays this operation for the specified amount of time.
    /// </summary>
    /// <param name="delay">Delay.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public static Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
        => Task.Delay(delay, cancellationToken);


    /// <summary>
    /// Delays this operation for the specified amount of time.
    /// </summary>
    /// <param name="delay">Delay.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public async Task LoggedDelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        LogInformation($"Delay {delay}s...");
        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Delays this operation for the specified amount of time, after the first call to this method or one of its overloads.
    /// </summary>
    /// <param name="delay">Delay.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public Task DelayAfterFirstAsync(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        if (_delayFirstCalled)
        {
            return DelayAsync(delay, cancellationToken);
        }
        _delayFirstCalled = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Delays this operation for the specified amount of time, after the first call to this method or one of its overloads.
    /// </summary>
    /// <param name="delay">Delay.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public Task LoggedDelayAfterFirstAsync(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        if (_delayFirstCalled)
        {
            return LoggedDelayAsync(delay, cancellationToken);
        }
        _delayFirstCalled = true;
        return Task.CompletedTask;
    }

    #endregion

    #endregion
}
