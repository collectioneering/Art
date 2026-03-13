using NUnit.Framework;

namespace Art.Common.Tests;

public class DataSizesDecimalTests
{
    public record LongTestCase(long Value, double ExpectedValue, string ExpectedUnit, double Tolerance = 0.001, bool RequireExclusiveOfCeiling = false);

    public record ULongTestCase(ulong Value, double ExpectedValue, string ExpectedUnit, double Tolerance = 0.001, bool RequireExclusiveOfCeiling = false);

    public record DoubleTestCase(double Value, double ExpectedValue, string ExpectedUnit, double Tolerance = 0.001, bool RequireExclusiveOfCeiling = false, bool SkipBoundCheck = false);

    public static readonly LongTestCase[] SignedLongTestCases =
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
        ..SignedLongTestCases,
        ..SignedLongTestCases.Select(static v => v with { Value = -v.Value, ExpectedValue = -v.ExpectedValue }),
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

    public static readonly DoubleTestCase[] SignedDoubleTestCases =
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
        ..SignedDoubleTestCases,
        ..SignedDoubleTestCases.Select(static v => v with { Value = -v.Value, ExpectedValue = -v.ExpectedValue }),
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
