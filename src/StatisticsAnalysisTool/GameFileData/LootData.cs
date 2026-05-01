using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.GameFileData;

public static class LootData
{
    private static IReadOnlyList<LootListJsonObject> _lootLists = [];

    public static IReadOnlyList<LootListJsonObject> LootLists => _lootLists;

    public static bool IsDataLoaded()
    {
        return _lootLists.Count > 0;
    }

    public static LootListJsonObject GetByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new LootListJsonObject();
        }

        return _lootLists.FirstOrDefault(x => x.Name == name) ?? new LootListJsonObject();
    }

    public static IReadOnlyList<LootItemJsonObject> GetItemsByName(string name)
    {
        return GetByName(name).Item;
    }

    public static IReadOnlyList<LootListReferenceJsonObject> GetReferencesByName(string name)
    {
        return GetByName(name).LootListReference;
    }

    public static async Task<bool> LoadDataAsync()
    {
        var lootLists = await GameData.LoadDataAsync<LootListJsonObject, LootJsonRootObject>(
            Settings.Default.LootDataFileName,
            Settings.Default.LootDataFileName,
            new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                ReadCommentHandling = JsonCommentHandling.Skip
            }).ConfigureAwait(false);

        _lootLists = lootLists;
        return lootLists.Count >= 0;
    }
}