using Art.BrowserCookies.Util;

namespace Art.BrowserCookies.Chromium;

/// <summary>
/// Represents a <see cref="CookieSource"/> for the Microsoft Edge web browser.
/// </summary>
/// <param name="Profile">Profile name.</param>
public record EdgeCookieSource(string Profile = "Default") : ChromiumProfileCookieSource(Profile), IPlatformSupportCheck, ICookieSourceFactory
{
    /// <inheritdoc />
    public override string Name => "Edge";

    /// <inheritdoc />
    protected override string GetUserDataDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft/Edge/User Data");
        }
        if (OperatingSystem.IsMacOS())
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Application Support/Microsoft Edge");
        }
        if (OperatingSystem.IsLinux())
        {
            return Path.Combine(PathUtil.GetXdgConfigHomeOrFallback(), "microsoft-edge");
        }
        throw new PlatformNotSupportedException();
    }

    /// <inheritdoc />
    protected override string GetPath(UserDataKind kind, string profile)
    {
        string userDataPath = GetUserDataDirectory();
        if (OperatingSystem.IsWindows())
        {
            return kind switch
            {
                UserDataKind.Cookies => Path.Combine(userDataPath, profile, "Network/Cookies"),
                UserDataKind.Preferences => Path.Combine(userDataPath, profile, "Preferences"),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }
        if (OperatingSystem.IsMacOS())
        {
            return kind switch
            {
                UserDataKind.Cookies => Path.Combine(userDataPath, profile, "Cookies"),
                UserDataKind.Preferences => Path.Combine(userDataPath, profile, "Preferences"),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }
        if (OperatingSystem.IsLinux())
        {
            return kind switch
            {
                UserDataKind.Cookies => Path.Combine(userDataPath, profile, "Cookies"),
                UserDataKind.Preferences => Path.Combine(userDataPath, profile, "Preferences"),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }
        throw new PlatformNotSupportedException();
    }

    /// <inheritdoc />
    protected override IChromiumKeychain GetKeychain(IToolLogHandler? toolLogHandler)
    {
        if (OperatingSystem.IsWindows())
        {
            return ChromiumKeychainUtil.GetWindowsKeychain(ChromiumVariant.Edge, GetUserDataDirectory(), toolLogHandler);
        }
        if (OperatingSystem.IsMacOS())
        {
            return ChromiumKeychainUtil.GetMacosKeychain("Microsoft Edge Safe Storage");
        }
        if (OperatingSystem.IsLinux())
        {
            throw new NotImplementedException();
        }
        throw new PlatformNotSupportedException();
    }

    /// <inheritdoc />
    protected override Task<IChromiumKeychain> GetKeychainAsync(IToolLogHandler? toolLogHandler, CancellationToken cancellationToken = default)
    {
        if (OperatingSystem.IsWindows())
        {
            return ChromiumKeychainUtil.GetWindowsKeychainAsync(ChromiumVariant.Edge, GetUserDataDirectory(), toolLogHandler, cancellationToken);
        }
        if (OperatingSystem.IsMacOS())
        {
            return ChromiumKeychainUtil.GetMacosKeychainAsync("Microsoft Edge Safe Storage", cancellationToken);
        }
        if (OperatingSystem.IsLinux())
        {
            throw new NotImplementedException();
        }
        throw new PlatformNotSupportedException();
    }

    static bool IPlatformSupportCheck.IsSupported
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return true;
            }
            if (OperatingSystem.IsMacOS())
            {
                return true;
            }
            return false;
        }
    }

    static CookieSource ICookieSourceFactory.CreateCookieSource(string? profile)
    {
        return profile != null ? new EdgeCookieSource(profile) : new EdgeCookieSource();
    }
}
