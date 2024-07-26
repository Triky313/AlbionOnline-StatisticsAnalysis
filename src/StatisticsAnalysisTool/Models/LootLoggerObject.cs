using System;
using System.Globalization;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.EstimatedMarketValue;

namespace StatisticsAnalysisTool.Models;

public class LootLoggerObject
{
    public LootLoggerObject()
    {
        UtcPickupTime = DateTime.UtcNow;
    }

    public int ItemId { get; set; }
    public string UniqueItemName { get; set; }
    public DateTime UtcPickupTime { get; }
    public int Quantity { get; set; }
    public string LootedFromName { get; set; }
    public string LootedFromGuild { get; set; }
    public string LootedFromAlliance { get; set; }
    public string LootedByName { get; set; }
    public string LootedByGuild { get; set; }
    public string LootedByAlliance { get; set; }
    public long AverageEstMarketValue { get; set; }

    public string CsvOutput => GetCsvOutputStringWithRealItemName();

    // CSV Format for https://matheus.sampaio.us/ao-loot-logger-viewer/
    // 'UtcPickupTime'       (ISO8601 format, example: `2019-09-07T-15:50+00`)
    // 'LootedByAlliance'    (can be empty),
    // 'LootedByGuild'       (can be empty),
    // 'LootedByName',
    // 'UniqueItemName'      (example `T8_SHOES_LEATHER_ROYAL`),
    // 'itemName'            (example `Ancient Royal Shoes`),
    // 'Quantity',
    // 'LootedFromAlliance'  (can be empty),
    // 'LootedFromGuild'     (can be empty),
    // 'LootedFromName'
    private string GetCsvOutputStringWithRealItemName()
    {
        var item = ItemController.GetItemByUniqueName(UniqueItemName);
        var itemName = (string.IsNullOrEmpty(item?.LocalizedName)) ? UniqueItemName : item.LocalizedName;
            
        return $"{UtcPickupTime.ToString("O", CultureInfo.InvariantCulture)};{LootedByAlliance};{LootedByGuild};{LootedByName};{UniqueItemName};{itemName.ToString(CultureInfo.InvariantCulture)}" +
               $";{Quantity};{LootedFromAlliance};{LootedFromGuild};{LootedFromName}";
    }
}