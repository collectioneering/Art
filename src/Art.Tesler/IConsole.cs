namespace Art.Tesler;

public interface IConsole
{
    /// <summary>
    /// Error output.
    /// </summary>
    public TextWriter Error { get; }

    /// <summary>
    /// Output.
    /// </summary>
    public TextWriter Out { get; }

    // TODO below isn't necessary anymore

    /// <summary>
    /// If true, error is redirected.
    /// </summary>
    public bool IsErrorRedirected { get; }

    /// <summary>
    /// If true, output is redirected.
    /// </summary>
    public bool IsOutputRedirected { get; }

    /// <summary>
    /// If true, input is redirected.
    /// </summary>
    public bool IsInputRedirected { get; }
}
