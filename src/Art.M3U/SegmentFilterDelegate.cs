namespace Art.M3U;

/// <summary>
/// Delegate type for filtering segments.
/// </summary>
public delegate SegmentSettings SegmentFilterDelegate(Uri uri, long msn);
