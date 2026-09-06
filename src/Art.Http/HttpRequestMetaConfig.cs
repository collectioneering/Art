using System.Net;

namespace Art.Http;

/// <summary>
/// Metaconfiguration for a <see cref="HttpRequestMessage"/> sent via <see cref="HttpClient"/>.
/// </summary>
/// <param name="HttpCompletionOption">Custom <see cref="System.Net.Http.HttpCompletionOption"/>.</param>
/// <param name="RetryCount">Number of retries for <see cref="HttpStatusCode.TooManyRequests"/>.</param>
/// <param name="RetryTime">Fallback or override time for <see cref="HttpStatusCode.TooManyRequests"/>, depending on <see cref="OverrideRetryTime"/>.</param>
/// <param name="OverrideRetryTime">Override wait time specified in response for <see cref="HttpStatusCode.TooManyRequests"/>.</param>
/// <param name="Timeout">Custom timeout to apply to request.</param>
public record HttpRequestMetaConfig(
    HttpCompletionOption? HttpCompletionOption = null,
    int? RetryCount = null,
    TimeSpan? RetryTime = null,
    bool OverrideRetryTime = false,
    TimeSpan? Timeout = null);
