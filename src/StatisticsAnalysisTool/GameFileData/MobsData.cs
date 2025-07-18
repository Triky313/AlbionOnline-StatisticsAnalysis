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
            >= 99 and <= 111 => 0,
            >= 115 and <= 127 => 1,
            >= 135 and <= 147 => 2,
            >= 157 and <= 174 => 3,
            >= 183 and <= 195 => 4,
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