using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Art.Common;

/// <summary>
/// Stores data relevant to an artifact.
/// </summary>
public class ArtifactData : IArtifactData
{
    /// <summary>
    /// Info for this artifact.
    /// </summary>
    public ArtifactInfo Info { get; }

    /// <inheritdoc />
    ArtifactResourceInfo? IArtifactData.PrimaryResource => PrimaryResource;

    /// <inheritdoc cref="IArtifactData.PrimaryResource"/>
    public ArtifactResourceInfo? PrimaryResource { get; set; }

    /// <summary>
    /// Tool associated with this instance.
    /// </summary>
    public IArtifactTool? Tool { get; }

    /// <summary>
    /// Resources for this artifact.
    /// </summary>
    public readonly Dictionary<ArtifactResourceKey, ArtifactResourceInfo> Resources = new();

    /// <inheritdoc/>
    public IEnumerable<ArtifactResourceKey> Keys => Resources.Keys;

    /// <inheritdoc/>
    public IEnumerable<ArtifactResourceInfo> Values => Resources.Values;

    /// <inheritdoc/>
    public int Count => Resources.Count;

    /// <inheritdoc/>
    public ArtifactResourceInfo this[ArtifactResourceKey key] => Resources[key];

    /// <summary>
    /// Creates a new instance of <see cref="ArtifactData"/>.
    /// </summary>
    /// <param name="tool">Tool id.</param>
    /// <param name="group">Group.</param>
    /// <param name="id">Artifact ID.</param>
    /// <param name="name">Name.</param>
    /// <param name="date">Artifact creation date.</param>
    /// <param name="updateDate">Artifact update date.</param>
    /// <param name="retrievalDate">Artifact retrieval date.</param>
    /// <param name="full">True if this is a full artifact.</param>
    public ArtifactData(
        string tool,
        string group,
        string id,
        string? name = null,
        DateTimeOffset? date = null,
        DateTimeOffset? updateDate = null,
        DateTimeOffset? retrievalDate = null,
        bool full = true)
    {
        Info = new ArtifactInfo(new ArtifactKey(tool, group, id), name, date, updateDate, retrievalDate, full);
    }

    /// <summary>
    /// Creates a new instance of <see cref="ArtifactData"/>.
    /// </summary>
    /// <param name="key">Artifact key.</param>
    /// <param name="name">Name.</param>
    /// <param name="date">Artifact creation date.</param>
    /// <param name="updateDate">Artifact update date.</param>
    /// <param name="retrievalDate">Artifact retrieval date.</param>
    /// <param name="full">True if this is a full artifact.</param>
    public ArtifactData(
        ArtifactKey key,
        string? name = null,
        DateTimeOffset? date = null,
        DateTimeOffset? updateDate = null,
        DateTimeOffset? retrievalDate = null,
        bool full = true)
    {
        Info = new ArtifactInfo(key, name, date, updateDate, retrievalDate, full);
    }

    /// <summary>
    /// Creates a new instance of <see cref="ArtifactData"/>.
    /// </summary>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="tool">Tool id.</param>
    /// <param name="group">Group.</param>
    /// <param name="id">Artifact ID.</param>
    /// <param name="name">Name.</param>
    /// <param name="date">Artifact creation date.</param>
    /// <param name="updateDate">Artifact update date.</param>
    /// <param name="retrievalDate">Artifact retrieval date.</param>
    /// <param name="full">True if this is a full artifact.</param>
    public ArtifactData(
        IArtifactTool artifactTool,
        string tool,
        string group,
        string id,
        string? name = null,
        DateTimeOffset? date = null,
        DateTimeOffset? updateDate = null,
        DateTimeOffset? retrievalDate = null,
        bool full = true)
    {
        Info = new ArtifactInfo(new ArtifactKey(tool, group, id), name, date, updateDate, retrievalDate, full);
        Tool = artifactTool;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ArtifactData"/>.
    /// </summary>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="key">Artifact key.</param>
    /// <param name="name">Name.</param>
    /// <param name="date">Artifact creation date.</param>
    /// <param name="updateDate">Artifact update date.</param>
    /// <param name="retrievalDate">Artifact retrieval date.</param>
    /// <param name="full">True if this is a full artifact.</param>
    public ArtifactData(
        IArtifactTool artifactTool,
        ArtifactKey key,
        string? name = null,
        DateTimeOffset? date = null,
        DateTimeOffset? updateDate = null,
        DateTimeOffset? retrievalDate = null,
        bool full = true)
    {
        Info = new ArtifactInfo(key, name, date, updateDate, retrievalDate, full);
        Tool = artifactTool;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ArtifactData"/>.
    /// </summary>
    /// <param name="info">Artifact info.</param>
    public ArtifactData(ArtifactInfo info)
    {
        Info = info;
    }

    /// <summary>
    /// Adds a resource to this instance.
    /// </summary>
    /// <param name="resource">Resource to add.</param>
    public void Add(ArtifactResourceInfo resource)
    {
        Resources[resource.Key] = resource;
    }

    /// <summary>
    /// Adds a resource to this instance.
    /// </summary>
    /// <param name="resource">Resource to add.</param>
    public void Add(ArtifactDataResource resource)
    {
        if (resource.Data != this)
        {
            throw new ArgumentException("Cannot add a data resource with different source data object");
        }
        Resources[resource.Info.Key] = resource.Info;
    }

    /// <summary>
    /// Adds resources to this instance.
    /// </summary>
    /// <param name="resources">Resources to add.</param>
    public void AddRange(IEnumerable<ArtifactResourceInfo> resources)
    {
        foreach (ArtifactResourceInfo resource in resources)
        {
            Add(resource);
        }
    }

    /// <summary>
    /// Attempts to cast <see cref="Tool"/> to a <see cref="IArtifactTool"/> of the specified type.
    /// </summary>
    /// <typeparam name="T">Tool type.</typeparam>
    /// <returns><see cref="Tool"/> with a type cast.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Tool"/> is not specified.</exception>
    /// <exception cref="InvalidCastException">Thrown for invalid type.</exception>
    public T GetArtifactTool<T>() where T : class, IArtifactTool
        => (Tool
            ?? throw new InvalidOperationException("Data object was not initialized with an artifact tool")) as T
           ?? throw new InvalidCastException("Tool type for this data object is not compatible with needed type");

    /// <inheritdoc/>
    public bool ContainsKey(ArtifactResourceKey key) => Resources.ContainsKey(key);

    /// <inheritdoc/>
    public bool TryGetValue(ArtifactResourceKey key, [MaybeNullWhen(false)] out ArtifactResourceInfo value) => Resources.TryGetValue(key, out value);

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<ArtifactResourceKey, ArtifactResourceInfo>> GetEnumerator() => Resources.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Resources).GetEnumerator();
}
