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
        public string CsvOutput => $"{UtcPickupTime.ToString("O", CultureInfo.InvariantCulture)};{LootedByAlliance};{LootedByGuild};{LootedByName};{ItemId}" +
                                   $";{UniqueItemName};{Quantity};{LootedFromAlliance};{LootedFromGuild};{LootedFromName}";
        public string CsvOutputWithRealItemName => GetCsvOutputStringWithRealItemName();

        private string GetCsvOutputStringWithRealItemName()
        {
            var item = ItemController.GetItemByUniqueName(UniqueItemName);
            var itemName = (string.IsNullOrEmpty(item?.LocalizedName)) ? UniqueItemName : item.LocalizedName;
            
            return $"{UtcPickupTime.ToString("O", CultureInfo.InvariantCulture)};{LootedByAlliance};{LootedByGuild};{LootedByName};{ItemId}" +
                   $";{itemName.ToString(CultureInfo.InvariantCulture)};{Quantity};{LootedFromAlliance};{LootedFromGuild};{LootedFromName}";
        }
    }
}