using Serilog;
using StatisticAnalysisTool.Extractor;
using StatisticAnalysisTool.Extractor.Enums;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Dungeon.Models;
using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.GameFileData;

public static class GameData
{
    public static async Task<bool> InitializeMainGameDataFilesAsync(ServerType serverType)
    {
        if (string.IsNullOrEmpty(SettingsController.CurrentSettings.MainGameFolderPath))
        {
            var result = await GetMainGameDataWithDialogAsync(serverType);
            if (!result)
            {
                return false;
            }
        }
        else if (!string.IsNullOrEmpty(SettingsController.CurrentSettings.MainGameFolderPath)
                 && Extractor.IsBinFileNewer(
                     Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.IndexedItemsFileName),
                     SettingsController.CurrentSettings.MainGameFolderPath, serverType, "items"))
        {
            await GetMainGameDataAsync(SettingsController.CurrentSettings.MainGameFolderPath, serverType);
            return true;
        }

        if (!Extractor.IsValidMainGameFolder(SettingsController.CurrentSettings?.MainGameFolderPath ?? string.Empty, serverType))
        {
            var result = await GetMainGameDataWithDialogAsync(serverType);
            if (!result)
            {
                return false;
            }
        }

        return true;
    }

    public static async Task<bool> GetMainGameDataWithDialogAsync(ServerType serverType)
    {
        var dialogWindow = new GameDataPreparationWindow();
        var dialogResult = dialogWindow.ShowDialog();

        if (dialogResult is true)
        {
            var gameDataPreparationWindowViewModel = (GameDataPreparationWindowViewModel) dialogWindow.DataContext;
            var mainGameFolderPath = gameDataPreparationWindowViewModel.Path;

            SettingsController.CurrentSettings.MainGameFolderPath = mainGameFolderPath;
            return await GetMainGameDataAsync(SettingsController.CurrentSettings.MainGameFolderPath, serverType);
        }

        return false;
    }

    public static async Task<bool> GetMainGameDataAsync(string mainGameFolderPath, ServerType serverType)
    {
        try
        {
            var toolLoadingWindowViewModel = new ToolLoadingWindowViewModel();
            var toolLoadingWindow = new ToolLoadingWindow(toolLoadingWindowViewModel);
            toolLoadingWindow.Show();

            var tempDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.TempDirecoryName);
            var gameFilesDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.GameFilesDirectoryName);

            var extractor = new Extractor(mainGameFolderPath, serverType);
            var fileNamesToLoad = new List<string>();

            DirectoryController.CreateDirectoryWhenNotExists(tempDirPath);
            DirectoryController.CreateDirectoryWhenNotExists(gameFilesDirPath);

            List<Func<Task>> taskFactories = new List<Func<Task>>();

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, "indexedItems.json"), mainGameFolderPath, serverType, "items")
                || Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, "items.json"), mainGameFolderPath, serverType, "items"))
            {
                taskFactories.Add(() => extractor.ExtractIndexedItemGameDataAsync(gameFilesDirPath, "indexedItems.json"));
                taskFactories.Add(() => extractor.ExtractGameDataAsync(gameFilesDirPath, new[] { "items" }));
            }

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, "spells.xml"), mainGameFolderPath, serverType, "spells"))
            {
                taskFactories.Add(() => extractor.ExtractGameDataFromXmlAsync(gameFilesDirPath, new[] { "spells" }));
                taskFactories.Add(() => extractor.ExtractSpellGameDataAsync(gameFilesDirPath));
            }

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, "mobs-modified.json"), mainGameFolderPath, serverType, "mobs"))
            {
                fileNamesToLoad.Add("mobs");
            }

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, "world-modified.json"), mainGameFolderPath, serverType, "cluster\\world"))
            {
                fileNamesToLoad.Add("cluster\\world");
            }

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, "mists-modified.json"), mainGameFolderPath, serverType, "mists"))
            {
                fileNamesToLoad.Add("mists");
            }


            taskFactories.Add(() => extractor.ExtractGameDataAsync(tempDirPath, fileNamesToLoad.ToArray()));
            taskFactories.Add(ItemController.LoadIndexedItemsDataAsync);
            taskFactories.Add(ItemController.LoadItemsDataAsync);
            taskFactories.Add(MobsData.LoadDataAsync);
            taskFactories.Add(MistsData.LoadDataAsync);
            taskFactories.Add(WorldData.LoadDataAsync);
            taskFactories.Add(SpellData.LoadDataAsync);

            int totalTasks = taskFactories.Count;
            int completedTasks = 0;

            foreach (var taskFactory in taskFactories)
            {
                await taskFactory();

                completedTasks++;
                toolLoadingWindowViewModel.ProgressBarValue = (completedTasks / (double) totalTasks) * 100;
            }

            extractor.Dispose();
            toolLoadingWindow.Close();

            return true;
        }
        catch (Exception e)
        {
            SettingsController.CurrentSettings.MainGameFolderPath = string.Empty;
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return false;
        }
    }

    public static async Task<List<T>> LoadDataAsync<T, TRoot>(string tempFileName, string regularDataFileName, JsonSerializerOptions jsonSerializerOptions) where T : new()
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

        if (File.Exists(tempFilePath))
        {
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

    public static List<T> GetSpecificDataFromJsonFileLocal<T>(string localFilePath, JsonSerializerOptions options)
    {
        try
        {
            var localItemString = File.ReadAllText(localFilePath, Encoding.UTF8);
            return JsonSerializer.Deserialize<List<T>>(localItemString, options);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
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
                WorldJsonRootObject worldJsonRoot => worldJsonRoot.World?.Clusters?.Cluster as List<T> ?? new List<T>(),
                MistsJsonRootObject mistsJsonRoot => mistsJsonRoot.Mists?.MistsMaps?.MapSet?.SelectMany(x => x.Map).Select(map => new MistsJsonObject
                {
                    Id = map.Id,
                    TemplatePool = map.TemplatePool,
                    ClusterTier = map.ClusterTier,
                    SubBiome = map.SubBiome
                }).ToList() as List<T> ?? new List<T>(),
                _ => new List<T>()
            };
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return new List<T>();
        }
    }
}