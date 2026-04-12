namespace Art.Extensions.BrowserCookies;

/// <summary>
/// Represents a method that accepts a log consisting of an optional title and optional body.
/// </summary>
/// <param name="title">Log title.</param>
/// <param name="body">Log body.</param>
public delegate void LogHandler(string? title, string? body);
