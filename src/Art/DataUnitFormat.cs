namespace Art;

/// <summary>
/// Specifies the format of units to use when displaying data units.
/// </summary>
public enum DataUnitFormat
{
    /// <summary>
    /// Use short units (B, kB, MB, etc. or bytes, B, KiB, MiB, etc.).
    /// </summary>
    Short,
    /// <summary>
    /// Use short units (bytes, kilobytes, megabytes, etc. or bytes, kibibytes, mebibytes, etc.).
    /// </summary>
    Long,
}
