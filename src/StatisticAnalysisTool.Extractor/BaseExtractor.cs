using System.Diagnostics;
using StatisticAnalysisTool.Extractor.Utilities;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace StatisticAnalysisTool.Extractor;

public abstract class BaseExtractor
{
    protected readonly string OutputFolderPath;
    protected readonly string MainGameFolder;

    protected BaseExtractor(string mainGameFolder, string outputFolderPath)
    {
        OutputFolderPath = outputFolderPath;
        MainGameFolder = mainGameFolder;
    }

    protected abstract string GetBinFilePath();
    protected abstract void ExtractFromXml(Stream inputXmlFile, Stream outputStream, Action<Stream, IdContainer, bool> writeItem, LocalizationData localizationData);

    protected static XmlElement? FindElement(XmlNode node, string elementName)
    {
        foreach (XmlNode childNode in node.ChildNodes)
        {
            if (childNode is XmlElement ele && ele.Name == elementName)
            {
                return ele;
            }
        }

        return null;
    }

    public void Extract(LocalizationData localizationData = default)
    {
        var xmlPath = DecryptBinFile(GetBinFilePath(), OutputFolderPath);
        Debug.Print("Attribute of the File " + OutputFolderPath);
        try
        {
            using var inputFile = File.OpenRead(xmlPath);
            var stream = GetExportStream();

            ExtractFromXml(inputFile, stream, WriteItem, localizationData);

            CloseExportStream(stream);
            stream.Close();
        }
        catch
        {
            throw new ArgumentException();
        }
    }

    public static string DecryptBinFile(string binFile, string outputFolderPath)
    {
        var finalOutPath = Path.ChangeExtension(Path.Combine(outputFolderPath, binFile[(binFile.LastIndexOf("GameData\\", StringComparison.Ordinal) + 9)..]), ".xml");
        Directory.CreateDirectory(Path.GetDirectoryName(finalOutPath) ?? string.Empty);

        using var outputStream = File.Create(finalOutPath);
        BinaryDecrypter.DecryptBinaryFile(binFile, outputStream);
        return finalOutPath;
    }
    
    private Stream GetExportStream()
    {
        var filePathWithoutExtension = Path.Combine(OutputFolderPath, "formatted", Path.GetFileNameWithoutExtension(GetBinFilePath()));
        if (!Directory.Exists(Path.GetDirectoryName(filePathWithoutExtension)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePathWithoutExtension) ?? string.Empty);
        }

        var stream = File.Create(filePathWithoutExtension + ".json");
        WriteString(stream, "[" + Environment.NewLine);
        return stream;
    }

    private static void CloseExportStream(Stream stream)
    {
        WriteString(stream, Environment.NewLine + "]");
    }

    protected static void WriteItem(Stream stream, IdContainer idContainer, bool first = false)
    {
        var output = new StringBuilder();

        if (!first)
        {
            output.AppendLine(",");
        }
        output.Append(JsonSerializer.Serialize(idContainer, new JsonSerializerOptions { WriteIndented = true }));

        WriteString(stream, output.ToString());
        output.Clear();
    }

    protected static void WriteString(Stream stream, string val)
    {
        var buffer = Encoding.UTF8.GetBytes(val);
        stream.Write(buffer, 0, buffer.Length);
    }
}