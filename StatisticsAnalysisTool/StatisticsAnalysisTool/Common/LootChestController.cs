using log4net;
using Newtonsoft.Json;
using PcapDotNet.Base;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common
{
    public class LootChestController
    {
        public static IEnumerable<LootChest> LootChestData;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static ChestType GetChestType(string value)
        {
            if (value.Contains("BOOKCHEST"))
            {
                return ChestType.BookChest;
            }

            if (value.Contains("CHEST"))
            {
                return ChestType.Chest;
            }

            return ChestType.Unknown;
        }

        public static DungeonMode GetDungeonMode(string value)
        {
            if (value.Contains("MORGANA_SOLO_CHEST") || value.Contains("KEEPER_SOLO_CHEST") || value.Contains("HERETIC_SOLO_CHEST") || value.Contains("UNDEAD_SOLO_CHEST"))
            {
                return DungeonMode.Solo;
            }

            if (value.Contains("MORGANA_CHEST") || value.Contains("KEEPER_CHEST") || value.Contains("HERETIC_CHEST") || value.Contains("UNDEAD_CHEST"))
            {
                return DungeonMode.Standard;
            }

            if (value.Contains("AVALON_STANDARD"))
            {
                return DungeonMode.Avalon;
            }

            return DungeonMode.Unknown;
        }

        public static ChestRarity GetChestRarity(string value)
        {
            if (value.Contains("BOOKCHEST_STANDARD") || value.Contains("CHEST_STANDARD"))
            {
                return ChestRarity.Standard;
            }
            
            if (value.Contains("BOOKCHEST_UNCOMMON") || value.Contains("CHEST_UNCOMMON") || value.Contains("CHEST_BOSS_UNCOMMON"))
            {
                return ChestRarity.Uncommon;
            }

            if (value.Contains("BOOKCHEST_RARE") || value.Contains("CHEST_RARE") || value.Contains("CHEST_BOSS_RARE"))
            {
                return ChestRarity.Rare;
            }

            if (value.Contains("BOOKCHEST_LEGENDARY") || value.Contains("CHEST_LEGENDARY") || value.Contains("CHEST_BOSS_LEGENDARY"))
            {
                return ChestRarity.Legendary;
            }

            return ChestRarity.Unknown;
        }

        public static bool IsBossChest(string value) => value.Contains("CHEST_BOSS");

        public static Faction GetFaction(string value)
        {
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

        public static async Task<bool> GetDataListFromJsonAsync()
        {
            var url = Settings.Default.LootChestDataSourceUrl;
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.LootChestDataFileName}";

            if (string.IsNullOrEmpty(url))
            {
                Log.Warn($"{nameof(GetDataListFromJsonAsync)}: No LootChestDataSourceUrl found.");
                return false;
            }

            if (File.Exists(localFilePath))
            {
                var fileDateTime = File.GetLastWriteTime(localFilePath);

                if (fileDateTime.AddDays(Settings.Default.UpdateWorldDataByDays) < DateTime.Now)
                {
                    if (await GetLootChestListFromWebAsync(url))
                    {
                        LootChestData = GetLootChestDataFromLocal();
                    }
                    return LootChestData != null && !LootChestData.IsNullOrEmpty();
                }

                LootChestData = GetLootChestDataFromLocal();
                return LootChestData != null && !LootChestData.IsNullOrEmpty();
            }

            if (await GetLootChestListFromWebAsync(url))
            {
                LootChestData = GetLootChestDataFromLocal();
            }
            return LootChestData != null && !LootChestData.IsNullOrEmpty();
        }

        #region Helper methods

        private static IEnumerable<LootChest> GetLootChestDataFromLocal()
        {
            try
            {
                var localItemString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameFiles", Settings.Default.LootChestDataFileName), Encoding.UTF8);
                return JsonConvert.DeserializeObject<LootChestRoot>(localItemString).LootChests.LootChest;
            }
            catch
            {
                return new List<LootChest>();
            }
        }

        private static async Task<bool> GetLootChestListFromWebAsync(string url)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                try
                {
                    using (var response = await client.GetAsync(url))
                    {
                        using (var content = response.Content)
                        {
                            var fileString = await content.ReadAsStringAsync();
                            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameFiles", Settings.Default.LootChestDataFileName), fileString, Encoding.UTF8);
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(nameof(GetLootChestListFromWebAsync), e);
                    return false;
                }
            }
        }

        #endregion
    }
}