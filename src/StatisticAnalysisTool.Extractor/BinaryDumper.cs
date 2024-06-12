using Newtonsoft.Json;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

namespace StatisticAnalysisTool.Extractor;

internal class BinaryDumper
{
    public static async Task ExtractAsJsonAsync(string mainGameFolder, string outputFolderPath, string[] binFileNamesToExtract)
    {
        var allFiles = GetBinFilePaths(mainGameFolder, binFileNamesToExtract);
        var outFiles = (string[]) allFiles.Clone();
        for (var i = 0; i < outFiles.Length; i++)
        {
            outFiles[i] = outFiles[i].Remove(0, outFiles[i].LastIndexOf("GameData\\", StringComparison.Ordinal) + "GameData\\".Length);
        }

        foreach (string binFilePath in allFiles)
        {
            await DecryptBinFileAsJsonAsync(outputFolderPath, binFilePath);
        }
    }

    public static async Task ExtractAsXmlAsync(string mainGameFolder, string outputFolderPath, string[] binFileNamesToExtract)
    {
        var allFiles = GetBinFilePaths(mainGameFolder, binFileNamesToExtract);
        var outFiles = (string[]) allFiles.Clone();
        for (var i = 0; i < outFiles.Length; i++)
        {
            outFiles[i] = outFiles[i].Remove(0, outFiles[i].LastIndexOf("GameData\\", StringComparison.Ordinal) + "GameData\\".Length);
        }

        foreach (string binFilePath in allFiles)
        {
            await DecryptBinFileAsXmlAsync(outputFolderPath, binFilePath);
        }
    }

    private static string[] GetBinFilePaths(string mainGameFolder, IEnumerable<string> binFileNamesToExtract)
    {
        return binFileNamesToExtract.Select(fileName => Path.Combine(ExtractorUtilities.GetBinFilePath(mainGameFolder), $"{fileName}.bin"))
            .ToArray();
    }

    private static async Task DecryptBinFileAsJsonAsync(string outputFolderPath, string binFilePath)
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

    private static async Task DecryptBinFileAsXmlAsync(string outputFolderPath, string binFilePath)
    {
        var binFileWoe = Path.GetFileNameWithoutExtension(binFilePath);

        if (binFileWoe.StartsWith("profanity", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var finalXmlPath = Path.Combine(outputFolderPath, binFileWoe + ".xml");

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

        await using var writer = XmlWriter.Create(finalXmlPath, new XmlWriterSettings
        {
            Indent = true,
            Async = true
        });

        xmlDocument.Save(writer);
    }
}