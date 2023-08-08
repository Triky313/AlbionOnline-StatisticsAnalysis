using System.Text;
using System.Xml;

namespace StatisticAnalysisTool.Extractor.Utilities;

public class LocalizationData : IDisposable
{
    public const string ItemPrefix = "@ITEMS_";
    public const string DescPostfix = "_DESC";

    public Dictionary<string, Dictionary<string, string>> LocalizedNames = new();
    public Dictionary<string, Dictionary<string, string>> LocalizedDescriptions = new();

    public LocalizationData(string mainGameFolder)
    {
        var localizationBinFilePath = Path.Combine(mainGameFolder, ".\\Albion-Online_Data\\StreamingAssets\\GameData\\localization.bin");
        var localizationDataByteArray = BinaryDecrypter.DecryptAndDecompress(localizationBinFilePath);

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(Encoding.UTF8.GetString(localizationDataByteArray));

        var rootNode = xmlDoc.LastChild?.LastChild?.ChildNodes;

        if (rootNode is null)
        {
            return;
        }

        foreach (XmlNode node in rootNode)
        {
            if (node.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            var tuId = node.Attributes?["tuid"];

            if (tuId?.Value.StartsWith(ItemPrefix) != true)
            {
                continue;
            }

            Dictionary<string, string> languages;

            try
            {
                languages = node.ChildNodes
                    .Cast<XmlNode>()
                    .ToDictionary(x => x.Attributes!["xml:lang"]!.Value, y => y.LastChild!.InnerText);
            }
            catch (Exception)
            {
                continue;
            }

            // Is item description
            if (tuId.Value.EndsWith(DescPostfix))
            {
                LocalizedDescriptions[tuId.Value] = languages;
            }
            // Is item name
            else
            {
                LocalizedNames[tuId.Value] = languages;
            }
        }
    }

    public void Dispose()
    {
        LocalizedNames.Clear();
        LocalizedDescriptions.Clear();
        GC.SuppressFinalize(this);
    }

    ~LocalizationData()
    {
        Dispose();
    }
}