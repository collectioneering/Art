namespace Art.M3U;

/// <summary>
/// Defines required steps prior to handling a segment.
/// </summary>
/// <param name="RunHeartbeat">Heartbeat callback must be executed.</param>
public record struct PreSegmentAction(bool RunHeartbeat);
