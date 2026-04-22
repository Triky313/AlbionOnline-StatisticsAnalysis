using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Enumerations;
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
            },
            new MobJsonObject
            {
                UniqueName = "T5_MOB_RD_UNDEAD_MAGE",
                Tier = 5,
                HitPointsMax = 923
            },
            new MobJsonObject
            {
                UniqueName = "T7_MOB_RD_MORGANA_BOSS",
                Tier = 7,
                HitPointsMax = 12000
            },
            new MobJsonObject
            {
                UniqueName = "T7_MOB_RD_MORGANA_SUMMON",
                Tier = 7,
                HitPointsMax = 1000
            },
            new MobJsonObject
            {
                UniqueName = "T7_MOB_ROAMING_MORGANA_SOLDIER",
                Tier = 7,
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
    public void GetRandomDungeonMobTierByIndex_WithReliableRandomDungeonMob_ReturnsDungeonTier()
    {
        var result = MobsData.GetRandomDungeonMobTierByIndex(15);

        result.Should().Be(5);
    }

    [Test]
    public void GetRandomDungeonMobTierByIndex_WithUserUndeadDungeonMob_ReturnsDungeonTierFour()
    {
        var result = MobsData.GetRandomDungeonMobTierByIndex(16);

        result.Should().Be(4);
    }

    [Test]
    public void GetRandomDungeonMobTierByIndex_WithBoss_ReturnsUnknown()
    {
        var result = MobsData.GetRandomDungeonMobTierByIndex(17);

        result.Should().Be((int) Tier.Unknown);
    }

    [Test]
    public void GetRandomDungeonMobTierByIndex_WithSummon_ReturnsUnknown()
    {
        var result = MobsData.GetRandomDungeonMobTierByIndex(18);

        result.Should().Be((int) Tier.Unknown);
    }

    [Test]
    public void GetRandomDungeonMobTierByIndex_WithNonRandomDungeonMob_ReturnsUnknown()
    {
        var result = MobsData.GetRandomDungeonMobTierByIndex(19);

        result.Should().Be((int) Tier.Unknown);
    }

    [Test]
    public void GetMobLevelByIndex_WithCurrentDungeonLevelFourHp_ReturnsLevelFour()
    {
        var result = MobsData.GetMobLevelByIndex(15, 2265);

        result.Should().Be(4);
    }

    [Test]
    public void GetRandomDungeonMobLevelByIndex_WithLevelTwoBonus_ReturnsLevelTwo()
    {
        var levelTwoHitPoints = 1328 * 136 / 116d;

        var result = MobsData.GetRandomDungeonMobLevelByIndex(15, levelTwoHitPoints);

        result.Should().Be(2);
    }

    [Test]
    public void GetRandomDungeonMobLevelByIndex_WithUserLevelFourUndeadMob_ReturnsLevelFour()
    {
        var result = MobsData.GetRandomDungeonMobLevelByIndex(16, 1494);

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
