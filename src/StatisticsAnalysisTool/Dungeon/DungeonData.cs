using StatisticsAnalysisTool.Enumerations;
using System;

namespace StatisticsAnalysisTool.Dungeon;

public static class DungeonData
{
    private static readonly double[] RandomDungeonLootFactors = [1, 1.4268, 1.8768, 2.4332, 3.1832];

    public static DungeonMode GetDungeonMode(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return DungeonMode.Unknown;
        }

        if (value.Contains("HD_SHRINE_WRATH_BUFF"))
        {
            return DungeonMode.AbyssalDepths;
        }

        if (value.Contains("CORRUPTED"))
        {
            return DungeonMode.Corrupted;
        }

        if (value.Contains("HELL_") || value.Contains("HELLGATE"))
        {
            return DungeonMode.HellGate;
        }

        if (value.Contains("_SOLO_")
            || value.Contains("GENERAL_SHRINE_COMBAT_BUFF"))
        {
            return DungeonMode.Solo;
        }

        if (value.Contains("_VETERAN_")
            || value.Contains("_HALLOWEEN"))
        {
            return DungeonMode.Standard;
        }

        if (value.Contains("AVALON"))
        {
            return DungeonMode.Avalon;
        }

        return DungeonMode.Unknown;
    }

    public static DungeonMode GetDungeonMode(params string[] values)
    {
        foreach (var value in values ?? [])
        {
            var dungeonMode = GetDungeonMode(value);
            if (dungeonMode != DungeonMode.Unknown)
            {
                return dungeonMode;
            }
        }

        return DungeonMode.Unknown;
    }

    public static DungeonMode GetRandomDungeonModeFromExit(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DungeonMode.Unknown;
        }

        if (value.Contains("AVALON"))
        {
            return DungeonMode.Avalon;
        }

        if (value.Contains("_SOLO"))
        {
            return DungeonMode.Solo;
        }

        return GetDungeonTierFromExit(value) != Tier.Unknown
            ? DungeonMode.Standard
            : DungeonMode.Unknown;
    }

    public static Tier GetDungeonTierFromExit(string value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Length < 2
            || value[0] != 'T'
            || !int.TryParse(value.AsSpan(1, 1), out var tier)
            || tier is < 1 or > 8)
        {
            return Tier.Unknown;
        }

        return (Tier) tier;
    }

    public static double GetDungeonMobHitPointsFactor(string dungeonType)
    {
        var quality = GetBlackZoneQuality(dungeonType);
        return quality > 1 ? 1 + (quality - 1) * 0.05 : 1;
    }

    public static double GetDungeonZoneLootFactor(string dungeonType)
    {
        var quality = GetBlackZoneQuality(dungeonType);
        if (quality > 0)
        {
            return 2.4 + quality * 0.2;
        }

        if (dungeonType?.Contains("BLACK") == true)
        {
            return 2.6;
        }

        if (dungeonType?.Contains("ORANGE") == true)
        {
            return 1.8;
        }

        if (dungeonType?.Contains("RED") == true)        {
            return 2.25;
        }

        if (dungeonType?.Contains("YELLOW") == true)
        {
            return 1.33;
        }

        if (dungeonType?.Contains("SAFE") == true || dungeonType?.Contains("BLUE") == true)
        {
            return 1.25;
        }

        return 0;
    }

    public static int GetDungeonLevelFromLootFactor(double combinedLootFactor, double zoneLootFactor)
    {
        if (combinedLootFactor <= 0 || zoneLootFactor <= 0)
        {
            return -1;
        }

        var dungeonLootFactor = combinedLootFactor / zoneLootFactor;
        for (var level = 0; level < RandomDungeonLootFactors.Length; level++)
        {
            if (System.Math.Abs(dungeonLootFactor - RandomDungeonLootFactors[level]) < 0.001)
            {
                return level;
            }
        }

        return -1;
    }

    private static int GetBlackZoneQuality(string dungeonType)
    {
        if (string.IsNullOrWhiteSpace(dungeonType))
        {
            return 0;
        }

        const string blackZoneIdentifier = "BLACK_";
        var qualityIndex = dungeonType.LastIndexOf(blackZoneIdentifier, System.StringComparison.Ordinal);
        if (qualityIndex < 0)
        {
            return 0;
        }

        var valueIndex = qualityIndex + blackZoneIdentifier.Length;
        return valueIndex < dungeonType.Length
               && int.TryParse(dungeonType.AsSpan(valueIndex, 1), out var quality)
               && quality is >= 1 and <= 6
            ? quality
            : 0;
    }

    public static Faction GetFaction(string value)
    {
        if (value.Contains("HIGHLAND_DEAD_DNG_HELL_BUFFSHRINE"))
        {
            return Faction.AbyssalDepths;
        }

        if (value.Contains("HELLGATE"))
        {
            return Faction.HellGate;
        }

        if (value.Contains("CORRUPTED"))
        {
            return Faction.Corrupted;
        }

        if (value.Contains("KEEPER"))
        {
            return Faction.Keeper;
        }

        if (value.Contains("HERETIC"))
        {
            return Faction.Heretic;
        }

        if (value.Contains("MORGANA"))
        {
            return Faction.Morgana;
        }

        if (value.Contains("UNDEAD"))
        {
            return Faction.Undead;
        }

        if (value.Contains("AVALON"))
        {
            return Faction.Avalon;
        }

        return Faction.Unknown;
    }

    public static EventType GetDungeonEventType(string value)
    {
        if (value.Contains("SHRINE_COMBAT"))
        {
            return EventType.CombatShrine;
        }

        if (value.Contains("SHRINE_SILVER"))
        {
            return EventType.SilverShrine;
        }

        if (value.Contains("SHRINE_FAME"))
        {
            return EventType.FameShrine;
        }

        if (value.Contains("BOOKCHEST"))
        {
            return EventType.BookChest;
        }

        if (value.Contains("CHEST") || value.Contains("AVALON") || value.Contains("HELL_STD_PVP")
            || value.Contains("HELL_HRD_PVP") || value.Contains("HELL_STD_PVE") || value.Contains("HELL_HRD_PVE")
            || value.Contains("HD_DEMON_") || value.Contains("HD_DEMON_CHEST_") || value.Contains("TREASURE_"))
        {
            return EventType.Chest;
        }

        return EventType.Unknown;
    }

    public static bool IsRandomDungeonLootChest(string value, DungeonMode mode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return mode switch
        {
            DungeonMode.Solo => value.Contains("_SOLO_CHEST_"),
            DungeonMode.Standard => value.Contains("_VETERAN_CHEST_") || value.Contains("HALLOWEEN"),
            DungeonMode.Avalon => value.Contains("AVALON_ELITE"),
            _ => false
        };
    }

    #region Chest

    public static TreasureRarity GetChestRarity(string value)
    {
        if (value.Contains("_STANDARD")
            || value.Contains("AVALON") && value.Contains("STANDARD"))
        {
            return TreasureRarity.Common;
        }

        if (value.Contains("_UNCOMMON")
            || value.Contains("AVALON") && value.Contains("UNCOMMON"))
        {
            return TreasureRarity.Uncommon;
        }

        if (value.Contains("_RARE")
            || value.Contains("AVALON") && value.Contains("RARE"))
        {
            return TreasureRarity.Rare;
        }

        if (value.Contains("LEGENDARY")
            || value.Contains("AVALON") && value.Contains("LEGENDARY"))
        {
            return TreasureRarity.Legendary;
        }

        return TreasureRarity.Unknown;
    }

    public static bool IsBossChest(string value)
    {
        return !value.Contains("BOSS_BUFF") && value.Contains("BOSS") || value.Contains("BOSSLAIR");
    }

    #endregion

    #region Shrine

    public static ShrineBuff GetShrineBuff(string value)
    {
        if (value.Contains("SILVER"))
        {
            return ShrineBuff.Silver;
        }

        if (value.Contains("FAME"))
        {
            return ShrineBuff.Fame;
        }

        if (value.Contains("COMBAT"))
        {
            return ShrineBuff.Combat;
        }

        return ShrineBuff.Unknown;
    }

    public static ShrineType GetShrineType(string value)
    {
        if (!value.Contains("AVALON") && value.Contains("STANDARD"))
        {
            return ShrineType.Standard;
        }

        if (value.Contains("COMBAT"))
        {
            return ShrineType.Combat;
        }

        return ShrineType.Unknown;
    }

    #endregion
}