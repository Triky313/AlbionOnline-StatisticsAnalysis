using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Notification;

public class FactionPointsNotificationFragment : LineFragment
{
    public FactionPointsNotificationFragment(string startText, AttributeStatOperator statOperator, CityFaction cityFaction, double gainedFractionPoints, double bonusPremiumGainedFractionPoints, string valueText, string endText)
    {
        StartText = startText;
        Operator = statOperator;
        CityFaction = cityFaction;
        GainedFractionPoints = gainedFractionPoints;
        BonusPremiumGainedFractionPoints = bonusPremiumGainedFractionPoints;
        ValueText = valueText;
        EndText = endText;
    }

    public string StartText { get; }
    public AttributeStatOperator Operator { get; }
    public CityFaction CityFaction { get; }
    public double GainedFractionPoints { get; }
    public double BonusPremiumGainedFractionPoints { get; }
    public string ValueText { get; }
    public string EndText { get; }
}