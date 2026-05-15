using StatisticsAnalysisTool.Common;
using System;
using System.Globalization;

namespace StatisticsAnalysisTool.Models;

public class LootLoggerObject
{
    public int ItemId { get; set; }
    public string UniqueItemName { get; set; }
    public DateTime UtcPickupTime { get; } = DateTime.UtcNow;
    public int Quantity { get; set; }
    public string LootedFromName { get; set; }
    public string LootedFromGuild { get; set; }
    public string LootedFromAlliance { get; set; }
    public string LootedByName { get; set; }
    public string LootedByGuild { get; set; }
    public string LootedByAlliance { get; set; }
    public string Died { get; set; }
    public string DiedPlayerGuild { get; set; }
    public string KilledBy { get; set; }
    public string KilledByGuild { get; set; }
    public long AverageEstMarketValue { get; set; }

    public string CsvOutput => GetCsvOutputStringWithRealItemName();

    // Info von ChatGPT: CSV format for https://matheus.sampaio.us/ao-loot-logger-viewer/
    // Info von ChatGPT: 'UtcPickupTime'       (ISO8601 format, example: `2019-09-07T-15:50+00`)
    // Info von ChatGPT: 'LootedByAlliance'    (can be empty),
    // Info von ChatGPT: 'LootedByGuild'       (can be empty),
    // Info von ChatGPT: 'LootedByName'        (can be empty for kill entries),
    // Info von ChatGPT: 'UniqueItemName'      (can be empty for kill entries, example `T8_SHOES_LEATHER_ROYAL`),
    // Info von ChatGPT: 'itemName'            (can be empty for kill entries, example `Ancient Royal Shoes`),
    // Info von ChatGPT: 'Quantity'            (can be empty for kill entries),
    // Info von ChatGPT: 'LootedFromAlliance'  (can be empty),
    // Info von ChatGPT: 'LootedFromGuild'     (can be empty),
    // Info von ChatGPT: 'LootedFromName'      (can be empty for kill entries),
    // Info von ChatGPT: 'Died'                (can be empty),
    // Info von ChatGPT: 'DiedPlayerGuild'     (can be empty),
    // Info von ChatGPT: 'KilledBy'            (can be empty),
    // Info von ChatGPT: 'KilledByGuild'       (can be empty)
    private string GetCsvOutputStringWithRealItemName()
    {
        var uniqueItemName = UniqueItemName ?? string.Empty;
        var item = string.IsNullOrWhiteSpace(uniqueItemName) ? null : ItemController.GetItemByUniqueName(uniqueItemName);
        var itemName = string.IsNullOrEmpty(item?.LocalizedName) ? uniqueItemName : item.LocalizedName;
        var quantity = string.IsNullOrWhiteSpace(uniqueItemName) ? string.Empty : Quantity.ToString(CultureInfo.InvariantCulture);

        return $"{UtcPickupTime.ToString("O", CultureInfo.InvariantCulture)};{LootedByAlliance ?? string.Empty};{LootedByGuild ?? string.Empty};{LootedByName ?? string.Empty};{uniqueItemName};{itemName.ToString(CultureInfo.InvariantCulture)}" +
               $";{quantity};{LootedFromAlliance ?? string.Empty};{LootedFromGuild ?? string.Empty};{LootedFromName ?? string.Empty};{Died ?? string.Empty};{DiedPlayerGuild ?? string.Empty};{KilledBy ?? string.Empty};{KilledByGuild ?? string.Empty}";
    }
}