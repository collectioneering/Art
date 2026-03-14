using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Art.Common.Proxies;

internal class FindAsListTool : IArtifactListTool
{
    private readonly IArtifactFindTool _baseFindTool;
    private readonly IReadOnlyList<string> _ids;

    public FindAsListTool(IArtifactFindTool baseFindTool, IReadOnlyList<string> ids)
    {
        _baseFindTool = baseFindTool ?? throw new ArgumentNullException(nameof(baseFindTool));
        _ids = ids ?? throw new ArgumentNullException(nameof(ids));
    }

    public void Dispose() => _baseFindTool.Dispose();

    public bool DebugMode
    {
        get => _baseFindTool.DebugMode;
        set => _baseFindTool.DebugMode = value;
    }

    public IToolLogHandler? LogHandler
    {
        get => _baseFindTool.LogHandler;
        set => _baseFindTool.LogHandler = value;
    }

    public ArtifactToolProfile Profile => _baseFindTool.Profile;
    public ArtifactToolConfig Config => _baseFindTool.Config;
    public TimeProvider TimeProvider => _baseFindTool.TimeProvider;
    public EagerFlags AllowedEagerModes => _baseFindTool.AllowedEagerModes;

    public IArtifactRegistrationManager RegistrationManager
    {
        get => _baseFindTool.RegistrationManager;
        set => _baseFindTool.RegistrationManager = value;
    }

    public IArtifactDataManager DataManager
    {
        get => _baseFindTool.DataManager;
        set => _baseFindTool.DataManager = value;
    }

    public JsonSerializerOptions JsonOptions
    {
        get => _baseFindTool.JsonOptions;
        set => _baseFindTool.JsonOptions = value;
    }

    public string GroupFallback => _baseFindTool.GroupFallback;

    public Task InitializeAsync(ArtifactToolConfig? config = null, ArtifactToolProfile? profile = null, CancellationToken cancellationToken = default) => _baseFindTool.InitializeAsync(config, profile, cancellationToken);

    public async IAsyncEnumerable<IArtifactData> ListAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (string id in _ids)
        {
            var artifact = await _baseFindTool.FindAsync(id, cancellationToken).ConfigureAwait(false);
            if (artifact != null)
            {
                yield return artifact;
            }
        }
    }
}
