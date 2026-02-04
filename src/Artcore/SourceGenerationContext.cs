using System.Text.Json;
using System.Text.Json.Serialization;

namespace Artcore;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    WriteIndented = true,
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ModuleManifestContent))]
[JsonSerializable(typeof(ModuleSearchConfiguration))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
    static SourceGenerationContext()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new JsonStringEnumConverter());
        s_context = new SourceGenerationContext(options);
    }

    internal static readonly SourceGenerationContext s_context;
}
