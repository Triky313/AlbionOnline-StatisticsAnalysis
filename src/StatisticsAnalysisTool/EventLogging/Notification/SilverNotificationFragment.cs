using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.EventLogging.Notification;

public class SilverNotificationFragment : LineFragment
{
    public SilverNotificationFragment(string startText, AttributeStatOperator statOperator, FixPoint totalGainedSilver,
        string silverText, FixPoint clusterBonus, FixPoint premiumBonus, FixPoint clusterTax, string endText)
    {
        StartText = startText;
        Operator = statOperator;
        TotalGainedSilver = totalGainedSilver.DoubleValue;
        SilverText = silverText;
        ClusterBonus = clusterBonus.DoubleValue;
        PremiumBonus = premiumBonus.DoubleValue;
        ClusterTax = clusterTax.DoubleValue;
        EndText = endText;
    }

    public string StartText { get; }
    public AttributeStatOperator Operator { get; }
    public double TotalGainedSilver { get; }
    public string SilverText { get; }
    public double ClusterBonus { get; }
    public double PremiumBonus { get; }
    public double ClusterTax { get; }
    public string EndText { get; }
}