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
        IConfiguration configuration = Configuration.Default.WithDefaultLoader().WithOnly<ICookieProvider>(new OpenMemoryCookieProvider(CookieContainer));
        _browser = BrowsingContext.New(configuration);
    }

    #endregion

    #region Main API

    /// <summary>
    /// Opens a new document loaded from the provided address.
    /// </summary>
    /// <param name="content">Content to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning the loaded document.</returns>
    public async Task<IDocument> OpenStringAsync(string content, CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return Document = await Browser.OpenAsync(r => r.Content(content), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Opens a new document loaded from the provided address.
    /// </summary>
    /// <param name="address">Address to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning the loaded document.</returns>
    public async Task<IDocument> OpenAsync(string address, CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return Document = await Browser.OpenAsync(address, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Opens a new document loaded from the provided address.
    /// </summary>
    /// <param name="address">Address to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning the loaded document.</returns>
    public async Task<IDocument> OpenAsync(Url address, CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return Document = await Browser.OpenAsync(address, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Opens a new document loaded from the provided request.
    /// </summary>
    /// <param name="request">Request to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning the loaded document.</returns>
    public async Task<IDocument> OpenAsync(DocumentRequest request, CancellationToken cancellationToken = default)
    {
        NotDisposed();
        return Document = await Browser.OpenAsync(request, cancellationToken).ConfigureAwait(false);
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
