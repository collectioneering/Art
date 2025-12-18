using System.Collections.Immutable;
using System.Globalization;

namespace Art.M3U;

/// <summary>
/// Provides methods to manage video existence files.
/// </summary>
public static class VxFiles
{
    /// <summary>
    /// Gets manifest data for the specified path.
    /// </summary>
    /// <param name="namespacedArtifactDataManager">Data manager.</param>
    /// <param name="path">Path corresponding to location of vxfiles ("vxf") directory.</param>
    /// <param name="numberExtractor">Function for extracting numbers, e.g. <see cref="M3UDownloaderContextTopDownSaver.TryExtractNumberFromName"/>.</param>
    /// <param name="numberReplacer">Function for replacing numbers, e.g. <see cref="M3UDownloaderContextTopDownSaver.TranslateNameMatchLength"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Manifest, if available.</returns>
    public static async Task<VxFileManifest?> GetManifestForDirectoryAsync(
        INamespacedArtifactDataManager namespacedArtifactDataManager,
        string path,
        VxFileNumberTryExtractionDelegate numberExtractor,
        VxFileNumberReplaceDelegate numberReplacer,
        CancellationToken cancellationToken = default)
    {
        string[] files = await namespacedArtifactDataManager.ListFilesAsync(path, cancellationToken).ConfigureAwait(false);
        List<KeyValuePair<long, string>> useFiles = [];
        foreach (string file in files)
        {
            if (numberExtractor(file, out long number))
            {
                useFiles.Add(new KeyValuePair<long, string>(number, file));
            }
        }
        if (useFiles.Count == 0)
        {
            return null;
        }
        useFiles = useFiles.DistinctBy(static file => file.Key).OrderBy(static file => file.Key).ToList();
        List<VxFileInfo> missingFileInfos = [];
        for (int i = 0; i + 1 < useFiles.Count; i++)
        {
            var useFile = useFiles[i];
            var fileInfo = await ExtractFileInfoAsync(namespacedArtifactDataManager, path, numberExtractor, useFile.Value, cancellationToken).ConfigureAwait(false);
            long missingRange = useFiles[i + 1].Key - useFile.Key - 1;
            for (long j = 0; j < missingRange; j++)
            {
                missingFileInfos.Add(new VxFileInfo(
                    numberReplacer(useFile.Value, (useFile.Key + 1 + j).ToString(CultureInfo.InvariantCulture)),
                    useFile.Key + 1 + j,
                    fileInfo.Msn is { } msn ? msn + 1 + j : null));
            }
        }
        return new VxFileManifest(
            await ExtractFileInfoAsync(namespacedArtifactDataManager, path, numberExtractor, useFiles[0].Value, cancellationToken).ConfigureAwait(false),
            [..missingFileInfos]);
    }

    private static async Task<VxFileInfo> ExtractFileInfoAsync(
        INamespacedArtifactDataManager namespacedArtifactDataManager,
        string path,
        VxFileNumberTryExtractionDelegate numberExtractor,
        string file,
        CancellationToken cancellationToken)
    {
        if (!numberExtractor(file, out long number))
        {
            throw new ArgumentException($"Value \"{file}\" couldn't be processed for a number");
        }
        await using (var fs = await namespacedArtifactDataManager.OpenInputStreamAsync(file, path, cancellationToken).ConfigureAwait(false))
        {
            using var sr = new StreamReader(fs, leaveOpen: true);
            string content = await sr.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            if (long.TryParse(content, out long value))
            {
                return new VxFileInfo(file, number, value);
            }
        }
        return new VxFileInfo(file, number, null);
    }
}

/// <summary>
/// Manifest for video existence files in a directory.
/// </summary>
/// <param name="LowestKey">Lowest present key.</param>
/// <param name="MissingKeys"></param>
public record VxFileManifest(VxFileInfo LowestKey, ImmutableArray<VxFileInfo> MissingKeys);

/// <summary>
/// Info for a video existence file.
/// </summary>
/// <param name="Key">File key.</param>
/// <param name="Number">File number.</param>
/// <param name="Msn">Media sequence number, if applicable.</param>
public record struct VxFileInfo(string Key, long Number, long? Msn);

/// <summary>
/// Delegate for attempting extraction of a number from a name.
/// </summary>
public delegate bool VxFileNumberTryExtractionDelegate(string name, out long number);

/// <summary>
/// Delegate for replacing a number in a name.
/// </summary>
public delegate string VxFileNumberReplaceDelegate(string name, string number);
