using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DamageObject
    {
        public DateTime TimeStamp { get; }
        public long CauserId { get; }
        public long TargetId { get; }
        public string Name { get; }
        public long Damage { get; }

        public DamageObject(long causerId, long targetId, string name, long damage)
        {
            TimeStamp = DateTime.UtcNow;
            CauserId = causerId;
            TargetId = targetId;
            Name = name;
            Damage = damage;
        }
    }
}