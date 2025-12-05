namespace Art.Tesler.Tests;

public class TestConsole : IConsole
{
    public TestConsole(TextWriter outWriter, TextWriter errorWriter)
    {
        Out = outWriter;
        Error = errorWriter;
    }

    /// <inheritdoc />
    public TextWriter Error { get; }

    /// <inheritdoc />
    public TextWriter Out { get; }

}
