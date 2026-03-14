using System.Globalization;
using System.Numerics;
using System.Text;

namespace Art.Common;

public static partial class DataSizes
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
    public const long EiBl = 1024L * PiBl;

    private static readonly string[] s_binaryUnitNames = ["B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB", "RiB", "QiB"];
    private static readonly string[] s_binaryUnitNamesLong = ["byte", "kibibyte", "mebibyte", "gibibyte", "tebibyte", "pebibyte", "exbibyte", "zebibyte", "yobibyte", "robibyte", "quebibyte"];
    private static readonly string[] s_binaryUnitNamesLongPlural = ["bytes", "kibibytes", "mebibytes", "gibibytes", "tebibytes", "pebibytes", "exbibytes", "zebibytes", "yobibytes", "robibytes", "quebibytes"];

    /// <summary>
    /// Simplifies size of datum with the largest nameable binary units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KiB, MiB, etc.).</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    public static void GetBinarySize(
        long size,
        out double value,
        out string unit,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        if (size < 0)
        {
            GetBinarySize((ulong)-size, out double v, out unit, dataUnitFormat);
            value = -v;
        }
        else
        {
            GetBinarySize((ulong)size, out value, out unit, dataUnitFormat);
        }
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable binary units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unitIndex">Unit index (e.g. 0=B, 1=KiB, 2=MiB, etc.).</param>
    public static void GetBinarySize(long size, out double value, out int unitIndex)
    {
        if (size < 0)
        {
            GetBinarySize((ulong)-size, out double v, out unitIndex);
            value = -v;
        }
        else
        {
            GetBinarySize((ulong)size, out value, out unitIndex);
        }
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable binary units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KiB, MiB, etc.).</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    public static void GetBinarySize(
        ulong size,
        out double value,
        out string unit,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetBinarySize(size, out value, out int unitIndex);
        unit = dataUnitFormat == DataUnitFormat.Short
            ? s_binaryUnitNames[unitIndex]
            : Math.Abs(value - 1) < double.Epsilon
                ? s_binaryUnitNamesLong[unitIndex]
                : s_binaryUnitNamesLongPlural[unitIndex];
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable binary units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unitIndex">Unit index (e.g. 0=B, 1=KiB, 2=MiB, etc.).</param>
    public static void GetBinarySize(ulong size, out double value, out int unitIndex)
    {
        int activeBits = 64 - BitOperations.LeadingZeroCount(size);
        unitIndex = Math.Max(activeBits - 1, 0) / 10;
        value = size / (double)(1UL << (unitIndex * 10));
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable binary units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="valueWhole">Integer portion of value for given units.</param>
    /// <param name="valueFraction">Fractional portion of value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KiB, MiB, etc.).</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    public static void GetBinarySize(
        BigInteger size,
        out BigInteger valueWhole,
        out double valueFraction,
        out string unit,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetBinarySize(size, out valueWhole, out valueFraction, out int unitIndex);
        unit = dataUnitFormat == DataUnitFormat.Short
            ? s_binaryUnitNames[unitIndex]
            : valueWhole == 1 && valueFraction < double.Epsilon
                ? s_binaryUnitNamesLong[unitIndex]
                : s_binaryUnitNamesLongPlural[unitIndex];
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable binary units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="valueWhole">Integer portion of value for given units.</param>
    /// <param name="valueFraction">Fractional portion of value for given units.</param>
    /// <param name="unitIndex">Unit index (e.g. 0=B, 1=KiB, 2=MiB, etc.).</param>
    public static void GetBinarySize(BigInteger size, out BigInteger valueWhole, out double valueFraction, out int unitIndex)
    {
        BigInteger sizeAbs = BigInteger.Abs(size);
        if (sizeAbs == BigInteger.Zero)
        {
            valueWhole = 0;
            valueFraction = 0;
            unitIndex = 0;
            return;
        }
        long activeBits = sizeAbs.GetBitLength();
        unitIndex = (int)Math.Min(Math.Max(activeBits - 1, 0) / 10, s_binaryUnitNames.Length - 1);
        BigInteger divisor = (BigInteger)1 << (unitIndex * 10);
        valueWhole = BigInteger.CopySign(BigInteger.DivRem(sizeAbs, divisor, out BigInteger remainder), size);
        valueFraction = Math.Min(Math.Abs(GetFraction(remainder, divisor)), Math.BitDecrement(1));
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable binary units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KiB, MiB, etc.).</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    public static void GetBinarySize(
        double size,
        out double value,
        out string unit,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        if (!double.IsRealNumber(size) || double.IsInfinity(size))
        {
            throw new ArgumentException("Invalid size value", nameof(size));
        }
        GetBinarySizeNonNegative(Math.Abs(size), out double v, out int unitIndex);
        value = double.CopySign(v, size);
        unit = dataUnitFormat == DataUnitFormat.Short
            ? s_binaryUnitNames[unitIndex]
            : Math.Abs(value - 1) < double.Epsilon
                ? s_binaryUnitNamesLong[unitIndex]
                : s_binaryUnitNamesLongPlural[unitIndex];
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable binary units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unitIndex">Unit index (e.g. 0=B, 1=KiB, 2=MiB, etc.).</param>
    public static void GetBinarySize(double size, out double value, out int unitIndex)
    {
        if (!double.IsRealNumber(size) || double.IsInfinity(size))
        {
            throw new ArgumentException("Invalid size value", nameof(size));
        }
        GetBinarySizeNonNegative(Math.Abs(size), out double v, out unitIndex);
        value = double.CopySign(v, size);
    }

    private static void GetBinarySizeNonNegative(double size, out double value, out int unitIndex)
    {
        if (size < double.Epsilon)
        {
            value = 0;
            unitIndex = 0;
            return;
        }
        ulong bits = BitConverter.DoubleToUInt64Bits(size);
        int exponent = (int)((bits >> 52) & 0x7_FF) - 1023;
        unitIndex = Math.Min(Math.Max(exponent, 0) / 10, s_binaryUnitNames.Length - 1);
        value = BitConverter.UInt64BitsToDouble(((ulong)Math.Clamp(exponent - unitIndex * 10 + 1023, 0, 0x7_FF) << 52) | (bits & 0xF_FF_FF_FF_FF_FF_FFul));
    }

    /// <summary>
    /// Gets data size in binary units as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>Data size string.</returns>
    public static string GetBinarySizeString(long size, DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetBinarySize(size, out double sizeValue, out string sizeUnits, dataUnitFormat);
        return $"{sizeValue:F3} {sizeUnits}";
    }

    /// <summary>
    /// Gets data size in binary units as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>Data size string.</returns>
    public static string GetBinarySizeString(ulong size, DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetBinarySize(size, out double sizeValue, out string sizeUnits, dataUnitFormat);
        return $"{sizeValue:F3} {sizeUnits}";
    }

    /// <summary>
    /// Gets data size in binary units as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>Data size string.</returns>
    public static string GetBinarySizeString(BigInteger size, DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetBinarySize(size, out BigInteger sizeValueWhole, out double sizeValueFraction, out string sizeUnits, dataUnitFormat);
        return $"{FormatBigIntegerAndFraction(sizeValueWhole, sizeValueFraction)} {sizeUnits}";
    }

    /// <summary>
    /// Gets data size in binary units as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>Data size string.</returns>
    public static string GetBinarySizeString(double size, DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetBinarySize(size, out double sizeValue, out string sizeUnits, dataUnitFormat);
        return $"{sizeValue:F3} {sizeUnits}";
    }

    /// <summary>
    /// Appends data size in binary units as a string.
    /// </summary>
    /// <param name="stringBuilder">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendBinarySize(
        this StringBuilder stringBuilder,
        long size,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetBinarySize(size, out double sizeValue, out string sizeUnits, dataUnitFormat);
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{sizeValue:F3}").Append(' ').Append(sizeUnits);
        return stringBuilder;
    }

    /// <summary>
    /// Appends data size in binary units as a string.
    /// </summary>
    /// <param name="stringBuilder">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendBinarySize(
        this StringBuilder stringBuilder,
        ulong size,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetBinarySize(size, out double sizeValue, out string sizeUnits, dataUnitFormat);
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{sizeValue:F3}").Append(' ').Append(sizeUnits);
        return stringBuilder;
    }

    /// <summary>
    /// Appends data size in binary units as a string.
    /// </summary>
    /// <param name="stringBuilder">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendBinarySize(
        this StringBuilder stringBuilder,
        BigInteger size,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetBinarySize(size, out BigInteger sizeValueWhole, out double sizeValueFraction, out string sizeUnits, dataUnitFormat);
        stringBuilder.Append(FormatBigIntegerAndFraction(sizeValueWhole, sizeValueFraction)).Append(' ').Append(sizeUnits);
        return stringBuilder;
    }

    /// <summary>
    /// Appends data size in binary units as a string.
    /// </summary>
    /// <param name="stringBuilder">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendBinarySize(
        this StringBuilder stringBuilder,
        double size,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        GetBinarySize(size, out double sizeValue, out string sizeUnits, dataUnitFormat);
        stringBuilder.Append(CultureInfo.InvariantCulture, $"{sizeValue:F3}").Append(' ').Append(sizeUnits);
        return stringBuilder;
    }

    /// <summary>
    /// Appends data rate in binary units as a string.
    /// </summary>
    /// <param name="stringBuilder">String builder.</param>
    /// <param name="rate">Data rate.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendBinaryRate(
        this StringBuilder stringBuilder,
        double rate,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        return stringBuilder.AppendBinarySize(rate, dataUnitFormat).Append("/s");
    }
}
