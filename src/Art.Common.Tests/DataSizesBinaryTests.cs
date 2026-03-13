using NUnit.Framework;

namespace Art.Common.Tests;

public class DataSizesBinaryTests
{
    public record LongTestCase(long Value, double ExpectedValue, string ExpectedUnit, double Tolerance = 0.001, bool RequireExclusiveOfCeiling = false);

    public record ULongTestCase(ulong Value, double ExpectedValue, string ExpectedUnit, double Tolerance = 0.001, bool RequireExclusiveOfCeiling = false);

    public record DoubleTestCase(double Value, double ExpectedValue, string ExpectedUnit, double Tolerance = 0.001, bool RequireExclusiveOfCeiling = false);

    public static readonly LongTestCase[] SignedLongTestCases =
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
        ..SignedLongTestCases,
        ..SignedLongTestCases.Select(static v => v with { Value = -v.Value, ExpectedValue = -v.ExpectedValue }),
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

    public static readonly DoubleTestCase[] SignedDoubleTestCases =
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
        ..SignedDoubleTestCases,
        ..SignedDoubleTestCases.Select(static v => v with { Value = -v.Value, ExpectedValue = -v.ExpectedValue }),
    ];

    [Test]
    public void GetBinarySize_Long_Correct([ValueSource(nameof(LongTestCases))] LongTestCase testCase)
    {
        DataSizes.GetBinarySize(testCase.Value, out double value, out string unit);
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
    public void GetBinarySize_ULong_Correct([ValueSource(nameof(ULongTestCases))] ULongTestCase testCase)
    {
        DataSizes.GetBinarySize(testCase.Value, out double value, out string unit);
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
    public void GetBinarySize_Double_Correct([ValueSource(nameof(DoubleTestCases))] DoubleTestCase testCase)
    {
        DataSizes.GetBinarySize(testCase.Value, out double value, out string unit);
        Assert.That(unit, Is.EqualTo(testCase.ExpectedUnit));
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

    // ReSharper disable NotAccessedPositionalProperty.Local
    private record DatumSize(double Value, string Unit);
    // ReSharper restore NotAccessedPositionalProperty.Local
}
