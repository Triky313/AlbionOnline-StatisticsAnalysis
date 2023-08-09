using System.Text;
using System.Text.Json;
using System.Xml;

namespace StatisticAnalysisTool.Extractor.Utilities;

public class ItemData : IDisposable
{
    public static async Task CreateItemDataAsync(string mainGameFolder, LocalizationData localizationData, string outputFolderPath, string outputFileNameWithExtension = "items.json")
    {
        var itemBinPath = Path.Combine(mainGameFolder, ".\\Albion-Online_Data\\StreamingAssets\\GameData\\items.bin");
        var itemDataByteArray = await BinaryDecrypter.DecryptAndDecompressAsync(itemBinPath);

        ExtractFromByteArray(itemDataByteArray, GetExportStream(outputFolderPath, outputFileNameWithExtension), localizationData);
    }

    private static Stream GetExportStream(string outputFolderPath, string outputFileNameWithExtension)
    {
        var stream = File.Create(Path.Combine(outputFolderPath, outputFileNameWithExtension));
        WriteString(stream, "[" + Environment.NewLine);
        return stream;
    }

    private static void ExtractFromByteArray(byte[] itemDataByteArray, Stream outputStream, LocalizationData localizationData)
    {
        var journals = new List<IdContainer>();

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(RemoveNonPrintableCharacters(Encoding.UTF8.GetString(RemoveBom(itemDataByteArray))));

        var rootNode = xmlDoc.LastChild;

        var index = 1;
        var first = true;

        if (rootNode?.ChildNodes != null)
        {
            foreach (XmlNode node in rootNode.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element || string.IsNullOrEmpty(node.Attributes?["uniquename"]?.Value))
                {
                    continue;
                }

                var uniqueName = node.Attributes["uniquename"]?.Value;
                var enchantmentLevel = node.Attributes["enchantmentlevel"];
                var description = node.Attributes["descriptionlocatag"];
                var name = node.Attributes["descvariable0"];
                var enchantment = "";
                if (enchantmentLevel != null && enchantmentLevel.Value != "0")
                {
                    enchantment = "@" + enchantmentLevel.Value;
                }

                var localizationNameVariable = name != null ? name.Value : LocalizationData.ItemPrefix + uniqueName;
                if (uniqueName != null && uniqueName.Contains("ARTEFACT"))
                {
                    localizationNameVariable = LocalizationData.ItemPrefix + uniqueName;
                }

                var container = new ItemContainer()
                {
                    Index = index.ToString(),
                    UniqueName = uniqueName + enchantment,
                    LocalizationDescriptionVariable = description != null
                        ? description.Value
                        : LocalizationData.ItemPrefix + uniqueName + LocalizationData.DescPostfix,
                    LocalizationNameVariable = localizationNameVariable
                };

                SetLocalization(localizationData, container);
                WriteItem(outputStream, container, first);
                if (first)
                {
                    first = false;
                }

                index++;

                if (node.Name == "journalitem")
                {
                    journals.Add(new ItemContainer()
                    {
                        UniqueName = uniqueName ?? string.Empty
                    });
                }

                var element = FindElement(node, "enchantments");
                if (element == null)
                {
                    continue;
                }

                foreach (XmlElement childNode in element.ChildNodes)
                {
                    var enchantmentName = node.Attributes["uniquename"]?.Value + "@" +
                                          childNode.Attributes["enchantmentlevel"]?.Value;
                    container = new ItemContainer()
                    {
                        Index = index.ToString(),
                        UniqueName = enchantmentName,
                        LocalizationDescriptionVariable = description != null
                            ? description.Value
                            : LocalizationData.ItemPrefix + uniqueName + LocalizationData.DescPostfix,
                        LocalizationNameVariable = name != null ? name.Value : LocalizationData.ItemPrefix + uniqueName
                    };
                    SetLocalization(localizationData, container);
                    WriteItem(outputStream, container);

                    index++;
                }
            }
        }

        foreach (var idContainer in journals)
        {
            var itemContainer = (ItemContainer) idContainer;
            var container = new ItemContainer()
            {
                Index = index.ToString(),
                UniqueName = itemContainer.UniqueName + "_EMPTY",
                LocalizationDescriptionVariable = LocalizationData.ItemPrefix + itemContainer.UniqueName + "_EMPTY" + LocalizationData.DescPostfix,
                LocalizationNameVariable = LocalizationData.ItemPrefix + itemContainer.UniqueName + "_EMPTY"
            };

            SetLocalization(localizationData, container);
            WriteItem(outputStream, container);
            index++;

            container = new ItemContainer()
            {
                Index = index.ToString(),
                UniqueName = itemContainer.UniqueName + "_FULL",
                LocalizationDescriptionVariable = LocalizationData.ItemPrefix + itemContainer.UniqueName + "_FULL" + LocalizationData.DescPostfix,
                LocalizationNameVariable = LocalizationData.ItemPrefix + itemContainer.UniqueName + "_FULL"
            };

            SetLocalization(localizationData, container);
            WriteItem(outputStream, container);
            index++;
        }

        WriteString(outputStream, Environment.NewLine + "]");
        outputStream.Close();
    }

    private static void SetLocalization(LocalizationData data, ItemContainer item)
    {
        if (data.LocalizedDescriptions.TryGetValue(item.LocalizationDescriptionVariable, out var descriptions))
        {
            item.LocalizedDescriptions = descriptions;
        }

        if (data.LocalizedNames.TryGetValue(item.LocalizationNameVariable, out var names))
        {
            item.LocalizedNames = names;
        }
    }

    private static XmlElement? FindElement(XmlNode node, string elementName)
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

    private static void WriteItem(Stream stream, IdContainer idContainer, bool first = false)
    {
        var output = new StringBuilder();

        if (!first)
        {
            output.AppendLine(",");
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        if (idContainer is ItemContainer itemContainer)
        {
            output.Append(JsonSerializer.Serialize(itemContainer, options));
        }
        else
        {
            output.Append(JsonSerializer.Serialize(idContainer, options));
        }

        WriteString(stream, output.ToString());
        output.Clear();
    }

    private static void WriteString(Stream stream, string val)
    {
        var buffer = Encoding.UTF8.GetBytes(val);
        stream.Write(buffer, 0, buffer.Length);
    }

    private static string RemoveNonPrintableCharacters(string input)
    {
        return new string(input.Where(c => !char.IsControl(c) || char.IsWhiteSpace(c)).ToArray());
    }

    private static byte[] RemoveBom(byte[] byteArray)
    {
        byte[] utf8Bom = { 0xEF, 0xBB, 0xBF };

        if (byteArray.Length >= utf8Bom.Length && byteArray[0] == utf8Bom[0] && byteArray[1] == utf8Bom[1] && byteArray[2] == utf8Bom[2])
        {
            return byteArray.Skip(utf8Bom.Length).ToArray();
        }

        return byteArray;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    ~ItemData()
    {
        Dispose();
    }
}