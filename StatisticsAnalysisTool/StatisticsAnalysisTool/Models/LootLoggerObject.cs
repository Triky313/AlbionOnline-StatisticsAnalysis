using System;

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
        public string CsvOutput => $"{UtcPickupTime:d h:mm:ss tt};{LooterName};{UniqueName};{Quantity};{BodyName}";
    }
}