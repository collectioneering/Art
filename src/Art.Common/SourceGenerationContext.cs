﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace Art.Common;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(ulong))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(ArtifactToolProfile))]
[JsonSerializable(typeof(ArtifactToolProfile[]))]
[JsonSerializable(typeof(ArtifactInfo))]
[JsonSerializable(typeof(ArtifactResourceInfo))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}