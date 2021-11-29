﻿using System.Net;
using AngleSharp.Dom;
using Art.Html;
using Xs;

namespace Art.Xs;

/// <summary>
/// Represents an artifact tool that uses <see cref="XsClient"/>.
/// </summary>
public abstract class XsArtifactTool : HtmlArtifactTool
{
    /// <summary>
    /// Asynchronously creates a <see cref="XsClient"/>.
    /// </summary>
    /// <returns>Task rturning an Xs client.</returns>
    protected static Task<XsClient> CreateXsClientAsync() => XsManager.CreateClientAsync();

    #region Html overloads

    /// <summary>
    /// Opens a new document loaded from the provided <see cref="XsClient"/>.
    /// </summary>
    /// <param name="xs">Client with active page.</param>
    /// <returns>Task returning the loaded document.</returns>
    protected async Task<IDocument> OpenAsync(XsClient xs)
        => await OpenAsync(await xs.GetContentAsync()).ConfigureAwait(false);

    #endregion

    #region HttpClient syncing

    /// <summary>
    /// Synchronizes settings from an <see cref="XsClient"/> to current <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="xs">Xs client.</param>
    /// <returns>Task.</returns>
    protected async Task SyncToHttpClientAsync(XsClient xs)
    {
        (List<Cookie> cookies, string ua) = await xs.ExecuteAsync(
            async w => ((await w.CookieManager.GetCookiesAsync("")).Select(c => c.ToSystemNetCookie()).ToList(), w.Settings.UserAgent));
        foreach (Cookie c in cookies)
            HttpClientHandler.CookieContainer.Add(c);
        HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(ua);
    }

    /// <summary>
    /// Synchronizes settings from current <see cref="HttpClient"/> to an <see cref="XsClient"/>.
    /// </summary>
    /// <param name="xs">Xs client.</param>
    /// <returns>Task.</returns>
    protected async Task SyncFromHttpClientAsync(XsClient xs)
    {
        List<Cookie> cookies = HttpClientHandler.CookieContainer.GetAllCookies().ToList();
        string ua = HttpClient.DefaultRequestHeaders.UserAgent.ToString();
        await xs.ExecuteAsync(w =>
        {
            foreach (Cookie c in cookies)
                w.CookieManager.AddOrUpdateCookie(w.CookieManager.CreateCookieWithSystemNetCookie(c));
            w.Settings.UserAgent = ua;
        });
    }

    #endregion
}