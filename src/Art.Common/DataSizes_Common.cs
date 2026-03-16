using System.Globalization;
using System.Numerics;

namespace Art.Common;

/// <summary>
/// Constants and utility methods for data sizes.
/// </summary>
public static partial class DataSizes
{
    internal static string FormatBigIntegerAndFraction(BigInteger valueWhole, double valueFraction)
    {
        if (valueFraction is >= 1 or < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(valueFraction));
        }
        ReadOnlySpan<char> fractionString = string.Format(CultureInfo.InvariantCulture, "{0:F3}", valueFraction);
        int fractionIndex = fractionString.IndexOf('.');
        if (fractionIndex < 0)
        {
            throw new InvalidDataException();
        }
        if (fractionString[..fractionIndex] is not "0")
        {
            fractionString = "999";
        }
        else
        {
            fractionString = fractionString[(fractionIndex + 1)..];
        }
        return $"{valueWhole}{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}{fractionString}";
    }

    private static double GetFraction(BigInteger dividend, BigInteger divisor)
    {
        // preserve at least 52 bits
        long shiftBits = Math.Min(dividend.GetBitLength(), divisor.GetBitLength()) - 54;
        if (shiftBits is > 0 and <= int.MaxValue)
        {
            return (double)(dividend >> (int)shiftBits) / (double)(divisor >> (int)shiftBits);
        }
        return (double)dividend / (double)divisor;
    }

    private static double GetFraction(ulong dividend, ulong divisor)
    {
        // preserve at least 52 bits
        long shiftBits = Math.Min(64 - BitOperations.LeadingZeroCount(dividend), 64 - BitOperations.LeadingZeroCount(divisor)) - 54;
        if (shiftBits is > 0 and <= int.MaxValue)
        {
            return (double)(dividend >> (int)shiftBits) / (divisor >> (int)shiftBits);
        }
        return (double)dividend / divisor;
    }
}
