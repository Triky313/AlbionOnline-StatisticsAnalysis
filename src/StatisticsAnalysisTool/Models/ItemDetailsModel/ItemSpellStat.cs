namespace StatisticsAnalysisTool.Models.ItemDetailsModel;

public sealed class ItemSpellStat
{
    public ItemSpellStat(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }
    public string Value { get; }
}