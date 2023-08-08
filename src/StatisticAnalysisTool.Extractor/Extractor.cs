using StatisticAnalysisTool.Extractor.Enums;
using StatisticAnalysisTool.Extractor.Utilities;

namespace StatisticAnalysisTool.Extractor;

public class Extractor
{
    public void ExtractGameData(string mainGameFolder, ServerType serverType, string outputDirPath)
    {
        string mainGameFolderString = Path.Combine(mainGameFolder, GetServerTypeString(serverType));
        mainGameFolderString = mainGameFolderString.Replace("'", "");

        using var localizationData = new LocalizationData(mainGameFolderString);

        ExtractItems(mainGameFolderString, localizationData, outputDirPath);
        // TODO ...
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
