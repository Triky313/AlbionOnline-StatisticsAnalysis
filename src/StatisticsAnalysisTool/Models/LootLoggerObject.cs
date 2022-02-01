using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models.NetworkModel
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
        public string CsvOutput => $"{UtcPickupTime.ToString("MM/dd/yyyy H:mm:ss", CultureInfo.InvariantCulture)};{LooterName};{UniqueName};{Quantity};{BodyName}";
        public string CsvOutputWithRealItemName => GetCsvOutputStringWithRealItemName();

        private string GetCsvOutputStringWithRealItemName()
        {
            var item = ItemController.GetItemByUniqueName(UniqueName);
            var itemName = (string.IsNullOrEmpty(item?.LocalizedName)) ? UniqueName : item.LocalizedName;
            
            return $"{UtcPickupTime.ToString("MM/dd/yyyy H:mm:ss", CultureInfo.InvariantCulture)};{LooterName};{itemName.ToString(CultureInfo.InvariantCulture)};{Quantity};{BodyName}";
        }
    }
}