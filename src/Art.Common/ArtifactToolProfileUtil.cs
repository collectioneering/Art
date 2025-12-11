#define USE_PRESERIALIZE
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Art.Common;

/// <summary>
/// Utility for <see cref="ArtifactToolProfile"/>.
/// </summary>
public static class ArtifactToolProfileUtil
{
    /// <summary>
    /// Deserializes profiles.
    /// </summary>
    /// <param name="path">Path to file containing profile or profile array.</param>
    /// <returns>Array of profiles.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="path"/> is null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static ArtifactToolProfile[] DeserializeProfilesFromFile(string path)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        return DeserializeProfilesFromFileInternal(path, SourceGenerationContext.s_context, out _);
    }

    /// <summary>
    /// Deserializes profiles.
    /// </summary>
    /// <param name="path">Path to file containing profile or profile array.</param>
    /// <param name="options">Custom serializer options.</param>
    /// <returns>Array of profiles.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="path"/> or <paramref name="options"/> are null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static ArtifactToolProfile[] DeserializeProfilesFromFile(string path, JsonSerializerOptions options)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        if (options == null) throw new ArgumentNullException(nameof(options));
        return DeserializeProfilesFromFileInternal(path, new SourceGenerationContext(options), out _);
    }

    /// <summary>
    /// Deserializes profiles.
    /// </summary>
    /// <param name="path">Path to file containing profile or profile array.</param>
    /// <param name="isSingleObject">If true, deserialized content came from a single JSON object as opposed to an array.</param>
    /// <returns>Array of profiles.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="path"/> is null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static ArtifactToolProfile[] DeserializeProfilesFromFile(string path, out bool isSingleObject)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        return DeserializeProfilesFromFileInternal(path, SourceGenerationContext.s_context, out isSingleObject);
    }

    /// <summary>
    /// Deserializes profiles.
    /// </summary>
    /// <param name="path">Path to file containing profile or profile array.</param>
    /// <param name="isSingleObject">If true, deserialized content came from a single JSON object as opposed to an array.</param>
    /// <param name="options">Custom serializer options.</param>
    /// <returns>Array of profiles.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="path"/> or <paramref name="options"/> are null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static ArtifactToolProfile[] DeserializeProfilesFromFile(string path, JsonSerializerOptions options, out bool isSingleObject)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        if (options == null) throw new ArgumentNullException(nameof(options));
        return DeserializeProfilesFromFileInternal(path, new SourceGenerationContext(options), out isSingleObject);
    }

    private static ArtifactToolProfile[] DeserializeProfilesFromFileInternal(
        string path,
        SourceGenerationContext sourceGenerationContext,
        out bool isSingleObject)
    {
        return DeserializeProfilesInternal(JsonSerializer.Deserialize(File.ReadAllText(path), sourceGenerationContext.JsonElement), sourceGenerationContext, out isSingleObject);
    }

    /// <summary>
    /// Serializes profiles to a file.
    /// </summary>
    /// <param name="path">Path to write profiles to.</param>
    /// <param name="profiles">Array of profiles.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="profiles"/> is null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static void SerializeProfilesToFile(string path, params ArtifactToolProfile[] profiles)
    {
        if (profiles == null) throw new ArgumentNullException(nameof(profiles));
        SerializeProfilesToFileInternal(path, SourceGenerationContext.s_context, profiles);
    }

    /// <summary>
    /// Serializes profiles to a file.
    /// </summary>
    /// <param name="path">Path to write profiles to.</param>
    /// <param name="options">Custom serializer options.</param>
    /// <param name="profiles">Array of profiles.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> or <paramref name="profiles"/> are null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static void SerializeProfilesToFile(string path, JsonSerializerOptions options, params ArtifactToolProfile[] profiles)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (profiles == null) throw new ArgumentNullException(nameof(profiles));
        SerializeProfilesToFileInternal(path, new SourceGenerationContext(options), profiles);
    }

    /// <summary>
    /// Serializes profiles to a file.
    /// </summary>
    /// <param name="path">Path to write profiles to.</param>
    /// <param name="profiles">Array of profiles.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="profiles"/> is null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static void SerializeProfilesToFile(string path, IReadOnlyList<ArtifactToolProfile> profiles)
    {
        if (profiles == null) throw new ArgumentNullException(nameof(profiles));
        SerializeProfilesToFileInternal(path, SourceGenerationContext.s_context, profiles);
    }

    /// <summary>
    /// Serializes profiles to a file.
    /// </summary>
    /// <param name="path">Path to write profiles to.</param>
    /// <param name="options">Custom serializer options.</param>
    /// <param name="profiles">Array of profiles.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> or <paramref name="profiles"/> are null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static void SerializeProfilesToFile(string path, JsonSerializerOptions options, IReadOnlyList<ArtifactToolProfile> profiles)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (profiles == null) throw new ArgumentNullException(nameof(profiles));
        SerializeProfilesToFileInternal(path, new SourceGenerationContext(options), profiles);
    }

    /// <summary>
    /// Serializes a profile to a file.
    /// </summary>
    /// <param name="path">Path to write profile to.</param>
    /// <param name="profile">Profile.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="profile"/> is null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static void SerializeProfileToFile(string path, ArtifactToolProfile profile)
    {
        if (profile == null) throw new ArgumentNullException(nameof(profile));
        SerializeProfileToFileInternal(path, SourceGenerationContext.s_context, profile);
    }

    /// <summary>
    /// Serializes a profile to a file.
    /// </summary>
    /// <param name="path">Path to write profile to.</param>
    /// <param name="options">Custom serializer options.</param>
    /// <param name="profile">Profile.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> or <paramref name="profile"/> is null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static void SerializeProfileToFile(string path, JsonSerializerOptions options, ArtifactToolProfile profile)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (profile == null) throw new ArgumentNullException(nameof(profile));
        SerializeProfileToFileInternal(path, new SourceGenerationContext(options), profile);
    }

    private static void SerializeProfilesToFileInternal(string path, SourceGenerationContext sourceGenerationContext, IReadOnlyList<ArtifactToolProfile> profiles)
    {
        SerializeToFile(path, profiles, sourceGenerationContext.IReadOnlyListArtifactToolProfile);
    }

    private static void SerializeProfileToFileInternal(string path, SourceGenerationContext sourceGenerationContext, ArtifactToolProfile profile)
    {
        SerializeToFile(path, profile, sourceGenerationContext.ArtifactToolProfile);
    }

    private static void SerializeToFile<T>(string path, T value, JsonTypeInfo<T> jsonTypeInfo)
    {
#if USE_PRESERIALIZE
        using var ms = new MemoryStream();
        JsonSerializer.Serialize(ms, value, jsonTypeInfo);
        ms.Position = 0;
        using var fs = File.Create(path);
        ms.CopyTo(fs);
#else
        using var fs = File.Create(path);
        JsonSerializer.Serialize(fs, value, jsonTypeInfo);
#endif
    }

    /// <summary>
    /// Deserializes profiles.
    /// </summary>
    /// <param name="utf8Stream">UTF-8 stream containing profile or profile array.</param>
    /// <returns>Array of profiles.</returns>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static ArtifactToolProfile[] DeserializeProfiles(Stream utf8Stream)
    {
        if (utf8Stream == null) throw new ArgumentNullException(nameof(utf8Stream));
        return DeserializeProfilesInternal(utf8Stream, SourceGenerationContext.s_context, out _);
    }

    /// <summary>
    /// Deserializes profiles.
    /// </summary>
    /// <param name="utf8Stream">UTF-8 stream containing profile or profile array.</param>
    /// <param name="options">Custom serializer options.</param>
    /// <returns>Array of profiles.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static ArtifactToolProfile[] DeserializeProfiles(Stream utf8Stream, JsonSerializerOptions options)
    {
        if (utf8Stream == null) throw new ArgumentNullException(nameof(utf8Stream));
        if (options == null) throw new ArgumentNullException(nameof(options));
        return DeserializeProfilesInternal(utf8Stream, new SourceGenerationContext(options), out _);
    }

    /// <summary>
    /// Deserializes profiles.
    /// </summary>
    /// <param name="utf8Stream">UTF-8 stream containing profile or profile array.</param>
    /// <param name="isSingleObject">If true, deserialized content came from a single JSON object as opposed to an array.</param>
    /// <returns>Array of profiles.</returns>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static ArtifactToolProfile[] DeserializeProfiles(Stream utf8Stream, out bool isSingleObject)
    {
        if (utf8Stream == null) throw new ArgumentNullException(nameof(utf8Stream));
        return DeserializeProfilesInternal(utf8Stream, SourceGenerationContext.s_context, out isSingleObject);
    }

    /// <summary>
    /// Deserializes profiles.
    /// </summary>
    /// <param name="utf8Stream">UTF-8 stream containing profile or profile array.</param>
    /// <param name="isSingleObject">If true, deserialized content came from a single JSON object as opposed to an array.</param>
    /// <param name="options">Custom serializer options.</param>
    /// <returns>Array of profiles.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static ArtifactToolProfile[] DeserializeProfiles(Stream utf8Stream, JsonSerializerOptions options, out bool isSingleObject)
    {
        if (utf8Stream == null) throw new ArgumentNullException(nameof(utf8Stream));
        if (options == null) throw new ArgumentNullException(nameof(options));
        return DeserializeProfilesInternal(utf8Stream, new SourceGenerationContext(options), out isSingleObject);
    }

    /// <summary>
    /// Serializes profiles.
    /// </summary>
    /// <param name="utf8Stream">UTF-8 stream to output to.</param>
    /// <param name="profiles">Array of profiles.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="profiles"/> is null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static void SerializeProfiles(Stream utf8Stream, params ArtifactToolProfile[] profiles)
    {
        if (utf8Stream == null) throw new ArgumentNullException(nameof(utf8Stream));
        if (profiles == null) throw new ArgumentNullException(nameof(profiles));
        SerializeProfilesInternal(utf8Stream, SourceGenerationContext.s_context, profiles);
    }

    /// <summary>
    /// Serializes profiles.
    /// </summary>
    /// <param name="utf8Stream">UTF-8 stream to output to.</param>
    /// <param name="options">Custom serializer options.</param>
    /// <param name="profiles">Array of profiles.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> or <paramref name="profiles"/> are null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static void SerializeProfiles(Stream utf8Stream, JsonSerializerOptions options, params ArtifactToolProfile[] profiles)
    {
        if (utf8Stream == null) throw new ArgumentNullException(nameof(utf8Stream));
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (profiles == null) throw new ArgumentNullException(nameof(profiles));
        SerializeProfilesInternal(utf8Stream, new SourceGenerationContext(options), profiles);
    }

    /// <summary>
    /// Serializes profiles.
    /// </summary>
    /// <param name="utf8Stream">UTF-8 stream to output to.</param>
    /// <param name="profiles">Array of profiles.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="profiles"/> is null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static void SerializeProfiles(Stream utf8Stream, IReadOnlyList<ArtifactToolProfile> profiles)
    {
        if (utf8Stream == null) throw new ArgumentNullException(nameof(utf8Stream));
        if (profiles == null) throw new ArgumentNullException(nameof(profiles));
        SerializeProfilesInternal(utf8Stream, SourceGenerationContext.s_context, profiles);
    }

    /// <summary>
    /// Serializes profiles.
    /// </summary>
    /// <param name="utf8Stream">UTF-8 stream to output to.</param>
    /// <param name="options">Custom serializer options.</param>
    /// <param name="profiles">Array of profiles.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> or <paramref name="profiles"/> are null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static void SerializeProfiles(Stream utf8Stream, JsonSerializerOptions options, IReadOnlyList<ArtifactToolProfile> profiles)
    {
        if (utf8Stream == null) throw new ArgumentNullException(nameof(utf8Stream));
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (profiles == null) throw new ArgumentNullException(nameof(profiles));
        SerializeProfilesInternal(utf8Stream, new SourceGenerationContext(options), profiles);
    }

    private static void SerializeProfilesInternal(Stream utf8Stream, SourceGenerationContext sourceGenerationContext, IReadOnlyList<ArtifactToolProfile> profiles)
    {
#if USE_PRESERIALIZE
        using var ms = new MemoryStream();
        JsonSerializer.Serialize(ms, profiles, sourceGenerationContext.IReadOnlyListArtifactToolProfile);
        ms.Position = 0;
        ms.CopyTo(utf8Stream);
#else
        JsonSerializer.Serialize(utf8Stream, profiles, sourceGenerationContext.IReadOnlyListArtifactToolProfile);
#endif
    }

    /// <summary>
    /// Deserializes profiles.
    /// </summary>
    /// <param name="element">Element containing profile or profile array.</param>
    /// <returns>Array of profiles.</returns>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static ArtifactToolProfile[] DeserializeProfiles(JsonElement element)
    {
        return DeserializeProfilesInternal(element, SourceGenerationContext.s_context, out _);
    }

    /// <summary>
    /// Deserializes profiles.
    /// </summary>
    /// <param name="element">Element containing profile or profile array.</param>
    /// <param name="options">Custom serializer options.</param>
    /// <returns>Array of profiles.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static ArtifactToolProfile[] DeserializeProfiles(JsonElement element, JsonSerializerOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        return DeserializeProfilesInternal(element, new SourceGenerationContext(options), out _);
    }

    /// <summary>
    /// Deserializes profiles.
    /// </summary>
    /// <param name="element">Element containing profile or profile array.</param>
    /// <param name="isSingleObject">If true, deserialized content came from a single JSON object as opposed to an array.</param>
    /// <returns>Array of profiles.</returns>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static ArtifactToolProfile[] DeserializeProfiles(JsonElement element, out bool isSingleObject)
    {
        return DeserializeProfilesInternal(element, SourceGenerationContext.s_context, out isSingleObject);
    }

    /// <summary>
    /// Deserializes profiles.
    /// </summary>
    /// <param name="element">Element containing profile or profile array.</param>
    /// <param name="options">Custom serializer options.</param>
    /// <param name="isSingleObject">If true, deserialized content came from a single JSON object as opposed to an array.</param>
    /// <returns>Array of profiles.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
    /// <exception cref="InvalidDataException">Thrown if null value encountered.</exception>
    public static ArtifactToolProfile[] DeserializeProfiles(JsonElement element, JsonSerializerOptions options, out bool isSingleObject)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        return DeserializeProfilesInternal(element, new SourceGenerationContext(options), out isSingleObject);
    }

    private static ArtifactToolProfile[] DeserializeProfilesInternal(
        Stream utf8Stream,
        SourceGenerationContext sourceGenerationContext,
        out bool isSingleObject)
    {
        return DeserializeProfilesInternal(JsonSerializer.Deserialize(utf8Stream, sourceGenerationContext.JsonElement), sourceGenerationContext, out isSingleObject);
    }

    private static ArtifactToolProfile[] DeserializeProfilesInternal(
        JsonElement element,
        SourceGenerationContext sourceGenerationContext,
        out bool isSingleObject)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            isSingleObject = true;
            return [element.Deserialize(sourceGenerationContext.ArtifactToolProfile) ?? throw new InvalidDataException()];
        }
        isSingleObject = false;
        return element.Deserialize(sourceGenerationContext.ArtifactToolProfileArray) ?? throw new InvalidDataException();
    }

    /// <summary>
    /// Serializes profiles.
    /// </summary>
    /// <param name="profiles">Array of profiles.</param>
    /// <returns>Serialized profiles.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="profiles"/> is null.</exception>
    public static JsonElement SerializeProfiles(params ArtifactToolProfile[] profiles)
    {
        if (profiles == null) throw new ArgumentNullException(nameof(profiles));
        return SerializeProfilesInternal(SourceGenerationContext.s_context, profiles);
    }

    /// <summary>
    /// Serializes profiles.
    /// </summary>
    /// <param name="options">Custom serializer options.</param>
    /// <param name="profiles">Array of profiles.</param>
    /// <returns>Serialized profiles.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> or <paramref name="profiles"/> are null.</exception>
    public static JsonElement SerializeProfiles(JsonSerializerOptions options, params ArtifactToolProfile[] profiles)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (profiles == null) throw new ArgumentNullException(nameof(profiles));
        return SerializeProfilesInternal(new SourceGenerationContext(options), profiles);
    }

    /// <summary>
    /// Serializes profiles.
    /// </summary>
    /// <param name="profiles">Array of profiles.</param>
    /// <returns>Serialized profiles.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="profiles"/> is null.</exception>
    public static JsonElement SerializeProfiles(IReadOnlyList<ArtifactToolProfile> profiles)
    {
        if (profiles == null) throw new ArgumentNullException(nameof(profiles));
        return SerializeProfilesInternal(SourceGenerationContext.s_context, profiles);
    }

    /// <summary>
    /// Serializes profiles.
    /// </summary>
    /// <param name="options">Custom serializer options.</param>
    /// <param name="profiles">Array of profiles.</param>
    /// <returns>Serialized profiles.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> or <paramref name="profiles"/> are null.</exception>
    public static JsonElement SerializeProfiles(JsonSerializerOptions options, IReadOnlyList<ArtifactToolProfile> profiles)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (profiles == null) throw new ArgumentNullException(nameof(profiles));
        return SerializeProfilesInternal(new SourceGenerationContext(options), profiles);
    }

    private static JsonElement SerializeProfilesInternal(SourceGenerationContext sourceGenerationContext, IReadOnlyList<ArtifactToolProfile> profiles)
    {
        return JsonSerializer.SerializeToElement(profiles, sourceGenerationContext.IReadOnlyListArtifactToolProfile);
    }

    /// <summary>
    /// Gets group value or fallback to <paramref name="fallback"/>.
    /// </summary>
    /// <param name="artifactToolProfile">Artifact tool profile.</param>
    /// <param name="fallback">Fallback value to use if profile does not contain a specified group value.</param>
    /// <returns>Group value.</returns>
    public static string GetGroupOrFallback(this ArtifactToolProfile artifactToolProfile, string fallback = "default") => artifactToolProfile.Group ?? fallback;

    /// <summary>
    /// Creates a tool profile for the specified tool.
    /// </summary>
    /// <param name="tool">Target tool string.</param>
    /// <param name="group">Target group.</param>
    /// <param name="options">Options.</param>
    /// <returns>Profile.</returns>
    public static ArtifactToolProfile Create(string tool, string group, params (string, JsonElement)[] options)
        => new(tool, group, options.ToDictionary(v => v.Item1, v => v.Item2));

    /// <summary>
    /// Creates a tool profile for the specified tool.
    /// </summary>
    /// <typeparam name="TTool">Tool type.</typeparam>
    /// <param name="group">Target group.</param>
    /// <param name="options">Options.</param>
    /// <returns>Profile.</returns>
    public static ArtifactToolProfile Create<TTool>(string group, params (string, JsonElement)[] options) where TTool : IArtifactTool
        => new(ArtifactToolIDUtil.CreateToolString<TTool>(), group, options.ToDictionary(v => v.Item1, v => v.Item2));

    /// <summary>
    /// Creates a tool profile for the specified tool.
    /// </summary>
    /// <param name="toolType">Tool type.</param>
    /// <param name="group">Target group.</param>
    /// <param name="options">Options.</param>
    /// <returns>Profile.</returns>
    /// <remarks>
    /// This overload sets <see cref="ArtifactToolProfile.Options"/> to null if no options are specified.
    /// </remarks>
    public static ArtifactToolProfile Create(Type toolType, string group, params (string, JsonElement)[] options)
        => CreateInternal(toolType, group, options, false);

    /// <summary>
    /// Creates a tool profile for the specified tool.
    /// </summary>
    /// <param name="toolType">Tool type.</param>
    /// <param name="group">Target group.</param>
    /// <param name="options">Options.</param>
    /// <returns>Profile.</returns>
    /// <remarks>
    /// This overload sets <see cref="ArtifactToolProfile.Options"/> to a valid dictionary even if no options are specified.
    /// </remarks>
    public static ArtifactToolProfile CreateWithOptions(Type toolType, string group, params (string, JsonElement)[] options)
        => CreateInternal(toolType, group, options, true);

    private static ArtifactToolProfile CreateInternal(Type toolType, string group, (string, JsonElement)[] options, bool alwaysOptions)
        => new(ArtifactToolIDUtil.CreateToolString(toolType), group, options.Length == 0 ? alwaysOptions ? new Dictionary<string, JsonElement>() : null : options.ToDictionary(v => v.Item1, v => v.Item2));

    /// <summary>
    /// Creates an instance of this profile with most derived core type of instance or instance's type.
    /// </summary>
    /// <param name="artifactToolProfile">Profile.</param>
    /// <param name="instance">Instance to derive tool type from.</param>
    /// <returns>Profile.</returns>
    public static ArtifactToolProfile WithCoreTool(this ArtifactToolProfile artifactToolProfile, object instance) => artifactToolProfile.WithCoreTool(instance.GetType());

    /// <summary>
    /// Creates an instance of this profile with most derived core type or given type.
    /// </summary>
    /// <param name="artifactToolProfile">Profile.</param>
    /// <param name="type">Tool type.</param>
    /// <returns>Profile.</returns>
    public static ArtifactToolProfile WithCoreTool(this ArtifactToolProfile artifactToolProfile, Type type) => artifactToolProfile with { Tool = ArtifactToolIDUtil.CreateCoreToolString(type) };

    /// <summary>
    /// Creates an instance of this profile with specified comparer.
    /// </summary>
    /// <param name="artifactToolProfile">Profile.</param>
    /// <param name="comparer">Comparer to use.</param>
    /// <returns>Profile</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="comparer"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if duplicate keys are encountered using the specified comparer.</exception>
    public static ArtifactToolProfile WithOptionsComparer(this ArtifactToolProfile artifactToolProfile, StringComparer comparer)
    {
        if (comparer == null) throw new ArgumentNullException(nameof(comparer));
        return artifactToolProfile.Options == null ? artifactToolProfile : artifactToolProfile with { Options = new Dictionary<string, JsonElement>(artifactToolProfile.Options, comparer) };
    }

    /// <summary>
    /// Creates a new options dictionary for this profile with the specified comparer.
    /// </summary>
    /// <param name="artifactToolProfile">Profile.</param>
    /// <param name="comparer">Comparer to use.</param>
    /// <returns>Profile</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="comparer"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if duplicate keys are encountered using the specified comparer.</exception>
    public static Dictionary<string, JsonElement> GetOptionsWithOptionsComparer(this ArtifactToolProfile artifactToolProfile, StringComparer comparer)
    {
        if (comparer == null) throw new ArgumentNullException(nameof(comparer));
        return artifactToolProfile.Options == null ? new Dictionary<string, JsonElement>() : new Dictionary<string, JsonElement>(artifactToolProfile.Options, comparer);
    }
}
