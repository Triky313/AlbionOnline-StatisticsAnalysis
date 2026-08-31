namespace StatisticsAnalysisTool.Models.ItemDetailsModel;

public sealed class ItemSpellType
{
    public ItemSpellType(string key, string name)
    {
        Key = key;
        Name = name;
    }

    public string Key { get; }
    public string Name { get; }
}