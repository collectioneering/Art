namespace Art.M3U;

/// <summary>
/// Delegate type for performing action before handling segment.
/// </summary>
public delegate PreSegmentAction PreSegmentActionDelegate(Uri uri, long msn);
