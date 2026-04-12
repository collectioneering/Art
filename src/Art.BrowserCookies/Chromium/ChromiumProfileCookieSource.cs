using System.Collections.Frozen;
using System.Text.Json;
using Art.Extensions.BrowserCookies;

namespace Art.BrowserCookies.Chromium;

/// <summary>
/// Represents a <see cref="ChromiumCookieSource"/> with an associated profile.
/// </summary>
public abstract record ChromiumProfileCookieSource(string Profile = "Default") : ChromiumCookieSource
{
    /// <inheritdoc />
    public override ChromiumProfileCookieSource Resolve()
    {
        string path = GetPath(UserDataKind.Cookies);
        try
        {
            if (!File.Exists(path))
            {
                throw new BrowserProfileNotFoundException(Name, Profile, GetPotentialProfileNames().ToFrozenSet());
            }
            return this;
        }
        catch
        {
            foreach (string profileDirectory in Directory.EnumerateDirectories(GetUserDataDirectory()))
            {
                string newProfile = Path.GetFileName(profileDirectory);
                string preferences = GetPath(UserDataKind.Preferences, newProfile);
                if (File.Exists(preferences))
                {
                    using var fs = File.OpenRead(preferences);
                    string name = (JsonSerializer.Deserialize(fs, SourceGenerationContext.s_context.ChromiumPreferences) ?? throw new InvalidDataException()).Profile.Name;
                    if (name.Equals(Profile, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return this with { Profile = newProfile };
                    }
                }
            }
            throw;
        }
    }

    private List<string> GetPotentialProfileNames()
    {
        List<string> potentialProfileNames = [];
        foreach (string profileDirectory in Directory.EnumerateDirectories(GetUserDataDirectory()))
        {
            string newProfile = Path.GetFileName(profileDirectory);
            string preferences = GetPath(UserDataKind.Preferences, newProfile);
            if (File.Exists(preferences))
            {
                try
                {
                    using var fs = File.OpenRead(preferences);
                    string name = (JsonSerializer.Deserialize(fs, SourceGenerationContext.s_context.ChromiumPreferences) ?? throw new InvalidDataException()).Profile.Name;
                    potentialProfileNames.Add(newProfile);
                    potentialProfileNames.Add(name);
                }
                catch
                {
                    // ignored
                }
            }
        }
        return potentialProfileNames;
    }

    /// <summary>
    /// Gets a path with the specified configuration.
    /// </summary>
    /// <param name="kind">Kind of path to get.</param>
    /// <param name="profile">Profile.</param>
    /// <returns>Retrieved path.</returns>
    protected abstract string GetPath(UserDataKind kind, string profile);

    /// <inheritdoc />
    protected override string GetPath(UserDataKind kind)
    {
        return GetPath(kind, Profile);
    }
}
