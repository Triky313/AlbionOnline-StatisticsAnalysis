namespace StatisticsAnalysisTool.Network.Notification
{
    public class CombatNotificationFragment : LineFragment
    {
        public CombatNotificationFragment(string causerName, string targetName, double damage)
        {
            CauserName = causerName;
            TargetName = targetName;
            Damage = damage;
        }

        public string CauserName { get; }
        public string TargetName { get; }
        public double Damage { get; }
    }
}