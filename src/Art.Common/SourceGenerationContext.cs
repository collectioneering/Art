using System.Text.Json;
using System.Text.Json.Serialization;

namespace Art.Common;


internal partial class SourceGenerationContext
{
    internal static SourceGenerationContextImpl s_context => SourceGenerationContextImpl.s_context;

    /*[JsonSourceGenerationOptions(
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]*/
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(ulong))]
    [JsonSerializable(typeof(long))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(string[]))]
    [JsonSerializable(typeof(JsonElement))]
    [JsonSerializable(typeof(ArtifactToolProfile))]
    [JsonSerializable(typeof(ArtifactToolProfile[]))]
    [JsonSerializable(typeof(IReadOnlyList<ArtifactToolProfile>))]
    [JsonSerializable(typeof(ArtifactInfo))]
    [JsonSerializable(typeof(ArtifactResourceInfo))]
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