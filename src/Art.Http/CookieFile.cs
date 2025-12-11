using System.Globalization;
using System.Net;

namespace Art.Http;

/// <summary>
/// Utility to work with cookie files.
/// </summary>
public static class CookieFile
{
    /// <summary>
    /// Loads a cookie file into a cookie container.
    /// </summary>
    /// <param name="cc">Cookie container to operate on.</param>
    /// <param name="tr">Source text of cookie file.</param>
    /// <exception cref="InvalidDataException">Thrown if cookie file was in an unexpected format.</exception>
    public static void LoadCookieFile(this CookieContainer cc, TextReader tr)
    {
        int i = 0;
        while (tr.ReadLine() is { } line)
        {
            i++;
            AddLine(cc, line, i);
        }
    }

    /// <summary>
    /// Loads a cookie file into a cookie container.
    /// </summary>
    /// <param name="cc">Cookie container to operate on.</param>
    /// <param name="tr">Source text of cookie file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="InvalidDataException">Thrown if cookie file was in an unexpected format.</exception>
    public static async Task LoadCookieFileAsync(this CookieContainer cc, TextReader tr, CancellationToken cancellationToken = default)
    {
        int i = 0;
        while (await tr.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
        {
            i++;
            AddLine(cc, line, i);
        }
    }

    private static void AddLine(CookieContainer cc, string line, int i)
    {
        string[] elem = line.Split(['\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (elem.Length == 0 || elem[0].StartsWith('#')) return;
        if (elem.Length < 6 || elem.Length > 7) throw new InvalidDataException($"Line {i} had invalid number of elements {elem.Length}");
        string domain = elem[0];
        //bool access = elem[1].Equals("true", StringComparison.InvariantCultureIgnoreCase);
        string path = elem[2];
        bool secure = elem[3].Equals("true", StringComparison.InvariantCultureIgnoreCase);
        long expiration = long.Parse(elem[4], CultureInfo.InvariantCulture);
        string name = elem[5];
        string? value = elem.Length < 7 ? null : elem[6];
        // TODO expiration 0?
        DateTime expires = expiration == 0 ? DateTime.MaxValue : DateTime.UnixEpoch.AddSeconds(expiration);
        cc.Add(new Cookie
        {
            Expires = expires,
            Secure = secure,
            Name = name,
            Value = value,
            Path = path,
            Domain = domain
        });
    }
}
