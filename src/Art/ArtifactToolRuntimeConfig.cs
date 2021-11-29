﻿namespace Art;

/// <summary>
/// Runtime configuration for a <see cref="ArtifactTool"/>.
/// </summary>
/// <param name="RegistrationManager">Registration manager.</param>
/// <param name="DataManager">Data manager.</param>
/// <param name="Profile">Profile.</param>
/// <param name="Options">Runtime options.</param>
public record ArtifactToolRuntimeConfig(ArtifactRegistrationManager RegistrationManager, ArtifactDataManager DataManager, ArtifactToolProfile Profile, ArtifactToolRuntimeOptions Options);