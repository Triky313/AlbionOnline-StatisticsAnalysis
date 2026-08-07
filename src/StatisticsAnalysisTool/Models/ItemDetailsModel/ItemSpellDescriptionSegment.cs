namespace StatisticsAnalysisTool.Models.ItemDetailsModel;

public sealed class ItemSpellDescriptionSegment
{
    public ItemSpellDescriptionSegment(string text, string typeKey)
    {
        Text = text;
        TypeKey = typeKey;
    }

    public string Text { get; }
    public string TypeKey { get; }
}