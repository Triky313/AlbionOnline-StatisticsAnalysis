using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace StatisticsAnalysisTool.GameData
{
    public static class DungeonObjectData
    {
        public static IEnumerable<LootChest> LootChests;

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

            if (value.Contains("MORGANA_CHEST")
                || value.Contains("KEEPER_CHEST")
                || value.Contains("HERETIC_CHEST")
                || value.Contains("UNDEAD_CHEST")
                || value.Contains("UNDEAD_CHEST_BOSS_HALLOWEEN")
                || value.Contains("MORGANA_BOOKCHEST")
                || value.Contains("KEEPER_BOOKCHEST")
                || value.Contains("HERETIC_BOOKCHEST")
                || value.Contains("UNDEAD_BOOKCHEST"))
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

        public static bool GetDataListFromJson()
        {
            var localFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameFiles", Settings.Default.LootChestDataFileName);

            if (!File.Exists(localFilePath))
            {
                return false;
            }

            LootChests = GetLootChestDataFromLocal();
            return LootChests?.Count() > 0;
        }

        public static DungeonEventObjectType GetDungeonEventObjectType(string value)
        {
            if (value.Contains("SHRINE_COMBAT"))
            {
                return DungeonEventObjectType.CombatShrine;
            }

            if (value.Contains("SHRINE_SILVER"))
            {
                return DungeonEventObjectType.SilverShrine;
            }

            if (value.Contains("SHRINE_FAME"))
            {
                return DungeonEventObjectType.FameShrine;
            }

            if (value.Contains("BOOKCHEST"))
            {
                return DungeonEventObjectType.BookChest;
            }

            if (value.Contains("CHEST") || value.Contains("AVALON") || value.Contains("HELL_STD_PVP") || value.Contains("HELL_HRD_PVP"))
            {
                return DungeonEventObjectType.Chest;
            }

            return DungeonEventObjectType.Unknown;
        }

        #region Chest
        public static TreasureRarity GetChestRarity(string value)
        {
            if (value.Contains("BOOKCHEST_STANDARD")
                || value.Contains("CHEST_STANDARD")
                || value.Contains("NORMAL_STANDARD")
                || value.Contains("CHEST_BOSS_HALLOWEEN_STANDARD")
                || (value.Contains("AVALON") && value.Contains("STANDARD")))
            {
                return TreasureRarity.Standard;
            }

            if (value.Contains("BOOKCHEST_UNCOMMON")
                || value.Contains("CHEST_UNCOMMON")
                || value.Contains("NORMAL_UNCOMMON")
                || value.Contains("CHEST_BOSS_UNCOMMON")
                || value.Contains("CHEST_BOSS_HALLOWEEN_UNCOMMON")
                || (value.Contains("AVALON") && value.Contains("UNCOMMON")))
            {
                return TreasureRarity.Uncommon;
            }

            if (value.Contains("BOOKCHEST_RARE")
                || value.Contains("CHEST_RARE")
                || value.Contains("NORMAL_RARE")
                || value.Contains("CHEST_BOSS_RARE")
                || value.Contains("CHEST_BOSS_HALLOWEEN_RARE")
                || (value.Contains("AVALON") && value.Contains("RARE")))
            {
                return TreasureRarity.Rare;
            }

            if (value.Contains("BOOKCHEST_LEGENDARY")
                || value.Contains("CHEST_LEGENDARY")
                || value.Contains("NORMAL_LEGENDARY")
                || value.Contains("CHEST_BOSS_LEGENDARY")
                || value.Contains("CHEST_BOSS_HALLOWEEN_LEGENDARY")
                || (value.Contains("AVALON") && value.Contains("LEGENDARY")))
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

        #region Helper methods

        private static IEnumerable<LootChest> GetLootChestDataFromLocal()
        {
            try
            {
                var localItemString =
                    File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameFiles", Settings.Default.LootChestDataFileName),
                        Encoding.UTF8);
                return JsonSerializer.Deserialize<LootChestRoot>(localItemString)?.LootChests.LootChest;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForWarning(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return new List<LootChest>();
            }
        }

        #endregion
    }
}