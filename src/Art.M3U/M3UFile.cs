/*
 * https://github.com/Riboe/M3USharp/blob/master/M3UParser/M3UFile.cs
 * @9d5d04d
 * MIT-licensed by Riboe
 */

namespace Art.M3U;

/// <summary>
/// Represents an M3U container.
/// </summary>
public class M3UFile
{
    /// <summary>
    /// Encryption information.
    /// </summary>
    public M3UEncryptionInfo? EncryptionInfo { get; set; }

    private readonly List<StreamInfo> _streams = [];
    private readonly List<AlternateStreamInfo> _alternateStreams = [];

    /// <summary>
    /// Data lines.
    /// </summary>
    public List<string> DataLines { get; set; } = [];

    /// <summary>
    /// M3U version.
    /// </summary>
    public string? Version { get; internal set; }

    /// <summary>
    /// First media sequence number.
    /// </summary>
    // It's OK if default is 0 because it's 0 implicitly by spec
    public long FirstMediaSequenceNumber { get; set; }

    /// <summary>
    /// Whether this stream has separate sub streams.
    /// </summary>
    public bool HasIndependentSegments { get; set; }

    /// <summary>
    /// Available streams.
    /// </summary>
    public IReadOnlyList<StreamInfo> Streams => _streams;

    /// <summary>
    /// Available alternate streams.
    /// </summary>
    public IReadOnlyList<AlternateStreamInfo> AlternateStreams => _alternateStreams;

    internal void AddStream(StreamInfo stream)
    {
        _streams.Add(stream);
    }

    internal void AddAlternateStream(AlternateStreamInfo stream)
    {
        _alternateStreams.Add(stream);
    }
}
