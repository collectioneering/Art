using System.Numerics;
using System.Text;

namespace Art.Common;

public static partial class DataSizes
{
    /// <summary>
    /// Simplifies size of datum with the largest nameable units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KB, MB, etc.).</param>
    /// <param name="dataUnits">Units to use.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    public static void GetSize(
        long size,
        out double value,
        out string unit,
        DataUnits dataUnits = DataUnits.Binary,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                GetBinarySize(size, out value, out unit, dataUnitFormat);
                break;
            case DataUnits.Decimal:
                GetDecimalSize(size, out value, out unit, dataUnitFormat);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unitIndex">Unit index (e.g. 0=B, 1=KB, 2=MB, etc.).</param>
    /// <param name="dataUnits">Units to use.</param>
    public static void GetSize(
        long size,
        out double value,
        out int unitIndex,
        DataUnits dataUnits)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                GetBinarySize(size, out value, out unitIndex);
                break;
            case DataUnits.Decimal:
                GetDecimalSize(size, out value, out unitIndex);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KB, MB, etc.).</param>
    /// <param name="dataUnits">Units to use.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    public static void GetSize(
        ulong size,
        out double value,
        out string unit,
        DataUnits dataUnits = DataUnits.Binary,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                GetBinarySize(size, out value, out unit, dataUnitFormat);
                break;
            case DataUnits.Decimal:
                GetDecimalSize(size, out value, out unit, dataUnitFormat);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }


    /// <summary>
    /// Simplifies size of datum with the largest nameable units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unitIndex">Unit index (e.g. 0=B, 1=KB, 2=MB, etc.).</param>
    /// <param name="dataUnits">Units to use.</param>
    public static void GetSize(
        ulong size,
        out double value,
        out int unitIndex,
        DataUnits dataUnits)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                GetBinarySize(size, out value, out unitIndex);
                break;
            case DataUnits.Decimal:
                GetDecimalSize(size, out value, out unitIndex);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="valueWhole">Integer portion of value for given units.</param>
    /// <param name="valueFraction">Fractional portion of value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KB, MB, etc.).</param>
    /// <param name="dataUnits">Units to use.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    public static void GetSize(
        BigInteger size,
        out BigInteger valueWhole,
        out double valueFraction,
        out string unit,
        DataUnits dataUnits = DataUnits.Binary,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                GetBinarySize(size, out valueWhole, out valueFraction, out unit, dataUnitFormat);
                break;
            case DataUnits.Decimal:
                GetDecimalSize(size, out valueWhole, out valueFraction, out unit, dataUnitFormat);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="valueWhole">Integer portion of value for given units.</param>
    /// <param name="valueFraction">Fractional portion of value for given units.</param>
    /// <param name="unitIndex">Unit index (e.g. 0=B, 1=KB, 2=MB, etc.).</param>
    /// <param name="dataUnits">Units to use.</param>
    public static void GetSize(
        BigInteger size,
        out BigInteger valueWhole,
        out double valueFraction,
        out int unitIndex,
        DataUnits dataUnits)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                GetBinarySize(size, out valueWhole, out valueFraction, out unitIndex);
                break;
            case DataUnits.Decimal:
                GetDecimalSize(size, out valueWhole, out valueFraction, out unitIndex);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unit">Unit (e.g. B, KB, MB, etc.).</param>
    /// <param name="dataUnits">Units to use.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    public static void GetSize(
        double size,
        out double value,
        out string unit,
        DataUnits dataUnits = DataUnits.Binary,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                GetBinarySize(size, out value, out unit, dataUnitFormat);
                break;
            case DataUnits.Decimal:
                GetDecimalSize(size, out value, out unit, dataUnitFormat);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }

    /// <summary>
    /// Simplifies size of datum with the largest nameable units.
    /// </summary>
    /// <param name="size">Datum size.</param>
    /// <param name="value">Value for given units.</param>
    /// <param name="unitIndex">Unit index (e.g. 0=B, 1=KB, 2=MB, etc.).</param>
    /// <param name="dataUnits">Units to use.</param>
    public static void GetSize(
        double size,
        out double value,
        out int unitIndex,
        DataUnits dataUnits)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                GetBinarySize(size, out value, out unitIndex);
                break;
            case DataUnits.Decimal:
                GetDecimalSize(size, out value, out unitIndex);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }

    /// <summary>
    /// Gets data size in units as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnits">Units to use.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>Data size string.</returns>
    public static string GetSizeString(
        long size,
        DataUnits dataUnits = DataUnits.Binary,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                return GetBinarySizeString(size, dataUnitFormat);
            case DataUnits.Decimal:
                return GetDecimalSizeString(size, dataUnitFormat);
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }

    /// <summary>
    /// Gets data size in units as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnits">Units to use.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>Data size string.</returns>
    public static string GetSizeString(
        ulong size,
        DataUnits dataUnits = DataUnits.Binary,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                return GetBinarySizeString(size, dataUnitFormat);
            case DataUnits.Decimal:
                return GetDecimalSizeString(size, dataUnitFormat);
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }

    /// <summary>
    /// Gets data size in units as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnits">Units to use.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>Data size string.</returns>
    public static string GetSizeString(
        BigInteger size,
        DataUnits dataUnits = DataUnits.Binary,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                return GetBinarySizeString(size, dataUnitFormat);
            case DataUnits.Decimal:
                return GetDecimalSizeString(size, dataUnitFormat);
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }

    /// <summary>
    /// Gets data size in units as a string (e.g. 5.4 MB).
    /// </summary>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnits">Units to use.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>Data size string.</returns>
    public static string GetSizeString(
        double size,
        DataUnits dataUnits = DataUnits.Binary,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                return GetBinarySizeString(size, dataUnitFormat);
            case DataUnits.Decimal:
                return GetDecimalSizeString(size, dataUnitFormat);
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }

    /// <summary>
    /// Appends data size in units as a string.
    /// </summary>
    /// <param name="stringBuilder">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnits">Units to use.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendSize(
        this StringBuilder stringBuilder,
        long size,
        DataUnits dataUnits = DataUnits.Binary,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                return stringBuilder.AppendBinarySize(size, dataUnitFormat);
            case DataUnits.Decimal:
                return stringBuilder.AppendDecimalSize(size, dataUnitFormat);
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }

    /// <summary>
    /// Appends data size in units as a string.
    /// </summary>
    /// <param name="stringBuilder">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnits">Units to use.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendSize(
        this StringBuilder stringBuilder,
        ulong size,
        DataUnits dataUnits = DataUnits.Binary,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                return stringBuilder.AppendBinarySize(size, dataUnitFormat);
            case DataUnits.Decimal:
                return stringBuilder.AppendDecimalSize(size, dataUnitFormat);
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }

    /// <summary>
    /// Appends data size in units as a string.
    /// </summary>
    /// <param name="stringBuilder">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnits">Units to use.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendSize(
        this StringBuilder stringBuilder,
        BigInteger size,
        DataUnits dataUnits = DataUnits.Binary,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                return stringBuilder.AppendBinarySize(size, dataUnitFormat);
            case DataUnits.Decimal:
                return stringBuilder.AppendDecimalSize(size, dataUnitFormat);
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }

    /// <summary>
    /// Appends data size in units as a string.
    /// </summary>
    /// <param name="stringBuilder">String builder.</param>
    /// <param name="size">Data size.</param>
    /// <param name="dataUnits">Units to use.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendSize(
        this StringBuilder stringBuilder,
        double size,
        DataUnits dataUnits = DataUnits.Binary,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                return stringBuilder.AppendBinarySize(size, dataUnitFormat);
            case DataUnits.Decimal:
                return stringBuilder.AppendDecimalSize(size, dataUnitFormat);
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }

    /// <summary>
    /// Appends data rate in units as a string.
    /// </summary>
    /// <param name="stringBuilder">String builder.</param>
    /// <param name="rate">Data rate.</param>
    /// <param name="dataUnits">Units to use.</param>
    /// <param name="dataUnitFormat">The format to use for units.</param>
    /// <returns>String builder (for chaining).</returns>
    public static StringBuilder AppendRate(
        this StringBuilder stringBuilder,
        double rate,
        DataUnits dataUnits = DataUnits.Binary,
        DataUnitFormat dataUnitFormat = DataUnitFormat.Short)
    {
        switch (dataUnits)
        {
            case DataUnits.Binary:
                return stringBuilder.AppendBinaryRate(rate, dataUnitFormat);
            case DataUnits.Decimal:
                return stringBuilder.AppendDecimalRate(rate, dataUnitFormat);
            default:
                throw new ArgumentOutOfRangeException(nameof(dataUnits), dataUnits, null);
        }
    }
}
