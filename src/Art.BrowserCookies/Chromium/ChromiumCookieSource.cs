using System.Net;
using Art.Extensions.BrowserCookies;
using Microsoft.Data.Sqlite;

namespace Art.BrowserCookies.Chromium;

/// <summary>
/// Represents a <see cref="CookieSource"/> for a Chromium-based browser.
/// </summary>
public abstract record ChromiumCookieSource : CookieSource
{
    /// <inheritdoc />
    public override void LoadCookies(CookieContainer cookieContainer, CookieFilter domain, LogHandler? logHandler)
    {
        LoadCookies(cookieContainer, [domain], logHandler);
    }

    /// <inheritdoc />
    public override Task LoadCookiesAsync(CookieContainer cookieContainer, CookieFilter domain, LogHandler? logHandler = null, CancellationToken cancellationToken = default)
    {
        return LoadCookiesAsync(cookieContainer, [domain], logHandler, cancellationToken);
    }

    /// <inheritdoc />
    public override void LoadCookies(CookieContainer cookieContainer, IReadOnlyCollection<CookieFilter> domains, LogHandler? logHandler)
    {
        foreach (var domain in domains)
        {
            domain.Validate();
        }
        IChromiumKeychain? keychain = null;
        try
        {
            // NT epoch
            DateTime expiryBase = new(1601, 1, 1);
            string temp = Path.GetTempFileName();
            try
            {
                File.Copy(sourceFileName: GetPath(UserDataKind.Cookies), destFileName: temp, overwrite: true);
                using var connection = new SqliteConnection($"Data Source={temp};Pooling=False;");
                connection.Open();
                foreach ((string domain, bool includeSubdomains) in domains)
                {
                    var command = connection.CreateCommand();
                    if (includeSubdomains)
                    {
                        command.CommandText = """
                                              SELECT name, "value", encrypted_value, path, expires_utc, is_secure, host_key
                                              FROM cookies
                                              WHERE host_key = $hostKey OR host_key LIKE $dotHostKey
                                              """;
                        command.Parameters.AddWithValue("$dotHostKey", "%." + domain);
                    }
                    else
                    {
                        command.CommandText = """
                                              SELECT name, "value", encrypted_value, path, expires_utc, is_secure, host_key
                                              FROM cookies
                                              WHERE host_key = $hostKey OR host_key = $dotHostKey
                                              """;
                        command.Parameters.AddWithValue("$dotHostKey", "." + domain);
                    }
                    command.Parameters.AddWithValue("$hostKey", domain);
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string value = reader.GetString(1);
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            string cookieDomain = reader.GetString(6);
                            byte[] buf = ReadBytes(reader.GetStream(2));
                            if (buf.Length != 0)
                            {
                                value = (keychain ??= GetKeychain(logHandler)).Unlock(cookieDomain, buf, logHandler);
                            }
                            else
                            {
                                value = "";
                            }
                        }
                        long expiry = reader.GetInt64(4);
                        DateTime expires = expiry == 0 ? DateTime.MinValue : expiryBase.AddMicroseconds(expiry);
                        cookieContainer.Add(new Cookie
                        {
                            Expires = expires,
                            Secure = reader.GetBoolean(5),
                            Name = reader.GetString(0),
                            Value = EscapeValue(value),
                            Path = reader.GetString(3),
                            Domain = reader.GetString(6)
                        });
                    }
                }
            }
            finally
            {
                File.Delete(temp);
            }
        }
        finally
        {
            keychain?.Dispose();
        }
    }

    /// <inheritdoc />
    public override async Task LoadCookiesAsync(CookieContainer cookieContainer, IReadOnlyCollection<CookieFilter> domains, LogHandler? logHandler = null, CancellationToken cancellationToken = default)
    {
        foreach (var domain in domains)
        {
            domain.Validate();
        }
        IChromiumKeychain? keychain = null;
        try
        {
            // NT epoch
            DateTime expiryBase = new(1601, 1, 1);
            string temp = Path.GetTempFileName();
            try
            {
                File.Copy(sourceFileName: GetPath(UserDataKind.Cookies), destFileName: temp, overwrite: true);
                await using var connection = new SqliteConnection($"Data Source={temp};Pooling=False;");
                connection.Open();
                foreach ((string domain, bool includeSubdomains) in domains)
                {
                    var command = connection.CreateCommand();
                    if (includeSubdomains)
                    {
                        command.CommandText = """
                                              SELECT name, "value", encrypted_value, path, expires_utc, is_secure, host_key
                                              FROM cookies
                                              WHERE host_key = $hostKey OR host_key LIKE $dotHostKey
                                              """;
                        command.Parameters.AddWithValue("$dotHostKey", "%." + domain);
                    }
                    else
                    {
                        command.CommandText = """
                                              SELECT name, "value", encrypted_value, path, expires_utc, is_secure, host_key
                                              FROM cookies
                                              WHERE host_key = $hostKey OR host_key = $dotHostKey
                                              """;
                        command.Parameters.AddWithValue("$dotHostKey", "." + domain);
                    }
                    command.Parameters.AddWithValue("$hostKey", domain);
                    await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                    while (reader.Read())
                    {
                        string value = reader.GetString(1);
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            string cookieDomain = reader.GetString(6);
                            byte[] buf = ReadBytes(reader.GetStream(2));
                            if (buf.Length != 0)
                            {
                                value = (keychain ??= await GetKeychainAsync(logHandler, cancellationToken).ConfigureAwait(false)).Unlock(cookieDomain, buf, logHandler);
                            }
                            else
                            {
                                value = "";
                            }
                        }
                        long expiry = reader.GetInt64(4);
                        DateTime expires = expiry == 0 ? DateTime.MinValue : expiryBase.AddMicroseconds(expiry);
                        Cookie cookie = new()
                        {
                            Expires = expires,
                            Secure = reader.GetBoolean(5),
                            Name = reader.GetString(0),
                            Value = EscapeValue(value),
                            Path = reader.GetString(3),
                            Domain = reader.GetString(6)
                        };
                        cookieContainer.Add(cookie);
                    }
                }
            }
            finally
            {
                File.Delete(temp);
            }
        }
        finally
        {
            keychain?.Dispose();
        }
    }

    private static string EscapeValue(string value)
    {
        // rfc6265-nonconforming...
        // https://www.rfc-editor.org/rfc/rfc6265
        if (value.Contains(';') || value.Contains(','))
        {
            return $"\"{value.Replace("\"", "\\\"")}\"";
        }
        return value;
    }

    private static byte[] ReadBytes(Stream stream)
    {
        byte[] buf = new byte[stream.Length];
        MemoryStream ms = new(buf);
        stream.CopyTo(ms);
        return buf;
    }

    /// <summary>
    /// Synchronously gets a keychain accessor corresponding to this browser for the current user.
    /// </summary>
    /// <param name="logHandler">Tool log handler.</param>
    /// <returns>Keychain.</returns>
    protected abstract IChromiumKeychain GetKeychain(LogHandler? logHandler);

    /// <summary>
    /// Gets a keychain accessor corresponding to this browser for the current user.
    /// </summary>
    /// <param name="logHandler">Tool log handler.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning a keychain.</returns>
    protected abstract Task<IChromiumKeychain> GetKeychainAsync(LogHandler? logHandler, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the primary user data directory.
    /// </summary>
    /// <returns>Retrieved path.</returns>
    protected abstract string GetUserDataDirectory();

    /// <summary>
    /// Gets a path with the current configuration.
    /// </summary>
    /// <param name="kind">Kind of path to get.</param>
    /// <returns>Retrieved path.</returns>
    protected abstract string GetPath(UserDataKind kind);

    /// <summary>
    /// Kind of user data.
    /// </summary>
    protected enum UserDataKind
    {
        /// <summary>
        /// Cookie file.
        /// </summary>
        Cookies,

        /// <summary>
        /// Preferences file.
        /// </summary>
        Preferences
    }
}
