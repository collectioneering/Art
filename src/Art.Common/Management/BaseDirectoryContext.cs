namespace Art.Common.Management;

internal record ArtifactDataBaseDirectoryContext(string BaseDirectory) : BaseDirectoryContext(BaseDirectory)
{
    internal string GetBasePath(string tool, string group)
    {
        return DiskPaths.ArtifactDataPaths.GetBasePath(BaseDirectory, tool, group);
    }
}

internal record ArtifactRegistrationBaseDirectoryContext(string BaseDirectory) : BaseDirectoryContext(BaseDirectory)
{
    internal string GetSubPath(string sub)
    {
        return DiskPaths.ArtifactRegistrationPaths.GetSubPath(BaseDirectory, sub);
    }

    internal string GetSubPath(string sub, string tool)
    {
        return DiskPaths.ArtifactRegistrationPaths.GetSubPath(BaseDirectory, sub, tool);
    }

    internal string GetSubPath(string sub, string tool, string group)
    {
        return DiskPaths.ArtifactRegistrationPaths.GetSubPath(BaseDirectory, sub, tool, group);
    }

    internal string GetSubPath(string sub, string tool, string group, string id)
    {
        return DiskPaths.ArtifactRegistrationPaths.GetSubPath(BaseDirectory, sub, tool, group, id);
    }
}

internal record BaseDirectoryContext(string BaseDirectory)
{
    internal string JoinValidated(string? path1, string? path2)
    {
        return DiskPaths.JoinValidated(BaseDirectory, path1, path2);
    }

    internal string JoinValidated(string? path1, string? path2, string? path3)
    {
        return DiskPaths.JoinValidated(BaseDirectory, path1, path2, path3);
    }

    internal string JoinValidated(string? path1, string? path2, string? path3, string? path4)
    {
        return DiskPaths.JoinValidated(BaseDirectory, path1, path2, path3, path4);
    }

    internal string JoinValidated(params string?[] paths)
    {
        return DiskPaths.JoinValidated(BaseDirectory, paths);
    }
}
