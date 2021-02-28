using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DamageObject
    {
        public DateTime TimeStamp { get; }
        public long CauserId { get; }
        public long TargetId { get; }
        public long Damage { get; }

        public DamageObject(long causerId, long targetId, long damage)
        {
            TimeStamp = DateTime.UtcNow;
            CauserId = causerId;
            TargetId = targetId;
            Damage = damage;
        }
    }
}