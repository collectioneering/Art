using System.Diagnostics.CodeAnalysis;
using System.Runtime.Loader;
using System.Text.Json;
using Art.Common.Management;

namespace Art.Common;

/// <summary>
/// Common type for artifact tools.
/// </summary>
public partial class ArtifactTool : IArtifactTool
{
    #region Fields

    /// <inheritdoc />
    public bool DebugMode { get; set; }

    /// <summary>
    /// Default delay time in seconds.
    /// </summary>
    public virtual double DelaySeconds => 0.25;

    /// <inheritdoc />
    public IToolLogHandler? LogHandler { get; set; }

    /// <inheritdoc />
    public ArtifactToolProfile Profile { get; private set; }

    /// <inheritdoc />
    public ArtifactToolConfig Config { get; private set; }

    /// <inheritdoc />
    public TimeProvider TimeProvider { get; private set; }

    /// <inheritdoc />
    public virtual EagerFlags AllowedEagerModes => EagerFlags.None;

    /// <inheritdoc />
    public IArtifactRegistrationManager RegistrationManager { get; set; }

    /// <inheritdoc />
    public IArtifactDataManager DataManager { get; set; }

    /// <inheritdoc />
    public JsonSerializerOptions JsonOptions
    {
        get => _jsonOptions ??= new JsonSerializerOptions();
        set => _jsonOptions = value;
    }

    /// <inheritdoc />
    public virtual string GroupFallback => "default";

    #endregion

    #region Private fields

    private bool _delayFirstCalled;

    private JsonSerializerOptions? _jsonOptions;

    private bool _disposed;

    //private bool _initialized;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new instance of <see cref="ArtifactTool"/>.
    /// </summary>
    public ArtifactTool()
    {
        RegistrationManager = null!;
        DataManager = null!;
        Profile = null!;
        Config = null!;
        TimeProvider = null!;
    }

    #endregion

    #region Setup

    /// <inheritdoc />
    public async Task InitializeAsync(ArtifactToolConfig? config = null, ArtifactToolProfile? profile = null, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        InitializeCore(config, profile);
        await ConfigureAsync(cancellationToken).ConfigureAwait(false);
        //_initialized = true;
    }

    private void InitializeCore(ArtifactToolConfig? config, ArtifactToolProfile? profile)
    {
        config ??= new ArtifactToolConfig(new NullArtifactRegistrationManager(), new NullArtifactDataManager(), TimeProvider.System, true, true);
        if (config.RegistrationManager == null)
        {
            throw new ArgumentException("Cannot configure with null registration manager");
        }
        if (config.DataManager == null)
        {
            throw new ArgumentException("Cannot configure with null data manager");
        }
        if (config.TimeProvider == null)
        {
            throw new ArgumentException("Cannot configure with null time provider");
        }
        Config = config;
        RegistrationManager = config.RegistrationManager;
        DataManager = config.DataManager;
        TimeProvider = config.TimeProvider;
        Profile = profile ?? new ArtifactToolProfile(ArtifactToolIDUtil.CreateToolString(GetType()), null, null);
        if (Profile.Options != null)
            ConfigureOptions();
    }

    /// <summary>
    /// Initializes this tool.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <remarks>This is called implicitly by <see cref="InitializeAsync"/> which performs first initialization for a given profile / config set.</remarks>
    public virtual Task ConfigureAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Configures options.
    /// </summary>
    /// <remarks>
    /// This method is called by <see cref="InitializeAsync"/> when <see cref="Profile"/>.<see cref="ArtifactToolProfile.Options"/> is not null.
    /// </remarks>
    public virtual void ConfigureOptions()
    {
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Disposes unmanaged resources.
    /// </summary>
    /// <param name="disposing">Is disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /*private void EnsureInitialized()
    {
        if (!_initialized) throw new InvalidOperationException("Instance has not been initialized");
    }*/

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    #endregion

    /// <summary>
    /// Prepares a tool for the specified profile.
    /// </summary>
    /// <param name="artifactToolProfile">Tool profile.</param>
    /// <param name="artifactRegistrationManager">Registration manager.</param>
    /// <param name="artifactDataManager">Data manager.</param>
    /// <param name="timeProvider">Time provider.</param>
    /// <param name="getArtifactRetrievalTimestamps">Get artifact retrieval timestamps.</param>
    /// <param name="getResourceRetrievalTimestamps">Get resource retrieval timestamps.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid profile is provided.</exception>
    /// <exception cref="ArtifactToolNotFoundException">Thrown when tool is not found.</exception>
    [RequiresUnreferencedCode($"Loading artifact tools might require types that cannot be statically analyzed. Consider making use of the overload that takes {nameof(IArtifactToolRegistry)} where possible.")]
    public static Task<IArtifactTool> PrepareToolAsync(
        ArtifactToolProfile artifactToolProfile,
        IArtifactRegistrationManager artifactRegistrationManager,
        IArtifactDataManager artifactDataManager,
        TimeProvider timeProvider,
        bool getArtifactRetrievalTimestamps,
        bool getResourceRetrievalTimestamps,
        CancellationToken cancellationToken = default)
    {
        if (!ArtifactToolLoader.TryLoad(artifactToolProfile.Tool, out IArtifactTool? t))
            throw new ArtifactToolNotFoundException(artifactToolProfile.Tool);
        return PrepareToolInternalAsync(t, artifactToolProfile, artifactRegistrationManager, artifactDataManager, timeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken);
    }

    /// <summary>
    /// Prepares a tool for the specified profile.
    /// </summary>
    /// <param name="assemblyLoadContext">Custom <see cref="AssemblyLoadContext"/>.</param>
    /// <param name="artifactToolProfile">Tool profile.</param>
    /// <param name="artifactRegistrationManager">Registration manager.</param>
    /// <param name="artifactDataManager">Data manager.</param>
    /// <param name="timeProvider">Time provider.</param>
    /// <param name="getArtifactRetrievalTimestamps">Get artifact retrieval timestamps.</param>
    /// <param name="getResourceRetrievalTimestamps">Get resource retrieval timestamps.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid profile is provided.</exception>
    /// <exception cref="ArtifactToolNotFoundException">Thrown when tool is not found.</exception>
    [RequiresUnreferencedCode($"Loading artifact tools might require types that cannot be statically analyzed. Consider making use of the overload that takes {nameof(IArtifactToolRegistry)} where possible.")]
    public static Task<IArtifactTool> PrepareToolAsync(
        AssemblyLoadContext assemblyLoadContext,
        ArtifactToolProfile artifactToolProfile,
        IArtifactRegistrationManager artifactRegistrationManager,
        IArtifactDataManager artifactDataManager,
        TimeProvider timeProvider,
        bool getArtifactRetrievalTimestamps,
        bool getResourceRetrievalTimestamps,
        CancellationToken cancellationToken = default)
    {
        if (!ArtifactToolLoader.TryLoad(assemblyLoadContext, artifactToolProfile.Tool, out IArtifactTool? t))
            throw new ArtifactToolNotFoundException(artifactToolProfile.Tool);
        return PrepareToolInternalAsync(t, artifactToolProfile, artifactRegistrationManager, artifactDataManager, timeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken);
    }

    /// <summary>
    /// Prepares a tool for the specified profile.
    /// </summary>
    /// <param name="artifactToolRegistry">Custom <see cref="IArtifactToolRegistry"/>.</param>
    /// <param name="artifactToolProfile">Tool profile.</param>
    /// <param name="artifactRegistrationManager">Registration manager.</param>
    /// <param name="artifactDataManager">Data manager.</param>
    /// <param name="timeProvider">Time provider.</param>
    /// <param name="getArtifactRetrievalTimestamps">Get artifact retrieval timestamps.</param>
    /// <param name="getResourceRetrievalTimestamps">Get resource retrieval timestamps.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid profile is provided.</exception>
    /// <exception cref="ArtifactToolNotFoundException">Thrown when tool is not found.</exception>
    public static Task<IArtifactTool> PrepareToolAsync(
        IArtifactToolRegistry artifactToolRegistry,
        ArtifactToolProfile artifactToolProfile,
        IArtifactRegistrationManager artifactRegistrationManager,
        IArtifactDataManager artifactDataManager,
        TimeProvider timeProvider,
        bool getArtifactRetrievalTimestamps,
        bool getResourceRetrievalTimestamps,
        CancellationToken cancellationToken = default)
    {
        if (!artifactToolRegistry.TryLoad(ArtifactToolIDUtil.ParseID(artifactToolProfile.Tool), out IArtifactTool? t))
            throw new ArtifactToolNotFoundException(artifactToolProfile.Tool);
        return PrepareToolInternalAsync(t, artifactToolProfile, artifactRegistrationManager, artifactDataManager, timeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken);
    }

    /// <summary>
    /// Prepares a tool for the specified profile.
    /// </summary>
    /// <typeparam name="T">Artifact tool factory type.</typeparam>
    /// <param name="artifactToolProfile">Tool profile.</param>
    /// <param name="artifactRegistrationManager">Registration manager.</param>
    /// <param name="artifactDataManager">Data manager.</param>
    /// <param name="timeProvider">Time provider.</param>
    /// <param name="getArtifactRetrievalTimestamps">Get artifact retrieval timestamps.</param>
    /// <param name="getResourceRetrievalTimestamps">Get resource retrieval timestamps.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid profile is provided.</exception>
    public static Task<IArtifactTool> PrepareToolAsync<T>(
        ArtifactToolProfile artifactToolProfile,
        IArtifactRegistrationManager artifactRegistrationManager,
        IArtifactDataManager artifactDataManager,
        TimeProvider timeProvider,
        bool getArtifactRetrievalTimestamps,
        bool getResourceRetrievalTimestamps,
        CancellationToken cancellationToken = default) where T : IArtifactToolFactory
    {
        var t = T.CreateArtifactTool();
        return PrepareToolInternalAsync(t, artifactToolProfile, artifactRegistrationManager, artifactDataManager, timeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken);
    }

    private static async Task<IArtifactTool> PrepareToolInternalAsync(
        IArtifactTool tool,
        ArtifactToolProfile artifactToolProfile,
        IArtifactRegistrationManager artifactRegistrationManager,
        IArtifactDataManager artifactDataManager,
        TimeProvider timeProvider,
        bool getArtifactRetrievalTimestamps,
        bool getResourceRetrievalTimestamps,
        CancellationToken cancellationToken = default)
    {
        ArtifactToolConfig config = new(artifactRegistrationManager, artifactDataManager, timeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps);
        artifactToolProfile = artifactToolProfile.WithCoreTool(tool);
        await tool.InitializeAsync(config, artifactToolProfile, cancellationToken).ConfigureAwait(false);
        return tool;
    }
}
