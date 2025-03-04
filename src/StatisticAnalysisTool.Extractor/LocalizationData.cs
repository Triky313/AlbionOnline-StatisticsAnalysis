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
        var localizationBinFilePath = Path.Combine(mainGameFolder, ".\\Albion-Online_Data\\StreamingAssets\\GameData\\localization.bin");

        using var localizationData = await BinaryDecrypter.DecryptAndDecompressAsync(localizationBinFilePath);

        using var reader = XmlReader.Create(new MemoryStream(localizationData.ToArray()), new XmlReaderSettings
        {
            Async = true
        });

        var itemLocalizedNames = new Dictionary<string, Dictionary<string, string>>();
        var itemLocalizedDescriptions = new Dictionary<string, Dictionary<string, string>>();
        var spellLocalizedNames = new Dictionary<string, Dictionary<string, string>>();
        var spellLocalizedDescriptions = new Dictionary<string, Dictionary<string, string>>();

        while (await reader.ReadAsync())
        {
            if (reader is { NodeType: XmlNodeType.Element, Name: "node" })
            {
                var tuId = reader.GetAttribute("tuid");

                if (string.IsNullOrEmpty(tuId))
                {
                    continue;
                }

                var languages = new Dictionary<string, string>();

                using (var subtree = reader.ReadSubtree())
                {
                    while (await subtree.ReadAsync())
                    {
                        if (subtree is { NodeType: XmlNodeType.Element, Name: "value" })
                        {
                            var lang = subtree.GetAttribute("xml:lang");
                            var text = await subtree.ReadElementContentAsStringAsync();

                            if (!string.IsNullOrEmpty(lang))
                            {
                                languages[lang] = text;
                            }
                        }
                    }
                }

                if (tuId.StartsWith(ItemPrefix))
                {
                    if (tuId.EndsWith(DescPostfix))
                    {
                        itemLocalizedDescriptions[tuId] = languages;
                    }
                    else
                    {
                        itemLocalizedNames[tuId] = languages;
                    }
                }
                else if (tuId.StartsWith(SpellPrefix))
                {
                    if (tuId.EndsWith(DescPostfix))
                    {
                        spellLocalizedDescriptions[tuId] = languages;
                    }
                    else
                    {
                        spellLocalizedNames[tuId] = languages;
                    }
                }
            }
        }

        ItemLocalizedNames = itemLocalizedNames;
        ItemLocalizedDescriptions = itemLocalizedDescriptions;
        SpellLocalizedNames = spellLocalizedNames;
        SpellLocalizedDescriptions = spellLocalizedDescriptions;
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