namespace Art;

/// <summary>
/// Runtime configuration for a <see cref="IArtifactTool"/>.
/// </summary>
/// <param name="RegistrationManager">Registration manager.</param>
/// <param name="DataManager">Data manager.</param>
/// <param name="ExtensionsContext">Extensions context.</param>
/// <param name="TimeProvider">Time provider.</param>
/// <param name="GetArtifactRetrievalTimestamps">Get artifact retrieval timestamps.</param>
/// <param name="GetResourceRetrievalTimestamps">Get resource retrieval timestamps.</param>
public record ArtifactToolConfig(
    IArtifactRegistrationManager RegistrationManager,
    IArtifactDataManager DataManager,
    IExtensionsContext ExtensionsContext,
    TimeProvider TimeProvider,
    bool GetArtifactRetrievalTimestamps,
    bool GetResourceRetrievalTimestamps);
