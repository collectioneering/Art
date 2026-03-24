using System.Text;
using System.Text.RegularExpressions;

namespace Art.BrowserCookies;

internal static partial class PsScriptUtil
{
    [GeneratedRegex(@"<#[\s\S]*#>")]
    private static partial Regex GetScriptBlockCommentRegex();

    public static StringBuilder AppendSimplifiedPsScript(StringBuilder sb, string value)
    {
        string src = GetScriptBlockCommentRegex().Replace(value, "");
#if NET10_0_OR_GREATER
        foreach (var line in src.EnumerateLines())
#else
        var sr = new StringReader(src);
        while (sr.ReadLine() is { } line)
#endif
        {
            // ReSharper disable RedundantCast
            ReadOnlySpan<char> subLine = ((ReadOnlySpan<char>)line).Trim();
            // ReSharper restore RedundantCast
            if (subLine.StartsWith("#"))
            {
                continue;
            }
            if (subLine.Trim().Length > 0)
            {
                sb.Append(line).AppendLine();
            }
        }
        return sb;
    }
}
