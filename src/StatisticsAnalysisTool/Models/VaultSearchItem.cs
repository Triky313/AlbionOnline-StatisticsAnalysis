using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class VaultSearchItem
{
    public Item Item { get; set; }
    public int Quantity { get; set; }
    public string VaultContainerName { get; set; }
    public string Location { get; set; }
    public string MainLocationIndex { get; set; }
    public MapType MapType { get; set; } = MapType.Unknown;
    public string MainLocation => WorldData.GetUniqueNameOrDefault(MainLocationIndex);

    [JsonIgnore]
    public string OutputString
    {
        get
        {
            return MapType switch
            {
                MapType.Hideout => $"{MainLocation} ({TranslationHideout}: {Location})",
                MapType.Island => $"{TranslationIsland}: {Location}",
                _ => Location
            };
        }
    }

    [JsonIgnore]
    public bool IsHideout => MapType == MapType.Hideout;
    [JsonIgnore]
    public static string TranslationStorageName => LocalizationController.Translation("STORAGE_NAME");
    [JsonIgnore]
    public static string TranslationLocation => LocalizationController.Translation("LOCATION");
    [JsonIgnore]
    public static string TranslationHideout => LocalizationController.Translation("HIDEOUT");
    [JsonIgnore]
    public static string TranslationIsland => LocalizationController.Translation("ISLAND");
}