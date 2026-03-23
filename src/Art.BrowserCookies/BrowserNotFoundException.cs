namespace Art.BrowserCookies;

/// <summary>
/// Exception thrown when a browser was not found.
/// </summary>
public class BrowserNotFoundException : BrowserLookupConfigException
{
    /// <summary>
    /// Browser name.
    /// </summary>
    public string BrowserName { get; }

    /// <summary>
    /// Initializes an instance of <see cref="BrowserNotFoundException"/>.
    /// </summary>
    /// <param name="browserName">Browser name.</param>
    public BrowserNotFoundException(string browserName)
    {
        BrowserName = browserName;
    }

    /// <inheritdoc />
    public override string Message => $"Browser \"{BrowserName}\" was not found.";
}
