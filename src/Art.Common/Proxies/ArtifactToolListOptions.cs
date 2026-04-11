namespace Art.Common.Proxies;

/// <summary>
/// Represents runtime options for list operation.
/// </summary>
/// <param name="IncludeNonFull">Include non-full entries.</param>
/// <param name="SkipMode">Skip mode.</param>
/// <param name="EagerFlags">Eager evaluation flags.</param>
public record ArtifactToolListOptions(
    bool IncludeNonFull = true,
    ArtifactSkipMode SkipMode = ArtifactSkipMode.None,
    EagerFlags EagerFlags = EagerFlags.None)
{
    /// <summary>
    /// Default options.
    /// </summary>
    public static readonly ArtifactToolListOptions Default = new();

    /// <summary>
    /// Validates an instance of <see cref="ArtifactToolListOptions"/>.
    /// </summary>
    /// <param name="options">Options to validate.</param>
    /// <param name="constructor">True if called from an object constructor.</param>
    /// <exception cref="ArgumentException">Exception thrown for invalid configuration in constructor.</exception>
    /// <exception cref="InvalidOperationException">Exception thrown for invalid configuration anywhere except constructor.</exception>
    public static void Validate(ArtifactToolListOptions options, bool constructor)
    {
        ArgumentNullException.ThrowIfNull(options);
        switch (options.SkipMode)
        {
            case ArtifactSkipMode.None:
            case ArtifactSkipMode.FastExit:
            case ArtifactSkipMode.FastExitFull:
            case ArtifactSkipMode.Known:
            case ArtifactSkipMode.KnownFull:
                break;
            default:
                if (constructor)
                {
                    throw new ArgumentException($"Invalid {nameof(ArtifactToolListOptions)}.{nameof(SkipMode)}");
                }
                throw new InvalidOperationException($"Invalid {nameof(ArtifactToolListOptions)}.{nameof(SkipMode)}");
        }
    }
}
