namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DamageObject
    {
        public long CauserId { get; }
        public long TargetId { get; }
        public long Damage { get; }

        public DamageObject(long causerId, long targetId, long damage)
        {
            CauserId = causerId;
            TargetId = targetId;
            Damage = damage;
        }
    }
}