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
using StatisticsAnalysisTool.Models;
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

    public static async Task<bool> InitializeMainGameDataFilesAsync(
        ServerType serverType,
        Action<double, string> reportProgress = null,
        double progressStart = 0,
        double progressEnd = 100)
    {
        if (string.IsNullOrEmpty(SettingsController.CurrentSettings.MainGameFolderPath))
        {
            return await GetMainGameDataWithDialogAsync(serverType, reportProgress, progressStart, progressEnd);
        }

        if (!Extractor.IsValidMainGameFolder(SettingsController.CurrentSettings?.MainGameFolderPath ?? string.Empty, serverType))
        {
            return await GetMainGameDataWithDialogAsync(serverType, reportProgress, progressStart, progressEnd);
        }

        return await GetMainGameDataAsync(
            SettingsController.CurrentSettings?.MainGameFolderPath,
            serverType,
            reportProgress,
            progressStart,
            progressEnd);
    }

    public static async Task<bool> GetMainGameDataWithDialogAsync(
        ServerType serverType,
        Action<double, string> reportProgress = null,
        double progressStart = 0,
        double progressEnd = 100)
    {
        var dialogWindow = new GameDataPreparationWindow();
        var dialogResult = dialogWindow.ShowDialog();

        if (dialogResult is true)
        {
            var gameDataPreparationWindowViewModel = (GameDataPreparationWindowViewModel) dialogWindow.DataContext;
            var mainGameFolderPath = gameDataPreparationWindowViewModel.Path;

            SettingsController.CurrentSettings.MainGameFolderPath = mainGameFolderPath;
            return await GetMainGameDataAsync(
                SettingsController.CurrentSettings.MainGameFolderPath,
                serverType,
                reportProgress,
                progressStart,
                progressEnd);
        }

        return false;
    }

    public static async Task<bool> GetMainGameDataAsync(
        string mainGameFolderPath,
        ServerType serverType,
        Action<double, string> reportProgress = null,
        double progressStart = 0,
        double progressEnd = 100)
    {
        Extractor extractor = null;

        try
        {

            var tempDirPath = AppDataPaths.TempDirectory;
            var gameFilesDirPath = AppDataPaths.GameFilesDirectory;

            extractor = new Extractor(mainGameFolderPath, serverType);
            var fileNamesToLoad = new List<string>();

            DirectoryController.CreateDirectoryWhenNotExists(tempDirPath);
            DirectoryController.CreateDirectoryWhenNotExists(gameFilesDirPath);

            List<(string Name, Func<Task> TaskFactory)> extractionTaskFactories = [];

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, "localization.xml"), mainGameFolderPath, serverType, "localization"))
            {
                extractionTaskFactories.Add(("localization.xml", () => extractor.ExtractGameDataFromXmlAsync(gameFilesDirPath, ["localization"])));
            }

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, "indexedItems.json"), mainGameFolderPath, serverType, "items")
                || Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, "items.json"), mainGameFolderPath, serverType, "items"))
            {
                extractionTaskFactories.Add(("indexedItems.json", () => extractor.ExtractIndexedItemGameDataAsync(gameFilesDirPath, "indexedItems.json")));
                extractionTaskFactories.Add(("items.json", () => extractor.ExtractGameDataAsync(gameFilesDirPath, ["items"])));
            }

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, "spells.xml"), mainGameFolderPath, serverType, "spells"))
            {
                extractionTaskFactories.Add(("spells.xml", () => extractor.ExtractGameDataFromXmlAsync(gameFilesDirPath, ["spells"])));
            }

            var mobsModifiedFilePath = Path.Combine(gameFilesDirPath, "mobs-modified.json");
            if (Extractor.IsBinFileNewer(mobsModifiedFilePath, mainGameFolderPath, serverType, "mobs")
                || IsMobDataMissingOpenWorldFields(mobsModifiedFilePath))
            {
                fileNamesToLoad.Add("mobs");
            }

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, "world-modified.json"), mainGameFolderPath, serverType, "cluster\\world"))
            {
                fileNamesToLoad.Add("cluster\\world");
            }

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, CraftingLocationData.ModifiedFileName), mainGameFolderPath, serverType, "craftingmodifiers"))
            {
                fileNamesToLoad.Add("craftingmodifiers");
            }

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, HideoutData.ModifiedFileName), mainGameFolderPath, serverType, "hideouts"))
            {
                fileNamesToLoad.Add("hideouts");
            }

            if (Extractor.IsBinFileNewer(Path.Combine(gameFilesDirPath, "mists-modified.json"), mainGameFolderPath, serverType, "mists"))
            {
                fileNamesToLoad.Add("mists");
            }

            if (fileNamesToLoad.Count > 0)
            {
                var extractionTaskName = string.Join(", ", fileNamesToLoad.Select(Path.GetFileName));
                extractionTaskFactories.Add((extractionTaskName, () => extractor.ExtractGameDataAsync(tempDirPath, fileNamesToLoad.ToArray())));
            }

            List<(string Name, Func<Task> TaskFactory)> loadTaskFactories =
            [
                ("indexedItems.json, items.json", LoadItemGameDataAsync),
                ("mobs-modified.json", async () => await MobsData.LoadDataAsync().ConfigureAwait(false)),
                ("mists-modified.json", async () => await MistsData.LoadDataAsync().ConfigureAwait(false)),
                ("world-modified.json", async () => await WorldData.LoadDataAsync().ConfigureAwait(false)),
                (CraftingLocationData.ModifiedFileName, async () => await CraftingLocationData.LoadDataAsync().ConfigureAwait(false)),
                (HideoutData.ModifiedFileName, async () => await HideoutData.LoadDataAsync().ConfigureAwait(false)),
                ("spells.xml", async () => await SpellData.LoadDataAsync().ConfigureAwait(false)),
                ("localization.xml", () => LoadGameLocalizationsAsync(extractor, gameFilesDirPath))
            ];

            int totalTasks = extractionTaskFactories.Count + loadTaskFactories.Count;
            int completedTasks = 0;

            void UpdateProgress(string currentTaskName)
            {
                var progress = progressStart + completedTasks / (double) totalTasks * (progressEnd - progressStart);
                reportProgress?.Invoke(progress, currentTaskName);
            }

            foreach (var (name, taskFactory) in extractionTaskFactories)
            {
                UpdateProgress(name);
                await taskFactory();
                completedTasks++;
                UpdateProgress(name);
            }

            await RunTaskFactoriesInParallelAsync(
                loadTaskFactories,
                UpdateProgress,
                currentTaskName =>
                {
                    completedTasks++;
                    UpdateProgress(currentTaskName);
                });

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

    private static async Task RunTaskFactoriesInParallelAsync(
        IReadOnlyCollection<(string Name, Func<Task> TaskFactory)> taskFactories,
        Action<string> onTasksStarted,
        Action<string> onTaskCompleted)
    {
        var runningTasks = taskFactories
            .Select(taskFactory => (taskFactory.Name, Task: taskFactory.TaskFactory()))
            .ToList();

        if (runningTasks.Count > 0)
        {
            onTasksStarted(runningTasks[0].Name);
        }

        while (runningTasks.Count > 0)
        {
            var completedTask = await Task.WhenAny(runningTasks.Select(x => x.Task));
            var completedTaskIndex = runningTasks.FindIndex(x => x.Task == completedTask);
            var completedTaskName = runningTasks[completedTaskIndex].Name;
            runningTasks.RemoveAt(completedTaskIndex);

            await completedTask;
            onTaskCompleted(runningTasks.Count > 0 ? runningTasks[0].Name : completedTaskName);
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

        var data = await GetSpecificDataFromJsonFileLocalAsync<T>(regularDataFilePath, jsonSerializerOptions).ConfigureAwait(false);
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
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return new List<T>();
        }
    }

    private static async Task<List<T>> GetSpecificDataFromJsonFileLocalAsync<T>(string localFilePath, JsonSerializerOptions options)
    {
        try
        {
            await using var stream = CreateReadStream(localFilePath);
            return await JsonSerializer.DeserializeAsync<List<T>>(stream, options).ConfigureAwait(false) ?? new List<T>();
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
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
                MobJsonRootObject mobRootObject => MobsData.EnrichMissingNameLocatags(mobRootObject.Mobs?.Mob ?? []) as List<T> ?? [],
                LootChestRoot lootChestRoot => lootChestRoot.LootChests?.LootChest as List<T> ?? [],
                WorldJsonRootObject worldJsonRoot => worldJsonRoot.World?.Clusters?.Cluster as List<T> ?? [],
                CraftingModifiersRootObject craftingModifiersRootObject => craftingModifiersRootObject.CraftingModifiers?.CraftingLocation as List<T> ?? [],
                HideoutsRootObject hideoutsRootObject => hideoutsRootObject.Hideouts?.Hideout?.PowerLevels?.PowerLevel as List<T> ?? [],
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

    private static bool IsMobDataMissingOpenWorldFields(string mobDataFilePath)
    {
        if (!File.Exists(mobDataFilePath))
        {
            return false;
        }

        try
        {
            using var stream = File.OpenRead(mobDataFilePath);
            using var document = JsonDocument.Parse(stream);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            var hasAvatar = false;
            var hasFaction = false;
            var hasNameLocatag = false;

            foreach (var mob in document.RootElement.EnumerateArray())
            {
                hasAvatar |= mob.TryGetProperty("@avatar", out _);
                hasFaction |= mob.TryGetProperty("@faction", out _);
                hasNameLocatag |= mob.TryGetProperty("@namelocatag", out _);

                if (hasAvatar && hasFaction && hasNameLocatag)
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return false;
        }
    }
}
