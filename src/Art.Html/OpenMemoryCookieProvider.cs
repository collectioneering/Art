/*
 * This code is based on the MIT-licensed code of AngleSharp
 * Copyright (c) 2013 - 2021 AngleSharp
 * 
 * Retrieved MemoryCookieProvider@5308259
 * 
 * Modifications:
 * - Created distinct type in different namespace
 * - Added suppression attributes to preserve original code style
 * - Opened parameter to allow injection of custom cookie container
 */

using AngleSharp.Dom;
using AngleSharp.Io;
using System.Diagnostics;
using System.Globalization;
using System.Net;

namespace Art.Html;

/// <summary>
/// Represents the default cookie service. This class can be inherited.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0032:Use auto property", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0049:Simplify Names", Justification = "<Pending>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0008:Use explicit type", Justification = "<Pending>")]
public class OpenMemoryCookieProvider : ICookieProvider
{
    private readonly CookieContainer _container;

    /// <summary>
    /// Creates a new cookie service for non-persistent cookies.
    /// </summary>
    /// <param name="cookieContainer">Cookie container to be used by this provider.</param>
    public OpenMemoryCookieProvider(CookieContainer cookieContainer)
    {
        _container = cookieContainer;
    }

    /// <summary>
    /// Gets the associated cookie container.
    /// </summary>
    public CookieContainer Container => _container;


    /// <summary>
    /// Gets the cookie value of the given address.
    /// </summary>
    /// <param name="url">The origin of the cookie.</param>
    /// <returns>The value of the cookie.</returns>
    public String GetCookie(Url url)
    {
        return _container.GetCookieHeader(url);
    }

    /// <summary>
    /// Sets the cookie value for the given address.
    /// </summary>
    /// <param name="url">The origin of the cookie.</param>
    /// <param name="value">The value of the cookie.</param>
    public void SetCookie(Url url, String value)
    {
        var cookies = Sanatize(url.HostName, value);

        try
        {
            _container.SetCookies(url, cookies);
        }
        catch (CookieException ex)
        {
            Debug.WriteLine("Cookie exception, see {0} for details.", ex);
        }
    }

    private static String Sanatize(String host, String cookie)
    {
        var expires = "expires=";
        var domain = String.Concat("Domain=", host, ";");
        var start = 0;

        while (start < cookie.Length)
        {
            var index = cookie.IndexOf(expires, start, StringComparison.OrdinalIgnoreCase);

            if (index != -1)
            {
                var position = index + expires.Length;
                var end = cookie.IndexOfAny([';', ','], position + 4);

                if (end == -1)
                {
                    end = cookie.Length;
                }

                var front = cookie.Substring(0, position);
                var middle = cookie.Substring(position, end - position);
                var back = cookie.Substring(end);

                if (DateTime.TryParse(middle.Replace("UTC", "GMT"), out var utc))
                {
                    var time = utc.ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    cookie = $"{front}{time}{back}";
                }

                start = end;
            }
            else
            {
                break;
            }
        }

        return cookie.Replace(domain, String.Empty);
    }
}