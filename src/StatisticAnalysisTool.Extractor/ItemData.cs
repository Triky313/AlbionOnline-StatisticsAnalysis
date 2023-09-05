using System.Text;
using System.Xml;

namespace StatisticAnalysisTool.Extractor;

internal class ItemData : IDisposable
{
    public static async Task CreateItemDataAsync(string mainGameFolder, LocalizationData localizationData, string outputFolderPath, string outputFileNameWithExtension = "indexedItems.json")
    {
        var itemBinPath = Path.Combine(mainGameFolder, ".\\Albion-Online_Data\\StreamingAssets\\GameData\\items.bin");
        var itemDataByteArray = await BinaryDecrypter.DecryptAndDecompressAsync(itemBinPath);

        ExtractFromByteArray(itemDataByteArray.ToArray(), GetExportStream(outputFolderPath, outputFileNameWithExtension), localizationData);
    }

    private static Stream GetExportStream(string outputFolderPath, string outputFileNameWithExtension)
    {
        var stream = File.Create(Path.Combine(outputFolderPath, outputFileNameWithExtension));
        ExtractorUtilities.WriteString(stream, "[" + Environment.NewLine);
        return stream;
    }

    private static void ExtractFromByteArray(byte[] itemDataByteArray, Stream outputStream, LocalizationData localizationData)
    {
        var journals = new List<IdContainer>();

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(RemoveNonPrintableCharacters(Encoding.UTF8.GetString(RemoveBom(itemDataByteArray))));

        using var childNodes = xmlDoc.LastChild?.ChildNodes;

        var index = 1;
        var first = true;

        if (childNodes != null)
        {
            foreach (XmlNode node in childNodes)
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
                ExtractorUtilities.WriteItem(outputStream, container, first);
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
                    ExtractorUtilities.WriteItem(outputStream, container);

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
            ExtractorUtilities.WriteItem(outputStream, container);
            index++;

            container = new ItemContainer()
            {
                Index = index.ToString(),
                UniqueName = itemContainer.UniqueName + "_FULL",
                LocalizationDescriptionVariable = LocalizationData.ItemPrefix + itemContainer.UniqueName + "_FULL" + LocalizationData.DescPostfix,
                LocalizationNameVariable = LocalizationData.ItemPrefix + itemContainer.UniqueName + "_FULL"
            };

            SetLocalization(localizationData, container);
            ExtractorUtilities.WriteItem(outputStream, container);
            index++;
        }

        ExtractorUtilities.WriteString(outputStream, Environment.NewLine + "]");
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