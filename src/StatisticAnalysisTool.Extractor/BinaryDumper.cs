using Newtonsoft.Json;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

namespace StatisticAnalysisTool.Extractor;

internal class BinaryDumper
{
    public static async Task ExtractAsync(string mainGameFolder, string outputFolderPath, string[] binFileNamesToExtract)
    {
        var allFiles = GetBinFilePaths(mainGameFolder, binFileNamesToExtract);
        var outFiles = (string[]) allFiles.Clone();
        for (var i = 0; i < outFiles.Length; i++)
        {
            outFiles[i] = outFiles[i].Remove(0, outFiles[i].LastIndexOf("GameData\\", StringComparison.Ordinal) + "GameData\\".Length);
        }

        foreach (string binFilePath in allFiles)
        {
            await DecryptBinFileAsync(outputFolderPath, binFilePath);
        }
    }

    private static string[] GetBinFilePaths(string mainGameFolder, IEnumerable<string> binFileNamesToExtract)
    {
        return binFileNamesToExtract.Select(fileName => Path.Combine(ExtractorUtilities.GetBinFilePath(mainGameFolder), $"{fileName}.bin"))
            .ToArray();
    }

    private static async Task DecryptBinFileAsync(string outputFolderPath, string binFilePath)
    {
        var binFileWoe = Path.GetFileNameWithoutExtension(binFilePath);

        if (binFileWoe.StartsWith("profanity", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var finalJsonPath = Path.Combine(outputFolderPath, binFileWoe + ".json");

        await using var memoryStream = new MemoryStream();
        await BinaryDecrypter.DecryptBinaryFileAsync(binFilePath, memoryStream);
        memoryStream.Position = 0;

        var xmlDocument = new XmlDocument();
        var xmlReaderSettings = new XmlReaderSettings
        {
            IgnoreComments = true
        };
        var xmlReader = XmlReader.Create(memoryStream, xmlReaderSettings);
        xmlDocument.Load(xmlReader);

        await File.WriteAllTextAsync(finalJsonPath, JsonConvert.SerializeXmlNode(xmlDocument, Formatting.Indented, false));
    }
}