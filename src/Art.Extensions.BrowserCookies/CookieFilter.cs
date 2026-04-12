namespace Art.Extensions.BrowserCookies;

/// <summary>
/// Represents a search filter for cookies.
/// </summary>
/// <param name="Domain">Primary domain.</param>
/// <param name="IncludeSubdomains">Include subdomains.</param>
public record struct CookieFilter(string Domain, bool IncludeSubdomains = true)
{
    /// <summary>
    /// Validates this instance.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown for invalid value.</exception>
    public void Validate()
    {
        if (Domain.StartsWith('.'))
        {
            throw new ArgumentException("Domain for cookie filter should not start with leading '.'");
        }
    }

    /// <summary>
    /// Provides implicit conversion from <see cref="System.String"/> to <see cref="CookieFilter"/>.
    /// </summary>
    /// <param name="domain">Primary domain.</param>
    /// <returns>Search filter.</returns>
    public static implicit operator CookieFilter(string domain) => new(domain);
}
