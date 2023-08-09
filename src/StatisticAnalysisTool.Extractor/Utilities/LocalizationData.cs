using System.Collections.Concurrent;
using System.Text;
using System.Xml;

namespace StatisticAnalysisTool.Extractor.Utilities;

public class LocalizationData : IDisposable
{
    public const string ItemPrefix = "@ITEMS_";
    public const string DescPostfix = "_DESC";

    public ConcurrentDictionary<string, ConcurrentDictionary<string, string>> LocalizedNames = new();
    public ConcurrentDictionary<string, ConcurrentDictionary<string, string>> LocalizedDescriptions = new();

    public async Task LoadDataAsync(string mainGameFolder)
    {
        var localizationBinFilePath = Path.Combine(mainGameFolder, ".\\Albion-Online_Data\\StreamingAssets\\GameData\\localization.bin");
        var localizationDataByteArray = await BinaryDecrypter.DecryptAndDecompressAsync(localizationBinFilePath);

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(Encoding.UTF8.GetString(localizationDataByteArray));

        var rootNode = xmlDoc.LastChild?.LastChild?.ChildNodes;

        if (rootNode is null)
        {
            return;
        }

        await Parallel.ForEachAsync(rootNode.Cast<XmlNode>(), (node, _)  =>
        {
            if (node.NodeType != XmlNodeType.Element)
            {
                return default;
            }

            var tuId = node.Attributes?["tuid"];

            if (tuId?.Value.StartsWith(ItemPrefix) != true)
            {
                return default;
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
                return default;
            }

            // Is item description
            if (tuId.Value.EndsWith(DescPostfix))
            {
                LocalizedDescriptions[tuId.Value] = new ConcurrentDictionary<string, string>(languages);
            }
            // Is item name
            else
            {
                LocalizedNames[tuId.Value] = new ConcurrentDictionary<string, string>(languages); 
            }

            return default;
        });
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