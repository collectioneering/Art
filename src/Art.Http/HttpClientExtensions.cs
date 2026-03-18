namespace Art.Http;

internal static class HttpClientExtensions
{
    internal static async Task<HttpResponseMessage> SendAsync(this HttpClient httpClient, HttpRequestMessage httpRequestMessage, HttpCompletionOption defaultCompletionOption, HttpRequestConfig? httpRequestConfig, CancellationToken cancellationToken = default)
    {
        if (httpRequestConfig != null)
        {
            httpRequestMessage.SetOriginAndReferrer(httpRequestConfig.Origin, httpRequestConfig.Referrer);
            httpRequestConfig.RequestAction?.Invoke(httpRequestMessage);
            if (httpRequestConfig.Timeout is { } timeout)
            {
                using var cts = new CancellationTokenSource(timeout);
                using var lcts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var localCancellationToken = lcts.Token;
                try
                {
                    return await httpClient.SendAsync(httpRequestMessage, httpRequestConfig.HttpCompletionOption ?? defaultCompletionOption, localCancellationToken).ConfigureAwait(false);
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
            return await httpClient.SendAsync(httpRequestMessage, httpRequestConfig.HttpCompletionOption ?? defaultCompletionOption, cancellationToken).ConfigureAwait(false);
        }
        return await httpClient.SendAsync(httpRequestMessage, defaultCompletionOption, cancellationToken).ConfigureAwait(false);
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
