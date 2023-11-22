using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace StatisticsAnalysisTool.UnitTests.Network.Manager;

public class TreasureControllerTests
{
    [Fact]
    public void GetRarity_ReturnsExpectedValues()
    {
        // Arrange
        var method = typeof(TreasureController).GetMethod("GetRarity", BindingFlags.NonPublic | BindingFlags.Static);
        var valuesToTest = new List<Tuple<string, TreasureRarity>>
        {
            Tuple.Create((string)null, TreasureRarity.Unknown),
            Tuple.Create("", TreasureRarity.Unknown),
            Tuple.Create("SOME_STANDARD", TreasureRarity.Common),
            Tuple.Create("SOME_STANDARD_T5", TreasureRarity.Common),
            Tuple.Create("SOME_STANDARD_STANDARD_T7", TreasureRarity.Common),
            Tuple.Create("STATIC_SOME_POI", TreasureRarity.Common),
            Tuple.Create("SOME_UNCOMMON", TreasureRarity.Uncommon),
            Tuple.Create("SOME_UNCOMMON_T6", TreasureRarity.Uncommon),
            Tuple.Create("SOME_STANDARD_UNCOMMON_T8", TreasureRarity.Uncommon),
            Tuple.Create("STATIC_SOME_CHAMPION", TreasureRarity.Uncommon),
            Tuple.Create("SOME_RARE", TreasureRarity.Rare),
            Tuple.Create("SOME_RARE_T4", TreasureRarity.Rare),
            Tuple.Create("SOME_STANDARD_RARE_T7", TreasureRarity.Rare),
            Tuple.Create("STATIC_SOME_MINIBOSS", TreasureRarity.Rare),
            Tuple.Create("SOME_LEGENDARY", TreasureRarity.Legendary),
            Tuple.Create("SOME_LEGENDARY_T8", TreasureRarity.Legendary),
            Tuple.Create("SOME_STANDARD_LEGENDARY_T4", TreasureRarity.Legendary),
            Tuple.Create("STATIC_SOME_BOSS", TreasureRarity.Legendary),
            Tuple.Create("SOME_OTHER_VALUE", TreasureRarity.Unknown)
        };

        // Act and Assert
        foreach (var valueToTest in valuesToTest)
        {
            var result = (TreasureRarity)method.Invoke(null, new object[] { valueToTest.Item1 })!;
            Assert.Equal(valueToTest.Item2, result);
        }
    }

    [Fact]
    public void GetTreasureType_ReturnsExpectedValues()
    {
        // Arrange
        var method = typeof(TreasureController).GetMethod("GetTreasureType", BindingFlags.NonPublic | BindingFlags.Static);
        var valuesToTest = new List<Tuple<string, TreasureType>>
        {
            Tuple.Create("TREASURE", TreasureType.OpenWorld),
            Tuple.Create("STATIC", TreasureType.StaticDungeon),
            Tuple.Create("AVALON", TreasureType.Avalon),
            Tuple.Create("CORRUPTED", TreasureType.Corrupted),
            Tuple.Create("HELL", TreasureType.HellGate),
            Tuple.Create("SOME_VETERAN_CHEST_SOMETHING", TreasureType.RandomGroupDungeon),
            Tuple.Create("SOME_CHEST_BOSS_HALLOWEEN_SOMETHING", TreasureType.RandomGroupDungeon),
            Tuple.Create("SOME_SOLO_BOOKCHEST_SOMETHING", TreasureType.RandomSoloDungeon),
            Tuple.Create("SOME_SOLO_CHEST_SOMETHING", TreasureType.RandomSoloDungeon),
            Tuple.Create("SOME_OTHER_VALUE", TreasureType.Unknown)
        };

        // Act and Assert
        foreach (var valueToTest in valuesToTest)
        {
            var result = (TreasureType)method!.Invoke(null, new object[] { valueToTest.Item1 })!;
            Assert.Equal(valueToTest.Item2, result);
        }
    }
}