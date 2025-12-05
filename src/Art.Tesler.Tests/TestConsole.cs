namespace Art.Tesler.Tests;

public class TestConsole : IConsole
{
    private readonly int _windowWidth;
    private readonly bool _outputRedirected;
    private readonly bool _errorRedirected;
    private readonly bool _inputRedirected;

    public TestConsole(TextWriter outWriter, TextWriter errorWriter, int windowWidth, bool outputRedirected, bool errorRedirected, bool inputRedirected)
    {
        _windowWidth = windowWidth;
        _outputRedirected = outputRedirected;
        _errorRedirected = errorRedirected;
        _inputRedirected = inputRedirected;
        Out = outWriter;
        Error = errorWriter;
    }

    /// <inheritdoc />
    public TextWriter Error { get; }

    /// <inheritdoc />
    public bool IsErrorRedirected => _errorRedirected;

    /// <inheritdoc />
    public TextWriter Out { get; }

    /// <inheritdoc />
    public bool IsOutputRedirected => _outputRedirected;

    /// <inheritdoc />
    public bool IsInputRedirected => _inputRedirected;

    internal int GetWindowWidth() => _windowWidth;
}
