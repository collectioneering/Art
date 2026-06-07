using Art.Extensions.BrowserCookies;

namespace Art.BrowserCookies;

/// <summary>
/// Utility to manage cookie filters.
/// </summary>
public static class CookieFilterUtility
{
    /// <summary>
    /// Validates this instance.
    /// </summary>
    /// <param name="cookieFilter">Instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown for invalid value.</exception>
    public static void Validate(this in CookieFilter cookieFilter)
    {
        if (cookieFilter.Domain.StartsWith('.'))
        {
            throw new ArgumentException("Domain for cookie filter should not start with leading '.'");
        }
    }

}
