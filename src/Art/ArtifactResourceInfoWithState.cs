namespace Art;

/// <summary>
/// Represents an <see cref="Art.ArtifactResourceInfo"/> with an associated <see cref="Art.ItemStateFlags"/>.
/// </summary>
/// <param name="ArtifactResourceInfo">Artifact resource information.</param>
/// <param name="State">Item state flags.</param>
public record struct ArtifactResourceInfoWithState(ArtifactResourceInfo ArtifactResourceInfo, ItemStateFlags State);