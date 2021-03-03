using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DamageObject
    {
        public DateTime TimeStamp { get; }
        public long CauserId { get; }
        public long? TargetId { get; }
        public string CauserName { get; set; }
        public long Damage { get; set; }

        public DamageObject(long causerId, long? targetId, string causerName, long damage)
        {
            TimeStamp = DateTime.UtcNow;
            CauserId = causerId;
            TargetId = targetId;
            CauserName = causerName;
            Damage = damage;
        }
    }
}