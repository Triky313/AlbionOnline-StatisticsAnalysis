using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.Reflection;

namespace StatisticsAnalysisTool.UnitTests.Common;

[TestFixture]
public class TreasureControllerTests
{
    [Test]
    public void GetTreasureType_ReturnsCorrectValue()
    {
        // Arrange
        string[] inputs = { "TREASURE", "STATIC", "AVALON", "CORRUPTED", "HELL", "INVALID" };
        TreasureType[] expected = { TreasureType.OpenWorld, TreasureType.StaticDungeon, TreasureType.Avalon, TreasureType.Corrupted, TreasureType.HellGate, TreasureType.Unknown };

        // Act and assert
        var method = typeof(TreasureController).GetMethod("GetTreasureType", BindingFlags.NonPublic | BindingFlags.Static);

        if (method == null)
        {
            throw new Exception("Method not found.");
        }

        for (var i = 0; i < inputs.Length; i++)
        {
            var result = (TreasureType) method.Invoke(null, new object[] { inputs[i] })!;
            result.Should().Be(expected[i]);
        }
    }
}