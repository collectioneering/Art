using System.Numerics;

namespace Art.Common.Tests;

public class DataSizesDecimalTests
{
    public record LongTestCase(long Value, double ExpectedValue, string ExpectedUnit, double Tolerance = 0.001, bool RequireExclusiveOfCeiling = false);

    public record ULongTestCase(ulong Value, double ExpectedValue, string ExpectedUnit, double Tolerance = 0.001, bool RequireExclusiveOfCeiling = false);

    public record BigIntegerTestCase(BigInteger Value, BigInteger ExpectedValueWhole, double ExpectedValueFraction, string ExpectedUnit, double Tolerance = 0.001);

    public record DoubleTestCase(double Value, double ExpectedValue, string ExpectedUnit, double Tolerance = 0.001, bool RequireExclusiveOfCeiling = false, bool SkipBoundCheck = false);

    public static readonly LongTestCase[] UnsignedLongTestCases =
    [
        // byte
        new(0L, 0, "B"),
        new(1000L - 1, 999, "B"),
        // kilobyte
        new(1000L, 1, "kB"),
        new(1000L + 500, 1.5, "kB"),
        new(1000L * 1000 - 1, 999.9999, "kB", RequireExclusiveOfCeiling: true),
        // megabyte
        new(1000L * 1000, 1, "MB"),
        new((1000L + 500) * 1000, 1.5, "MB"),
        new(1000L * 1000 * 1000 - 1, 999.9999, "MB", RequireExclusiveOfCeiling: true),
        // gigabyte
        new(1000L * 1000 * 1000, 1, "GB"),
        new((1000L + 500) * 1000 * 1000, 1.5, "GB"),
        new(1000L * 1000 * 1000 * 1000 - 1, 999.9999, "GB", RequireExclusiveOfCeiling: true),
        // terabyte
        new(1000L * 1000 * 1000 * 1000, 1, "TB"),
        new((1000L + 500) * 1000 * 1000 * 1000, 1.5, "TB"),
        new(1000L * 1000 * 1000 * 1000 * 1000 - 1, 999.9999, "TB", RequireExclusiveOfCeiling: true),
        // petabyte
        new(1000L * 1000 * 1000 * 1000 * 1000, 1, "PB"),
        new((1000L + 500) * 1000 * 1000 * 1000 * 1000, 1.5, "PB"),
        new(1000L * 1000 * 1000 * 1000 * 1000 * 1000 - 1, 999.9999, "PB", RequireExclusiveOfCeiling: true),
        // exabyte
        new(1000L * 1000 * 1000 * 1000 * 1000 * 1000, 1, "EB"),
        new((1000L + 500) * 1000 * 1000 * 1000 * 1000 * 1000, 1.5, "EB"),
    ];

    public static readonly LongTestCase[] LongTestCases =
    [
        ..UnsignedLongTestCases,
        ..UnsignedLongTestCases.Select(static v => v with { Value = -v.Value, ExpectedValue = -v.ExpectedValue }),
        new(long.MaxValue, 9.2233, "EB"),
        new(long.MinValue, -9.2233, "EB"),
    ];

    public static readonly ULongTestCase[] ULongTestCases =
    [
        // byte
        new(0L, 0, "B"),
        new(1000L - 1, 999, "B"),
        // kilobyte
        new(1000L, 1, "kB"),
        new(1000L + 500, 1.5, "kB"),
        new(1000L * 1000 - 1, 999.9999, "kB", RequireExclusiveOfCeiling: true),
        // megabyte
        new(1000L * 1000, 1, "MB"),
        new((1000L + 500) * 1000, 1.5, "MB"),
        new(1000L * 1000 * 1000 - 1, 999.9999, "MB", RequireExclusiveOfCeiling: true),
        // gigabyte
        new(1000L * 1000 * 1000, 1, "GB"),
        new((1000L + 500) * 1000 * 1000, 1.5, "GB"),
        new(1000L * 1000 * 1000 * 1000 - 1, 999.9999, "GB", RequireExclusiveOfCeiling: true),
        // terabyte
        new(1000L * 1000 * 1000 * 1000, 1, "TB"),
        new((1000L + 500) * 1000 * 1000 * 1000, 1.5, "TB"),
        new(1000L * 1000 * 1000 * 1000 * 1000 - 1, 999.9999, "TB", RequireExclusiveOfCeiling: true),
        // petabyte
        new(1000L * 1000 * 1000 * 1000 * 1000, 1, "PB"),
        new((1000L + 500) * 1000 * 1000 * 1000 * 1000, 1.5, "PB"),
        new(1000L * 1000 * 1000 * 1000 * 1000 * 1000 - 1, 999.9999, "PB", RequireExclusiveOfCeiling: true),
        // exabyte
        new(1000L * 1000 * 1000 * 1000 * 1000 * 1000, 1, "EB"),
        new((1000L + 500) * 1000 * 1000 * 1000 * 1000 * 1000, 1.5, "EB"),
        new(ulong.MaxValue, 18.4467, "EB"),
    ];

    public static readonly BigIntegerTestCase[] UnsignedBigIntegerTestCases =
    [
        // byte
        new(0L, 0, 0, "B"),
        new(1000L - 1, 999, 0, "B"),
        // kilobyte
        new(1000L, 1, 0, "kB"),
        new(1000L + 500, 1, 0.5, "kB"),
        new(1000L * 1000 - 1, 999, 0.9999, "kB"),
        // megabyte
        new(1000L * 1000, 1, 0, "MB"),
        new((1000L + 500) * 1000, 1, 0.5, "MB"),
        new(1000L * 1000 * 1000 - 1, 999, 0.9999, "MB"),
        // gigabyte
        new(1000L * 1000 * 1000, 1, 0, "GB"),
        new((1000L + 500) * 1000 * 1000, 1, 0.5, "GB"),
        new(1000L * 1000 * 1000 * 1000 - 1, 999, 0.9999, "GB"),
        // terabyte
        new(1000L * 1000 * 1000 * 1000, 1, 0, "TB"),
        new((1000L + 500) * 1000 * 1000 * 1000, 1, 0.5, "TB"),
        new(1000L * 1000 * 1000 * 1000 * 1000 - 1, 999, 0.9999, "TB"),
        // petabyte
        new(1000L * 1000 * 1000 * 1000 * 1000, 1, 0, "PB"),
        new((1000L + 500) * 1000 * 1000 * 1000 * 1000, 1, 0.5, "PB"),
        new(1000L * 1000 * 1000 * 1000 * 1000 * 1000 - 1, 999, 0.9999, "PB"),
        // exabyte
        new((BigInteger)1000L * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0, "EB"),
        new(((BigInteger)1000L + 500) * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0.5, "EB"),
        new((BigInteger)1000L * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 - 1, 999, 0.9999, "EB"),
        // zettabyte
        new((BigInteger)1000L * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0, "ZB"),
        new(((BigInteger)1000L + 500) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0.5, "ZB"),
        new((BigInteger)1000L * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 - 1, 999, 0.9999, "ZB"),
        // yottabyte
        new((BigInteger)1000L * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0, "YB"),
        new(((BigInteger)1000L + 500) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0.5, "YB"),
        new((BigInteger)1000L * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 - 1, 999, 0.9999, "YB"),
        // ronnabyte
        new((BigInteger)1000L * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0, "RB"),
        new(((BigInteger)1000L + 500) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0.5, "RB"),
        new((BigInteger)1000L * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 - 1, 999, 0.9999, "RB"),
        // quettabyte
        new((BigInteger)1000L * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0, "QB"),
        new(((BigInteger)1000L + 500) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0.5, "QB"),
        new((BigInteger)1000L * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 - 1, 999, 0.9999, "QB"),
        new((BigInteger)1000L * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1000, 0, "QB"),
        new((BigInteger)1000L * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1000 * 1000, 0, "QB"),
        new(BigInteger.Pow(10, 1024), 1 * BigInteger.Pow(10, 1024 - 3 * 10), 0, "QB"),
        new(
            BigInteger.Pow(10, 1024) + (BigInteger)500 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000,
            1 * BigInteger.Pow(10, 1024 - 3 * 10),
            0.500,
            "QB"),
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
        new(1000L - 1, 999, "B"),
        // kilobyte
        new(1000L, 1, "kB"),
        new(1000L + 500, 1.5, "kB"),
        new(1000L * 1000 - 1, 999.9999, "kB", RequireExclusiveOfCeiling: true),
        // megabyte
        new(1000L * 1000, 1, "MB"),
        new((1000L + 500) * 1000, 1.5, "MB"),
        new(1000L * 1000 * 1000 - 1, 999.9999, "MB", RequireExclusiveOfCeiling: true),
        // gigabyte
        new(1000L * 1000 * 1000, 1, "GB"),
        new((1000L + 500) * 1000 * 1000, 1.5, "GB"),
        new(1000L * 1000 * 1000 * 1000 - 1, 999.9999, "GB", RequireExclusiveOfCeiling: true),
        // terabyte
        new(1000L * 1000 * 1000 * 1000, 1, "TB"),
        new((1000L + 500) * 1000 * 1000 * 1000, 1.5, "TB"),
        //new(1000L * 1000 * 1000 * 1000 * 1000 - 1, 999.9999, "TB", RequireExclusiveOfCeiling: true),
        new((1000L * 1000 * 1000 * 1000 - 1) * 1000, 999.9999, "TB", RequireExclusiveOfCeiling: true),
        // petabyte
        new(1000L * 1000 * 1000 * 1000 * 1000, 1, "PB"),
        new((1000L + 500) * 1000 * 1000 * 1000 * 1000, 1.5, "PB"),
        //new((1000L * 1000 * 1000 * 1000 * 1000 - 1) * 1000, 999.9999, "PB", RequireExclusiveOfCeiling: true),
        new((1000L * 1000 * 1000 * 1000 - 1) * 1000 * 1000, 999.9999, "PB", RequireExclusiveOfCeiling: true),
        // exabyte
        new(1000.0 * 1000 * 1000 * 1000 * 1000 * 1000, 1, "EB"),
        new((1000.0 + 500) * 1000 * 1000 * 1000 * 1000 * 1000, 1.5, "EB"),
        //new((1000.0 * 1000 * 1000 * 1000 * 1000 - 1) * 1000 * 1000, 999.9999, "EB", RequireExclusiveOfCeiling: true),
        new((1000.0 * 1000 * 1000 * 1000 - 1) * 1000 * 1000 * 1000, 999.9999, "EB", RequireExclusiveOfCeiling: true),
        // zettabyte
        new(1000.0 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, "ZB"),
        new((1000.0 + 500) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1.5, "ZB"),
        //new((1000.0 * 1000 * 1000 * 1000 * 1000 - 1) * 1000 * 1000 * 1000, 999.9999, "ZB", RequireExclusiveOfCeiling: true),
        new((1000.0 * 1000 * 1000 * 1000 - 1) * 1000 * 1000 * 1000 * 1000, 999.9999, "ZB", RequireExclusiveOfCeiling: true),
        // yottabyte
        new(1000.0 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, "YB"),
        new((1000.0 + 500) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1.5, "YB"),
        //new((1000.0 * 1000 * 1000 * 1000 * 1000 - 1) * 1000 * 1000 * 1000 * 1000, 999.9999, "YB", RequireExclusiveOfCeiling: true),
        new((1000.0 * 1000 * 1000 * 1000 - 1) * 1000 * 1000 * 1000 * 1000 * 1000, 999.9999, "YB", RequireExclusiveOfCeiling: true),
        // ronnabyte
        new(1000.0 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, "RB"),
        new((1000.0 + 500) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1.5, "RB"),
        //new((1000.0 * 1000 * 1000 * 1000 * 1000 - 1) * 1000 * 1000 * 1000 * 1000 * 1000, 999.9999, "RB", RequireExclusiveOfCeiling: true),
        new((1000.0 * 1000 * 1000 * 1000 - 1) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 999.9999, "RB", RequireExclusiveOfCeiling: true),
        // quettabyte
        new(1000.0 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, "QB"),
        new((1000.0 + 500) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1.5, "QB"),
        //new((1000.0 * 1000 * 1000 * 1000 * 1000 - 1) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 999.9999, "QB", RequireExclusiveOfCeiling: true),
        new((1000.0 * 1000 * 1000 * 1000 - 1) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 999.9999, "QB", RequireExclusiveOfCeiling: true),
        new(1000.0 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1000, "QB", SkipBoundCheck: true),
        new(1000.0 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1000 * 1000, "QB"),
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
    public void GetDecimalSize_Long_Correct(LongTestCase testCase)
    {
        DataSizes.GetDecimalSize(testCase.Value, out double value, out string unit, dataUnitFormat: DataUnitFormat.Short);
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
    public void GetDecimalSize_ULong_Correct(ULongTestCase testCase)
    {
        DataSizes.GetDecimalSize(testCase.Value, out double value, out string unit, dataUnitFormat: DataUnitFormat.Short);
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
    public void GetDecimalSize_BigInteger_Correct(BigIntegerTestCase testCase)
    {
        DataSizes.GetDecimalSize(testCase.Value, out BigInteger valueWhole, out double valueFraction, out string unit, dataUnitFormat: DataUnitFormat.Short);
        Assert.True(valueFraction >= 0);
        Assert.True(valueFraction < 1);
        Assert.Equal(testCase.ExpectedUnit, unit);
        Assert.Equal(testCase.ExpectedValueWhole, valueWhole);
        Assert.Equal(testCase.ExpectedValueFraction, valueFraction, testCase.Tolerance);
    }

    [Theory]
    [MemberData(nameof(TdDoubleTestCases))]
    public void GetDecimalSize_Double_Correct(DoubleTestCase testCase)
    {
        DataSizes.GetDecimalSize(testCase.Value, out double value, out string unit, dataUnitFormat: DataUnitFormat.Short);
        Assert.Equal(testCase.ExpectedUnit, unit);
        Assert.Equal(testCase.ExpectedValue, value, testCase.Tolerance);
        if (!testCase.SkipBoundCheck)
        {
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
}
