using System.Text.Json;
using System.Text.Json.Serialization;

namespace Art.BrowserCookies;

internal partial class SourceGenerationContext
{
    internal static SourceGenerationContextImpl SharedContext => SourceGenerationContextImpl.s_context;

    [JsonSerializable(typeof(Chromium.ChromiumWindowsLocalState))]
    [JsonSerializable(typeof(Chromium.ChromiumPreferences))]
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
