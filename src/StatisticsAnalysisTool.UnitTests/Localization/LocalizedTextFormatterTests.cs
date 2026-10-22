using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Localization;
using System.Globalization;

namespace StatisticsAnalysisTool.UnitTests.Localization;

[TestFixture]
public class LocalizedTextFormatterTests
{
    [Test]
    public void Format_WithValidTranslation_PreservesCultureAndArgumentOrder()
    {
        var culture = CultureInfo.GetCultureInfo("tr-TR");
        var timestamp = new DateTime(2026, 10, 22, 14, 1, 10);

        var result = LocalizedTextFormatter.Format(
            "READY", "{1:g} · {0:N0} gün", "{0:N0} days · {1:g}", culture, 1234, timestamp);

        result.Should().Be($"{timestamp.ToString("g", culture)} · {1234.ToString("N0", culture)} gün");
    }

    [TestCase("{0:N0} gününüzden {7} gün · güncellendi {1:g}")]
    [TestCase("{0:N0} days · {1:g")]
    [TestCase("{days} days · {1:g}")]
    [TestCase("{0:Q} days · {1:g}")]
    [TestCase("{0:N0} days · {1:Q}")]
    [TestCase("")]
    public void Format_WithInvalidTranslation_UsesEnglishFallback(string translation)
    {
        var culture = CultureInfo.GetCultureInfo("tr-TR");
        var timestamp = new DateTime(2026, 10, 22, 14, 1, 10);

        var result = LocalizedTextFormatter.Format(
            "READY", translation, "{0:N0} of 7 days stored · updated {1:g}", culture, 3, timestamp);

        result.Should().Be($"3 of 7 days stored · updated {timestamp.ToString("g", culture)}");
    }

    [TestCase("{7}")]
    [TestCase("{0")]
    [TestCase("{0:Q}")]
    [TestCase("")]
    public void Format_WithInvalidTranslationAndFallback_ReturnsKey(string fallback)
    {
        var result = LocalizedTextFormatter.Format("READY", "{7}", fallback, CultureInfo.InvariantCulture, 3);

        result.Should().Be("READY");
    }

    [Test]
    public void Format_WithEscapedBraces_FormatsLiteralBraces()
    {
        var result = LocalizedTextFormatter.Format(
            "COUNT", "{{{0}}}", "Count: {0}", CultureInfo.InvariantCulture, 3);

        result.Should().Be("{3}");
    }
}
