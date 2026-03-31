namespace Art.Common.Proxies;

/// <summary>
/// Represents runtime options for dump operation.
/// </summary>
/// <param name="ResourceUpdate">Resource update mode.</param>
/// <param name="IncludeNonFull">Overwrite full entries with non-full if newer.</param>
/// <param name="SkipMode">Skip mode.</param>
/// <param name="ChecksumSource">Optional checksum source, if resources are to have their checksums computed.</param>
/// <param name="IgnoreException">Predicate used to conditionally ignore exceptions.</param>
/// <param name="EagerFlags">Eager evaluation flags.</param>
public record ArtifactToolDumpOptions(
    ResourceUpdateMode ResourceUpdate = ResourceUpdateMode.ArtifactHard,
    bool IncludeNonFull = true,
    ArtifactSkipMode SkipMode = ArtifactSkipMode.None,
    ChecksumSource? ChecksumSource = null,
    Func<Exception, bool>? IgnoreException = null,
    EagerFlags EagerFlags = EagerFlags.None)
{
    /// <summary>
    /// Default options.
    /// </summary>
    public static readonly ArtifactToolDumpOptions Default = new();

    /// <summary>
    /// Validates an instance of <see cref="ArtifactToolDumpOptions"/>.
    /// </summary>
    /// <param name="options">Options to validate.</param>
    /// <param name="constructor">True if called from an object constructor.</param>
    /// <exception cref="ArgumentException">Exception thrown for invalid configuration in constructor.</exception>
    /// <exception cref="InvalidOperationException">Exception thrown for invalid configuration anywhere except constructor.</exception>
    public static void Validate(ArtifactToolDumpOptions options, bool constructor)
    {
        ArgumentNullException.ThrowIfNull(options);
        switch (options.SkipMode)
        {
            case ArtifactSkipMode.None:
            case ArtifactSkipMode.FastExit:
            case ArtifactSkipMode.Known:
                break;
            default:
                if (constructor)
                    throw new ArgumentException($"Invalid {nameof(ArtifactToolDumpOptions)}.{nameof(SkipMode)}");
                else
                    throw new InvalidOperationException($"Invalid {nameof(ArtifactToolDumpOptions)}.{nameof(SkipMode)}");
        }
        switch (options.ResourceUpdate)
        {
            case ResourceUpdateMode.ArtifactSoft:
            case ResourceUpdateMode.ArtifactHard:
            case ResourceUpdateMode.Soft:
            case ResourceUpdateMode.Hard:
                break;
            default:
                if (constructor)
                    throw new ArgumentException($"Invalid {nameof(ArtifactToolDumpOptions)}.{nameof(ResourceUpdate)}");
                else
                    throw new InvalidOperationException($"Invalid {nameof(ArtifactToolDumpOptions)}.{nameof(ResourceUpdate)}");
        }
    }
}
