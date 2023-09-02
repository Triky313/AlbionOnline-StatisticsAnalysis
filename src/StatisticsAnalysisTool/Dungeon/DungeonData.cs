using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System.Reflection;

namespace StatisticsAnalysisTool.Dungeon;

public static class DungeonData
{
    public static DungeonMode GetDungeonMode(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return DungeonMode.Unknown;
        }

        if (value.Contains("CORRUPTED"))
        {
            return DungeonMode.Corrupted;
        }

        if (value.Contains("HELL_") || value.Contains("HELLGATE"))
        {
            return DungeonMode.HellGate;
        }

        if (value.Contains("MORGANA_SOLO_CHEST")
            || value.Contains("KEEPER_SOLO_CHEST")
            || value.Contains("HERETIC_SOLO_CHEST")
            || value.Contains("UNDEAD_SOLO_CHEST")
            || value.Contains("UNDEAD_SOLO_CHEST_BOSS_HALLOWEEN")
            || value.Contains("MORGANA_SOLO_BOOKCHEST")
            || value.Contains("KEEPER_SOLO_BOOKCHEST")
            || value.Contains("HERETIC_SOLO_BOOKCHEST")
            || value.Contains("UNDEAD_SOLO_BOOKCHEST")
            || value.Contains("GENERAL_SHRINE_COMBAT_BUFF"))
        {
            return DungeonMode.Solo;
        }

        if (value.Contains("MORGANA_VETERAN_CHEST")
            || value.Contains("KEEPER_VETERAN_CHEST")
            || value.Contains("HERETIC_VETERAN_CHEST")
            || value.Contains("UNDEAD_VETERAN_CHEST")
            || value.Contains("UNDEAD_CHEST_BOSS_HALLOWEEN")
            || value.Contains("MORGANA_VETERAN_BOOKCHEST")
            || value.Contains("KEEPER_VETERAN_BOOKCHEST")
            || value.Contains("HERETIC_VETERAN_BOOKCHEST")
            || value.Contains("UNDEAD_VETERAN_BOOKCHEST"))
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

        ConsoleManager.WriteLineForMessage(MethodBase.GetCurrentMethod()?.DeclaringType, $"GetFaction Unknown: {value}", ConsoleColorType.EventMapChangeColor);
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

        if (value.Contains("CHEST") || value.Contains("AVALON") || value.Contains("HELL_STD_PVP") || value.Contains("HELL_HRD_PVP") || value.Contains("HELL_STD_PVE") || value.Contains("HELL_HRD_PVE"))
        {
            return EventType.Chest;
        }

        return EventType.Unknown;
    }

    #region Chest
    public static TreasureRarity GetChestRarity(string value)
    {
        if (value.Contains("BOOKCHEST_STANDARD")
            || value.Contains("CHEST_STANDARD")
            || value.Contains("NORMAL_STANDARD")
            || value.Contains("CHEST_BOSS_HALLOWEEN_STANDARD")
            || value.Contains("AVALON") && value.Contains("STANDARD"))
        {
            return TreasureRarity.Common;
        }

        if (value.Contains("BOOKCHEST_UNCOMMON")
            || value.Contains("CHEST_UNCOMMON")
            || value.Contains("NORMAL_UNCOMMON")
            || value.Contains("CHEST_BOSS_UNCOMMON")
            || value.Contains("CHEST_BOSS_HALLOWEEN_UNCOMMON")
            || value.Contains("AVALON") && value.Contains("UNCOMMON"))
        {
            return TreasureRarity.Uncommon;
        }

        if (value.Contains("BOOKCHEST_RARE")
            || value.Contains("CHEST_RARE")
            || value.Contains("NORMAL_RARE")
            || value.Contains("CHEST_BOSS_RARE")
            || value.Contains("CHEST_BOSS_HALLOWEEN_RARE")
            || value.Contains("AVALON") && value.Contains("RARE"))
        {
            return TreasureRarity.Rare;
        }

        if (value.Contains("BOOKCHEST_LEGENDARY")
            || value.Contains("CHEST_LEGENDARY")
            || value.Contains("NORMAL_LEGENDARY")
            || value.Contains("CHEST_BOSS_LEGENDARY")
            || value.Contains("CHEST_BOSS_HALLOWEEN_LEGENDARY")
            || value.Contains("AVALON") && value.Contains("LEGENDARY"))
        {
            return TreasureRarity.Legendary;
        }

        return TreasureRarity.Unknown;
    }

    public static bool IsBossChest(string value)
    {
        return !value.Contains("BOSS_BUFF") && value.Contains("BOSS");
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