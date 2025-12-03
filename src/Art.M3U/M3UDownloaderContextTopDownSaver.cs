using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Art.Http;

namespace Art.M3U;

/// <summary>
/// Represents a top-down saver.
/// </summary>
public partial class M3UDownloaderContextTopDownSaver : M3UDownloaderContextSaver, IExtraSaverOperation
{
    [GeneratedRegex(@"(^[\S\s]*[^\d]|)\d+(\.\w+)$")]
    private static partial Regex GetBitRegex();

    [GeneratedRegex(@"(?<prefix>^[\S\s]*[^\d]|)(?<number>\d+)(?<suffix>\.\w+)$")]
    private static partial Regex GetBit2Regex();

    private readonly long _top;
    private readonly long? _topMsn;
    private readonly Func<string, long, string> _nameTransform;
    private long _currentTop;
    private bool _ended;

    internal M3UDownloaderContextTopDownSaver(M3UDownloaderContext context, long top, long? topMsn)
        : this(context, top, topMsn, TranslateNameDefault)
    {
    }

    internal M3UDownloaderContextTopDownSaver(M3UDownloaderContext context, long top, long? topMsn, Func<long, string> idFormatter)
        : this(context, top, topMsn, (a, b) => TranslateNameDefault(a, idFormatter(b)))
    {
    }

    internal M3UDownloaderContextTopDownSaver(M3UDownloaderContext context, long top, long? topMsn, Func<string, long, string> nameTransform) : base(context)
    {
        _top = top;
        _topMsn = topMsn;
        _currentTop = _top;
        _nameTransform = nameTransform;
    }

    /// <summary>
    /// Translates a generic filename.
    /// </summary>
    /// <param name="name">Donor filename.</param>
    /// <param name="i">Index.</param>
    /// <returns>Generated filename.</returns>
    /// <exception cref="InvalidDataException">Regex match failed.</exception>
    public static string TranslateNameDefault(string name, long i) => TranslateNameDefault(name, i.ToString(CultureInfo.InvariantCulture));

    /// <summary>
    /// Translates a generic filename.
    /// </summary>
    /// <param name="name">Donor filename.</param>
    /// <param name="i">Index.</param>
    /// <returns>Generated filename.</returns>
    /// <exception cref="InvalidDataException">Regex match failed.</exception>
    public static string TranslateNameDefault(string name, string i)
    {
        if (GetBitRegex().Match(name) is not { Success: true } bits)
        {
            throw new InvalidDataException();
        }
        return $"{bits.Groups[1].Value}{i}{bits.Groups[2].Value}";
    }

    /// <summary>
    /// Translates a generic filename, with padding to match the string length of the numeric part.
    /// </summary>
    /// <param name="name">Donor filename.</param>
    /// <param name="i">Index.</param>
    /// <returns>Generated filename.</returns>
    /// <exception cref="InvalidDataException">Regex match failed.</exception>
    public static string TranslateNameMatchLength(string name, string i)
    {
        if (GetBit2Regex().Match(name) is not { Success: true } match)
        {
            throw new InvalidDataException();
        }
        int nameLength = match.Groups["number"].Length;
        string paddedI = i.PadLeft(nameLength, '0');
        return match.Groups["prefix"].Value + paddedI + match.Groups["suffix"].Value;
    }

    /// <inheritdoc />
    public override async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Reset();
        while (true)
        {
            if (HeartbeatCallback != null) await HeartbeatCallback().ConfigureAwait(false);
            M3UFile m3;
            Context.Tool.LogInformation("Reading main...");
            try
            {
                m3 = await Context.GetAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (ArtHttpResponseMessageException e)
            {
                await HandleRequestExceptionAsync(e, cancellationToken).ConfigureAwait(false);
                continue;
            }
            bool shouldContinue = await TickAsync(m3, cancellationToken).ConfigureAwait(false);
            if (!shouldContinue)
            {
                return;
            }
            await Task.Delay(500, cancellationToken).ConfigureAwait(false);
        }
    }

    void IExtraSaverOperation.Reset()
    {
        Reset();
    }

    private void Reset()
    {
        _ended = false;
        _currentTop = _top;
        ConsecutiveFailCounter = 0;
        TotalFailCounter = 0;
    }

    Task<bool> IExtraSaverOperation.TickAsync(M3UFile m3, CancellationToken cancellationToken)
    {
        return TickAsync(m3, cancellationToken);
    }

    private async Task<bool> TickAsync(M3UFile m3, CancellationToken cancellationToken = default)
    {
        if (_ended || _currentTop < 0)
        {
            return false;
        }
        string str = m3.DataLines.First();
        Uri origUri = new(Context.MainUri, str);
        int idx = str.IndexOf('?');
        if (idx >= 0) str = str[..idx];
        Uri uri = new UriBuilder(new Uri(Context.MainUri, _nameTransform(str, _currentTop))) { Query = origUri.Query }.Uri;
        Context.Tool.LogInformation($"[{Context.Name}] Top-downloading segment {uri.Segments[^1]}...");
        try
        {
            long? msn;
            if (_topMsn is { } topMsn)
            {
                msn = _top - _currentTop + topMsn;
            }
            else
            {
                msn = null;
            }
            // Don't assume MSN, and just accept failure (exception) when trying to decrypt with no IV
            // Also don't depend on current file since it probably won't do us good anyway for this use case
            await Context.DownloadSegmentAsync(uri, null, msn, null, cancellationToken).ConfigureAwait(false);
            _currentTop--;
        }
        catch (ArtHttpResponseMessageException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                Context.Tool.LogInformation("HTTP NotFound returned, ending top-down operation");
                _ended = true;
                return false;
            }
            await HandleRequestExceptionAsync(e, cancellationToken).ConfigureAwait(false);
        }
        ConsecutiveFailCounter = 0;
        return true;
    }
}
