using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class FameNotificationFragment : LineFragment
    {
        public FameNotificationFragment(string startText, AttributeStatOperator statOperator, double totalPlayerFame, double totalGainedFame, string fameText, FameTypeOperator fameTypeOperator, double zone, double premium, double satchel, string endText)
        {
            StartText = startText;
            Operator = statOperator;
            TotalPlayerFame = totalPlayerFame;
            TotalGainedFame = totalGainedFame;
            FameText = fameText;
            FameTypeOperator = fameTypeOperator;
            Zone = zone;
            Premium = premium;
            Satchel = satchel;
            EndText = endText;
        }

        public string StartText { get; }
        public AttributeStatOperator Operator { get; }
        public double TotalPlayerFame { get; }
        public double TotalGainedFame { get; }
        public string FameText { get; }
        public FameTypeOperator FameTypeOperator { get; }
        public double Zone { get; }
        public double Premium { get; }
        public double Satchel { get; }
        public string EndText { get; }
    }
}