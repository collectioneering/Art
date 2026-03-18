namespace Art;

/// <summary>
/// Representation of a tool's metadata.
/// </summary>
/// <param name="Type">Tool type.</param>
/// <param name="Id">Tool ID.</param>
/// <param name="Properties">Miscellaneous informational properties of the tool, where available.</param>
public readonly record struct ArtifactToolDescription(Type Type, ArtifactToolID Id, IReadOnlyDictionary<string, string> Properties);
