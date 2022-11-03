using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameData;

// 0: MobObjectId, 13 and 14 Mob HP this | 100 / HitPointsMax * InGameMobHp = 0,16,36,58%
// [116]evDebugAggroInfo - map[0:41 1:2782 2:-1 6: 7:[-23.47 6.39] 8:[-23.47 6.39] 9:28863518 10:103.52002 11:1.5 13:991 14:991 16:28860088 17:340 18:340 19:10 20:28863488 28:0 31:0 252:116]

public static class MobsData
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

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
            >= 99 and <= 101 => 0,
            >= 115 and <= 117 => 1,
            >= 135 and <= 137 => 2,
            >= 157 and <= 159 => 3,
            >= 183 and <= 185 => 4,
            _ => -1
        };
    }

    private static MobJsonObject GetMobJsonObjectByIndex(int index)
    {
        return _mobs.IsInBounds(index) ? _mobs?.ElementAt((int) index) : new MobJsonObject();
    }
    
    public static bool GetDatFromLocalFile()
    {
        var localFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.GameFilesDirectoryName, Settings.Default.MobDataFileName);

        if (!File.Exists(localFilePath))
        {
            return false;
        }

        _mobs = GetDataFromLocal(localFilePath);
        return _mobs?.Count() > 0;
    }

    private static IEnumerable<MobJsonObject> GetDataFromLocal(string localFilePath)
    {
        try
        {
            var options = new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var localItemString = File.ReadAllText(localFilePath, Encoding.UTF8);
            return JsonSerializer.Deserialize<ObservableCollection<MobJsonObject>>(localItemString, options);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            return new ObservableCollection<MobJsonObject>();
        }
    }
}