using System.Collections.Frozen;
using System.Text;

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

    private readonly string _message;

    /// <summary>
    /// Initializes an instance of <see cref="BrowserProfileNotFoundException"/>.
    /// </summary>
    /// <param name="browserName">Browser name.</param>
    /// <param name="profile">Browser profile name.</param>
    /// <param name="potentialProfiles">Potential profiles for submission.</param>
    public BrowserProfileNotFoundException(string browserName, string profile, FrozenSet<string> potentialProfiles)
    {
        BrowserName = browserName;
        Profile = profile;
        var sb = new StringBuilder($"Profile \"{Profile}\" for the browser \"{BrowserName}\" was not found.");
        if (potentialProfiles.Count > 0)
        {
            sb.Append(" Potential profiles: ").AppendJoin(", ", potentialProfiles.Select(static v => $"[{v}]"));
        }
        _message = sb.ToString();
    }

    /// <inheritdoc />
    public override string Message => _message;
}
