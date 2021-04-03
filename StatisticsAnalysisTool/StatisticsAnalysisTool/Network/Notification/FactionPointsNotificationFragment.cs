using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class FactionPointsNotificationFragment : LineFragment
    {
        public FactionPointsNotificationFragment(string startText, AttributeStatOperator statOperator, CityFaction cityFaction, double gainedFractionPoints, double bonusPremiumGainedFractionPoints, string fameText, FameTypeOperator fameTypeOperator, string endText)
        {
            StartText = startText;
            Operator = statOperator;
            CityFaction = cityFaction;
            GainedFractionPoints = gainedFractionPoints;
            BonusPremiumGainedFractionPoints = bonusPremiumGainedFractionPoints;
            FameText = fameText;
            FameTypeOperator = fameTypeOperator;
            EndText = endText;
        }

        public string StartText { get; }
        public AttributeStatOperator Operator { get; }
        public CityFaction CityFaction { get; }
        public double GainedFractionPoints { get; }
        public double BonusPremiumGainedFractionPoints { get; }
        public string FameText { get; }
        public FameTypeOperator FameTypeOperator { get; }
        public string EndText { get; }
    }
}