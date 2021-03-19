using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class FactionNotificationFragment : LineFragment
    {
        //LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, CurrentFactionID, GainedFractionPoints, LanguageController.Translation("FAME"), FameTypeOperator.Pve, LanguageController.Translation("GAINED")
        public FactionNotificationFragment(string startText, AttributeStatOperator statOperator, double currentFactionID, double gainedFractionPoints, double bonusPremiumGainedFractionPoints, string fameText, FameTypeOperator fameTypeOperator, string endText)
        {
            StartText = startText;
            Operator = statOperator;
            CurrentFactionID = currentFactionID;
            GainedFractionPoints = gainedFractionPoints;
            BonusPremiumGainedFractionPoints = bonusPremiumGainedFractionPoints;
            FameText = fameText;
            FameTypeOperator = fameTypeOperator;
            EndText = endText;
        }

        public string StartText { get; }
        public AttributeStatOperator Operator { get; }
        public double CurrentFactionID { get; }
        public double GainedFractionPoints { get; }
        public double BonusPremiumGainedFractionPoints { get; }
        public string FameText { get; }
        public FameTypeOperator FameTypeOperator { get; }
        public string EndText { get; }
    }
}