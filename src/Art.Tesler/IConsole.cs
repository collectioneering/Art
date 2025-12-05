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
}
