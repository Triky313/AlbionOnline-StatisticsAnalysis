using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Dungeon.Models;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Properties;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Dungeon;

public class MistsData
{
    private static IEnumerable<MistsJsonObject> _mists;

    public static int GetTier(string worldMapDataType)
    {
        var mistData = Get(worldMapDataType);
        return int.TryParse(mistData.ClusterTier, NumberStyles.Any, LanguageController.CurrentCultureInfo, out int result) ? result : 0;
    }

    public static MistsJsonObject Get(string worldMapDataType)
    {
        var mistData = _mists.FirstOrDefault(x => x.Id == worldMapDataType);
        return mistData ?? new MistsJsonObject();
    }

    public static async Task<bool> LoadDataAsync()
    {
        var mists = await GameData.LoadDataAsync<MistsJsonObject, MistsJsonRootObject>(
            Settings.Default.MistsDataFileName,
            Settings.Default.ModifiedMistsDataFileName,
            new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

        _mists = mists;
        return mists.Count >= 0;
    }
}