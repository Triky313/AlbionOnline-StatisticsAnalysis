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
        public double Dps { get; set; }

        public DamageObject(DateTime startTime, Guid causerGuid, string causerName, int mainHandItemIndex, long damage, double dps)
        {
            StartTime = startTime;
            CauserGuid = causerGuid;
            CauserName = causerName;
            MainHandItemIndex = mainHandItemIndex;
            Damage = damage;
            Dps = dps;
        }
    }
}