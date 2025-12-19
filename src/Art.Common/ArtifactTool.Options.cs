using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Art.Common;

public partial class ArtifactTool
{
    #region Options

    #region Base

    /// <summary>
    /// Attempts to get option or throw exception if not found or if null.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="optKey">Key to search.</param>
    /// <returns>Value, if located and nonnull.</returns>
    /// <exception cref="ArtifactToolOptionNotFoundException">Thrown when option is not found.</exception>
    /// <exception cref="JsonException">Thrown when conversion failed.</exception>
    /// <exception cref="NotSupportedException">Thrown when type not supported.</exception>
    /// <exception cref="NullJsonDataException">Thrown for null JSON.</exception>
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
    public T GetOption<T>(string optKey)
    {
        return Profile.Options.GetOption<T>(optKey);
    }

    /// <summary>
    /// Attempts to get option or throw exception if not found or if null.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="optKey">Key to search.</param>
    /// <param name="jsonTypeInfo">JSON type info.</param>
    /// <returns>Value, if located and nonnull.</returns>
    /// <exception cref="ArtifactToolOptionNotFoundException">Thrown when option is not found.</exception>
    /// <exception cref="JsonException">Thrown when conversion failed.</exception>
    /// <exception cref="NotSupportedException">Thrown when type not supported.</exception>
    /// <exception cref="NullJsonDataException">Thrown for null JSON.</exception>
    public T GetOption<T>(string optKey, JsonTypeInfo<T> jsonTypeInfo)
    {
        return Profile.Options.GetOption(optKey, jsonTypeInfo);
    }

    /// <summary>
    /// Attempts to get option or throw exception if not found or if null.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="optKey">Key to search.</param>
    /// <param name="value">Value to set.</param>
    /// <param name="throwIfIncorrectType">If true, throw a <see cref="JsonException"/> if type is wrong.</param>
    /// <exception cref="JsonException">Thrown when conversion failed.</exception>
    /// <exception cref="NotSupportedException">Thrown when type not supported.</exception>
    /// <exception cref="NullJsonDataException">Thrown for null JSON.</exception>
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
    public void GetOption<T>(string optKey, ref T value, bool throwIfIncorrectType = false)
    {
        Profile.Options.GetOption(optKey, ref value, throwIfIncorrectType);
    }

    /// <summary>
    /// Attempts to get option or throw exception if not found or if null.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="optKey">Key to search.</param>
    /// <param name="value">Value to set.</param>
    /// <param name="jsonTypeInfo">JSON type info.</param>
    /// <param name="throwIfIncorrectType">If true, throw a <see cref="JsonException"/> if type is wrong.</param>
    /// <exception cref="JsonException">Thrown when conversion failed.</exception>
    /// <exception cref="NotSupportedException">Thrown when type not supported.</exception>
    /// <exception cref="NullJsonDataException">Thrown for null JSON.</exception>
    public void GetOption<T>(string optKey, ref T value, JsonTypeInfo<T> jsonTypeInfo, bool throwIfIncorrectType = false)
    {
        Profile.Options.GetOption(optKey, ref value, jsonTypeInfo, throwIfIncorrectType);
    }

    /// <summary>
    /// Attempts to get option.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="optKey">Key to search.</param>
    /// <param name="value">Value, if located and nonnull.</param>
    /// <param name="throwIfIncorrectType">If true, throw a <see cref="JsonException"/> if type is wrong.</param>
    /// <returns>True if value is located and of the right type.</returns>
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
    public bool TryGetOption<T>(string optKey, [NotNullWhen(true)] out T? value, bool throwIfIncorrectType = false)
    {
        return Profile.Options.TryGetOption(optKey, out value, throwIfIncorrectType);
    }

    /// <summary>
    /// Attempts to get option.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="optKey">Key to search.</param>
    /// <param name="value">Value, if located and nonnull.</param>
    /// <param name="jsonTypeInfo">JSON type info.</param>
    /// <param name="throwIfIncorrectType">If true, throw a <see cref="JsonException"/> if type is wrong.</param>
    /// <returns>True if value is located and of the right type.</returns>
    public bool TryGetOption<T>(string optKey, [NotNullWhen(true)] out T? value, JsonTypeInfo<T> jsonTypeInfo, bool throwIfIncorrectType = false)
    {
        return Profile.Options.TryGetOption(optKey, out value, jsonTypeInfo, throwIfIncorrectType);
    }

    #endregion

    #region String

    /// <summary>
    /// Attempts to get option, converted to string with <see cref="Object.ToString"/>, and throws exception if not found.
    /// </summary>
    /// <param name="optKey">Key to search.</param>
    /// <returns>Value, if located.</returns>
    /// <exception cref="ArtifactToolOptionNotFoundException">Thrown when option is not found.</exception>
    public string GetOptionToString(string optKey)
    {
        return Profile.Options.GetOptionToString(optKey);
    }

    /// <summary>
    /// Attempts to get option, converted to string with <see cref="Object.ToString"/>.
    /// </summary>
    /// <param name="optKey">Key to search.</param>
    /// <param name="value">Value to set.</param>
    /// <exception cref="NullJsonDataException">Thrown for null JSON.</exception>
    public void GetOptionToString(string optKey, ref string value)
    {
        Profile.Options.GetOptionToString(optKey, ref value);
    }

    /// <summary>
    /// Attempts to get option, converted to string with <see cref="Object.ToString"/>.
    /// </summary>
    /// <param name="optKey">Key to search.</param>
    /// <param name="value">Value, if located and nonnull.</param>
    /// <returns>True if value is located and of the right type.</returns>
    public bool TryGetOptionToString(string optKey, [NotNullWhen(true)] out string? value)
    {
        return Profile.Options.TryGetOptionToString(optKey, out value);
    }

    /// <summary>
    /// Gets a string option from a value converted to string with <see cref="Object.ToString"/>, or take value from <see cref="ArtifactToolProfile.Group"/>.
    /// </summary>
    /// <param name="optKey">Key to search.</param>
    /// <returns>Option value.</returns>
    public string GetOptionToStringOrGroup(string optKey)
    {
        return Profile.Options.GetOptionToStringOrGroup(optKey, Profile.Group);
    }

    #endregion

    #region Flag

    /// <summary>
    /// Checks if a flag is true.
    /// </summary>
    /// <param name="optKey">Key to search.</param>
    /// <param name="throwIfIncorrectType">If true, throw a <see cref="JsonException"/> or <see cref="NotSupportedException"/> if type is wrong.</param>
    /// <returns>True if flag is set to true.</returns>
    /// <exception cref="JsonException">Thrown when conversion failed.</exception>
    /// <exception cref="NotSupportedException">Thrown when type not supported.</exception>
    public bool GetFlag(string optKey, bool throwIfIncorrectType = false)
    {
        return Profile.Options.GetFlag(optKey, throwIfIncorrectType);
    }

    /// <summary>
    /// Modifies a ref bool if a flag option is present.
    /// </summary>
    /// <param name="optKey">Key to search.</param>
    /// <param name="flag">Value to set.</param>
    /// <param name="throwIfIncorrectType">If true, throw a <see cref="JsonException"/> or <see cref="NotSupportedException"/> if type is wrong.</param>
    /// <exception cref="JsonException">Thrown when conversion failed.</exception>
    /// <exception cref="NotSupportedException">Thrown when type not supported.</exception>
    public void GetFlag(string optKey, ref bool flag, bool throwIfIncorrectType = false)
    {
        Profile.Options.GetFlag(optKey, ref flag, throwIfIncorrectType);
    }

    /// <summary>
    /// Checks if a flag is true.
    /// </summary>
    /// <param name="optKey">Key to search.</param>
    /// <param name="flag">Value, if found and nonnull.</param>
    /// <param name="throwIfIncorrectType">If true, throw a <see cref="JsonException"/> or <see cref="NotSupportedException"/> if type is wrong.</param>
    /// <returns>True if value is located.</returns>
    /// <exception cref="JsonException">Thrown when conversion failed.</exception>
    /// <exception cref="NotSupportedException">Thrown when type not supported.</exception>
    public bool TryGetFlag(string optKey, out bool flag, bool throwIfIncorrectType = false)
    {
        return Profile.Options.TryGetFlag(optKey, out flag, throwIfIncorrectType);
    }

    #endregion

    #region Int

    /// <summary>
    /// Gets an Int64 option from a string or literal value.
    /// </summary>
    /// <param name="optKey">Key to search.</param>
    /// <param name="throwIfIncorrectType">If true, throw a <see cref="JsonException"/> or <see cref="NotSupportedException"/> if type is wrong.</param>
    /// <returns>Value.</returns>
    /// <exception cref="ArtifactToolOptionNotFoundException">Thrown when option is not found.</exception>
    /// <exception cref="JsonException">Thrown when conversion failed.</exception>
    /// <exception cref="NotSupportedException">Thrown when type not supported.</exception>
    public long GetInt64Option(string optKey, bool throwIfIncorrectType = false)
    {
        return Profile.Options.GetInt64Option(optKey, throwIfIncorrectType);
    }

    /// <summary>
    /// Attempts to get an Int64 option from a string or literal value.
    /// </summary>
    /// <param name="optKey">Key to search.</param>
    /// <param name="value">Value to set.</param>
    /// <param name="throwIfIncorrectType">If true, throw a <see cref="JsonException"/> or <see cref="NotSupportedException"/> if type is wrong.</param>
    /// <returns>True if found.</returns>
    /// <exception cref="JsonException">Thrown when conversion failed.</exception>
    /// <exception cref="NotSupportedException">Thrown when type not supported.</exception>
    public void GetInt64Option(string optKey, ref long value, bool throwIfIncorrectType = false)
    {
        Profile.Options.GetInt64Option(optKey, ref value, throwIfIncorrectType);
    }

    /// <summary>
    /// Attempts to get a Int64 option from a string or literal value.
    /// </summary>
    /// <param name="optKey">Key to search.</param>
    /// <param name="value">Value, if located and nonnull.</param>
    /// <param name="throwIfIncorrectType">If true, throw a <see cref="JsonException"/> or <see cref="NotSupportedException"/> if type is wrong.</param>
    /// <returns>True if found.</returns>
    /// <exception cref="JsonException">Thrown when conversion failed.</exception>
    /// <exception cref="NotSupportedException">Thrown when type not supported.</exception>
    public bool TryGetInt64Option(string optKey, [NotNullWhen(true)] out long? value, bool throwIfIncorrectType = false)
    {
        return Profile.Options.TryGetInt64Option(optKey, out value, throwIfIncorrectType);
    }

    /// <summary>
    /// Gets an Int64 option from a string or literal value, or parses value from <see cref="ArtifactToolProfile.Group"/>.
    /// </summary>
    /// <param name="optKey">Key to search.</param>
    /// <param name="throwIfIncorrectType">If true, throw a <see cref="JsonException"/> or <see cref="NotSupportedException"/> if type is wrong.</param>
    /// <returns>Option value.</returns>
    /// <exception cref="JsonException">Thrown when conversion failed.</exception>
    /// <exception cref="NotSupportedException">Thrown when type not supported.</exception>
    public long GetInt64OptionOrGroup(string optKey, bool throwIfIncorrectType = false)
    {
        return Profile.Options.GetInt64OptionOrGroup(optKey, Profile.Group, throwIfIncorrectType);
    }

    #endregion

    #region UInt

    /// <summary>
    /// Gets a UInt64 option from a string or literal value.
    /// </summary>
    /// <param name="optKey">Key to search.</param>
    /// <param name="throwIfIncorrectType">If true, throw a <see cref="JsonException"/> or <see cref="NotSupportedException"/> if type is wrong.</param>
    /// <returns>Value.</returns>
    /// <exception cref="ArtifactToolOptionNotFoundException">Thrown when option is not found.</exception>
    /// <exception cref="JsonException">Thrown when conversion failed.</exception>
    /// <exception cref="NotSupportedException">Thrown when type not supported.</exception>
    public ulong GetUInt64Option(string optKey, bool throwIfIncorrectType = false)
    {
        return Profile.Options.GetUInt64Option(optKey, throwIfIncorrectType);
    }

    /// <summary>
    /// Attempts to get a UInt64 option from a string or literal value.
    /// </summary>
    /// <param name="optKey">Key to search.</param>
    /// <param name="value">Value to set.</param>
    /// <param name="throwIfIncorrectType">If true, throw a <see cref="JsonException"/> or <see cref="NotSupportedException"/> if type is wrong.</param>
    /// <returns>True if found.</returns>
    /// <exception cref="JsonException">Thrown when conversion failed.</exception>
    /// <exception cref="NotSupportedException">Thrown when type not supported.</exception>
    public void GetUInt64Option(string optKey, ref ulong value, bool throwIfIncorrectType = false)
    {
        Profile.Options.GetUInt64Option(optKey, ref value, throwIfIncorrectType);
    }

    /// <summary>
    /// Attempts to get an UInt64 option from a string or literal value.
    /// </summary>
    /// <param name="optKey">Key to search.</param>
    /// <param name="value">Value, if located and nonnull.</param>
    /// <param name="throwIfIncorrectType">If true, throw a <see cref="JsonException"/> or <see cref="NotSupportedException"/> if type is wrong.</param>
    /// <returns>True if found.</returns>
    /// <exception cref="JsonException">Thrown when conversion failed.</exception>
    /// <exception cref="NotSupportedException">Thrown when type not supported.</exception>
    public bool TryGetUInt64Option(string optKey, [NotNullWhen(true)] out ulong? value, bool throwIfIncorrectType = false)
    {
        return Profile.Options.TryGetUInt64Option(optKey, out value, throwIfIncorrectType);
    }

    /// <summary>
    /// Gets a UInt64 option from a string or literal value, or parses value from <see cref="ArtifactToolProfile.Group"/>.
    /// </summary>
    /// <param name="optKey">Key to search.</param>
    /// <param name="throwIfIncorrectType">If true, throw a <see cref="JsonException"/> or <see cref="NotSupportedException"/> if type is wrong.</param>
    /// <returns>Option value.</returns>
    /// <exception cref="JsonException">Thrown when conversion failed.</exception>
    /// <exception cref="NotSupportedException">Thrown when type not supported.</exception>
    public ulong GetUInt64OptionOrGroup(string optKey, bool throwIfIncorrectType = false)
    {
        return Profile.Options.GetUInt64OptionOrGroup(optKey, Profile.Group, throwIfIncorrectType);
    }

    #endregion

    #endregion
}
