using System.Numerics;

namespace Art.Common.Tests;

public class DataSizesBinaryTests
{
    public record LongTestCase(long Value, double ExpectedValue, string ExpectedUnit, double Tolerance = 0.001, bool RequireExclusiveOfCeiling = false);

    public record ULongTestCase(ulong Value, double ExpectedValue, string ExpectedUnit, double Tolerance = 0.001, bool RequireExclusiveOfCeiling = false);

    public record BigIntegerTestCase(BigInteger Value, BigInteger ExpectedValueWhole, double ExpectedValueFraction, string ExpectedUnit, double Tolerance = 0.001);

    public record DoubleTestCase(double Value, double ExpectedValue, string ExpectedUnit, double Tolerance = 0.001, bool RequireExclusiveOfCeiling = false);

    public static readonly LongTestCase[] UnsignedLongTestCases =
    [
        // byte
        new(0L, 0, "B"),
        new(1024L - 1, 1023, "B"),
        // kibibyte
        new(1024L, 1, "KiB"),
        new(1024L + 512, 1.5, "KiB"),
        new(1024L * 1024 - 1, 1023.999, "KiB", RequireExclusiveOfCeiling: true),
        // mebibyte
        new(1024L * 1024, 1, "MiB"),
        new((1024L + 512) * 1024, 1.5, "MiB"),
        new(1024L * 1024 * 1024 - 1, 1023.999, "MiB", RequireExclusiveOfCeiling: true),
        // gibibyte
        new(1024L * 1024 * 1024, 1, "GiB"),
        new((1024L + 512) * 1024 * 1024, 1.5, "GiB"),
        new(1024L * 1024 * 1024 * 1024 - 1, 1023.999, "GiB", RequireExclusiveOfCeiling: true),
        // tebibyte
        new(1024L * 1024 * 1024 * 1024, 1, "TiB"),
        new((1024L + 512) * 1024 * 1024 * 1024, 1.5, "TiB"),
        new(1024L * 1024 * 1024 * 1024 * 1024 - 1, 1023.999, "TiB", RequireExclusiveOfCeiling: true),
        // pebibyte
        new(1024L * 1024 * 1024 * 1024 * 1024, 1, "PiB"),
        new((1024L + 512) * 1024 * 1024 * 1024 * 1024, 1.5, "PiB"),
        new((1024L * 1024 * 1024 * 1024 * 1024 - 1) * 1024, 1023.999, "PiB", RequireExclusiveOfCeiling: true),
        new(1024L * 1024 * 1024 * 1024 * 1024 * 1024 - 1, 1023.999, "PiB", RequireExclusiveOfCeiling: false),
        // exbibyte
        new(1024L * 1024 * 1024 * 1024 * 1024 * 1024, 1, "EiB"),
        new((1024L + 512) * 1024 * 1024 * 1024 * 1024 * 1024, 1.5, "EiB"),
    ];

    public static readonly LongTestCase[] LongTestCases =
    [
        ..UnsignedLongTestCases,
        ..UnsignedLongTestCases.Select(static v => v with { Value = -v.Value, ExpectedValue = -v.ExpectedValue }),
        new(long.MaxValue, 7.9999, "EiB"),
        new(long.MinValue, -8, "EiB"),
    ];

    public static readonly ULongTestCase[] ULongTestCases =
    [
        // byte
        new(0L, 0, "B"),
        new(1024L - 1, 1023, "B"),
        // kibibyte
        new(1024L, 1, "KiB"),
        new(1024L + 512, 1.5, "KiB"),
        new(1024L * 1024 - 1, 1023.999, "KiB", RequireExclusiveOfCeiling: true),
        // mebibyte
        new(1024L * 1024, 1, "MiB"),
        new((1024L + 512) * 1024, 1.5, "MiB"),
        new(1024L * 1024 * 1024 - 1, 1023.999, "MiB", RequireExclusiveOfCeiling: true),
        // gibibyte
        new(1024L * 1024 * 1024, 1, "GiB"),
        new((1024L + 512) * 1024 * 1024, 1.5, "GiB"),
        new(1024L * 1024 * 1024 * 1024 - 1, 1023.999, "GiB", RequireExclusiveOfCeiling: true),
        // tebibyte
        new(1024L * 1024 * 1024 * 1024, 1, "TiB"),
        new((1024L + 512) * 1024 * 1024 * 1024, 1.5, "TiB"),
        new(1024L * 1024 * 1024 * 1024 * 1024 - 1, 1023.999, "TiB", RequireExclusiveOfCeiling: true),
        // pebibyte
        new(1024L * 1024 * 1024 * 1024 * 1024, 1, "PiB"),
        new((1024L + 512) * 1024 * 1024 * 1024 * 1024, 1.5, "PiB"),
        new((1024L * 1024 * 1024 * 1024 * 1024 - 1) * 1024, 1023.999, "PiB", RequireExclusiveOfCeiling: true),
        new(1024L * 1024 * 1024 * 1024 * 1024 * 1024 - 1, 1023.999, "PiB", RequireExclusiveOfCeiling: false),
        // exbibyte
        new(1024L * 1024 * 1024 * 1024 * 1024 * 1024, 1, "EiB"),
        new((1024L + 512) * 1024 * 1024 * 1024 * 1024 * 1024, 1.5, "EiB"),
        new(ulong.MaxValue, 16, "EiB"),
    ];

    public static readonly BigIntegerTestCase[] UnsignedBigIntegerTestCases =
    [
        // byte
        new(0L, 0, 0, "B"),
        new(1024L - 1, 1023, 0, "B"),
        // kibibyte
        new(1024L, 1, 0, "KiB"),
        new(1024L + 512, 1, 0.5, "KiB"),
        new(1024L * 1024 - 1, 1023, 0.999, "KiB"),
        // mebibyte
        new(1024L * 1024, 1, 0, "MiB"),
        new((1024L + 512) * 1024, 1, 0.5, "MiB"),
        new(1024L * 1024 * 1024 - 1, 1023, 0.999, "MiB"),
        // gibibyte
        new(1024L * 1024 * 1024, 1, 0, "GiB"),
        new((1024L + 512) * 1024 * 1024, 1, 0.5, "GiB"),
        new(1024L * 1024 * 1024 * 1024 - 1, 1023, 0.999, "GiB"),
        // tebibyte
        new(1024L * 1024 * 1024 * 1024, 1, 0, "TiB"),
        new((1024L + 512) * 1024 * 1024 * 1024, 1, 0.5, "TiB"),
        new(1024L * 1024 * 1024 * 1024 * 1024 - 1, 1023, 0.999, "TiB"),
        // pebibyte
        new(1024L * 1024 * 1024 * 1024 * 1024, 1, 0, "PiB"),
        new((1024L + 512) * 1024 * 1024 * 1024 * 1024, 1, 0.5, "PiB"),
        new(1024L * 1024 * 1024 * 1024 * 1024 * 1024 - 1, 1023, 0.999, "PiB"),
        // exbibyte
        new((BigInteger)1024L * 1024 * 1024 * 1024 * 1024 * 1024, 1, 0, "EiB"),
        new(((BigInteger)1024L + 512) * 1024 * 1024 * 1024 * 1024 * 1024, 1, 0.5, "EiB"),
        new((BigInteger)1024L * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 - 1, 1023, 0.999, "EiB"),
        // zebibyte
        new((BigInteger)1024L * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1, 0, "ZiB"),
        new(((BigInteger)1024L + 512) * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1, 0.5, "ZiB"),
        new((BigInteger)1024L * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 - 1, 1023, 0.999, "ZiB"),
        // yobibyte
        new((BigInteger)1024L * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1, 0, "YiB"),
        new(((BigInteger)1024L + 512) * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1, 0.5, "YiB"),
        new((BigInteger)1024L * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 - 1, 1023, 0.999, "YiB"),
        // robibyte
        new((BigInteger)1024L * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1, 0, "RiB"),
        new(((BigInteger)1024L + 512) * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1, 0.5, "RiB"),
        new((BigInteger)1024L * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 - 1, 1023, 0.999, "RiB"),
        // quebibyte
        new((BigInteger)1024L * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1, 0, "QiB"),
        new(((BigInteger)1024L + 512) * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1, 0.5, "QiB"),
        new((BigInteger)1024L * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 - 1, 1023, 0.999, "QiB"),
        new((BigInteger)1024L * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1024, 0, "QiB"),
        new((BigInteger)1024L * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1024 * 1024, 0, "QiB"),
        new((BigInteger)1 << 1024, (BigInteger)1 << (1024 - 10 * 10), 0, "QiB"),
        new(
            ((BigInteger)1 << 1024) + (BigInteger)512 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024,
            (BigInteger)1 << (1024 - 10 * 10),
            0.500,
            "QiB"),
    ];

    public static readonly BigIntegerTestCase[] BigIntegerTestCases =
    [
        ..UnsignedBigIntegerTestCases,
        ..UnsignedBigIntegerTestCases.Select(static v => v with { Value = -v.Value, ExpectedValueWhole = -v.ExpectedValueWhole }),
    ];
    public static readonly DoubleTestCase[] UnsignedDoubleTestCases =
    [
        // byte
        new(0L, 0, "B"),
        new(1024L - 1, 1023, "B"),
        // kibibyte
        new(1024L, 1, "KiB"),
        new(1024L + 512, 1.5, "KiB"),
        new(1024L * 1024 - 1, 1023.999, "KiB", RequireExclusiveOfCeiling: true),
        // mebibyte
        new(1024L * 1024, 1, "MiB"),
        new((1024L + 512) * 1024, 1.5, "MiB"),
        new(1024L * 1024 * 1024 - 1, 1023.999, "MiB", RequireExclusiveOfCeiling: true),
        // gibibyte
        new(1024L * 1024 * 1024, 1, "GiB"),
        new((1024L + 512) * 1024 * 1024, 1.5, "GiB"),
        new(1024L * 1024 * 1024 * 1024 - 1, 1023.999, "GiB", RequireExclusiveOfCeiling: true),
        // tebibyte
        new(1024L * 1024 * 1024 * 1024, 1, "TiB"),
        new((1024L + 512) * 1024 * 1024 * 1024, 1.5, "TiB"),
        new(1024L * 1024 * 1024 * 1024 * 1024 - 1, 1023.999, "TiB", RequireExclusiveOfCeiling: true),
        // pebibyte
        new(1024L * 1024 * 1024 * 1024 * 1024, 1, "PiB"),
        new((1024L + 512) * 1024 * 1024 * 1024 * 1024, 1.5, "PiB"),
        new((1024L * 1024 * 1024 * 1024 * 1024 - 1) * 1024, 1023.999, "PiB", RequireExclusiveOfCeiling: true),
        // exbibyte
        new(1024.0 * 1024 * 1024 * 1024 * 1024 * 1024, 1, "EiB"),
        new((1024.0 + 512) * 1024 * 1024 * 1024 * 1024 * 1024, 1.5, "EiB"),
        new((1024.0 * 1024 * 1024 * 1024 * 1024 - 1) * 1024 * 1024, 1023.999, "EiB", RequireExclusiveOfCeiling: true),
        // zebibyte
        new(1024.0 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1, "ZiB"),
        new((1024.0 + 512) * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1.5, "ZiB"),
        new((1024.0 * 1024 * 1024 * 1024 * 1024 - 1) * 1024 * 1024 * 1024, 1023.999, "ZiB", RequireExclusiveOfCeiling: true),
        // yobibyte
        new(1024.0 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1, "YiB"),
        new((1024.0 + 512) * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1.5, "YiB"),
        new((1024.0 * 1024 * 1024 * 1024 * 1024 - 1) * 1024 * 1024 * 1024 * 1024, 1023.999, "YiB", RequireExclusiveOfCeiling: true),
        // robibyte
        new(1024.0 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1, "RiB"),
        new((1024.0 + 512) * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1.5, "RiB"),
        new((1024.0 * 1024 * 1024 * 1024 * 1024 - 1) * 1024 * 1024 * 1024 * 1024 * 1024, 1023.999, "RiB", RequireExclusiveOfCeiling: true),
        // quebibyte
        new(1024.0 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1, "QiB"),
        new((1024.0 + 512) * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1.5, "QiB"),
        new((1024.0 * 1024 * 1024 * 1024 * 1024 - 1) * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1023.999, "QiB", RequireExclusiveOfCeiling: true),
        new(1024.0 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1024, "QiB"),
        new(1024.0 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024 * 1024, 1024 * 1024, "QiB"),
    ];

    public static readonly DoubleTestCase[] DoubleTestCases =
    [
        ..UnsignedDoubleTestCases,
        ..UnsignedDoubleTestCases.Select(static v => v with { Value = -v.Value, ExpectedValue = -v.ExpectedValue }),
    ];

    public static readonly TheoryData<LongTestCase> TdLongTestCases = new(LongTestCases);
    public static readonly TheoryData<ULongTestCase> TdULongTestCases = new(ULongTestCases);
    public static readonly TheoryData<BigIntegerTestCase> TdBigIntegerTestCases = new(BigIntegerTestCases);
    public static readonly TheoryData<DoubleTestCase> TdDoubleTestCases = new(DoubleTestCases);

    [Theory]
    [MemberData(nameof(TdLongTestCases))]
    public void GetBinarySize_Long_Correct(LongTestCase testCase)
    {
        DataSizes.GetBinarySize(testCase.Value, out double value, out string unit, dataUnitFormat: DataUnitFormat.Short);
        Assert.Equal(testCase.ExpectedUnit, unit);
        Assert.Equal(testCase.ExpectedValue, value, testCase.Tolerance);
        Assert.True(value >= Math.Floor(testCase.ExpectedValue));
        if (testCase.RequireExclusiveOfCeiling)
        {
            Assert.True(value < Math.Ceiling(testCase.ExpectedValue));
        }
        else
        {
            Assert.True(value <= Math.Ceiling(testCase.ExpectedValue));
        }
    }

    [Theory]
    [MemberData(nameof(TdULongTestCases))]
    public void GetBinarySize_ULong_Correct(ULongTestCase testCase)
    {
        DataSizes.GetBinarySize(testCase.Value, out double value, out string unit, dataUnitFormat: DataUnitFormat.Short);
        Assert.Equal(testCase.ExpectedUnit, unit);
        Assert.Equal(testCase.ExpectedValue, value, testCase.Tolerance);
        Assert.True(value >= Math.Floor(testCase.ExpectedValue));
        if (testCase.RequireExclusiveOfCeiling)
        {
            Assert.True(value < Math.Ceiling(testCase.ExpectedValue));
        }
        else
        {
            Assert.True(value <= Math.Ceiling(testCase.ExpectedValue));
        }
    }

    [Theory]
    [MemberData(nameof(TdBigIntegerTestCases))]
    public void GetBinarySize_BigInteger_Correct(BigIntegerTestCase testCase)
    {
        DataSizes.GetBinarySize(testCase.Value, out BigInteger valueWhole, out double valueFraction, out string unit, dataUnitFormat: DataUnitFormat.Short);
        Assert.True(valueFraction >= 0);
        Assert.True(valueFraction < 1);
        Assert.Equal(testCase.ExpectedUnit, unit);
        Assert.Equal(testCase.ExpectedValueWhole, valueWhole);
        Assert.Equal(testCase.ExpectedValueFraction, valueFraction, testCase.Tolerance);
    }

    [Theory]
    [MemberData(nameof(TdDoubleTestCases))]
    public void GetBinarySize_Double_Correct(DoubleTestCase testCase)
    {
        DataSizes.GetBinarySize(testCase.Value, out double value, out string unit, dataUnitFormat: DataUnitFormat.Short);
        Assert.Equal(testCase.ExpectedUnit, unit);
        Assert.Equal(testCase.ExpectedValue, value, testCase.Tolerance);
        Assert.True(value >= Math.Floor(testCase.ExpectedValue));
        if (testCase.RequireExclusiveOfCeiling)
        {
            Assert.True(value < Math.Ceiling(testCase.ExpectedValue));
        }
        else
        {
            Assert.True(value <= Math.Ceiling(testCase.ExpectedValue));
        }
    }
}
