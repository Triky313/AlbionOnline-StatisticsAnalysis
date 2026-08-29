namespace StatisticsAnalysisTool.Models.ItemDetailsModel;

public sealed class ItemSpellDescriptionSegment
{
    public ItemSpellDescriptionSegment(string text, string typeKey, bool isBold, string colorHex)
    {
        Text = text;
        TypeKey = typeKey;
        IsBold = isBold;
        ColorHex = colorHex;
    }

    public string Text { get; }
    public string TypeKey { get; }
    public bool IsBold { get; }
    public string ColorHex { get; }
}