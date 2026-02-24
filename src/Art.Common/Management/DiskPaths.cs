namespace Art.Common.Management;

internal static class DiskPaths
{
    internal static class ArtifactDataPaths
    {
        internal static string GetBasePath(string baseDirectory, string tool, string group)
        {
            string result = Path.Join(baseDirectory, tool.SafeifyFileName(), group.SafeifyFileName());
            ValidatePathAgainstBaseDirectory(baseDirectory, result);
            return result;
        }
    }

    internal static class ArtifactRegistrationPaths
    {
        internal static string GetSubPath(string baseDirectory, string sub)
        {
            string result = Path.Join(baseDirectory, sub);
            ValidatePathAgainstBaseDirectory(baseDirectory, result);
            return result;
        }

        internal static string GetSubPath(string baseDirectory, string sub, string tool)
        {
            string result = Path.Join(baseDirectory, sub, tool.SafeifyFileName());
            ValidatePathAgainstBaseDirectory(baseDirectory, result);
            return result;
        }

        internal static string GetSubPath(string baseDirectory, string sub, string tool, string group)
        {
            string result = Path.Join(baseDirectory, sub, tool.SafeifyFileName(), group.SafeifyFileName());
            ValidatePathAgainstBaseDirectory(baseDirectory, result);
            return result;
        }

        internal static string GetSubPath(string baseDirectory, string sub, string tool, string group, string id)
        {
            string result = Path.Join(baseDirectory, sub, tool.SafeifyFileName(), group.SafeifyFileName(), id.SafeifyFileName());
            ValidatePathAgainstBaseDirectory(baseDirectory, result);
            return result;
        }
    }

    internal static string JoinValidated(string validationPath, string? path1, string? path2)
    {
        string result = Path.Join(path1, path2);
        ValidatePathAgainstBaseDirectory(validationPath, result);
        return result;
    }

    internal static string JoinValidated(string validationPath, string? path1, string? path2, string? path3)
    {
        string result = Path.Join(path1, path2, path3);
        ValidatePathAgainstBaseDirectory(validationPath, result);
        return result;
    }

    internal static string JoinValidated(string validationPath, string? path1, string? path2, string? path3, string? path4)
    {
        string result = Path.Join(path1, path2, path3, path4);
        ValidatePathAgainstBaseDirectory(validationPath, result);
        return result;
    }

    internal static string JoinValidated(string validationPath, params string?[] paths)
    {
        string result = Path.Join(paths);
        ValidatePathAgainstBaseDirectory(validationPath, result);
        return result;
    }

    internal static void ValidatePathAgainstBaseDirectory(string baseDirectory, string path)
    {
        string fullBaseDirectory = Path.GetFullPath(baseDirectory);
        string fullPath = Path.GetFullPath(path);
        if (fullPath.StartsWith(fullBaseDirectory))
        {
            if (fullPath.Length > baseDirectory.Length)
            {
                char c = fullPath[baseDirectory.Length];
                if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar)
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
        throw new InvalidOperationException($"Path \"{path}\" is not contained within base directory \"{baseDirectory}\"");
    }
}
