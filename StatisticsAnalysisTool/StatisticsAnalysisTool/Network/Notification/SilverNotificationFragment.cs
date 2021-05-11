using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class SilverNotificationFragment : LineFragment
    {
        public SilverNotificationFragment(string startText, AttributeStatOperator statOperator, double totalGainedSilver,
            string silverText, double clusterBonus, double premiumBonus, string endText)
        {
            StartText = startText;
            Operator = statOperator;
            TotalGainedSilver = totalGainedSilver;
            SilverText = silverText;
            ClusterBonus = clusterBonus;
            PremiumBonus = premiumBonus;
            EndText = endText;
        }

        public string StartText { get; }
        public AttributeStatOperator Operator { get; }
        public double TotalGainedSilver { get; }
        public string SilverText { get; }
        public double ClusterBonus { get; }
        public double PremiumBonus { get; }
        public string EndText { get; }
    }
}