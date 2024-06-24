using System.Text.Json;

namespace Art.Tesler;

public interface IRunnerDefaultPropertyProvider
{
    /// <summary>
    /// Enumerates default properties. Pairs returned later override earlier values for the same key.
    /// </summary>
    /// <returns>Sequence of key-value pairs for configuration.</returns>
    IEnumerable<KeyValuePair<string, JsonElement>> EnumerateDefaultProperties();

    /// <summary>
    /// Attempts to get default property with the specified key.
    /// </summary>
    /// <param name="key">Property key.</param>
    /// <param name="value">Resolved property key if successful.</param>
    /// <returns>True if successful.</returns>
    public bool TryGetDefaultProperty(string key, out JsonElement value);
}
