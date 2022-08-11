using System;
using System.Globalization;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models
{
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

        // CSV Format for https://matheus.sampaio.us/ao-loot-logger-viewer/
        // 'timestamp_utc'          (ISO8601 format, example: `2019-09-07T-15:50+00`)
        // 'looted_by__alliance'    (can be empty),
        // 'looted_by__guild'       (can be empty),
        // 'looted_by__name',
        // 'item_id'                (example `T8_SHOES_LEATHER_ROYAL`),
        // 'item_name'              (example `Ancient Royal Shoes`),
        // 'quantity',
        // 'looted_from__alliance'  (can be empty),
        // 'looted_from__guild'     (can be empty),
        // 'looted_from__name'
        public string CsvOutput => $"{UtcPickupTime.ToString("O", CultureInfo.InvariantCulture)};{LootedByAlliance};{LootedByGuild};{LootedByName};{ItemId};{UniqueItemName};{Quantity}" +
                                   $";{LootedFromAlliance};{LootedFromGuild};{LootedFromName}";
        public string CsvOutputWithRealItemName => GetCsvOutputStringWithRealItemName();

        private string GetCsvOutputStringWithRealItemName()
        {
            var item = ItemController.GetItemByUniqueName(UniqueItemName);
            var itemName = (string.IsNullOrEmpty(item?.LocalizedName)) ? UniqueItemName : item.LocalizedName;
            
            return $"{UtcPickupTime.ToString("O", CultureInfo.InvariantCulture)};{LootedByAlliance};{LootedByGuild};{LootedByName};{ItemId};{itemName.ToString(CultureInfo.InvariantCulture)}" +
                   $";{Quantity};{LootedFromAlliance};{LootedFromGuild};{LootedFromName}";
        }
    }
}