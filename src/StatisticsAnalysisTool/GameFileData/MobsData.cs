using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.GameFileData;

public static class MobsData
{
    private const double LevelZeroUpperHpPercent = 93;
    private const double LevelOneUpperHpPercent = 109;
    private const double LevelTwoUpperHpPercent = 125;
    private const double LevelThreeUpperHpPercent = 146;
    private const double LevelFourUpperHpPercent = 220;
    private static IEnumerable<MobJsonObject> _mobs;

    public static int GetMobTierByIndex(int index)
    {
        return GetMobJsonObjectByIndex(index).Tier;
    }

    public static int GetRandomDungeonMobTierByIndex(int index)
    {
        var mob = GetMobJsonObjectByIndex(index);
        if (!IsReliableRandomDungeonTierMob(mob))
        {
            return (int) Tier.Unknown;
        }

        return mob.Tier - 1;
    }

    public static int GetMobLevelByIndex(int index, double currentInGameMobHp)
    {
        var mob = GetMobJsonObjectByIndex(index);

        return GetMobLevel(mob, currentInGameMobHp);
    }

    public static int GetRandomDungeonMobLevelByIndex(int index, double currentInGameMobHp)
    {
        var mob = GetMobJsonObjectByIndex(index);
        if (!IsReliableRandomDungeonTierMob(mob))
        {
            return -1;
        }

        return GetMobLevel(mob, currentInGameMobHp);
    }

    private static int GetMobLevel(MobJsonObject mob, double currentInGameMobHp)
    {
        if (mob?.HitPointsMax <= 0 || currentInGameMobHp <= 0)
        {
            return -1;
        }

        var mobHpInPercentOverMaxValue = 100 / mob.HitPointsMax * currentInGameMobHp;
        return mobHpInPercentOverMaxValue switch
        {
            < LevelZeroUpperHpPercent => 0,
            < LevelOneUpperHpPercent => 1,
            < LevelTwoUpperHpPercent => 2,
            < LevelThreeUpperHpPercent => 3,
            <= LevelFourUpperHpPercent => 4,
            _ => -1
        };
    }

    private static MobJsonObject GetMobJsonObjectByIndex(int index)
    {
        // From July 18, 2025, the in-game index will start counting from 15.
        // The ID's were decreased by 15
        index -= 15;

        if (index < 0)
        {
            uint unsignedIndex = Convert.ToUInt32(index);
            index = (int) unsignedIndex;
        }

        return _mobs.IsInBounds(index) ? _mobs?.ElementAt(index) : new MobJsonObject();
    }

    private static bool IsReliableRandomDungeonTierMob(MobJsonObject mob)
    {
        if (mob?.Tier is < 1 or > 8 || string.IsNullOrWhiteSpace(mob.UniqueName))
        {
            return false;
        }

        var uniqueName = mob.UniqueName.ToUpperInvariant();
        if (!uniqueName.Contains("_MOB_RD_"))
        {
            return false;
        }

        return !uniqueName.Contains("_BOSS")
            && !uniqueName.Contains("_MINIBOSS")
            && !uniqueName.Contains("_SUMMON")
            && !uniqueName.Contains("_UNATTACKABLE")
            && !uniqueName.Contains("_TRAP");
    }

    public static async Task<bool> LoadDataAsync()
    {
        var mobs = await GameData.LoadDataAsync<MobJsonObject, MobJsonRootObject>(
            Settings.Default.MobDataFileName,
            Settings.Default.ModifiedMobDataFileName,
            new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

        _mobs = mobs;
        return mobs.Count >= 0;
    }
}
