/*
 * https://github.com/Riboe/M3USharp/blob/master/M3UParser/StreamInfo.cs
 * @9d5d04d
 * MIT-licensed by Riboe
 */

namespace Art.M3U;

/// <summary>
/// Stream info.
/// </summary>
public class StreamInfo
{
    /// <summary>
    /// Path.
    /// </summary>
    public string Path { get; internal set; } = "";

    /// <summary>
    /// Bandwidth.
    /// </summary>
    public long Bandwidth { get; internal set; }

    /// <summary>
    /// Average bandwidth.
    /// </summary>
    public long AverageBandwidth { get; internal set; }

    /// <summary>
    /// Name.
    /// </summary>
    public string? Name { get; internal set; }

    /// <summary>
    /// Codecs.
    /// </summary>
    public string? Codecs { get; internal set; }

    /// <summary>
    /// Resolution width.
    /// </summary>
    public int ResolutionWidth { get; internal set; }

    /// <summary>
    /// Resolution height.
    /// </summary>
    public int ResolutionHeight { get; internal set; }

    /// <summary>
    /// Audio group, when part of a parent stream with independent audio stream.
    /// </summary>
    public string? Audio { get; internal set; }
}
