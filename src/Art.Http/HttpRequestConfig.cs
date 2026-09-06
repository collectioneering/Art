using System.Net;

namespace Art.Http;

/// <summary>
/// Configuration for a <see cref="HttpRequestMessage"/> sent via <see cref="HttpClient"/>.
/// </summary>
/// <param name="Origin">Request origin.</param>
/// <param name="Referrer">Request referrer.</param>
/// <param name="RequestAction">Custom configuration callback for the <see cref="HttpRequestMessage"/> created.</param>
public record HttpRequestConfig(
    string? Origin = null,
    string? Referrer = null,
    Action<HttpRequestMessage>? RequestAction = null);
