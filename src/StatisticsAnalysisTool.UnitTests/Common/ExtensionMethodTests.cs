using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Common;
using System.Collections.ObjectModel;
using System.Globalization;

namespace StatisticsAnalysisTool.UnitTests.Common;

[TestFixture]
public class ExtensionMethodTests
{
    [Test]
    public void OrderByReference_ReordersObservableCollectionBasedOnList()
    {
        // Arrange
        var collection = new ObservableCollection<int> { 1, 2, 3 };
        var comparison = new List<int> { 3, 1, 2 };

        // Act
        collection.OrderByReference(comparison);

        // Assert
        collection.Should().Equal(new List<int> { 3, 1, 2 });
    }

    [Test]
    public void OrderByReference_DoesNotReorderObservableCollectionIfAlreadyOrdered()
    {
        // Arrange
        var collection = new ObservableCollection<int> { 1, 2, 3 };
        var comparison = new List<int> { 1, 2, 3 };

        // Act
        collection.OrderByReference(comparison);

        // Assert
        collection.Should().Equal(new List<int> { 1, 2, 3 });
    }

    [Test]
    public void ToDictionary_ReturnsDictionaryOfIndexAndValues()
    {
        // Arrange
        var array = new List<int> { 1, 2, 3 };

        // Act
        var result = array.ToDictionary();

        // Assert
        result.Should().Equal(new Dictionary<int, int> { { 0, 1 }, { 1, 2 }, { 2, 3 } });
    }

    [Test]
    public void ToDictionary_HandlesEmptyArray()
    {
        // Arrange
        var array = new List<int>();
        if (array == null) throw new ArgumentNullException(nameof(array));

        // Act
        var result = array.ToDictionary();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ToTimerString_ReturnsFormattedStringForTimeSpanInput()
    {
        // Arrange
        var span = new TimeSpan(1, 2, 3);

        // Act
        var result = span.ToTimerString();

        // Assert
        result.Should().Be("01:02:03");
    }

    [Test]
    public void ToTimerString_ReturnsFormattedStringForIntInput()
    {
        // Arrange
        var seconds = 3723;

        // Act
        var result = seconds.ToTimerString();

        // Assert
        result.Should().Be("01:02:03");
    }

    [Test]
    public void ToTimerString_HandlesZeroInput()
    {
        // Arrange
        var span = new TimeSpan(0, 0, 0);
        var seconds = 0;

        // Act
        var result1 = span.ToTimerString();
        var result2 = seconds.ToTimerString();

        // Assert
        result1.Should().Be("00:00:00");
        result2.Should().Be("00:00:00");
    }

    [Test]
    public void ToTimerString_HandlesNegativeInput()
    {
        // Arrange
        var span = new TimeSpan(-1, -2, -3);
        var seconds = -3723;

        // Act
        var result1 = span.ToTimerString();
        var result2 = seconds.ToTimerString();

        // Assert
        result1.Should().Be("-01:-02:-03");
        result2.Should().Be("-01:-02:-03");
    }
    [Test]
    public void ObjectToLong_WithValidValues_ReturnLongValue()
    {
        var value = (object) 15;

        var result = value.ObjectToLong();
        const long expected = 15L;

        result.Should().Be(expected);
    }

    [Test]
    public void ObjectToLong_WithInvalidValues_ReturnNull()
    {
        var result = 9999999999999999999.ObjectToLong();

        result.Should().Be(null);
    }

    [Test]
    public void ObjectToInt_WithInvalidValues_ReturnIntValue()
    {
        var value = (object) 15;

        var result = value.ObjectToInt();
        const int expected = 15;

        result.Should().Be(expected);
    }

    [Test]
    public void ObjectToInt_WithInvalidValues_ReturnNull()
    {
        var result = 9999999999999999999.ObjectToInt();
        int? expected = 0;

        result.Should().Be(expected);
    }

    [Test]
    public void GetShortNumber_ReturnsCorrectStringForVariousInputs()
    {
        // Arrange
        var inputOutputPairs = new Dictionary<decimal, string>
        {
            { 123456.78m, "123.46K" },
            { 12345.678m, "12.35K" },
            { 123.4567m, "123" },
            { 12.345678m, "12" },
            { 1.2345678m, "1" },
            { 12345678.9m, "12.35M" },
            { 1234567.89m, "1.23M" },
            { 123456.789m, "123.46K" },
            { 12345.6789m, "12.35K" },
            { 123.45679m, "123" }
        };

        // Act & Assert
        foreach (var pair in inputOutputPairs)
        {
            var input = pair.Key;
            var expectedOutput = pair.Value;
            var result = input.GetShortNumber(new CultureInfo("en-US"));
            result.Should().Be(expectedOutput);
        }
    }

    [Test]
    public void IsDateInWeekOfYear_WithValidValues_ReturnTrue()
    {
        // Arrange
        var dateTime1 = new DateTime(2023, 4, 4, 12, 12, 12);
        var dateTime2 = new DateTime(2023, 4, 5, 12, 12, 12);

        // Act
        var result = dateTime1.IsDateInWeekOfYear(dateTime2);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void GetValuePerHour_ValidSeconds_ReturnsCorrectValue()
    {
        // Arrange
        double value = 3600;
        double seconds = 3600;

        // Act
        double result = value.GetValuePerHour(seconds);

        // Assert
        result.Should().Be(3600);
    }

    [Test]
    public void GetValuePerHour_ZeroSeconds_ReturnsMaxValue()
    {
        // Arrange
        double value = 100;
        double seconds = 0;

        // Act
        double result = value.GetValuePerHour(seconds);

        // Assert
        result.Should().Be(double.MaxValue);
    }

    [Test]
    public void GetValuePerHour_NegativeSeconds_ReturnsMaxValue()
    {
        // Arrange
        double value = 100;
        double seconds = -3600;

        // Act
        double result = value.GetValuePerHour(seconds);

        // Assert
        result.Should().Be(double.MaxValue);
    }

    [Test]
    public void GetValuePerHour_FractionalSeconds_ReturnsCorrectValue()
    {
        // Arrange
        double value = 1800;
        double seconds = 1800;

        // Act
        double result = value.GetValuePerHour(seconds);

        // Assert
        result.Should().Be(3600);
    }

    [Test]
    public void GetValuePerHour_SmallSecondsValue_ReturnsLargeResult()
    {
        // Arrange
        double value = 1;
        double seconds = 1;

        // Act
        double result = value.GetValuePerHour(seconds);

        // Assert
        result.Should().Be(3600);
    }
}