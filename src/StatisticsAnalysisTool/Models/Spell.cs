using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameData;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Models;

public class Spell
{
    private readonly int _index;
    private BitmapImage _icon;

    public Spell(int index)
    {
        _index = index;
    }

    public string UniqueName => SpellData.GetUniqueName(_index);
    public BitmapImage Icon => Application.Current.Dispatcher.Invoke(() => _icon ??= ImageController.GetSpellImage(UniqueName));
}