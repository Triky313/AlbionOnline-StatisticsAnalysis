using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DamageObject
    {
        public DateTime TimeStamp { get; }
        public Guid CauserGuid { get; }
        public string CauserName { get; set; }
        public Item CauserMainHand { get; set; }
        public long Damage { get; set; }

        public DamageObject(Guid causerGuid, string causerName, Item causerMainHand, long damage)
        {
            TimeStamp = DateTime.UtcNow;
            CauserGuid = causerGuid;
            CauserName = causerName;
            CauserMainHand = causerMainHand;
            Damage = damage;
        }
    }
}