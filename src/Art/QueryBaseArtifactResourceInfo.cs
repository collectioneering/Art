﻿namespace Art;

/// <summary>
/// Provides artifact information.
/// </summary>
/// <param name="Key">Resource key.</param>
/// <param name="Updated">Updated date.</param>
/// <param name="Version">Version.</param>
public record QueryBaseArtifactResourceInfo(ArtifactResourceKey Key, DateTimeOffset? Updated = null, string? Version = null)
    : ArtifactResourceInfo(Key, Updated, Version)
{
    /// <summary>
    /// Gets this instance modified with metadata from specified response.
    /// </summary>
    /// <param name="response">Response.</param>
    /// <returns>Instance.</returns>
    protected ArtifactResourceInfo WithMetadata(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        string? version = response.Headers.ETag?.Tag;
        DateTimeOffset? updated = null;
        if (response.Headers.TryGetValues("last-modified", out IEnumerable<string>? seq) && seq.SingleOrDefault() is { } lmStr && DateTimeOffset.TryParse(lmStr, out DateTimeOffset dto))
        {
            updated = dto;
        }
        return this with { Version = version ?? Version, Updated = updated ?? Updated };
    }
}