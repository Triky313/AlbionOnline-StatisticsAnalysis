using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.GameFileData;

public static class SpellData
{
    private static IEnumerable<SpellsJsonObject> _spells;

    public static string GetUniqueName(int index)
    {
        return GetSpellJsonObjectByIndex(index)?.UniqueName ?? string.Empty;
    }

    public static bool IsDataLoaded()
    {
        return _spells?.Count() > 0;
    }

    private static SpellsJsonObject GetSpellJsonObjectByIndex(int index)
    {
        if (!IsDataLoaded())
        {
            return new SpellsJsonObject();
        }

        // The ID in the game has a difference of -360 to the file.
        index -= 360;

        if (index < 0)
        {
            uint unsignedIndex = Convert.ToUInt32(index);
            index = (int) unsignedIndex;
        }

        return _spells.IsInBounds(index) ? _spells?.ElementAt(index) : new SpellsJsonObject();
    }

    public static async Task<bool> LoadDataAsync()
    {
        var spells = await GameData.LoadDataAsync<SpellsJsonObject, SpellsJsonRootObject>(
            Settings.Default.SpellDataFileName,
            Settings.Default.ModifiedSpellDataFileName,
            new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

        _spells = spells;
        return spells.Count >= 0;
    }
}