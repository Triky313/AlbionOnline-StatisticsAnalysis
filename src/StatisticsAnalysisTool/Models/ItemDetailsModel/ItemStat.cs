using FontAwesome5;

namespace StatisticsAnalysisTool.Models.ItemDetailsModel;

public sealed class ItemStat
{
    public ItemStat(EFontAwesomeIcon icon, string name, double value, double maximum, string valueText)
    {
        Icon = icon;
        Name = name;
        Value = value;
        Maximum = maximum;
        ValueText = valueText;
    }

    public EFontAwesomeIcon Icon { get; }
    public string Name { get; }
    public double Value { get; }
    public double Maximum { get; }
    public string ValueText { get; }
}