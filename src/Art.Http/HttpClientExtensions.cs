using System.Net;

namespace Art.Http;

/// <summary>
/// Provides extensions for <see cref="HttpClient"/>.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Sends an HTTP request via a <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClient">Client to use to send request.</param>
    /// <param name="requestDelegate">Delegate to create instance of <see cref="HttpRequestMessage"/> and, if applicable, a <see cref="HttpRequestConfig"/>.</param>
    /// <param name="defaultCompletionOption">Completion option to use if not specified in <see cref="HttpRequestConfig"/> returned by <paramref name="requestDelegate"/>.</param>
    /// <param name="requestMetaConfig">Metaconfiguration to apply for request.</param>
    /// <param name="cancellationToken">Cancellation token, if applicable.</param>
    /// <returns>An instance of <see cref="HttpResponseMessage"/> representing the response sent by the host.</returns>
    /// <exception cref="TaskCanceledException">Thrown if HTTP request has timed out, following the semantics of <see cref="HttpClient"/>.</exception>
    public static async Task<HttpResponseMessage> SendAsync(
        this HttpClient httpClient,
        Func<HttpRequestEx> requestDelegate,
        HttpCompletionOption defaultCompletionOption,
        HttpRequestMetaConfig? requestMetaConfig,
        CancellationToken cancellationToken = default)
    {
        RetryConfig retryConfig = requestMetaConfig != null ? new RetryConfig(RetryCount: requestMetaConfig.RetryCount, RetryTime: requestMetaConfig.RetryTime, OverrideRetryTime: requestMetaConfig.OverrideRetryTime) : new RetryConfig();
        if (requestMetaConfig != null)
        {
            if (requestMetaConfig.Timeout is { } timeout)
            {
                using var cts = new CancellationTokenSource(timeout);
                using var lcts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var localCancellationToken = lcts.Token;
                try
                {
                    return await SendWithRetryAsync(httpClient, CreateRequest, requestMetaConfig.HttpCompletionOption ?? defaultCompletionOption, retryConfig, localCancellationToken).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    // HttpClient can timeout, on .NET 5 this will be InnerException TimeoutException
                    // Prioritize the passed cancellation token
                    cancellationToken.ThrowIfCancellationRequested();
                    // Cancel from local timeout if applicable, using same TaskCanceledException->TimeoutException semantics as HttpClient
                    try
                    {
                        ThrowForTimeout(timeout, localCancellationToken);
                    }
                    catch (Exception e)
                    {
                        throw new TaskCanceledException("An HTTP request timed out.", e);
                    }
                    // Fallback to the inner exception
                    throw;
                }
            }
            return await SendWithRetryAsync(httpClient, CreateRequest, requestMetaConfig.HttpCompletionOption ?? defaultCompletionOption, retryConfig, cancellationToken).ConfigureAwait(false);
        }
        return await SendWithRetryAsync(httpClient, CreateRequest, defaultCompletionOption, retryConfig, cancellationToken).ConfigureAwait(false);

        HttpRequestMessage CreateRequest()
        {
            (HttpRequestMessage httpRequestMessage, HttpRequestConfig? httpRequestConfig) = requestDelegate();
            if (httpRequestConfig != null)
            {
                httpRequestMessage.SetOriginAndReferrer(httpRequestConfig.Origin, httpRequestConfig.Referrer);
                httpRequestConfig.RequestAction?.Invoke(httpRequestMessage);
            }
            return httpRequestMessage;
        }
    }

    private record struct RetryConfig(int? RetryCount = null, TimeSpan? RetryTime = null, bool OverrideRetryTime = false);

    private static async Task<HttpResponseMessage> SendWithRetryAsync(
        HttpClient httpClient,
        Func<HttpRequestMessage> requestDelegate,
        HttpCompletionOption completionOption,
        RetryConfig retryConfig,
        CancellationToken cancellationToken)
    {
        int? remainingRetries = retryConfig.RetryCount;
        while (true)
        {
            var response = await httpClient.SendAsync(requestDelegate(), completionOption, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                if (remainingRetries is not ({ } remainingRetriesValue and > 0))
                {
                    string message = retryConfig.RetryCount is { } originalRetryCount
                        ? $"Response indicates too many requests, and {originalRetryCount} retries were exhausted"
                        : "Response indicates too many requests, and no retries are configured";
                    throw new ArtHttpResponseMessageException(message, response);
                }
                TimeSpan timeSpan = (retryConfig.OverrideRetryTime ? retryConfig.RetryTime : null) ?? response.Headers.RetryAfter?.Delta ?? retryConfig.RetryTime ?? TimeSpan.FromSeconds(1);
                await Task.Delay(timeSpan, cancellationToken).ConfigureAwait(false);
                remainingRetries = remainingRetriesValue - 1;
                continue;
            }
            return response;
        }
    }

    private static void ThrowForTimeout(TimeSpan timeSpan, CancellationToken cancellationToken)
    {
        // This has to throw TimeoutException, for matching TaskCanceledException->TimeoutException
        if (cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"A request timed out after {timeSpan.TotalSeconds} seconds.");
        }
    }
}
