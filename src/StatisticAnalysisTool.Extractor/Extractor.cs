using StatisticAnalysisTool.Extractor.Enums;

namespace StatisticAnalysisTool.Extractor;

public class Extractor
{
    private readonly LocalizationData _localizationData = new();
    private readonly string _mainGameServerFolderString;

    public Extractor(string mainGameFolder, ServerType serverType)
    {
        string mainGameFolderString = Path.Combine(mainGameFolder, GetServerTypeString(serverType));
        _mainGameServerFolderString = mainGameFolderString.Replace("'", "");
    }

    private async Task LoadLocationDataAsync()
    {
        if (_localizationData.IsDataLoaded())
        {
            return;
        }

        await _localizationData.LoadDataAsync(_mainGameServerFolderString);
    }

    public async Task ExtractIndexedItemGameDataAsync(string outputDirPath, string indexedItemsFileName)
    {
        await LoadLocationDataAsync();
        await ItemData.CreateItemDataAsync(_mainGameServerFolderString, _localizationData, outputDirPath, indexedItemsFileName);
    }

    public async Task ExtractGameDataAsync(string outputDirPath, string[] binFileNamesToExtract)
    {
        await LoadLocationDataAsync();
        await BinaryDumper.ExtractAsync(_mainGameServerFolderString, outputDirPath, binFileNamesToExtract);
    }

    private static string GetServerTypeString(ServerType serverType)
    {
        string serverTypeString = serverType switch
        {
            ServerType.Staging => "staging",
            ServerType.Playground => "playground",
            _ => "game"
        };

        return serverTypeString;
    }

    public static bool IsBinFileNewer(string toolFilePath, string mainGameFolder, ServerType serverType, string binFileName)
    {
        try
        {
            string mainGameFolderPath = Path.Combine(mainGameFolder, GetServerTypeString(serverType));
            var binFilePath = Path.Combine(ExtractorUtilities.GetBinFilePath(mainGameFolderPath), $"{binFileName}.bin");
            var toolFileDateTime = File.GetLastWriteTime(toolFilePath);

            if (!File.Exists(binFilePath))
            {
                return false;
            }

            try
            {
                if (File.GetLastWriteTime(binFilePath) > toolFileDateTime)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidMainGameFolder(string mainGameFolder, ServerType serverType)
    {
        string mainGameFolderPath = Path.Combine(mainGameFolder, GetServerTypeString(serverType));

        var itemsBinFilePath = Path.Combine(ExtractorUtilities.GetBinFilePath(mainGameFolderPath), "items.bin");
        var mobsBinFilePath = Path.Combine(ExtractorUtilities.GetBinFilePath(mainGameFolderPath), "mobs.bin");
        var spellsBinFilePath = Path.Combine(ExtractorUtilities.GetBinFilePath(mainGameFolderPath), "spells.bin");
        var mistsBinFilePath = Path.Combine(ExtractorUtilities.GetBinFilePath(mainGameFolderPath), "mists.bin");
        var worldBinFilePath = Path.Combine(ExtractorUtilities.GetBinFilePath(mainGameFolderPath), "cluster", "world.bin");

        return File.Exists(itemsBinFilePath)
               && File.Exists(mobsBinFilePath)
               && File.Exists(spellsBinFilePath)
               && File.Exists(mistsBinFilePath)
               && File.Exists(worldBinFilePath);
    }

    public void Dispose()
    {
        _localizationData.Dispose();
        GC.SuppressFinalize(this);
    }
}