using System.Net;

namespace Art.Http;

/// <summary>
/// Contains methods for failure bypass.
/// </summary>
public static class FailureBypass
{
    /// <summary>
    /// Evaluates whether the exception should be ignored based on ignore flags.
    /// </summary>
    /// <param name="exception">Exception.</param>
    /// <param name="flags">Flags specifying types of errors to ignore.</param>
    /// <returns>True if exception should be ignored.</returns>
    public static bool ShouldBypass(Exception exception, FailureFlags flags) => (FilterFlags(exception) & flags) != 0;

    /// <summary>
    /// Creates exception filter using the specified filter flags.
    /// </summary>
    /// <param name="flags">Flags specifying types of errors to ignore.</param>
    /// <returns>True if exception should be ignored.</returns>
    public static Func<Exception, bool> CreateFilter(FailureFlags flags) => exception => (FilterFlags(exception) & flags) != 0;

    private static FailureFlags FilterFlags(Exception exception) =>
        exception switch
        {
            AggregateException a => a.InnerExceptions.Aggregate(FailureFlags.None, (f, e) => f | FilterFlags(e)),
            GeoblockingException => FailureFlags.Geoblocking,
            AccessDeniedException => FailureFlags.AccessDenied,
            MaintenanceException => FailureFlags.Maintenance,
            HttpRequestException httpRequestException => FilterHttpFlags(httpRequestException.StatusCode),
            ArtHttpResponseMessageException exHttpResponseMessageException => FilterHttpFlags(exHttpResponseMessageException.StatusCode),
            IOException => FailureFlags.IO,
            _ => FailureFlags.Miscellaneous
        };

    private static FailureFlags FilterHttpFlags(HttpStatusCode? statusCode) =>
        statusCode switch
        {
            HttpStatusCode.Forbidden => FailureFlags.AccessDenied,
            _ => FailureFlags.Network
        };
}
