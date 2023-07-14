using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.GameData;

public static class SpellData
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private static IEnumerable<SpellsJsonObject> _spells;

    public static string GetUniqueName(int index)
    {
        return GetSpellJsonObjectByIndex(index).UniqueName;
    }

    public static bool IsDataLoaded()
    {
        return _spells?.Count() > 0;
    }

    private static SpellsJsonObject GetSpellJsonObjectByIndex(int index)
    {
        // The ID in the game has a difference of -360 to the file.
        index -= 360;

        if (index < 0)
        {
            uint unsignedIndex = Convert.ToUInt32(index);
            index = (int) unsignedIndex;
        }

        return _spells.IsInBounds(index) ? _spells?.ElementAt(index) : new SpellsJsonObject();
    }

    public static async Task<bool> LoadSpellsDataAsync()
    {
        var tempDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.TempDirecoryName);
        var tempFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.TempDirecoryName, Settings.Default.SpellDataFileName);
        var regularDataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.GameFilesDirectoryName, Settings.Default.ModifiedSpellDataFileName);

        if (!DirectoryController.CreateDirectoryWhenNotExists(tempDirPath))
        {
            return false;
        }

        var regularFileDateTime = File.GetLastWriteTime(regularDataFilePath);
        var tempFileDateTime = File.GetLastWriteTime(tempFilePath);

        if (!File.Exists(regularDataFilePath) || regularFileDateTime.AddDays(SettingsController.CurrentSettings.UpdateSpellsJsonByDays) < DateTime.Now)
        {
            if (!File.Exists(tempFilePath) || tempFileDateTime.AddDays(SettingsController.CurrentSettings.UpdateSpellsJsonByDays) < DateTime.Now)
            {
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(3600)
                };

                await client.DownloadFileAsync(SettingsController.CurrentSettings.SpellsJsonSourceUrl, tempFilePath, LanguageController.Translation("GET_SPELLS_JSON"));
            }

            var fullJson = GetDataFromFullJsonFileLocal(tempFilePath);
            if (fullJson.Count > 1)
            {
                await FileController.SaveAsync(fullJson, regularDataFilePath);
            }
        }

        _spells = GameData.GetSpecificDataFromJsonFileLocal<SpellsJsonObject>(regularDataFilePath, new JsonSerializerOptions()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            ReadCommentHandling = JsonCommentHandling.Skip
        });
        FileController.DeleteFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.TempDirecoryName, Settings.Default.SpellDataFileName));

        return _spells?.Count() > 0;
    }

    private static List<SpellsJsonObject> GetDataFromFullJsonFileLocal(string localFilePath)
    {
        try
        {
            var options = new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var localString = File.ReadAllText(localFilePath, Encoding.UTF8);
            var rootObject = JsonSerializer.Deserialize<SpellsJsonRootObject>(localString, options);
            return rootObject?.SpellsJson.ActiveSpells ?? new List<SpellsJsonObject>();
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            return new List<SpellsJsonObject>();
        }
    }
}