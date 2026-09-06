using DSaladin.FontAwesome.WPF;

namespace StatisticsAnalysisTool.Models.ItemDetailsModel;

public sealed class ItemStat
{
    public ItemStat(FontAwesomeIcon icon, string name, double value, double maximum, string valueText)
    {
        Icon = icon;
        Name = name;
        Value = value;
        Maximum = maximum;
        ValueText = valueText;
    }

    public FontAwesomeIcon Icon { get; }
    public string Name { get; }
    public double Value { get; }
    public double Maximum { get; }
    public string ValueText { get; }
}