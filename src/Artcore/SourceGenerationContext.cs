using System.Text.Json;
using System.Text.Json.Serialization;

namespace Artcore;

internal partial class SourceGenerationContext
{
    internal static SourceGenerationContextImpl SharedContext => SourceGenerationContextImpl.s_context;

    /*[JsonSourceGenerationOptions(
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]*/
    [JsonSerializable(typeof(ModuleManifestContent))]
    [JsonSerializable(typeof(ModuleSearchConfiguration))]
    internal partial class SourceGenerationContextImpl : JsonSerializerContext
    {
        static SourceGenerationContextImpl()
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
            s_context = new SourceGenerationContextImpl(options);
        }

        internal static readonly SourceGenerationContextImpl s_context;
    }
}
