using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.GameFileData.Models;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        var result = MobsData.GetMobTierByIndex(16);

        result.Should().Be(6);
    }

    [Test]
    public void GetRandomDungeonMobTierByIndex_WithReliableRandomDungeonMob_ReturnsDungeonTier()
    {
        var result = MobsData.GetRandomDungeonMobTierByIndex(16);

        result.Should().Be(6);
    }

    [Test]
    public void GetRandomDungeonMobTierByIndex_WithUserUndeadDungeonMob_ReturnsDungeonTierFive()
    {
        var result = MobsData.GetRandomDungeonMobTierByIndex(17);

        result.Should().Be(5);
    }

    [Test]
    public void GetRandomDungeonMobTierByIndex_WithBoss_ReturnsUnknown()
    {
        var result = MobsData.GetRandomDungeonMobTierByIndex(18);

        result.Should().Be((int) Tier.Unknown);
    }

    [Test]
    public void GetRandomDungeonMobTierByIndex_WithSummon_ReturnsUnknown()
    {
        var result = MobsData.GetRandomDungeonMobTierByIndex(19);

        result.Should().Be((int) Tier.Unknown);
    }

    [Test]
    public void GetRandomDungeonMobTierByIndex_WithNonRandomDungeonMob_ReturnsUnknown()
    {
        var result = MobsData.GetRandomDungeonMobTierByIndex(20);

        result.Should().Be((int) Tier.Unknown);
    }

    [Test]
    public void GetMobLevelByIndex_WithCurrentDungeonLevelFourHp_ReturnsLevelFour()
    {
        var result = MobsData.GetMobLevelByIndex(16, 2265);

        result.Should().Be(4);
    }

    [Test]
    public void GetRandomDungeonMobLevelByIndex_WithLevelTwoBonus_ReturnsLevelTwo()
    {
        var levelTwoHitPoints = 1328 * 1.36;

        var result = MobsData.GetRandomDungeonMobLevelByIndex(16, levelTwoHitPoints);

        result.Should().Be(2);
    }

    [Test]
    public void GetRandomDungeonMobLevelByIndex_WithUserLevelFourUndeadMob_ReturnsLevelFour()
    {
        var result = MobsData.GetRandomDungeonMobLevelByIndex(17, 923 * 1.84);

        result.Should().Be(4);
    }

    [TestCase(84, 0)]
    [TestCase(105, 1)]
    [TestCase(121, 2)]
    [TestCase(141, 3)]
    [TestCase(171, 4)]
    public void GetMobLevelByIndex_WithKnownHpPercent_ReturnsExpectedLevel(int hitPointsPercent, int expectedLevel)
    {
        var result = MobsData.GetMobLevelByIndex(16, 1328 * hitPointsPercent / 100d);

        result.Should().Be(expectedLevel);
    }

    [Test]
    public void MobJsonObject_WithExtendedAttributes_PreservesAllAvailableAttributes()
    {
        const string json = """
                            {
                              "@uniquename": "T6_MOB_DYNAMIC_HIDE_FOREST_GIANTSTAG",
                              "@tier": "6",
                              "@npchostility": "hostile",
                              "@abilitypower": "133",
                              "@fame": "138",
                              "@roamingradius": "10",
                              "@roamingidletimemin": "3",
                              "@roamingidletimemax": "10",
                              "@aggroradius": "0",
                              "@pursuitradius": "30",
                              "@damageaggrofactor": "1",
                              "@healingaggrofactor": "1",
                              "@shieldaggrofactor": "0.5",
                              "@alertradius": "5",
                              "@faction": "GIANTSTAG",
                              "@attackcollisionradius": "0.9",
                              "@attacktype": "melee",
                              "@attackrange": "2",
                              "@attackdamage": "287",
                              "@hitpointsmax": "1401",
                              "@hitpointsregeneration": "0",
                              "@energymax": "338",
                              "@energyregeneration": "10",
                              "@movespeed": "0.78",
                              "@attackmovespeed": "7.5",
                              "@meleeattackdamagetime": "0.52",
                              "@attackspeed": "0.3",
                              "@physicalarmor": "146",
                              "@magicresistance": "146",
                              "@crowdcontrolresistance": "64",
                              "@respawntimesecondsmin": "450",
                              "@respawntimesecondsmax": "1350",
                              "@namelocatag": "@MOB_T6_MOB_HIDE_FOREST_GIANTSTAG",
                              "@avatar": "GIANTSTAGMOOSE1",
                              "@maxcharges": "5",
                              "@timeperchargeseconds": "900",
                              "@dangerstate": "normal",
                              "@aggrodelayafterspawn": "5",
                              "@category": "hidemob",
                              "@chargesperchargeup": "1",
                              "@energyrewardspell": "ENERGYREWARD",
                              "@energyreward": "10",
                              "@mobvalue": "0",
                              "@ignoredifficultybonus": "true",
                              "@chargeupchance": "0.2554"
                            }
                            """;
        var options = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        var mob = JsonSerializer.Deserialize<MobJsonObject>(json, options);
        var serializedJson = JsonSerializer.Serialize(mob);

        mob.Should().NotBeNull();
        mob!.Fame.Should().Be(138);
        mob.AlertRadius.Should().Be(5);

        using var sourceDocument = JsonDocument.Parse(json);
        using var serializedDocument = JsonDocument.Parse(serializedJson);
        foreach (var sourceProperty in sourceDocument.RootElement.EnumerateObject())
        {
            serializedDocument.RootElement.TryGetProperty(sourceProperty.Name, out _)
                .Should().BeTrue($"the available attribute {sourceProperty.Name} must be preserved");
        }
    }

    [Test]
    public void MobJsonObject_WithoutOptionalAttributes_DoesNotAddEmptyAttributes()
    {
        var mob = new MobJsonObject
        {
            UniqueName = "T6_MOB_TEST",
            Tier = 6,
            HitPointsMax = 1401
        };

        var serializedJson = JsonSerializer.Serialize(mob);

        using var document = JsonDocument.Parse(serializedJson);
        document.RootElement.TryGetProperty("@fame", out _).Should().BeFalse();
        document.RootElement.TryGetProperty("@attackdamage", out _).Should().BeFalse();
        document.RootElement.TryGetProperty("@chargeupchance", out _).Should().BeFalse();
    }

    private static void SetMobs(IEnumerable<MobJsonObject> mobs)
    {
        var fieldInfo = typeof(MobsData).GetField("_mobs", BindingFlags.NonPublic | BindingFlags.Static);
        fieldInfo!.SetValue(null, mobs);
    }
}
