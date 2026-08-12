using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using System;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Windows;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class Loot
{
    public string UniqueName { get; set; }
    public DateTime UtcDiscoveryTime { get; set; }
    public int Quantity { get; set; }
    public long EstimatedMarketValueInternal { get; set; }
    public long SourceObjectId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public DungeonLootSourceType SourceType { get; set; }
    [JsonIgnore]
    public Item Item => ItemController.GetItemByUniqueName(UniqueName);
    [JsonIgnore]
    public FixPoint EstimatedMarketValue => FixPoint.FromInternalValue(EstimatedMarketValueInternal);
    [JsonIgnore]
    public string Hash => $"{UniqueName}{UtcDiscoveryTime.Ticks}{Quantity}{EstimatedMarketValueInternal}{SourceObjectId}{SourceType}";
    [JsonIgnore]
    public Visibility SourceVisibility => SourceType == DungeonLootSourceType.Unknown ? Visibility.Collapsed : Visibility.Visible;
    [JsonIgnore]
    public string SourceTypeText => SourceType switch
    {
        DungeonLootSourceType.Chest => LocalizationController.Translation("LOOT_SOURCE_CHEST"),
        DungeonLootSourceType.Mob => LocalizationController.Translation("LOOT_SOURCE_MOB"),
        DungeonLootSourceType.Player => LocalizationController.Translation("LOOT_SOURCE_PLAYER"),
        _ => LocalizationController.Translation("LOOT_SOURCE_UNKNOWN")
    };
    [JsonIgnore]
    public string SourceReferenceText
    {
        get
        {
            var reference = SourceObjectId > 0
                ? string.Format(CultureInfo.CurrentCulture, "{0} #{1}", SourceTypeText, SourceObjectId)
                : SourceTypeText;
            return string.IsNullOrWhiteSpace(SourceName) ? reference : $"{reference} - {SourceName}";
        }
    }
}