using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.GameData;

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

    public static bool IsDataLoaded()
    {
        return _mobs?.Count() > 0;
    }

    private static MobJsonObject GetMobJsonObjectByIndex(int index)
    {
        // From February 13, 2023, the in-game index will start counting from 2. The character data that was previously in the file is now in a separate file.
        // The ID's were decreased by 2 from ID 11
        index -= 2;

        if (index < 0)
        {
            uint unsignedIndex = Convert.ToUInt32(index);
            index = (int) unsignedIndex;
        }

        return _mobs.IsInBounds(index) ? _mobs?.ElementAt(index) : new MobJsonObject();
    }

    public static async Task<bool> LoadMobsDataAsync()
    {
        var tempDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.TempDirecoryName);
        var tempFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.TempDirecoryName, Settings.Default.MobDataFileName);
        var regularDataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.GameFilesDirectoryName, Settings.Default.ModifiedMobDataFileName);

        if (!DirectoryController.CreateDirectoryWhenNotExists(tempDirPath))
        {
            return false;
        }

        var regularFileDateTime = File.GetLastWriteTime(regularDataFilePath);
        var tempFileDateTime = File.GetLastWriteTime(tempFilePath);

        if (!File.Exists(regularDataFilePath) || regularFileDateTime.AddDays(SettingsController.CurrentSettings.UpdateMobsJsonByDays) < DateTime.Now)
        {
            if (!File.Exists(tempFilePath) || tempFileDateTime.AddDays(SettingsController.CurrentSettings.UpdateMobsJsonByDays) < DateTime.Now)
            {
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(3600)
                };

                await client.DownloadFileAsync(SettingsController.CurrentSettings.MobsJsonSourceUrl, tempFilePath, LanguageController.Translation("GET_MOBS_JSON"));
            }

            var fullMobsJson = GetDataFromFullJsonFileLocal(tempFilePath);
            if(fullMobsJson.Count() > 1)
            {
                await FileController.SaveAsync(fullMobsJson, regularDataFilePath);
            }
        }

        _mobs = GetSpecificDataFromJsonFileLocal(regularDataFilePath);
        FileController.DeleteFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.TempDirecoryName, Settings.Default.MobDataFileName));

        return _mobs?.Count() > 0;
    }

    private static IEnumerable<MobJsonObject> GetSpecificDataFromJsonFileLocal(string localFilePath)
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

    private static List<MobJsonObject> GetDataFromFullJsonFileLocal(string localFilePath)
    {
        try
        {
            var options = new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var localString = File.ReadAllText(localFilePath, Encoding.UTF8);
            var rootObject = JsonSerializer.Deserialize<MobJsonRootObject>(localString, options);
            return rootObject?.Mobs?.Mob ?? new List<MobJsonObject>();
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            return new List<MobJsonObject>();
        }
    }
}