using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.EventLogging.Notification;

public class SeasonPointsNotificationFragment : LineFragment
{
    public SeasonPointsNotificationFragment(string startText, AttributeStatOperator statOperator, int gainedSeasonPoints, string valueText, string endText)
    {
        StartText = startText;
        Operator = statOperator;
        GainedSeasonPoints = gainedSeasonPoints;
        ValueText = valueText;
        EndText = endText;
    }

    public string StartText { get; }
    public AttributeStatOperator Operator { get; }
    public int GainedSeasonPoints { get; }
    public string ValueText { get; }
    public string EndText { get; }
}