using System.Xml;

namespace StatisticAnalysisTool.Extractor;

internal class LocalizationData : IDisposable
{
    public const string ItemPrefix = "@ITEMS_";
    public const string DescPostfix = "_DESC";

    public Dictionary<string, Dictionary<string, string>> ItemLocalizedNames = new();
    public Dictionary<string, Dictionary<string, string>> ItemLocalizedDescriptions = new();

    public Dictionary<string, Dictionary<string, string>> AllLocalized = new(StringComparer.OrdinalIgnoreCase);

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

            var itemLocalizedNames = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            var itemLocalizedDescriptions = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            var allLocalized = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            string? currentTuId = null;
            Dictionary<string, string>? currentLanguages = null;

            while (await reader.ReadAsync())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "tu")
                        {
                            currentTuId = reader.GetAttribute("tuid") ?? string.Empty;
                            currentLanguages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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
                            allLocalized[currentTuId] = currentLanguages;

                            if (currentTuId.StartsWith(ItemPrefix, StringComparison.OrdinalIgnoreCase))
                            {
                                if (currentTuId.EndsWith(DescPostfix, StringComparison.OrdinalIgnoreCase))
                                {
                                    itemLocalizedDescriptions[currentTuId] = currentLanguages;
                                }
                                else
                                {
                                    itemLocalizedNames[currentTuId] = currentLanguages;
                                }
                            }

                            currentTuId = null;
                            currentLanguages = null;
                        }
                        break;
                }
            }

            ItemLocalizedNames = itemLocalizedNames;
            ItemLocalizedDescriptions = itemLocalizedDescriptions;
            AllLocalized = allLocalized;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading localization data: {ex.Message}");
            AllLocalized = new(StringComparer.OrdinalIgnoreCase);
        }
    }

    public bool IsDataLoaded()
    {
        return AllLocalized.Count > 0;
    }

    public void Dispose()
    {
        ItemLocalizedNames.Clear();
        ItemLocalizedDescriptions.Clear();
        AllLocalized.Clear();
        GC.SuppressFinalize(this);
    }
}