using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using Art.Http;

namespace Art.Html;

/// <summary>
/// Represents an instance of an artifact tool that works on HTML content using a <see cref="IBrowsingContext"/>.
/// </summary>
public abstract class HtmlArtifactTool : HttpArtifactTool
{
    /// <summary>
    /// Active browsing context.
    /// </summary>
    public IBrowsingContext Browser
    {
        get
        {
            NotDisposed();
            return _browser ?? throw new InvalidOperationException("Browser is not currently set.");
        }
        set
        {
            NotDisposed();
            _browser = value;
        }
    }

    private IBrowsingContext? _browser;

    /// <summary>
    /// Current document, if one is loaded.
    /// </summary>
    public IDocument? Document;

    /// <summary>
    /// Current document.
    /// </summary>
    /// <remarks>
    /// Accessing this property throws an <see cref="InvalidOperationException"/> if a document is not loaded.
    /// </remarks>
    public IDocument DocumentNotNull => Document ?? throw new InvalidOperationException("Document not currently loaded");

    private bool _disposed;

    #region Configuration

    /// <inheritdoc/>
    public override async Task ConfigureAsync(CancellationToken cancellationToken = default)
    {
        await base.ConfigureAsync(cancellationToken).ConfigureAwait(false);
        IConfiguration configuration = Configuration.Default.WithDefaultLoader().WithOnly<ICookieProvider>(new MemoryCookieProvider(CookieContainer));
        _browser = BrowsingContext.New(configuration);
    }

    #endregion

    #region Main API

    /// <summary>
    /// Opens a new document using the specified delegate.
    /// </summary>
    /// <param name="request">Content to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning the loaded document.</returns>
    /// <remarks>This member sets the <see cref="Document"/> property, and the document will be available at both <see cref="Document"/> and <see cref="DocumentNotNull"/>.</remarks>
    public async Task<IDocument> OpenAsync(Action<VirtualResponse> request, CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return Document = await Browser.OpenAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Opens a new document loaded from the provided address.
    /// </summary>
    /// <param name="address">Address to load.</param>
    /// <param name="httpRequestConfig">Request configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning the loaded document.</returns>
    /// <remarks>This member sets the <see cref="Document"/> property, and the document will be available at both <see cref="Document"/> and <see cref="DocumentNotNull"/>.</remarks>
    public Task<IDocument> OpenAsync(string address, HttpRequestConfig? httpRequestConfig = null, CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return OpenViaHttpClientAsync(new Uri(address), httpRequestConfig, cancellationToken);
    }

    /// <summary>
    /// Opens a new document loaded from the provided address.
    /// </summary>
    /// <param name="address">Address to load.</param>
    /// <param name="httpRequestConfig">Request configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning the loaded document.</returns>
    /// <remarks>This member sets the <see cref="Document"/> property, and the document will be available at both <see cref="Document"/> and <see cref="DocumentNotNull"/>.</remarks>
    public Task<IDocument> OpenAsync(Uri address, HttpRequestConfig? httpRequestConfig = null, CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return OpenViaHttpClientAsync(address, httpRequestConfig, cancellationToken);
    }

    /// <summary>
    /// Opens a new document loaded from the provided address.
    /// </summary>
    /// <param name="address">Address to load.</param>
    /// <param name="httpRequestConfig">Request configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning the loaded document.</returns>
    /// <remarks>This member sets the <see cref="Document"/> property, and the document will be available at both <see cref="Document"/> and <see cref="DocumentNotNull"/>.</remarks>
    public Task<IDocument> OpenAsync(Url address, HttpRequestConfig? httpRequestConfig = null, CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return OpenViaHttpClientAsync(address.ToUri(), httpRequestConfig, cancellationToken);
    }

    private async Task<IDocument> OpenViaHttpClientAsync(Uri uri, HttpRequestConfig? httpRequestConfig, CancellationToken cancellationToken = default)
    {
        using var response = await GetAsync(uri, httpRequestConfig, cancellationToken).ConfigureAwait(false);
        ArtHttpResponseMessageException.EnsureSuccessStatusCode(response);
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            if (header.Value.LastOrDefault() is { } value)
            {
                headers[header.Key] = value;
            }
        }
        foreach (var header in response.Content.Headers)
        {
            if (header.Value.LastOrDefault() is { } value)
            {
                headers[header.Key] = value;
            }
        }
        var contentCopyMs = response.Content.Headers.ContentLength is { } contentLength and > 0 and < int.MaxValue
            ? new MemoryStream(capacity: (int)contentLength)
            : new MemoryStream();
        await response.Content.CopyToAsync(contentCopyMs, cancellationToken).ConfigureAwait(false);
        contentCopyMs.Position = 0;
        foreach (var header in response.TrailingHeaders)
        {
            if (header.Value.LastOrDefault() is { } value)
            {
                headers[header.Key] = value;
            }
        }
        var status = response.StatusCode;
        return Document = await Browser.OpenAsync(r =>
        {
            r.Address(uri);
            r.Content(contentCopyMs);
            r.Headers(headers);
            r.Status(status);
        }, cancel: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the first element within the document
    /// (using depth-first pre-order traversal of the document's nodes)
    /// that matches the specified group of selectors.
    /// </summary>
    /// <param name="selectors">The group of selectors to use.</param>
    /// <returns>The found element.</returns>
    public IElement? QuerySelector(string selectors)
    {
        NotDisposed();
        return DocumentNotNull.QuerySelector(selectors);
    }

    /// <summary>
    /// Returns the first element within the document
    /// (using depth-first pre-order traversal of the document's nodes)
    /// that matches the specified group of selectors.
    /// </summary>
    /// <param name="selectors">The group of selectors to use.</param>
    /// <returns>The found element.</returns>
    public IElement QuerySelectorRequired(string selectors)
    {
        NotDisposed();
        return DocumentNotNull.QuerySelectorRequired(selectors);
    }

    /// <summary>
    /// Returns the first element within the document
    /// (using depth-first pre-order traversal of the document's nodes)
    /// that matches the specified group of selectors.
    /// </summary>
    /// <param name="selectors">The group of selectors to use.</param>
    /// <returns>The found element.</returns>
    public T? QuerySelector<T>(string selectors) where T : class, IElement
    {
        NotDisposed();
        return DocumentNotNull.QuerySelector<T>(selectors);
    }

    /// <summary>
    /// Returns a list of the elements within the document
    /// (using depth-first pre-order traversal of the document's nodes)
    /// that match the specified group of selectors.
    /// </summary>
    /// <param name="selectors">The group of selectors to use.</param>
    /// <returns>The found elements.</returns>
    public IHtmlCollection<IElement> QuerySelectorAll(string selectors)
    {
        NotDisposed();
        return DocumentNotNull.QuerySelectorAll(selectors);
    }

    /// <summary>
    /// Returns a list of the elements within the document
    /// (using depth-first pre-order traversal of the document's nodes)
    /// that match the specified group of selectors.
    /// </summary>
    /// <param name="selectors">The group of selectors to use.</param>
    /// <returns>The found elements.</returns>
    public IEnumerable<T> QuerySelectorAll<T>(string selectors) where T : class, IElement
    {
        NotDisposed();
        return DocumentNotNull.QuerySelectorAll<T>(selectors);
    }

    #endregion

    #region Http overloads

    /// <summary>
    /// Sends an HTTP HEAD request.
    /// </summary>
    /// <param name="url">Request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning response.</returns>
    public Task<HttpResponseMessage> HeadAsync(Url url, CancellationToken cancellationToken = default)
        => HeadAsync(url.ToUri(), cancellationToken: cancellationToken);

    /// <summary>
    /// Sends an HTTP GET request.
    /// </summary>
    /// <param name="url">Request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning response.</returns>
    public Task<HttpResponseMessage> GetAsync(Url url, CancellationToken cancellationToken = default)
        => GetAsync(url.ToUri(), cancellationToken: cancellationToken);

    /// <summary>
    /// Retrieve deserialized JSON using a <see cref="Url"/>.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="url">Request URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning deserialized data.</returns>
    /// <remarks>
    /// This overload usees <see cref="IArtifactTool.JsonOptions"/> member automatically.
    /// </remarks>
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
    public Task<T?> GetDeserializedJsonAsync<T>(Url url, CancellationToken cancellationToken = default)
        => GetDeserializedJsonAsync<T>(url.ToUri(), cancellationToken: cancellationToken);

    /// <summary>
    /// Retrieve deserialized JSON using a <see cref="Url"/>.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="url">Request URL.</param>
    /// <param name="jsonTypeInfo">JSON type info.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning deserialized data.</returns>
    /// <remarks>
    /// This overload usees <see cref="IArtifactTool.JsonOptions"/> member automatically.
    /// </remarks>
    public Task<T?> GetDeserializedJsonAsync<T>(Url url, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
        => GetDeserializedJsonAsync(url.ToUri(), jsonTypeInfo, cancellationToken: cancellationToken);

    /// <summary>
    /// Retrieve deserialized JSON using a <see cref="Url"/>.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="url">Request URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning deserialized data.</returns>
    /// <remarks>
    /// This overload usees <see cref="IArtifactTool.JsonOptions"/> member automatically.
    /// </remarks>
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
    public Task<T> GetDeserializedRequiredJsonAsync<T>(Url url, CancellationToken cancellationToken = default)
        => GetDeserializedRequiredJsonAsync<T>(url.ToUri(), cancellationToken: cancellationToken);

    /// <summary>
    /// Retrieve deserialized JSON using a <see cref="Url"/>.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="url">Request URL.</param>
    /// <param name="jsonTypeInfo">JSON type info.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning deserialized data.</returns>
    /// <remarks>
    /// This overload usees <see cref="IArtifactTool.JsonOptions"/> member automatically.
    /// </remarks>
    public Task<T> GetDeserializedRequiredJsonAsync<T>(Url url, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default)
        => GetDeserializedRequiredJsonAsync(url.ToUri(), jsonTypeInfo, cancellationToken: cancellationToken);

    /// <summary>
    /// Retrieve deserialized JSON using a <see cref="Url"/> and <see cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="url">Request URL.</param>
    /// <param name="jsonSerializerOptions">Optional deserialization options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning deserialized data.</returns>
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
    public Task<T?> GetDeserializedJsonAsync<T>(Url url, JsonSerializerOptions? jsonSerializerOptions, CancellationToken cancellationToken = default)
        => GetDeserializedJsonAsync<T>(url.ToUri(), jsonSerializerOptions, cancellationToken: cancellationToken);

    /// <summary>
    /// Retrieve deserialized JSON using a <see cref="Url"/> and <see cref="JsonSerializerOptions"/>.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="url">Request URL.</param>
    /// <param name="jsonSerializerOptions">Optional deserialization options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning deserialized data.</returns>
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
    public Task<T> GetDeserializedRequiredJsonAsync<T>(Url url, JsonSerializerOptions? jsonSerializerOptions, CancellationToken cancellationToken = default)
        => GetDeserializedRequiredJsonAsync<T>(url.ToUri(), jsonSerializerOptions, cancellationToken: cancellationToken);

    #endregion

    #region IDisposable

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!_disposed)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                _browser?.Dispose();
                Document?.Dispose();
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            _browser = null!;
            Document = null;
            _disposed = true;
        }
    }

    private void NotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    #endregion
}
