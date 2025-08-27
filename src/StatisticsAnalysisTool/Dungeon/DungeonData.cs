using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Dungeon;

public static class DungeonData
{
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