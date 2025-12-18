namespace Art.M3U;

/// <summary>
/// Settings per segment.
/// </summary>
/// <param name="Skip">Allow skipping processing of some segments.</param>
/// <param name="DisableDecryption">Disable segment decryption.</param>
/// <param name="WriteVxFiles">Output VxFile content.</param>
public record SegmentSettings(bool Skip, bool DisableDecryption, bool WriteVxFiles = true);
