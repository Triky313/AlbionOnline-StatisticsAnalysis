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

        try
        {
            using var localizationData = await BinaryDecrypter.DecryptAndDecompressAsync(localizationBinFilePath);

            using var reader = XmlReader.Create(new MemoryStream(localizationData.ToArray()), new XmlReaderSettings
            {
                Async = true,
                IgnoreWhitespace = true
            });

            var itemLocalizedNames = new Dictionary<string, Dictionary<string, string>>();
            var itemLocalizedDescriptions = new Dictionary<string, Dictionary<string, string>>();
            var spellLocalizedNames = new Dictionary<string, Dictionary<string, string>>();
            var spellLocalizedDescriptions = new Dictionary<string, Dictionary<string, string>>();

            string? currentTuId = null;
            Dictionary<string, string> currentLanguages = null!;

            while (await reader.ReadAsync())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "tu")
                        {
                            currentTuId = reader.GetAttribute("tuid") ?? string.Empty;
                            currentLanguages = new Dictionary<string, string>();
                        }
                        else if (reader.Name == "tuv" && currentTuId != null)
                        {
                            var lang = reader.GetAttribute("xml:lang");
                            if (!string.IsNullOrEmpty(lang) && await reader.ReadAsync() && reader.Name == "seg")
                            {
                                var text = await reader.ReadElementContentAsStringAsync();
                                currentLanguages![lang] = text;
                            }
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == "tu" && currentTuId != null && currentLanguages != null)
                        {
                            if (currentTuId.StartsWith(ItemPrefix))
                            {
                                if (currentTuId.EndsWith(DescPostfix))
                                {
                                    itemLocalizedDescriptions[currentTuId] = currentLanguages;
                                }
                                else
                                {
                                    itemLocalizedNames[currentTuId] = currentLanguages;
                                }
                            }
                            else if (currentTuId.StartsWith(SpellPrefix))
                            {
                                if (currentTuId.EndsWith(DescPostfix))
                                {
                                    spellLocalizedDescriptions[currentTuId] = currentLanguages;
                                }
                                else
                                {
                                    spellLocalizedNames[currentTuId] = currentLanguages;
                                }
                            }

                            currentTuId = null;
                            currentLanguages = null!;
                        }
                        break;
                }
            }

            ItemLocalizedNames = itemLocalizedNames;
            ItemLocalizedDescriptions = itemLocalizedDescriptions;
            SpellLocalizedNames = spellLocalizedNames;
            SpellLocalizedDescriptions = spellLocalizedDescriptions;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading localization data: {ex.Message}");
        }
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