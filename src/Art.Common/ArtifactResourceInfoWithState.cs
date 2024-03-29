namespace Art.Common;

/// <summary>
/// Represents an <see cref="Art.ArtifactResourceInfo"/> with an associated <see cref="ItemStateFlags"/>.
/// </summary>
/// <param name="ArtifactResourceInfo">Artifact resource information.</param>
/// <param name="State">Item state flags.</param>
public readonly record struct ArtifactResourceInfoWithState(ArtifactResourceInfo ArtifactResourceInfo, ItemStateFlags State);
