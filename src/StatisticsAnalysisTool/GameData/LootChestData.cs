using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.GameData;

public static class LootChestData
{
    private static IEnumerable<LootChest> _lootChest;

    public static bool IsDataLoaded()
    {
        return _lootChest?.Count() > 0;
    }

    private static LootChest Get(int index)
    {
        if (index < 0)
        {
            uint unsignedIndex = Convert.ToUInt32(index);
            index = (int) unsignedIndex;
        }

        return _lootChest.IsInBounds(index) ? _lootChest?.ElementAt(index) : new LootChest();
    }

    public static async Task<bool> LoadDataAsync()
    {
        var lootChestData = await GameData.LoadDataAsync<LootChest, LootChestRoot>(
            Settings.Default.LootChestDataFileName,
            Settings.Default.ModifiedLootChestDataFileName,
            SettingsController.CurrentSettings.LootChestJsonSourceUrl,
            SettingsController.CurrentSettings.UpdateLootChestJsonByDays,
            LanguageController.Translation("GET_LOOT_CHEST_JSON"),
            new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

        _lootChest = lootChestData;
        return lootChestData.Count >= 0;
    }
}