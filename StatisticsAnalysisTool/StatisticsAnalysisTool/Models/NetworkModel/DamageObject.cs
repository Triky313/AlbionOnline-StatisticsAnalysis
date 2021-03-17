using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DamageObject
    {
        public DateTime StartTime { get; set; }
        public Guid CauserGuid { get; }
        public string CauserName { get; set; }
        public int MainHandItemIndex { get; set; }
        public long Damage { get; set; }

        public DamageObject(Guid causerGuid, string causerName, int mainHandItemIndex, long damage)
        {
            StartTime = DateTime.UtcNow;
            CauserGuid = causerGuid;
            CauserName = causerName;
            MainHandItemIndex = mainHandItemIndex;
            Damage = damage;
        }
    }
}