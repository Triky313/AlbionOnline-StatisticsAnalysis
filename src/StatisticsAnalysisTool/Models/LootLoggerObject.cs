using System;
using System.Globalization;

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
    }
}