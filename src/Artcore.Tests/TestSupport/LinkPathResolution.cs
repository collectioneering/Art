namespace Artcore.Tests.TestSupport;

public static class LinkPathResolution
{
    public static string BuildRealPath(string path)
    {
        path = Path.GetFullPath(path);
        string? remaining = path;
        List<string> components = [];
        bool anyLinks = false;
        do
        {
            if (File.Exists(remaining))
            {
                if (File.ResolveLinkTarget(remaining, true) is { } linkTarget)
                {
                    remaining = linkTarget.FullName;
                    anyLinks = true;
                }
                else
                {
                    components.Add(Path.GetFileName(remaining));
                    remaining = Path.GetDirectoryName(remaining);
                }
            }
            else if (Directory.Exists(remaining))
            {
                if (remaining != Path.GetPathRoot(remaining) && Directory.ResolveLinkTarget(remaining, true) is { } linkTarget)
                {
                    remaining = linkTarget.FullName;
                    anyLinks = true;
                }
                else
                {
                    if (Path.GetFileName(remaining) is { } fileName && !string.IsNullOrEmpty(fileName))
                    {
                        components.Add(fileName);
                    }
                    remaining = Path.GetDirectoryName(remaining);
                }
            }
            else
            {
                throw new FileNotFoundException($"No file/directory found for {remaining}");
            }
        } while (remaining != null);
        if (!anyLinks)
        {
            return path;
        }
        if (Path.GetPathRoot(path) is { } root)
        {
            components.Add(root);
        }
        components.Reverse();
        return Path.Join(components.ToArray());
    }
}
