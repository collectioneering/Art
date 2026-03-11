using System.Globalization;
using System.Numerics;
using System.Text;

namespace Art.Common;

/// <summary>
/// Constants for data sizes.
/// </summary>
public static class DataSizes
{
    /// <summary>
    /// Stores the number of bytes in a kibibyte.
    /// </summary>
    public const int KiB = 1024;

    /// <summary>
    /// Stores the number of bytes in a mebibyte.
    /// </summary>
    public const int MiB = 1024 * KiB;

    /// <summary>
    /// Stores the number of bytes in a gibibyte.
    /// </summary>
    public const int GiB = 1024 * MiB;

    /// <summary>
    /// Stores the number of bytes in a kibibyte, as a <see cref="Int64">long</see>.
    /// </summary>
    public const long KiBl = 1024;

    /// <summary>
    /// Stores the number of bytes in a mebibyte, as a <see cref="Int64">long</see>.
    /// </summary>
    public const long MiBl = 1024 * KiBl;

    /// <summary>
    /// Stores the number of bytes in a gibibyte, as a <see cref="Int64">long</see>.
    /// </summary>
    public const long GiBl = 1024 * MiBl;

    /// <summary>
    /// Stores the number of bytes in a tebibyte, as a <see cref="Int64">long</see>.
    /// </summary>
    public const long TiBl = 1024L * GiBl;

    /// <summary>
    /// Stores the number of bytes in a pebibyte, as a <see cref="Int64">long</see>.
    /// </summary>
    public const long PiBl = 1024L * TiBl;

    /// <summary>
    /// Stores the number of bytes in an exbibyte, as a <see cref="Int64">long</see>.
    /// </summary>
    public const long EiBl = 1024L * 1024 * 1024 * 1024 * 1024 * 1024;

    private static readonly string[] UnitNames = ["B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB"];

    /// <summary>
    /// Gets size of datum with units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KiB, MiB, etc.).</param>
    public static void GetSize(long size, out double value, out string unit)
    {
        if (size < 0)
        {
            GetSize((ulong)-size, out double v, out unit);
            value = -v;
        }
        else
        {
            GetSize((ulong)size, out value, out unit);
        }
    }

    /// <summary>
    /// Gets size of datum with units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KiB, MiB, etc.).</param>
    public static void GetSize(ulong size, out double value, out string unit)
    {
        int activeBits = 64 - BitOperations.LeadingZeroCount(size);
        int unitIndex = Math.Max(activeBits - 1, 0) / 10;
        unit = UnitNames[unitIndex];
        value = size / (double)(1UL << (unitIndex * 10));
    }

    /// <summary>
    /// Gets size of datum with units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KiB, MiB, etc.).</param>
    public static void GetSize(double size, out double value, out string unit)
    {
        if (!double.IsRealNumber(size) || double.IsInfinity(size))
        {
            throw new ArgumentException("Invalid size value", nameof(size));
        }
        GetSizePositive(Math.Abs(size), out double v, out unit);
        value = double.CopySign(v, size);
    }

    private static void GetSizePositive(double size, out double value, out string unit)
    {
        int unitIndex = Math.Max((int)Math.Floor(double.Log2(size) / 10), 0);
        int unitIndexUsed = Math.Min(unitIndex, UnitNames.Length - 1);
        unit = UnitNames[unitIndexUsed];
        value = size / Math.Pow(2, unitIndexUsed * 10);
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
    /// Gets data size as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <returns>Data size string.</returns>
    public static string GetSizeString(ulong size)
    {
        GetSize(size, out double sizeValue, out string sizeUnits);
        return $"{sizeValue:F3} {sizeUnits}";
    }

    /// <summary>
    /// Gets data size as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <returns>Data size string.</returns>
    public static string GetSizeString(double size)
    {
        GetSize(size, out double sizeValue, out string sizeUnits);
        return $"{sizeValue:F3} {sizeUnits}";
    }

    /// <summary>
    /// Appends data size as a string.
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
    /// Appends data size as a string.
    /// </summary>
    /// <param name="sb">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendSize(this StringBuilder sb, ulong size)
    {
        GetSize(size, out double sizeValue, out string sizeUnits);
        sb.Append(CultureInfo.InvariantCulture, $"{sizeValue:F3}").Append(' ').Append(sizeUnits);
        return sb;
    }

    /// <summary>
    /// Appends data size as a string.
    /// </summary>
    /// <param name="sb">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendSize(this StringBuilder sb, double size)
    {
        GetSize(size, out double sizeValue, out string sizeUnits);
        sb.Append(CultureInfo.InvariantCulture, $"{sizeValue:F3}").Append(' ').Append(sizeUnits);
        return sb;
    }

    /// <summary>
    /// Appends data rate as a string.
    /// </summary>
    /// <param name="sb">String builder.</param>
    /// <param name="rate">Data rate.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendRate(this StringBuilder sb, double rate)
    {
        return sb.AppendSize(rate).Append("/s");
    }
}
