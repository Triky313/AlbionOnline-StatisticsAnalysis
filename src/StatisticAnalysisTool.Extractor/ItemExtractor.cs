using StatisticAnalysisTool.Extractor.Utilities;
using System.Xml;

namespace StatisticAnalysisTool.Extractor;

public class ItemExtractor : BaseExtractor
{
    public ItemExtractor(string mainGameFolder, string outputFolderPath) : base(mainGameFolder, outputFolderPath)
    {
    }

    protected override void ExtractFromXml(Stream inputXmlFile, Stream outputStream, Action<Stream, IdContainer, bool> writeItem, LocalizationData localizationData)
    {
        var journals = new List<IdContainer>();
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(inputXmlFile);

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
                writeItem(outputStream, container, first);
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
                    writeItem(outputStream, container, false);

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
            writeItem(outputStream, container, false);
            index++;

            container = new ItemContainer()
            {
                Index = index.ToString(),
                UniqueName = itemContainer.UniqueName + "_FULL",
                LocalizationDescriptionVariable = LocalizationData.ItemPrefix + itemContainer.UniqueName + "_FULL" + LocalizationData.DescPostfix,
                LocalizationNameVariable = LocalizationData.ItemPrefix + itemContainer.UniqueName + "_FULL"
            };

            SetLocalization(localizationData, container);
            writeItem(outputStream, container, false);
            index++;
        }
    }

    private void SetLocalization(LocalizationData data, ItemContainer item)
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

    protected override string GetBinFilePath()
    {
        return Path.Combine(MainGameFolder, ".\\Albion-Online_Data\\StreamingAssets\\GameData\\items.bin");
    }
}