using Serilog;
using StatisticAnalysisTool.Extractor;
using StatisticAnalysisTool.Extractor.Enums;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Dungeon.Models;
using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Localization;
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
    private const int FileBufferSize = 65536;

    public static async Task<bool> InitializeMainGameDataFilesAsync(ServerType serverType)
    {
        if (string.IsNullOrEmpty(SettingsController.CurrentSettings.MainGameFolderPath))
        {
            return await GetMainGameDataWithDialogAsync(serverType);
        }

        if (!Extractor.IsValidMainGameFolder(SettingsController.CurrentSettings?.MainGameFolderPath ?? string.Empty, serverType))
        {
            return await GetMainGameDataWithDialogAsync(serverType);
        }

        return await GetMainGameDataAsync(SettingsController.CurrentSettings?.MainGameFolderPath, serverType);
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
        Extractor extractor = null;
        ToolLoadingWindow toolLoadingWindow = null;

        try
        {
            var toolLoadingWindowViewModel = new ToolLoadingWindowViewModel();
            toolLoadingWindow = new ToolLoadingWindow(toolLoadingWindowViewModel);
            toolLoadingWindow.Show();

            var tempDirPath = AppDataPaths.TempDirectory;
            var gameFilesDirPath = AppDataPaths.GameFilesDirectory;

            extractor = new Extractor(mainGameFolderPath, serverType);
            var fileNamesToLoad = new List<string>();

            DirectoryController.CreateDirectoryWhenNotExists(tempDirPath);
            DirectoryController.CreateDirectoryWhenNotExists(gameFilesDirPath);

            List<Func<Task>> extractionTaskFactories = [];

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, "localization.xml"), mainGameFolderPath, serverType, "localization"))
            {
                extractionTaskFactories.Add(() => extractor.ExtractGameDataFromXmlAsync(gameFilesDirPath, ["localization"]));
            }

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, "indexedItems.json"), mainGameFolderPath, serverType, "items")
                || Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, "items.json"), mainGameFolderPath, serverType, "items"))
            {
                extractionTaskFactories.Add(() => extractor.ExtractIndexedItemGameDataAsync(gameFilesDirPath, "indexedItems.json"));
                extractionTaskFactories.Add(() => extractor.ExtractGameDataAsync(gameFilesDirPath, ["items"]));
            }

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, "spells.xml"), mainGameFolderPath, serverType, "spells"))
            {
                extractionTaskFactories.Add(() => extractor.ExtractGameDataFromXmlAsync(gameFilesDirPath, ["spells"]));
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

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, Settings.Default.LootChestDataFileName), mainGameFolderPath, serverType, "lootchests"))
            {
                fileNamesToLoad.Add("lootchests");
            }

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, Settings.Default.LootDataFileName), mainGameFolderPath, serverType, "loot"))
            {
                fileNamesToLoad.Add("loot");
            }

            if (fileNamesToLoad.Count > 0)
            {
                extractionTaskFactories.Add(() => extractor.ExtractGameDataAsync(tempDirPath, fileNamesToLoad.ToArray()));
            }

            List<Func<Task>> loadTaskFactories =
            [
                LoadItemGameDataAsync,
                async () => await MobsData.LoadDataAsync().ConfigureAwait(false),
                async () => await MistsData.LoadDataAsync().ConfigureAwait(false),
                async () => await WorldData.LoadDataAsync().ConfigureAwait(false),
                async () => await SpellData.LoadDataAsync().ConfigureAwait(false),
                async () => await LootChestsData.LoadDataAsync().ConfigureAwait(false),
                async () => await LootData.LoadDataAsync().ConfigureAwait(false),
                () => LoadGameLocalizationsAsync(extractor, gameFilesDirPath)
            ];

            int totalTasks = extractionTaskFactories.Count + loadTaskFactories.Count;
            int completedTasks = 0;

            void UpdateProgress()
            {
                completedTasks++;
                toolLoadingWindowViewModel.ProgressBarValue = (completedTasks / (double) totalTasks) * 100;
            }

            foreach (var taskFactory in extractionTaskFactories)
            {
                await taskFactory();
                UpdateProgress();
            }

            await RunTaskFactoriesInParallelAsync(loadTaskFactories, UpdateProgress);

            return true;
        }
        catch (Exception e)
        {
            SettingsController.CurrentSettings.MainGameFolderPath = string.Empty;
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return false;
        }
        finally
        {
            extractor?.Dispose();
            toolLoadingWindow?.Close();
        }
    }

    private static async Task LoadItemGameDataAsync()
    {
        await ItemController.LoadIndexedItemsDataAsync().ConfigureAwait(false);
        await ItemController.LoadItemsDataAsync().ConfigureAwait(false);
    }

    private static async Task LoadGameLocalizationsAsync(Extractor extractor, string gameFilesDirPath)
    {
        if (extractor.GameLocalization.Count > 0)
        {
            await LocalizationController.SetGameLocalizationsAsync(extractor.GameLocalization).ConfigureAwait(false);
            return;
        }

        var localizationFilePath = Path.Combine(gameFilesDirPath, "localization.xml");
        await LocalizationController.SetGameLocalizationsFromXmlFileAsync(localizationFilePath).ConfigureAwait(false);
    }

    private static async Task RunTaskFactoriesInParallelAsync(IReadOnlyCollection<Func<Task>> taskFactories, Action onTaskCompleted)
    {
        var runningTasks = taskFactories.Select(taskFactory => taskFactory()).ToList();

        while (runningTasks.Count > 0)
        {
            var completedTask = await Task.WhenAny(runningTasks);
            runningTasks.Remove(completedTask);

            await completedTask;
            onTaskCompleted();
        }
    }

    public static async Task<List<T>> LoadDataAsync<T, TRoot>(string tempFileName, string regularDataFileName, JsonSerializerOptions jsonSerializerOptions) where T : new()
    {
        var tempDirPath = AppDataPaths.TempDirectory;
        var tempFilePath = Path.Combine(tempDirPath, tempFileName);
        var gameFilesDirPath = AppDataPaths.GameFilesDirectory;
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
            var fullDataJson = await GetDataFromFullJsonFileLocalAsync<T, TRoot>(tempFilePath).ConfigureAwait(false);
            if (fullDataJson?.Count > 0)
            {
                var saveSucceeded = await FileController.SaveAsync(fullDataJson, regularDataFilePath).ConfigureAwait(false);
                if (saveSucceeded)
                {
                    FileController.DeleteFile(tempFilePath);
                }

                return fullDataJson;
            }
        }

        jsonSerializerOptions ??= new JsonSerializerOptions()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        var data = await GetSpecificDataFromJsonFileLocalAsync<T>(regularDataFilePath, jsonSerializerOptions, false).ConfigureAwait(false);
        FileController.DeleteFile(tempFilePath);

        if (data.Count > 0 || !File.Exists(regularDataFilePath))
        {
            return data;
        }

        var fullRegularDataJson = await GetDataFromFullJsonFileLocalAsync<T, TRoot>(regularDataFilePath).ConfigureAwait(false);
        if (fullRegularDataJson.Count > 0)
        {
            await FileController.SaveAsync(fullRegularDataJson, regularDataFilePath).ConfigureAwait(false);
            return fullRegularDataJson;
        }

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
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return new List<T>();
        }
    }

    private static async Task<List<T>> GetSpecificDataFromJsonFileLocalAsync<T>(string localFilePath, JsonSerializerOptions options, bool logErrors = true)
    {
        try
        {
            await using var stream = CreateReadStream(localFilePath);
            return await JsonSerializer.DeserializeAsync<List<T>>(stream, options).ConfigureAwait(false) ?? new List<T>();
        }
        catch (Exception e)
        {
            if (logErrors)
            {
                DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            }

            return new List<T>();
        }
    }

    private static async Task<List<T>> GetDataFromFullJsonFileLocalAsync<T, TRoot>(string localFilePath)
    {
        try
        {
            var options = new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            await using var stream = CreateReadStream(localFilePath);
            var rootObject = await JsonSerializer.DeserializeAsync<TRoot>(stream, options).ConfigureAwait(false);

            return rootObject switch
            {
                MobJsonRootObject mobRootObject => mobRootObject.Mobs?.Mob as List<T> ?? [],
                LootChestJsonRootObject lootChestRoot => lootChestRoot.LootChests?.LootChest as List<T> ?? [],
                LootJsonRootObject lootRoot => lootRoot.LootDefinition?.Lootlist as List<T> ?? [],
                WorldJsonRootObject worldJsonRoot => worldJsonRoot.World?.Clusters?.Cluster as List<T> ?? [],
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
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return new List<T>();
        }
    }

    private static FileStream CreateReadStream(string path)
    {
        return new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            FileBufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
    }
}
