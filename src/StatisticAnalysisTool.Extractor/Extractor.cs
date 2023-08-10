using StatisticAnalysisTool.Extractor.Enums;

namespace StatisticAnalysisTool.Extractor;

public class Extractor
{
    public static async Task ExtractGameDataAsync(string mainGameFolder, ServerType serverType, string outputDirPath)
    {
        string mainGameFolderString = Path.Combine(mainGameFolder, GetServerTypeString(serverType));
        mainGameFolderString = mainGameFolderString.Replace("'", "");

        using var localizationData = new LocalizationData();
        await localizationData.LoadDataAsync(mainGameFolderString);

        await ItemData.CreateItemDataAsync(mainGameFolderString, localizationData, outputDirPath);
        await BinaryDumper.ExtractAsync(mainGameFolderString, outputDirPath, new[] { "cluster\\world", "mobs", "spells" });
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
}
