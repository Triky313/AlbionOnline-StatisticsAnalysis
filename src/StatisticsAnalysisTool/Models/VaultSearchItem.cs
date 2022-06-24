using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameData;

namespace StatisticsAnalysisTool.Models;

public class VaultSearchItem
{
    public Item Item { get; set; }
    public int Quantity { get; set; }
    public string VaultContainerName { get; set; }
    public string Location { get; set; }
    public string MainLocationIndex { get; set; }
    public MapType MapType { get; set; }
    public string MainLocation => WorldData.GetUniqueNameOrDefault(MainLocationIndex);

    [JsonIgnore]
    public static string TranslationStorageName => LanguageController.Translation("STORAGE_NAME");
    [JsonIgnore]
    public static string TranslationLocation => LanguageController.Translation("LOCATION");
}