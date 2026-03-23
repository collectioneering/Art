namespace Art.BrowserCookies;

/// <summary>
/// Exception thrown when a browser profile was not found.
/// </summary>
public class BrowserProfileNotFoundException : BrowserLookupConfigException
{
    /// <summary>
    /// Browser name.
    /// </summary>
    public string BrowserName { get; }

    /// <summary>
    /// Browser profile name.
    /// </summary>
    public string Profile { get; }

    /// <summary>
    /// Initializes an instance of <see cref="BrowserProfileNotFoundException"/>.
    /// </summary>
    /// <param name="browserName">Browser name.</param>
    /// <param name="profile">Browser profile name.</param>
    public BrowserProfileNotFoundException(string browserName, string profile)
    {
        BrowserName = browserName;
        Profile = profile;
    }

    /// <inheritdoc />
    public override string Message => $"Profile \"{Profile}\" for the browser \"{BrowserName}\" was not found.";
}
