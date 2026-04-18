namespace Art.Common.IO;

/// <summary>
/// Utility for IO operations.
/// </summary>
public static class ArtIOUtility
{
    /// <summary>
    /// Creates directories to contain a file if they don't already exist.
    /// </summary>
    /// <param name="file">Path to file for which to create containing directories.</param>
    /// <exception cref="IOException">Thrown for errors creating the directories.</exception>
    public static void EnsureDirectoryForFileCreated(string file)
    {
        if (Path.GetDirectoryName(Path.GetFullPath(file)) is { } parentPath)
        {
            EnsureDirectoryCreated(parentPath);
        }
    }

    /// <summary>
    /// Creates a directory if it doesn't already exist.
    /// </summary>
    /// <param name="directory">Path to directory to create.</param>
    /// <exception cref="IOException">Thrown for errors creating the directories.</exception>
    public static void EnsureDirectoryCreated(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    /// Creates a random path for the specified sibling path.
    /// </summary>
    /// <param name="sibling">Sibling path.</param>
    /// <param name="extension">Extension to append to random name.</param>
    /// <param name="attempts">Maximum number of attempts to generate path.</param>
    /// <param name="pathCreationAction">Specifies the action to take on the newly created path.</param>
    /// <returns>Random path.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sibling"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown for invalid <paramref name="sibling"/> (e.g. drive root) or <paramref name="attempts"/> (&lt;=0) value.</exception>
    /// <exception cref="IOException">Thrown if failed to create sibling path with specified attempts.</exception>
    public static string CreateRandomPathForSibling(string sibling, string extension, int attempts = 10, PathCreationAction pathCreationAction = PathCreationAction.None)
    {
        string dir = Path.GetDirectoryName(sibling) ?? throw new ArgumentException("Sibling path cannot be a drive root", nameof(sibling));
        return CreateRandomPath(dir, extension, attempts, pathCreationAction);
    }

    /// <summary>
    /// Creates a random path directly under the specified base directory.
    /// </summary>
    /// <param name="baseDirectory">Base directory.</param>
    /// <param name="extension">Extension to append to random name.</param>
    /// <param name="attempts">Maximum number of attempts to generate path.</param>
    /// <param name="pathCreationAction">Specifies the action to take on the newly created path.</param>
    /// <returns>Random path.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="baseDirectory"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown for invalid <paramref name="attempts"/> (&lt;=0) value.</exception>
    /// <exception cref="IOException">Thrown if failed to create sibling path with specified attempts.</exception>
    public static string CreateRandomPath(string baseDirectory, string extension, int attempts = 10, PathCreationAction pathCreationAction = PathCreationAction.None)
    {
        ArgumentNullException.ThrowIfNull(baseDirectory);
        if (attempts <= 0)
        {
            throw new ArgumentException("Invalid max number of attempts", nameof(attempts));
        }
        for (int i = 0; i < attempts; i++)
        {
            string path = Path.Join(baseDirectory, $"{Guid.NewGuid():N}{extension}");
            var fileInfo = new FileInfo(path);
            var directoryInfo = new DirectoryInfo(path);
            if (fileInfo.Exists || directoryInfo.Exists)
            {
                continue;
            }
            switch (pathCreationAction)
            {
                case PathCreationAction.None:
                    return path;
                case PathCreationAction.CreateFile:
                    try
                    {
                        using (new FileStream(fileInfo.FullName, FileMode.CreateNew))
                        {
                        }
                        return path;
                    }
                    catch
                    {
                        // file got created before returned, or other I/O error
                        break;
                    }
                case PathCreationAction.CreateDirectory:
                    try
                    {
                        directoryInfo.Create();
                        if (directoryInfo.EnumerateFileSystemInfos().Any())
                        {
                            // directory has content already
                            break;
                        }
                        return path;
                    }
                    catch
                    {
                        // some I/O error
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(pathCreationAction), pathCreationAction, null);
            }
        }
        throw new IOException($"Failed to create random path under {baseDirectory}");
    }
}
