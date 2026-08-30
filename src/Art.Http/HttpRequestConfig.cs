using System.Net;

namespace Art.Http;

/// <summary>
/// Configuration for a <see cref="HttpRequestMessage"/> sent via <see cref="HttpClient"/>.
/// </summary>
/// <param name="Origin">Request origin.</param>
/// <param name="Referrer">Request referrer.</param>
/// <param name="RequestAction">Custom configuration callback for the <see cref="HttpRequestMessage"/> created.</param>
/// <param name="HttpCompletionOption">Custom <see cref="System.Net.Http.HttpCompletionOption"/>.</param>
/// <param name="RetryCount">Number of retries for <see cref="HttpStatusCode.TooManyRequests"/>.</param>
/// <param name="RetryTime">Fallback or override time for <see cref="HttpStatusCode.TooManyRequests"/>, depending on <see cref="OverrideRetryTime"/>.</param>
/// <param name="OverrideRetryTime">Override wait time specified in response for <see cref="HttpStatusCode.TooManyRequests"/>.</param>
/// <param name="Timeout">Custom timeout to apply to request.</param>
public record HttpRequestConfig(
    string? Origin = null,
    string? Referrer = null,
    Action<HttpRequestMessage>? RequestAction = null,
    HttpCompletionOption? HttpCompletionOption = null,
    int? RetryCount = null,
    TimeSpan? RetryTime = null,
    bool OverrideRetryTime = false,
    TimeSpan? Timeout = null);
