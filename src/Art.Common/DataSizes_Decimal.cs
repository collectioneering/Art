using System.Globalization;
using System.Numerics;
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
    private static readonly string[] s_decimalUnitNamesLong = ["byte", "kilobyte", "megabyte", "gigabyte", "terabyte", "petabyte", "exabyte", "zettabyte", "yottabyte", "ronnabyte", "quettabyte"];
    private static readonly string[] s_decimalUnitNamesLongPlural = ["bytes", "kilobytes", "megabytes", "gigabytes", "terabytes", "petabytes", "exabytes", "zettabytes", "yottabytes", "ronnabytes", "quettabytes"];

    /// <summary>
    /// Simplifies size of datum with the largest nameable decimal units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KB, MB, etc.).</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    public static void GetDecimalSize(
        long size,
        out double value,
        out string unit,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        if (size < 0)
        {
            GetDecimalSize((ulong)-size, out double v, out unit, dataUnitFormat);
            value = -v;
        }
        else
        {
            GetDecimalSize((ulong)size, out value, out unit, dataUnitFormat);
        }
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable decimal units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unitIndex">Unit index (e.g. 0=B, 1=KB, 2=MB, etc.).</param>
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
    /// <param name="unit">Unit (e.g. B, KB, MB, etc.).</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    public static void GetDecimalSize(
        ulong size,
        out double value,
        out string unit,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetDecimalSize(size, out value, out int unitIndex);
        unit = dataUnitFormat == DataUnitFormat.Short
            ? s_decimalUnitNames[unitIndex]
            : Math.Abs(value - 1) < double.Epsilon
                ? s_decimalUnitNamesLong[unitIndex]
                : s_decimalUnitNamesLongPlural[unitIndex];
    }


    /// <summary>
    /// Simplifies size of datum with the largest nameable decimal units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unitIndex">Unit index (e.g. 0=B, 1=KB, 2=MB, etc.).</param>
    public static void GetDecimalSize(ulong size, out double value, out int unitIndex)
    {
        unitIndex = Math.Max((int)Math.Floor(double.Log10(size) / 3), 0);
        value = size / Math.Pow(10, unitIndex * 3);
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable decimal units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="valueWhole">Integer portion of value for given units.</param>
    /// <param name="valueFraction">Fractional portion of value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KB, MB, etc.).</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    public static void GetDecimalSize(
        BigInteger size,
        out BigInteger valueWhole,
        out double valueFraction,
        out string unit,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetDecimalSize(size, out valueWhole, out valueFraction, out int unitIndex);
        unit = dataUnitFormat == DataUnitFormat.Short
            ? s_decimalUnitNames[unitIndex]
            : valueWhole == 1 && valueFraction < double.Epsilon
                ? s_decimalUnitNamesLong[unitIndex]
                : s_decimalUnitNamesLongPlural[unitIndex];
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable decimal units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="valueWhole">Integer portion of value for given units.</param>
    /// <param name="valueFraction">Fractional portion of value for given units.</param>
    /// <param name="unitIndex">Unit index (e.g. 0=B, 1=KB, 2=MB, etc.).</param>
    public static void GetDecimalSize(
        BigInteger size,
        out BigInteger valueWhole,
        out double valueFraction,
        out int unitIndex)
    {
        BigInteger sizeAbs = BigInteger.Abs(size);
        if (sizeAbs == BigInteger.Zero)
        {
            valueWhole = 0;
            valueFraction = 0;
            unitIndex = 0;
            return;
        }
        unitIndex = Math.Min(Math.Max(GetBestLog1000(sizeAbs), 0), s_decimalUnitNames.Length - 1);
        if (unitIndex == 0)
        {
            valueWhole = size;
            valueFraction = 0;
            unitIndex = 0;
            return;
        }
        BigInteger divisor = BigInteger.Pow(1000, unitIndex);
        valueWhole = BigInteger.CopySign(BigInteger.DivRem(sizeAbs, divisor, out BigInteger remainder), size);
        valueFraction = Math.Min(Math.Abs(GetFraction(remainder, divisor)), Math.BitDecrement(1));
    }

    private static int GetBestLog1000(BigInteger valuePositive)
    {
        double valueLog1000 = BigInteger.Log10(valuePositive) / 3;
        double valueLog1000Round = double.Round(valueLog1000);
        // bail out if it's just too big
        if (valueLog1000Round > int.MaxValue)
        {
            return 0;
        }
        int valueLog1000RoundInt = (int)valueLog1000Round;
        BigInteger divisor = BigInteger.Pow(1000, valueLog1000RoundInt);
        if (valueLog1000RoundInt > valueLog1000 && valuePositive >= divisor)
        {
            return valueLog1000RoundInt;
        }
        if (valuePositive < divisor)
        {
            return valueLog1000RoundInt - 1;
        }
        return (int)Math.Floor(valueLog1000);
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable decimal units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KB, MB, etc.).</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    public static void GetDecimalSize(
        double size,
        out double value,
        out string unit,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        if (!double.IsRealNumber(size) || double.IsInfinity(size))
        {
            throw new ArgumentException("Invalid size value", nameof(size));
        }
        GetDecimalSize(Math.Abs(size), out double v, out int unitIndex);
        value = double.CopySign(v, size);
        unit = dataUnitFormat == DataUnitFormat.Short
            ? s_decimalUnitNames[unitIndex]
            : Math.Abs(value - 1) < double.Epsilon
                ? s_decimalUnitNamesLong[unitIndex]
                : s_decimalUnitNamesLongPlural[unitIndex];
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable decimal units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unitIndex">Unit index (e.g. 0=B, 1=KB, 2=MB, etc.).</param>
    public static void GetDecimalSize(double size, out double value, out int unitIndex)
    {
        if (!double.IsRealNumber(size) || double.IsInfinity(size))
        {
            throw new ArgumentException("Invalid size value", nameof(size));
        }
        GetDecimalSizeNonNegative(Math.Abs(size), out double v, out unitIndex);
        value = double.CopySign(v, size);
    }

    private static void GetDecimalSizeNonNegative(double size, out double value, out int unitIndex)
    {
        if (size < double.Epsilon)
        {
            value = 0;
            unitIndex = 0;
            return;
        }
        unitIndex = Math.Min(Math.Max((int)Math.Floor(double.Log10(size) / 3), 0), s_decimalUnitNames.Length - 1);
        value = size / Math.Pow(10, unitIndex * 3);
    }

    /// <summary>
    /// Gets data size in decimal units as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>Data size string.</returns>
    public static string GetDecimalSizeString(long size, DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetDecimalSize(size, out double sizeValue, out string sizeUnits, dataUnitFormat);
        return $"{sizeValue:F3} {sizeUnits}";
    }

    /// <summary>
    /// Gets data size in decimal units as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>Data size string.</returns>
    public static string GetDecimalSizeString(ulong size, DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetDecimalSize(size, out double sizeValue, out string sizeUnits, dataUnitFormat);
        return $"{sizeValue:F3} {sizeUnits}";
    }

    /// <summary>
    /// Gets data size in decimal units as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>Data size string.</returns>
    public static string GetDecimalSizeString(BigInteger size, DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetDecimalSize(size, out BigInteger sizeValueWhole, out double sizeValueFraction, out string sizeUnits, dataUnitFormat);
        return $"{FormatBigIntegerAndFraction(sizeValueWhole, sizeValueFraction)} {sizeUnits}";
    }

    /// <summary>
    /// Gets data size in decimal units as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>Data size string.</returns>
    public static string GetDecimalSizeString(double size, DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetDecimalSize(size, out double sizeValue, out string sizeUnits, dataUnitFormat);
        return $"{sizeValue:F3} {sizeUnits}";
    }

    /// <summary>
    /// Appends data size in decimal units as a string.
    /// </summary>
    /// <param name="stringBuilder">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendDecimalSize(
        this StringBuilder stringBuilder,
        long size,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetDecimalSize(size, out double sizeValue, out string sizeUnits, dataUnitFormat);
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{sizeValue:F3}").Append(' ').Append(sizeUnits);
        return stringBuilder;
    }

    /// <summary>
    /// Appends data size in decimal units as a string.
    /// </summary>
    /// <param name="stringBuilder">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendDecimalSize(
        this StringBuilder stringBuilder,
        ulong size,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetDecimalSize(size, out double sizeValue, out string sizeUnits, dataUnitFormat);
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{sizeValue:F3}").Append(' ').Append(sizeUnits);
        return stringBuilder;
    }

    /// <summary>
    /// Appends data size in decimal units as a string.
    /// </summary>
    /// <param name="stringBuilder">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendDecimalSize(
        this StringBuilder stringBuilder,
        BigInteger size,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetDecimalSize(size, out BigInteger sizeValueWhole, out double sizeValueFraction, out string sizeUnits, dataUnitFormat);
        stringBuilder.Append(FormatBigIntegerAndFraction(sizeValueWhole, sizeValueFraction)).Append(' ').Append(sizeUnits);
        return stringBuilder;
    }

    /// <summary>
    /// Appends data size in decimal units as a string.
    /// </summary>
    /// <param name="stringBuilder">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendDecimalSize(
        this StringBuilder stringBuilder,
        double size,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetDecimalSize(size, out double sizeValue, out string sizeUnits, dataUnitFormat);
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{sizeValue:F3}").Append(' ').Append(sizeUnits);
        return stringBuilder;
    }

    /// <summary>
    /// Appends data rate in decimal units as a string.
    /// </summary>
    /// <param name="stringBuilder">String builder.</param>
    /// <param name="rate">Data rate.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendDecimalRate(
        this StringBuilder stringBuilder,
        double rate,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        return stringBuilder.AppendDecimalSize(rate, dataUnitFormat).Append("/s");
    }
}
