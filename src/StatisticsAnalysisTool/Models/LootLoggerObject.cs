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

        public string UniqueName { get; set; }
        public DateTime UtcPickupTime { get; }
        public int Quantity { get; set; }
        public string BodyName { get; set; }
        public string LooterName { get; set; }
        public string CsvOutput => $"{UtcPickupTime.ToString("O", CultureInfo.InvariantCulture)};{LooterName};{UniqueName};{Quantity};{BodyName}";
        public string CsvOutputWithRealItemName => GetCsvOutputStringWithRealItemName();

        private string GetCsvOutputStringWithRealItemName()
        {
            var item = ItemController.GetItemByUniqueName(UniqueName);
            var itemName = (string.IsNullOrEmpty(item?.LocalizedName)) ? UniqueName : item.LocalizedName;

            return $"{UtcPickupTime.ToString("O", CultureInfo.InvariantCulture)};{LooterName};{itemName.ToString(CultureInfo.InvariantCulture)};{Quantity};{BodyName}";
        }
    }
}