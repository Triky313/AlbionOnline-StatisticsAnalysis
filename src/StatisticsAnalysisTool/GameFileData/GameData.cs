using log4net;
using StatisticAnalysisTool.Extractor;
using StatisticAnalysisTool.Extractor.Enums;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.GameFileData;

public static class GameData
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public static async Task InitializeGameDataFilesAsync()
    {
        if (string.IsNullOrEmpty(SettingsController.CurrentSettings.MainGameFolderPath))
        {
            var result = await GetMainGameDataWithDialogAsync();
            if (!result)
            {
                Application.Current.Shutdown();
                return;
            }
        }
        else if (!string.IsNullOrEmpty(SettingsController.CurrentSettings.MainGameFolderPath)
                 && Extractor.IsBinFileNewer(
                     File.GetLastWriteTime(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.ItemListFileName)),
                     SettingsController.CurrentSettings.MainGameFolderPath, ServerType.Live, "items"))
        {
            await GetMainGameDataAsync(SettingsController.CurrentSettings.MainGameFolderPath);
            return;
        }

        if (!Extractor.IsValidMainGameFolder(SettingsController.CurrentSettings?.MainGameFolderPath ?? string.Empty, ServerType.Live))
        {
            var result = await GetMainGameDataWithDialogAsync();
            if (!result)
            {
                Application.Current.Shutdown();
            }
        }
    }

    public static async Task<bool> GetMainGameDataWithDialogAsync()
    {
        var dialogWindow = new GameDataPreparationWindow();
        var dialogResult = dialogWindow.ShowDialog();

        if (dialogResult is true)
        {
            var gameDataPreparationWindowViewModel = (GameDataPreparationWindowViewModel) dialogWindow.DataContext;
            var mainGameFolderPath = gameDataPreparationWindowViewModel.Path;

            SettingsController.CurrentSettings.MainGameFolderPath = mainGameFolderPath;
            return await GetMainGameDataAsync(SettingsController.CurrentSettings.MainGameFolderPath);
        }

        return false;
    }

    public static async Task<bool> GetMainGameDataAsync(string mainGameFolderPath)
    {
        try
        {
            var tempDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.TempDirecoryName);
            var gameFilesDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.GameFilesDirectoryName);

            var extractor = new Extractor(mainGameFolderPath, ServerType.Live);
            await extractor.ExtractIndexedItemGameDataAsync(gameFilesDirPath, "indexedItems.json");
            await extractor.ExtractGameDataAsync(gameFilesDirPath, new[] { "items" });
            await extractor.ExtractGameDataAsync(tempDirPath, new[] { "cluster\\world", "mobs", "spells", "items" });
            extractor.Dispose();

            return true;
        }
        catch (Exception e)
        {
            SettingsController.CurrentSettings.MainGameFolderPath = string.Empty;
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            return false;
        }
    }

    public static async Task<List<T>> LoadDataAsync<T, TRoot>(
        string tempFileName,
        string regularDataFileName,
        string sourceUrl,
        int updateByDays,
        string taskName,
        JsonSerializerOptions jsonSerializerOptions) where T : new()
    {
        var tempDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.TempDirecoryName);
        var tempFilePath = Path.Combine(tempDirPath, tempFileName);
        var gameFilesDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.GameFilesDirectoryName);
        var regularDataFilePath = Path.Combine(gameFilesDirPath, regularDataFileName);

        if (!DirectoryController.CreateDirectoryWhenNotExists(tempDirPath))
        {
            return new List<T>();
        }

        if (!DirectoryController.CreateDirectoryWhenNotExists(gameFilesDirPath))
        {
            return new List<T>();
        }

        var regularFileDateTime = File.GetLastWriteTime(regularDataFilePath);
        var tempFileDateTime = File.GetLastWriteTime(tempFilePath);

        if (!File.Exists(regularDataFilePath) || regularFileDateTime.AddDays(updateByDays) < DateTime.Now)
        {
            if (!File.Exists(tempFilePath) || tempFileDateTime.AddDays(updateByDays) < DateTime.Now)
            {
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(3600)
                };

                await client.DownloadFileAsync(sourceUrl, tempFilePath, taskName);
            }

            var fullDataJson = GetDataFromFullJsonFileLocal<T, TRoot>(tempFilePath);
            if (fullDataJson?.Count > 1)
            {
                await FileController.SaveAsync(fullDataJson, regularDataFilePath);
            }
        }

        jsonSerializerOptions ??= new JsonSerializerOptions()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        var data = GetSpecificDataFromJsonFileLocal<T>(regularDataFilePath, jsonSerializerOptions);
        FileController.DeleteFile(tempFilePath);

        return data;
    }

    private static List<T> GetSpecificDataFromJsonFileLocal<T>(string localFilePath, JsonSerializerOptions options)
    {
        try
        {
            var localItemString = File.ReadAllText(localFilePath, Encoding.UTF8);
            return JsonSerializer.Deserialize<List<T>>(localItemString, options);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            return new List<T>();
        }
    }

    private static List<T> GetDataFromFullJsonFileLocal<T, TRoot>(string localFilePath)
    {
        try
        {
            var options = new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var localString = File.ReadAllText(localFilePath, Encoding.UTF8);
            var rootObject = JsonSerializer.Deserialize<TRoot>(localString, options);

            return rootObject switch
            {
                MobJsonRootObject mobRootObject => mobRootObject.Mobs?.Mob as List<T> ?? new List<T>(),
                LootChestRoot lootChestRoot => lootChestRoot.LootChests?.LootChest as List<T> ?? new List<T>(),
                WorldJsonRootObject worldJsonRoot => worldJsonRoot.World.Clusters.Cluster as List<T> ?? new List<T>(),
                SpellsJsonRootObject spellsJsonRoot => spellsJsonRoot.SpellsJson.ActiveSpells as List<T> ?? new List<T>(),
                _ => new List<T>()
            };
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            return new List<T>();
        }
    }
}