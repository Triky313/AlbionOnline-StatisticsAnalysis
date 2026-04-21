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

public static class MobsData
{
    private static IEnumerable<MobJsonObject> _mobs;

    public static int GetMobTierByIndex(int index)
    {
        return GetMobJsonObjectByIndex(index).Tier;
    }

    public static int GetMobLevelByIndex(int index, double currentInGameMobHp)
    {
        var mob = GetMobJsonObjectByIndex(index);

        var mobHpInPercentOverMaxValue = 100 / mob.HitPointsMax * currentInGameMobHp;
        return mobHpInPercentOverMaxValue switch
        {
            >= 80 and <= 92 => 0,
            >= 99 and <= 111 => 1,
            >= 115 and <= 127 => 2,
            >= 135 and <= 147 => 3,
            >= 155 and <= 174 => 4,
            _ => -1
        };
    }

    private static MobJsonObject GetMobJsonObjectByIndex(int index)
    {
        // From July 18, 2025, the in-game index will start counting from 15.
        // The ID's were decreased by 15
        index -= 15;

        if (index < 0)
        {
            uint unsignedIndex = Convert.ToUInt32(index);
            index = (int) unsignedIndex;
        }

        return _mobs.IsInBounds(index) ? _mobs?.ElementAt(index) : new MobJsonObject();
    }

    public static async Task<bool> LoadDataAsync()
    {
        var mobs = await GameData.LoadDataAsync<MobJsonObject, MobJsonRootObject>(
            Settings.Default.MobDataFileName,
            Settings.Default.ModifiedMobDataFileName,
            new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

        _mobs = mobs;
        return mobs.Count >= 0;
    }
}
