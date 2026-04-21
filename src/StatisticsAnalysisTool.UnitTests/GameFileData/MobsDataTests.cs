using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.GameFileData.Models;
using System.Reflection;

namespace StatisticsAnalysisTool.UnitTests.GameFileData;

[TestFixture]
public class MobsDataTests
{
    [SetUp]
    public void SetUp()
    {
        SetMobs([
            new MobJsonObject
            {
                UniqueName = "T6_MOB_RD_MORGANA_SOLDIER",
                Tier = 6,
                HitPointsMax = 1328
            }
        ]);
    }

    [TearDown]
    public void TearDown()
    {
        SetMobs([]);
    }

    [Test]
    public void GetMobTierByIndex_WithShiftedInGameIndex_ReturnsTier()
    {
        var result = MobsData.GetMobTierByIndex(15);

        result.Should().Be(6);
    }

    [Test]
    public void GetMobLevelByIndex_WithCurrentDungeonLevelFourHp_ReturnsLevelFour()
    {
        var result = MobsData.GetMobLevelByIndex(15, 2265);

        result.Should().Be(4);
    }

    [TestCase(84, 0)]
    [TestCase(105, 1)]
    [TestCase(121, 2)]
    [TestCase(141, 3)]
    [TestCase(171, 4)]
    public void GetMobLevelByIndex_WithKnownHpPercent_ReturnsExpectedLevel(int hitPointsPercent, int expectedLevel)
    {
        var result = MobsData.GetMobLevelByIndex(15, 1328 * hitPointsPercent / 100d);

        result.Should().Be(expectedLevel);
    }

    private static void SetMobs(IEnumerable<MobJsonObject> mobs)
    {
        var fieldInfo = typeof(MobsData).GetField("_mobs", BindingFlags.NonPublic | BindingFlags.Static);
        fieldInfo!.SetValue(null, mobs);
    }
}
