﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace Art.Tesler.Properties;

public static class ConfigPropertyUtility
{
    public static string FormatPropertyKeyForDisplay(ConfigScope configScope, string key)
    {
        return $"{configScope}::{key}";
    }

    public static string FormatPropertyForDisplay(string key, JsonElement value, bool prettyPrint)
    {
        return $"{key}={FormatPropertyValueForDisplay(value, prettyPrint)}";
    }

    public static string FormatPropertyForDisplay(ConfigProperty configProperty, bool prettyPrint)
    {
        return $"{configProperty.ConfigScope}::{configProperty.Key}={FormatPropertyValueForDisplay(configProperty.Value, prettyPrint)}";
    }

    public static string FormatPropertyForDisplay(ArtifactToolID artifactToolId, ConfigProperty configProperty, bool prettyPrint)
    {
        return $"{artifactToolId}::{configProperty.ConfigScope}::{configProperty.Key}={FormatPropertyValueForDisplay(configProperty.Value, prettyPrint)}";
    }

    public static string FormatPropertyForDisplay(int profileIndex, string profileGroup, ArtifactToolID artifactToolId, ConfigProperty configProperty, bool prettyPrint)
    {
        return $"{profileIndex}::{profileGroup}::{artifactToolId}::{configProperty.ConfigScope}::{configProperty.Key}={FormatPropertyValueForDisplay(configProperty.Value, prettyPrint)}";
    }

    public static string FormatPropertyValueForDisplay(JsonElement value, bool prettyPrint)
    {
        return JsonSerializer.Serialize(value, prettyPrint
            ? PrettyPrintConfigPropertySourceGenerationContext.s_context.JsonElement
            : StandardConfigPropertySourceGenerationContext.s_context.JsonElement);
    }
}


[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(JsonElement))]
internal partial class StandardConfigPropertySourceGenerationContext : JsonSerializerContext
{
    static StandardConfigPropertySourceGenerationContext()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = false, AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip };
        options.Converters.Add(new JsonStringEnumConverter());
        s_context = new StandardConfigPropertySourceGenerationContext(options);
    }

    internal static readonly StandardConfigPropertySourceGenerationContext s_context;
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(JsonElement))]
internal partial class PrettyPrintConfigPropertySourceGenerationContext : JsonSerializerContext
{
    static PrettyPrintConfigPropertySourceGenerationContext()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true, AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip };
        options.Converters.Add(new JsonStringEnumConverter());
        s_context = new PrettyPrintConfigPropertySourceGenerationContext(options);
    }

    internal static readonly PrettyPrintConfigPropertySourceGenerationContext s_context;
}