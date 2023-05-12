using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Xunit;

namespace StatisticsAnalysisTool.UnitTests.Common;

public class ExtensionMethodTests
{
    [Fact]
    public void OrderByReference_ReordersObservableCollectionBasedOnList()
    {
        // Arrange
        var collection = new ObservableCollection<int> { 1, 2, 3 };
        var comparison = new List<int> { 3, 1, 2 };

        // Act
        collection.OrderByReference(comparison);

        // Assert
        Assert.Equal(new List<int> { 3, 1, 2 }, collection);
    }

    [Fact]
    public void OrderByReference_DoesNotReorderObservableCollectionIfAlreadyOrdered()
    {
        // Arrange
        var collection = new ObservableCollection<int> { 1, 2, 3 };
        var comparison = new List<int> { 1, 2, 3 };

        // Act
        collection.OrderByReference(comparison);

        // Assert
        Assert.Equal(new List<int> { 1, 2, 3 }, collection);
    }

    [Fact]
    public void ToDictionary_ReturnsDictionaryOfIndexAndValues()
    {
        // Arrange
        var array = new List<int> { 1, 2, 3 };

        // Act
        var result = array.ToDictionary();

        // Assert
        Assert.Equal(new Dictionary<int, int> { { 0, 1 }, { 1, 2 }, { 2, 3 } }, result);
    }

    [Fact]
    public void ToDictionary_HandlesEmptyArray()
    {
        // Arrange
        var array = new List<int>();
        if (array == null) throw new ArgumentNullException(nameof(array));

        // Act
        var result = array.ToDictionary();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ToTimerString_ReturnsFormattedStringForTimeSpanInput()
    {
        // Arrange
        var span = new TimeSpan(1, 2, 3);

        // Act
        var result = span.ToTimerString();

        // Assert
        Assert.Equal("01:02:03", result);
    }

    [Fact]
    public void ToTimerString_ReturnsFormattedStringForIntInput()
    {
        // Arrange
        var seconds = 3723;

        // Act
        var result = seconds.ToTimerString();

        // Assert
        Assert.Equal("01:02:03", result);
    }

    [Fact]
    public void ToTimerString_HandlesZeroInput()
    {
        // Arrange
        var span = new TimeSpan(0, 0, 0);
        var seconds = 0;

        // Act
        var result1 = span.ToTimerString();
        var result2 = seconds.ToTimerString();

        // Assert
        Assert.Equal("00:00:00", result1);
        Assert.Equal("00:00:00", result2);
    }

    [Fact]
    public void ToTimerString_HandlesNegativeInput()
    {
        // Arrange
        var span = new TimeSpan(-1, -2, -3);
        var seconds = -3723;

        // Act
        var result1 = span.ToTimerString();
        var result2 = seconds.ToTimerString();

        // Assert
        Assert.Equal("-01:-02:-03", result1);
        Assert.Equal("-01:-02:-03", result2);
    }

    [Fact]
    public void ObjectToLong_WithValidValues_ReturnLongValue()
    {
        var value = (object) 15;

        var result = value.ObjectToLong();
        const long expected = 15L;

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ObjectToLong_WithInvalidValues_ReturnNull()
    {
        var result = 9999999999999999999.ObjectToLong();
        long? expected = null;

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ObjectToInt_WithInvalidValues_ReturnIntValue()
    {
        var value = (object) 15;

        var result = value.ObjectToInt();
        const long expected = 15L;

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ObjectToInt_WithInvalidValues_ReturnNull()
    {
        var result = 9999999999999999999.ObjectToInt();
        long? expected = 0;

        Assert.Equal(expected, result);
    }

    [Fact]
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
            Assert.Equal(expectedOutput, result);
        }
    }

    [Fact]
    public void IsDateInWeekOfYear_WithValidValues_ReturnTrue()
    {
        // Arrange
        var dateTime1 = new DateTime(2023, 4, 4, 12, 12, 12);
        var dateTime2 = new DateTime(2023, 4, 5, 12, 12, 12);

        // Act
        var result = dateTime1.IsDateInWeekOfYear(dateTime2);

        // Assert
        Assert.True(result);
    }
}