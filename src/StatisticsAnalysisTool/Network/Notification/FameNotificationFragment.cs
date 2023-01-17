using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Notification;

public class FameNotificationFragment : LineFragment
{
    public FameNotificationFragment(string startText, AttributeStatOperator statOperator, double totalPlayerFame, double totalGainedFame,
        string fameText, PvpPveType pvpPveType, double zone, double premium, double satchel, bool isBonusFactorActive, double bonusFactorInPercent, string endText)
    {
        StartText = startText;
        Operator = statOperator;
        TotalPlayerFame = totalPlayerFame;
        TotalGainedFame = totalGainedFame;
        FameText = fameText;
        PvpPveType = pvpPveType;
        Zone = zone;
        Premium = premium;
        Satchel = satchel;
        IsBonusFactorActive = isBonusFactorActive;
        BonusFactorInPercent = bonusFactorInPercent;
        EndText = endText;
    }

    public string StartText { get; }
    public AttributeStatOperator Operator { get; }
    public double TotalPlayerFame { get; }
    public double TotalGainedFame { get; }
    public string FameText { get; }
    public PvpPveType PvpPveType { get; }
    public double Zone { get; }
    public double Premium { get; }
    public double Satchel { get; }
    public bool IsBonusFactorActive { get; }
    public double BonusFactorInPercent { get; }
    public string EndText { get; }
}