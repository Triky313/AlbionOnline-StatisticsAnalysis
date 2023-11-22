using StatisticsAnalysisTool.Common;
using System;
using Xunit;

namespace StatisticsAnalysisTool.UnitTests.Common;

public class UtilitiesTests
{
    private static readonly DateTime SomeDateTime = new(2021, 9, 7, 14, 22, 50);

    [Fact]
    public void GetHighestLength_WithValidValues_ReturnHighestValue()
    {
        var array1 = new[] { 25, 12, 480 };
        var array2 = new[] { 210, 155, 85, 12 };
        var array3 = new[] { 770, 910, 1600 };
        var array4 = new[] { 770, 910 };

        var result = Utilities.GetHighestLength(array1, array2, array3, array4);
        const long expected = 4;

        Assert.StrictEqual(expected, result);
    }

    [Fact]
    public void GetValuePerSecondToDouble_WithValidValues_ReturnValidValue()
    {
        var result = Utilities.GetValuePerSecondToDouble(500, SomeDateTime, new TimeSpan(0, 0, 2, 9));
        var expected = 3.8759689922480618;

        Assert.Equal(expected, result, 0);
    }

    [Fact]
    public void IsBlockingTimeExpired_WithValidValues_ReturnTrue()
    {
        var currentDateTime = DateTime.UtcNow.AddSeconds(-20);
        var result = Utilities.IsBlockingTimeExpired(currentDateTime, 17);

        Assert.True(result);
    }
}