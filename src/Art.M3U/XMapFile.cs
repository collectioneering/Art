namespace Art.M3U;

/// <summary>
/// Data from EXT-X-MAP with filename.
/// </summary>
/// <param name="Name">Filename.</param>
/// <param name="Data">Data.</param>
public record XMapFile(string Name, byte[] Data);
