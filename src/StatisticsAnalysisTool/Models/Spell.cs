using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameFileData;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Models;

public class Spell
{
    private BitmapImage _icon;

    public Spell(int index)
    {
        Index = index;

        var spellGameFileData = SpellData.GetSpellByIndex(Index);
        UniqueName = spellGameFileData.UniqueName;
        Target = spellGameFileData.Target;
        Category = spellGameFileData.Category;

    }

    public int Index { get; init; }
    public string UniqueName { get; init; }
    public string Target { get; init; }
    public string Category { get; init; }
    public BitmapImage Icon => Application.Current.Dispatcher.Invoke(() => _icon ??= ImageController.GetSpellImage(UniqueName));
    public string LocalizationName => SpellData.GetLocalizationName(UniqueName);
}