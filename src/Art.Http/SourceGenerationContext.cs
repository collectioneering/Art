using System.Text.Json;
using System.Text.Json.Serialization;

namespace Art.Http;

internal partial class SourceGenerationContext
{
    internal static SourceGenerationContextImpl s_context => SourceGenerationContextImpl.s_context;

    //[JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(string[]))]
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