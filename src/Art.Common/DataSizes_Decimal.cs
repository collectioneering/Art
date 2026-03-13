using System.Globalization;
using System.Text;

namespace Art.Common;

public static partial class DataSizes
{
    /// <summary>
    /// Stores the number of bytes in a kilobyte.
    /// </summary>
    public const int kB = 1000;

    /// <summary>
    /// Stores the number of bytes in a megabyte.
    /// </summary>
    public const int MB = 1000 * kB;

    /// <summary>
    /// Stores the number of bytes in a gigabyte.
    /// </summary>
    public const int GB = 1000 * MB;

    /// <summary>
    /// Stores the number of bytes in a kilobyte, as a <see cref="Int64">long</see>.
    /// </summary>
    public const long kBl = 1000;

    /// <summary>
    /// Stores the number of bytes in a megabyte, as a <see cref="Int64">long</see>.
    /// </summary>
    public const long MBl = 1000 * kBl;

    /// <summary>
    /// Stores the number of bytes in a gigabyte, as a <see cref="Int64">long</see>.
    /// </summary>
    public const long GBl = 1000 * MBl;

    /// <summary>
    /// Stores the number of bytes in a terabyte, as a <see cref="Int64">long</see>.
    /// </summary>
    public const long TBl = 1000L * GBl;

    /// <summary>
    /// Stores the number of bytes in a petabyte, as a <see cref="Int64">long</see>.
    /// </summary>
    public const long PBl = 1000L * TBl;

    /// <summary>
    /// Stores the number of bytes in an exabyte, as a <see cref="Int64">long</see>.
    /// </summary>
    public const long EBl = 1000L * PBl;

    private static readonly string[] s_decimalUnitNames = ["B", "kB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB", "RB", "QB"];

    /// <summary>
    /// Simplifies size of datum with the largest nameable decimal units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KiB, MiB, etc.).</param>
    public static void GetDecimalSize(long size, out double value, out string unit)
    {
        if (size < 0)
        {
            GetDecimalSize((ulong)-size, out double v, out unit);
            value = -v;
        }
        else
        {
            GetDecimalSize((ulong)size, out value, out unit);
        }
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable decimal units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unitIndex">Unit index (e.g. 0=B, 1=KiB, 2=MiB, etc.).</param>
    public static void GetDecimalSize(long size, out double value, out int unitIndex)
    {
        if (size < 0)
        {
            GetDecimalSize((ulong)-size, out double v, out unitIndex);
            value = -v;
        }
        else
        {
            GetDecimalSize((ulong)size, out value, out unitIndex);
        }
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable decimal units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KiB, MiB, etc.).</param>
    public static void GetDecimalSize(ulong size, out double value, out string unit)
    {
        GetDecimalSize(size, out value, out int unitIndex);
        unit = s_decimalUnitNames[unitIndex];
    }


    /// <summary>
    /// Simplifies size of datum with the largest nameable decimal units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unitIndex">Unit index (e.g. 0=B, 1=KiB, 2=MiB, etc.).</param>
    public static void GetDecimalSize(ulong size, out double value, out int unitIndex)
    {
        unitIndex = Math.Max((int)Math.Floor(double.Log10(size) / 3), 0);
        value = size / Math.Pow(10, unitIndex * 3);
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable decimal units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KiB, MiB, etc.).</param>
    public static void GetDecimalSize(double size, out double value, out string unit)
    {
        if (!double.IsRealNumber(size) || double.IsInfinity(size))
        {
            throw new ArgumentException("Invalid size value", nameof(size));
        }
        GetDecimalSize(Math.Abs(size), out double v, out int unitIndex);
        value = double.CopySign(v, size);
        unit = s_decimalUnitNames[unitIndex];
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable decimal units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unitIndex">Unit index (e.g. 0=B, 1=KiB, 2=MiB, etc.).</param>
    public static void GetDecimalSize(double size, out double value, out int unitIndex)
    {
        if (!double.IsRealNumber(size) || double.IsInfinity(size))
        {
            throw new ArgumentException("Invalid size value", nameof(size));
        }
        GetDecimalSizePositive(Math.Abs(size), out double v, out unitIndex);
        value = double.CopySign(v, size);
    }

    private static void GetDecimalSizePositive(double size, out double value, out int unitIndex)
    {
        unitIndex = Math.Min(Math.Max((int)Math.Floor(double.Log10(size) / 3), 0), s_decimalUnitNames.Length - 1);
        value = size / Math.Pow(10, unitIndex * 3);
    }

    /// <summary>
    /// Gets data size in decimal units as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <returns>Data size string.</returns>
    public static string GetDecimalSizeString(long size)
    {
        GetDecimalSize(size, out double sizeValue, out string sizeUnits);
        return $"{sizeValue:F3} {sizeUnits}";
    }

    /// <summary>
    /// Gets data size in decimal units as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <returns>Data size string.</returns>
    public static string GetDecimalSizeString(ulong size)
    {
        GetDecimalSize(size, out double sizeValue, out string sizeUnits);
        return $"{sizeValue:F3} {sizeUnits}";
    }

    /// <summary>
    /// Gets data size in decimal units as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <returns>Data size string.</returns>
    public static string GetDecimalSizeString(double size)
    {
        GetDecimalSize(size, out double sizeValue, out string sizeUnits);
        return $"{sizeValue:F3} {sizeUnits}";
    }

    /// <summary>
    /// Appends data size in decimal units as a string.
    /// </summary>
    /// <param name="sb">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendDecimalSize(this StringBuilder sb, long size)
    {
        GetDecimalSize(size, out double sizeValue, out string sizeUnits);
        sb.Append(CultureInfo.InvariantCulture, $"{sizeValue:F3}").Append(' ').Append(sizeUnits);
        return sb;
    }

    /// <summary>
    /// Appends data size in decimal units as a string.
    /// </summary>
    /// <param name="sb">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendDecimalSize(this StringBuilder sb, ulong size)
    {
        GetDecimalSize(size, out double sizeValue, out string sizeUnits);
        sb.Append(CultureInfo.InvariantCulture, $"{sizeValue:F3}").Append(' ').Append(sizeUnits);
        return sb;
    }

    /// <summary>
    /// Appends data size in decimal units as a string.
    /// </summary>
    /// <param name="sb">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendDecimalSize(this StringBuilder sb, double size)
    {
        GetDecimalSize(size, out double sizeValue, out string sizeUnits);
        sb.Append(CultureInfo.InvariantCulture, $"{sizeValue:F3}").Append(' ').Append(sizeUnits);
        return sb;
    }

    /// <summary>
    /// Appends data rate in decimal units as a string.
    /// </summary>
    /// <param name="sb">String builder.</param>
    /// <param name="rate">Data rate.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendDecimalRate(this StringBuilder sb, double rate)
    {
        return sb.AppendDecimalSize(rate).Append("/s");
    }
}
