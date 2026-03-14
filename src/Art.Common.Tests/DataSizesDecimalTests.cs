using System.Numerics;
using NUnit.Framework;

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
        //new(1000L * 1000 * 1000 * 1000 * 1000 - 1, 999.9999, "TB", RequireExclusiveOfCeiling: true),
        new((1000L * 1000 * 1000 * 1000 - 1) * 1000, 999.9999, "TB", RequireExclusiveOfCeiling: true),
        // petabyte
        new(1000L * 1000 * 1000 * 1000 * 1000, 1, "PB"),
        new((1000L + 500) * 1000 * 1000 * 1000 * 1000, 1.5, "PB"),
        //new((1000L * 1000 * 1000 * 1000 * 1000 - 1) * 1000, 999.9999, "PB", RequireExclusiveOfCeiling: true),
        //new(1000L * 1000 * 1000 * 1000 * 1000 * 1000 - 1, 999.9999, "PB", RequireExclusiveOfCeiling: false),
        new((1000L * 1000 * 1000 * 1000 - 1) * 1000 * 1000, 999.9999, "PB", RequireExclusiveOfCeiling: true),
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
        //new(1000L * 1000 * 1000 * 1000 * 1000 - 1, 999.9999, "TB", RequireExclusiveOfCeiling: true),
        new((1000L * 1000 * 1000 * 1000 - 1) * 1000, 999.9999, "TB", RequireExclusiveOfCeiling: true),
        // petabyte
        new(1000L * 1000 * 1000 * 1000 * 1000, 1, "PB"),
        new((1000L + 500) * 1000 * 1000 * 1000 * 1000, 1.5, "PB"),
        //new((1000L * 1000 * 1000 * 1000 * 1000 - 1) * 1000, 999.9999, "PB", RequireExclusiveOfCeiling: true),
        //new(1000L * 1000 * 1000 * 1000 * 1000 * 1000 - 1, 999.9999, "PB", RequireExclusiveOfCeiling: false),
        new((1000L * 1000 * 1000 * 1000 - 1) * 1000 * 1000, 999.9999, "PB", RequireExclusiveOfCeiling: true),
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
        new(((BigInteger)1000L * 1000 * 1000 * 1000 - 1) * 1000 * 1000 * 1000, 999, 0.9999, "EB"),
        // zettabyte
        new((BigInteger)1000L * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0, "ZB"),
        new(((BigInteger)1000L + 500) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0.5, "ZB"),
        new(((BigInteger)1000L * 1000 * 1000 * 1000 - 1) * 1000 * 1000 * 1000 * 1000, 999, 0.9999, "ZB"),
        // yottabyte
        new((BigInteger)1000L * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0, "YB"),
        new(((BigInteger)1000L + 500) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0.5, "YB"),
        new(((BigInteger)1000L * 1000 * 1000 * 1000 - 1) * 1000 * 1000 * 1000 * 1000 * 1000, 999, 0.9999, "YB"),
        // ronnabyte
        new((BigInteger)1000L * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0, "RB"),
        new(((BigInteger)1000L + 500) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0.5, "RB"),
        new(((BigInteger)1000L * 1000 * 1000 * 1000 - 1) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 999, 0.9999, "RB"),
        // quettabyte
        new((BigInteger)1000L * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0, "QB"),
        new(((BigInteger)1000L + 500) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000, 1, 0.5, "QB"),
        new(((BigInteger)1000L * 1000 * 1000 * 1000 - 1) * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 * 1000 - 1, 999, 0.9999, "QB"),
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

    [Test]
    public void GetDecimalSize_Long_Correct([ValueSource(nameof(ULongTestCases))] ULongTestCase testCase)
    {
        DataSizes.GetDecimalSize(testCase.Value, out double value, out string unit);
        Assert.That(
            new DatumSize(value, unit),
            Has.Property(nameof(DatumSize.Unit)).EqualTo(testCase.ExpectedUnit)
                .And.Property(nameof(DatumSize.Value)).EqualTo(testCase.ExpectedValue).Within(testCase.Tolerance),
            $"{value} {unit} does not match test case {testCase}");
        Assert.That(unit, Is.EqualTo(testCase.ExpectedUnit));
        Assert.That(value, Is.EqualTo(testCase.ExpectedValue).Within(testCase.Tolerance));
        Assert.That(value, Is.AtLeast(Math.Floor(testCase.ExpectedValue)));
        if (testCase.RequireExclusiveOfCeiling)
        {
            Assert.That(value, Is.LessThan(Math.Ceiling(testCase.ExpectedValue)));
        }
        else
        {
            Assert.That(value, Is.AtMost(Math.Ceiling(testCase.ExpectedValue)));
        }
    }

    [Test]
    public void GetDecimalSize_ULong_Correct([ValueSource(nameof(LongTestCases))] LongTestCase testCase)
    {
        DataSizes.GetDecimalSize(testCase.Value, out double value, out string unit);
        Assert.That(
            new DatumSize(value, unit),
            Has.Property(nameof(DatumSize.Unit)).EqualTo(testCase.ExpectedUnit)
                .And.Property(nameof(DatumSize.Value)).EqualTo(testCase.ExpectedValue).Within(testCase.Tolerance),
            $"{value} {unit} does not match test case {testCase}");
        Assert.That(unit, Is.EqualTo(testCase.ExpectedUnit));
        Assert.That(value, Is.EqualTo(testCase.ExpectedValue).Within(testCase.Tolerance));
        Assert.That(value, Is.AtLeast(Math.Floor(testCase.ExpectedValue)));
        if (testCase.RequireExclusiveOfCeiling)
        {
            Assert.That(value, Is.LessThan(Math.Ceiling(testCase.ExpectedValue)));
        }
        else
        {
            Assert.That(value, Is.AtMost(Math.Ceiling(testCase.ExpectedValue)));
        }
    }

    [Test]
    public void GetDecimalSize_BigInteger_Correct([ValueSource(nameof(BigIntegerTestCases))] BigIntegerTestCase testCase)
    {
        DataSizes.GetDecimalSize(testCase.Value, out BigInteger valueWhole, out double valueFraction, out string unit);
        string message = $"{DataSizes.FormatBigIntegerAndFraction(valueWhole, valueFraction)} {unit} does not match test case {testCase}";
        Assert.That(valueFraction, Is.AtLeast(0).And.LessThan(1), message);
        Assert.That(unit, Is.EqualTo(testCase.ExpectedUnit), message);
        Assert.That(valueWhole, Is.EqualTo(testCase.ExpectedValueWhole), message);
        Assert.That(valueFraction, Is.EqualTo(testCase.ExpectedValueFraction).Within(testCase.Tolerance), message);
    }

    [Test]
    public void GetDecimalSize_Double_Correct([ValueSource(nameof(DoubleTestCases))] DoubleTestCase testCase)
    {
        DataSizes.GetDecimalSize(testCase.Value, out double value, out string unit);
        Assert.That(
            new DatumSize(value, unit),
            Has.Property(nameof(DatumSize.Unit)).EqualTo(testCase.ExpectedUnit)
                .And.Property(nameof(DatumSize.Value)).EqualTo(testCase.ExpectedValue).Within(testCase.Tolerance),
            $"{value} {unit} does not match test case {testCase}");
        Assert.That(unit, Is.EqualTo(testCase.ExpectedUnit));
        Assert.That(value, Is.EqualTo(testCase.ExpectedValue).Within(testCase.Tolerance));
        if (!testCase.SkipBoundCheck)
        {
            Assert.That(value, Is.AtLeast(Math.Floor(testCase.ExpectedValue)));
            if (testCase.RequireExclusiveOfCeiling)
            {
                Assert.That(value, Is.LessThan(Math.Ceiling(testCase.ExpectedValue)));
            }
            else
            {
                Assert.That(value, Is.AtMost(Math.Ceiling(testCase.ExpectedValue)));
            }
        }
    }

    // ReSharper disable NotAccessedPositionalProperty.Local
    private record DatumSize(double Value, string Unit);
    // ReSharper restore NotAccessedPositionalProperty.Local
}
