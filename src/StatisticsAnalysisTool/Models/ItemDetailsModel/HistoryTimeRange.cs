namespace StatisticsAnalysisTool.Models.ItemDetailsModel;

public class HistoryTimeRange
{
    public HistoryTimeRange(string name, int days)
    {
        Name = name;
        Days = days;
    }

    public string Name { get; }
    public int Days { get; }
}