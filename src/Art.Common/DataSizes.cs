using System.Globalization;
using System.Text;

namespace Art.Common;

/// <summary>
/// Constants for data sizes
/// </summary>
public static class DataSizes
{
    /// <summary>
    /// Kibibyte
    /// </summary>
    public const int KiB = 1024;

    /// <summary>
    /// Mebibyte
    /// </summary>
    public const int MiB = 1024 * KiB;

    /// <summary>
    /// Gibibyte
    /// </summary>
    public const int GiB = 1024 * MiB;

    /// <summary>
    /// Kibibyte
    /// </summary>
    public const long KiBl = 1024;

    /// <summary>
    /// Mebibyte
    /// </summary>
    public const long MiBl = 1024 * KiBl;

    /// <summary>
    /// Gibibyte
    /// </summary>
    public const long GiBl = 1024 * MiBl;

    /// <summary>
    /// Tebibyte
    /// </summary>
    public const long TiBl = 1024L * GiBl;

    /// <summary>
    /// Pebibyte
    /// </summary>
    public const long PiBl = 1024L * TiBl;

    /// <summary>
    /// Exbibyte
    /// </summary>
    public const long EiBl = 1024L * 1024 * 1024 * 1024 * 1024 * 1024;

    private static readonly string[] UnitNames = ["B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB"];

    /// <summary>
    /// Get size of datum with units
    /// </summary>
    /// <param name="size">Datum size</param>
    /// <param name="value">Value for given units</param>
    /// <param name="unit">Unit (e.g. B, KiB, MiB, etc.)</param>
    public static void GetSize(long size, out double value, out string unit)
    {
        int i = 0;
        long past = 0L;
        long maxPast = 1L;
        while (i < UnitNames.Length && size >= 1024)
        {
            past += size % 1024 * maxPast;
            maxPast *= 1024;
            size /= 1024;
            i++;
        }

        value = size + (double)past / maxPast;
        unit = UnitNames[i];
    }

    /// <summary>
    /// Gets data size as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <returns>Data size string.</returns>
    public static string GetSizeString(long size)
    {
        GetSize(size, out double sizeValue, out string sizeUnits);
        return $"{sizeValue:F3} {sizeUnits}";
    }

    /// <summary>
    /// Append data size as a string.
    /// </summary>
    /// <param name="sb">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendSize(this StringBuilder sb, long size)
    {
        GetSize(size, out double sizeValue, out string sizeUnits);
        sb.Append(CultureInfo.InvariantCulture, $"{sizeValue:F3}").Append(' ').Append(sizeUnits);
        return sb;
    }

    /// <summary>
    /// Append data rate as a string.
    /// </summary>
    /// <param name="sb">String builder.</param>
    /// <param name="rate">Data rate.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendRate(this StringBuilder sb, float rate) =>
        sb.AppendSize((long)rate).Append("/s");
}
