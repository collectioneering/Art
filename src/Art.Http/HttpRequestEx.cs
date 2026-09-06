namespace Art.Http;

/// <summary>
/// Provides inputs for a request on a <see cref="HttpClient"/>.
/// </summary>
/// <param name="HttpRequestMessage">Request message.</param>
/// <param name="HttpRequestConfig">Request config.</param>
public record HttpRequestEx(HttpRequestMessage HttpRequestMessage, HttpRequestConfig? HttpRequestConfig);
