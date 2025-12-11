using System.Diagnostics;
using System.Net;
using Art.Http;

namespace Art.M3U;

/// <summary>
/// Base class for <see cref="M3UDownloaderContext"/>-bound savers.
/// </summary>
public abstract class M3UDownloaderContextProcessor
{
    private static readonly TimeSpan s_playlistDelay = TimeSpan.FromSeconds(1);
    private static readonly Guid s_operationWaitingForResult = Guid.ParseExact("4fd5c851a88c430c8f8da54dbcf70ab2", "N");

    /// <summary>
    /// Heartbeat callback to use before an iteration.
    /// </summary>
    public Func<Task>? HeartbeatCallback { get; set; }

    /// <summary>
    /// Recovery callback for errors.
    /// </summary>
    public Func<Exception, Task>? RecoveryCallback { get; set; }

    /// <summary>
    /// Parent context.
    /// </summary>
    protected readonly M3UDownloaderContext Context;

    /// <summary>
    /// Consecutive failure counter for this instance.
    /// </summary>
    protected volatile int ConsecutiveFailCounter;

    /// <summary>
    /// Total failure counter for this instance.
    /// </summary>
    protected volatile int TotalFailCounter;

    /// <summary>
    /// Initializes a new instance of <see cref="M3UDownloaderContextProcessor"/>.
    /// </summary>
    /// <param name="context">Parent context.</param>
    protected M3UDownloaderContextProcessor(M3UDownloaderContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Flags representing result of failure handling.
    /// </summary>
    [Flags]
    public enum FailureHandleFlags
    {
        /// <summary>
        /// No flags specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Currently require re-obtaining playlist.
        /// </summary>
        RequirePlaylistRetrieval = 1 << 0
    }

    /// <summary>
    /// Handles HTTP request exception.
    /// </summary>
    /// <param name="exception">Original exception.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="AggregateException">Thrown when handling exception failed.</exception>
    protected virtual async Task<FailureHandleFlags> HandleRequestExceptionAsync(ArtHttpResponseMessageException exception, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref ConsecutiveFailCounter);
        Interlocked.Increment(ref TotalFailCounter);
        Context.Tool.LogInformation("HTTP error encountered", exception.ToString());
        switch (exception.StatusCode)
        {
            case HttpStatusCode.Forbidden: // 403
                // ReSharper disable DuplicatedStatements
                await GetRecoveryCallbackOrThrow(exception)(exception).ConfigureAwait(false);
                return FailureHandleFlags.RequirePlaylistRetrieval;
            // ReSharper restore DuplicatedStatements
            case HttpStatusCode.InternalServerError: // 500
                ThrowForExceedTotalRetries(exception);
                ThrowForExceedConsecutiveRetries(exception);
                await DelayOrThrowAsync(exception, Context.ResolvedTiming.Http500RetryDelay, null, cancellationToken).ConfigureAwait(false);
                return FailureHandleFlags.None;
            case HttpStatusCode.ServiceUnavailable: // 503
                ThrowForExceedTotalRetries(exception);
                ThrowForExceedConsecutiveRetries(exception);
                await DelayOrThrowAsync(exception, Context.ResolvedTiming.Http503RetryDelay, null, cancellationToken).ConfigureAwait(false);
                return FailureHandleFlags.None;
        }
        await GetRecoveryCallbackOrThrow(exception)(exception).ConfigureAwait(false);
        return FailureHandleFlags.RequirePlaylistRetrieval;
    }

    private void ThrowForExceedTotalRetries(Exception exception)
    {
        int? maxTotalRetries = Context.Config.MaxTotalRetries;
        if (TotalFailCounter > maxTotalRetries) throw new AggregateException($"Encountered {TotalFailCounter} total failures (threshold is {maxTotalRetries})", exception);
        if (maxTotalRetries is { } maxTotalRetriesValue)
        {
            Context.Tool.LogInformation($"Now at {TotalFailCounter} total failures of {maxTotalRetriesValue} allowed");
        }
    }

    private void ThrowForExceedConsecutiveRetries(Exception exception)
    {
        int? maxConsecutiveRetries = Context.Config.MaxConsecutiveRetries;
        if (ConsecutiveFailCounter > maxConsecutiveRetries) throw new AggregateException($"Encountered {ConsecutiveFailCounter} consecutive failures (threshold is {maxConsecutiveRetries})", exception);
        if (maxConsecutiveRetries is { } maxConsecutiveRetriesValue)
        {
            Context.Tool.LogInformation($"Now at {ConsecutiveFailCounter} consecutive failures of {maxConsecutiveRetriesValue} allowed");
        }
    }

    private Func<Exception, Task> GetRecoveryCallbackOrThrow(Exception exception)
    {
        if (RecoveryCallback == null) throw new AggregateException("No recovery callback implemented", exception);
        return RecoveryCallback;
    }

    private static async Task DelayOrThrowAsync(ArtHttpResponseMessageException exception, TimeSpan? delay, ArtHttpResponseMessageException? responseMessageException, CancellationToken cancellationToken)
    {
        TimeSpan? delayMake = delay ?? responseMessageException?.RetryCondition?.Delta;
        if (delayMake is not { } delayActual) throw new AggregateException($"No retry delay specified for HTTP response {exception.StatusCode?.ToString() ?? "<unknown>"} and no default value provided", exception);
        await Task.Delay(delayActual, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Processes a playlist.
    /// </summary>
    /// <param name="oneOff">If true, complete after one pass through playlist.</param>
    /// <param name="timeout">Timeout to use to determine when a stream seems to have ended.</param>
    /// <param name="playlistElementProcessor">Processor to handle playlist elements.</param>
    /// <param name="segmentFilter">Optional segment filter.</param>
    /// <param name="extraOperation">Optional extra operation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    protected async Task ProcessPlaylistAsync(bool oneOff, TimeSpan timeout, IPlaylistElementProcessor playlistElementProcessor, Func<Uri, SegmentSettings>? segmentFilter, IExtraSaverOperation? extraOperation = null, CancellationToken cancellationToken = default)
    {
        extraOperation?.Reset();
        IOperationProgressContext? operationProgressContext = null;
        try
        {
            ConsecutiveFailCounter = 0;
            TotalFailCounter = 0;
            HashSet<string> hs = new();
            Stopwatch sw = new();
            sw.Start();
            TimeSpan remainingTimeout = timeout;
            while (true)
            {
                FailureHandleFlags failureHandleFlags = FailureHandleFlags.None;
                if (HeartbeatCallback != null)
                {
                    await HeartbeatCallback().ConfigureAwait(false);
                }
                if (operationProgressContext == null)
                {
                    string message = "Waiting for new segments";
                    IOperationProgressContext? context;
                    if (Context.IsConcurrent
                            ? Context.Tool.LogHandler?.TryGetConcurrentOperationProgressContext(message, s_operationWaitingForResult, out context) ?? false
                            : Context.Tool.LogHandler?.TryGetOperationProgressContext(message, s_operationWaitingForResult, out context) ?? false)
                    {
                        operationProgressContext = context;
                    }
                    else
                    {
                        operationProgressContext = null;
                    }
                }
                if (operationProgressContext != null)
                {
                    operationProgressContext.Report(Math.Clamp(1.0f - (float)remainingTimeout.Divide(timeout), 0.0f, 1.0f));
                }
                else
                {
                    if (!Context.DisableWaitingLog)
                    {
                        Context.Tool.LogInformation($"[{Context.Name}] Waiting up to {remainingTimeout.TotalSeconds:F3}s for new segments...");
                    }
                }
                M3UFile m3;
                while (true)
                {
                    try
                    {
                        m3 = await Context.GetAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    }
                    catch (ArtHttpResponseMessageException e)
                    {
                        if (operationProgressContext != null)
                        {
                            operationProgressContext.Dispose();
                            operationProgressContext = null;
                        }
                        await HandleRequestExceptionAsync(e, cancellationToken).ConfigureAwait(false);
                        sw.Restart();
                    }
                }
                if (Context.StreamInfo.EncryptionInfo is { Encrypted: true } ei && m3.EncryptionInfo is { Encrypted: true } ei2 && ei.Method == ei2.Method)
                {
                    ei2.Key ??= ei.Key; // assume key kept if it was supplied in the first place
                    ei2.Iv ??= ei.Iv; // assume IV kept if it was supplied in the first place
                }
                //Context.Tool.LogInformation($"{m3.DataLines.Count} segments...");
                int i = 0, j = 0;
                foreach (string entry in m3.DataLines)
                {
                    long msn = m3.FirstMediaSequenceNumber + i++;
                    var entryUri = new Uri(Context.MainUri, entry);
                    SegmentSettings? segmentSettings = null;
                    if (segmentFilter != null)
                    {
                        segmentSettings = segmentFilter.Invoke(entryUri);
                        if (segmentSettings.Skip)
                        {
                            continue;
                        }
                    }
                    // source could possibly be wonky and use query to differentiate?
                    string entryKey = entry; //entryUri.Segments[^1];
                    if (hs.Contains(entryKey))
                    {
                        continue;
                    }
                    if (operationProgressContext != null)
                    {
                        operationProgressContext.MarkSafe();
                        operationProgressContext.Dispose();
                        operationProgressContext = null;
                    }
                    while (true)
                    {
                        try
                        {
                            await playlistElementProcessor.ProcessPlaylistElementAsync(entryUri, m3, msn, segmentSettings, entry, new ItemNo(i, m3.DataLines.Count), cancellationToken).ConfigureAwait(false);
                            break;
                        }
                        catch (ArtHttpResponseMessageException e)
                        {
                            if (operationProgressContext != null)
                            {
                                operationProgressContext.Dispose();
                                operationProgressContext = null;
                            }
                            failureHandleFlags = await HandleRequestExceptionAsync(e, cancellationToken).ConfigureAwait(false);
                            sw.Restart();
                            // cascade
                            if ((failureHandleFlags & FailureHandleFlags.RequirePlaylistRetrieval) != 0)
                            {
                                break;
                            }
                        }
                    }
                    // cascade
                    if ((failureHandleFlags & FailureHandleFlags.RequirePlaylistRetrieval) != 0)
                    {
                        break;
                    }
                    ConsecutiveFailCounter = 0;
                    hs.Add(entryKey);
                    j++;
                }
                // cascade
                if ((failureHandleFlags & FailureHandleFlags.RequirePlaylistRetrieval) != 0)
                {
                    Context.Tool.LogInformation("Re-retrieving playlist as requested by error handling...");
                    // this will re-retrieve even if one-off, which is intended
                    continue;
                }
                if (j != 0)
                {
                    sw.Restart();
                    remainingTimeout = timeout;
                }
                else if (sw.IsRunning)
                {
                    if (extraOperation != null)
                    {
                        try
                        {
                            Context.Tool.LogInformation("No new segments, executing extra operation...");
                            bool shouldContinue = await extraOperation.TickAsync(m3, cancellationToken).ConfigureAwait(false);
                            if (!shouldContinue)
                            {
                                extraOperation = null;
                            }
                        }
                        catch (Exception e)
                        {
                            Context.Tool.LogError(e.Message, e.ToString());
                            extraOperation = null;
                        }
                    }
                    if (extraOperation != null)
                    {
                        sw.Restart();
                        remainingTimeout = timeout;
                    }
                    else
                    {
                        var elapsed = sw.Elapsed;
                        if (elapsed >= timeout)
                        {
                            if (operationProgressContext != null)
                            {
                                operationProgressContext.MarkSafe();
                                operationProgressContext.Dispose();
                                operationProgressContext = null;
                            }
                            Context.Tool.LogInformation($"No new entries for timeout {timeout}, stopping");
                            return;
                        }
                        remainingTimeout = timeout.Subtract(elapsed);
                    }
                }
                else
                {
                    if (operationProgressContext != null)
                    {
                        operationProgressContext.MarkSafe();
                        operationProgressContext.Dispose();
                        operationProgressContext = null;
                    }
                    Context.Tool.LogError("Timer stopped running (error?)");
                    return;
                }
                if (oneOff) break;
                await Task.Delay(s_playlistDelay, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            if (operationProgressContext != null)
            {
                operationProgressContext.Dispose();
            }
        }
    }
}
