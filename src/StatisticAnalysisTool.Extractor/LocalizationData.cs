using System.Text;
using System.Xml;

namespace StatisticAnalysisTool.Extractor;

internal class LocalizationData : IDisposable
{
    public const string ItemPrefix = "@ITEMS_";
    public const string SpellPrefix = "@SPELLS_";
    public const string DescPostfix = "_DESC";

    public Dictionary<string, Dictionary<string, string>> ItemLocalizedNames = new();
    public Dictionary<string, Dictionary<string, string>> ItemLocalizedDescriptions = new();

    public Dictionary<string, Dictionary<string, string>> SpellLocalizedNames = new();
    public Dictionary<string, Dictionary<string, string>> SpellLocalizedDescriptions = new();

    public async Task LoadDataAsync(string mainGameFolder)
    {
        await Task.Run(async () =>
        {
            var localizationBinFilePath = Path.Combine(mainGameFolder, ".\\Albion-Online_Data\\StreamingAssets\\GameData\\localization.bin");
            using var localizationData = await BinaryDecrypter.DecryptAndDecompressAsync(localizationBinFilePath);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(Encoding.UTF8.GetString(localizationData.ToArray()));

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

                if (tuId is null)
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
                
                if (tuId.Value.StartsWith(ItemPrefix))
                {
                    if (tuId.Value.EndsWith(DescPostfix))
                    {
                        ItemLocalizedDescriptions[tuId.Value] = languages;
                    }
                    else
                    {
                        ItemLocalizedNames[tuId.Value] = languages;
                    }
                }
                else if (tuId.Value.StartsWith(SpellPrefix))
                {
                    if (tuId.Value.EndsWith(DescPostfix))
                    {
                        SpellLocalizedDescriptions[tuId.Value] = languages;
                    }
                    else
                    {
                        SpellLocalizedNames[tuId.Value] = languages;
                    }
                }
            }
        });

        GC.Collect();
    }

    public bool IsDataLoaded()
    {
        return ItemLocalizedNames.Count > 0 && ItemLocalizedDescriptions.Count > 0 && SpellLocalizedNames.Count > 0 && SpellLocalizedDescriptions.Count > 0;
    }

    public void Dispose()
    {
        ItemLocalizedNames.Clear();
        ItemLocalizedDescriptions.Clear();
        SpellLocalizedNames.Clear();
        SpellLocalizedDescriptions.Clear();
        GC.SuppressFinalize(this);
    }
}