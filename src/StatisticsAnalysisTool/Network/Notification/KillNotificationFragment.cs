namespace StatisticsAnalysisTool.Network.Notification
{
    public class KillNotificationFragment : LineFragment
    {
        public KillNotificationFragment(string died, string killedBy, string valueText)
        {
            Died = died;
            KilledBy = killedBy;
            ValueText = valueText;
        }

        public string Died { get; }
        public string KilledBy { get; }
        public string ValueText { get; }
    }
}