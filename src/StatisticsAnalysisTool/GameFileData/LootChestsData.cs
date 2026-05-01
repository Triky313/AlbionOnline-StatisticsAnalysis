using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.GameFileData;

public static class LootChestsData
{
    private static IReadOnlyList<LootChestJsonObject> _lootChests = [];

    public static IReadOnlyList<LootChestJsonObject> LootChests => _lootChests;

    public static bool IsDataLoaded()
    {
        return _lootChests.Count > 0;
    }

    public static LootChestJsonObject GetByUniqueName(string uniqueName)
    {
        if (string.IsNullOrWhiteSpace(uniqueName))
        {
            return new LootChestJsonObject();
        }

        return _lootChests.FirstOrDefault(x => x.UniqueName == uniqueName) ?? new LootChestJsonObject();
    }

    public static async Task<bool> LoadDataAsync()
    {
        var lootChests = await GameData.LoadDataAsync<LootChestJsonObject, LootChestJsonRootObject>(
            Settings.Default.LootChestDataFileName,
            Settings.Default.LootChestDataFileName,
            new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                ReadCommentHandling = JsonCommentHandling.Skip
            }).ConfigureAwait(false);

        _lootChests = lootChests;
        return lootChests.Count >= 0;
    }
}