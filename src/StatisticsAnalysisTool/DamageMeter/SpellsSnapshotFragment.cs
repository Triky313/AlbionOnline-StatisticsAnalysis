using Newtonsoft.Json;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.DamageMeter;

public class SpellsSnapshotFragment : BaseViewModel
{
    private BitmapImage _icon;

    public int Index { get; set; }
    public string UniqueName { get; set; }
    public long DamageHealValue { get; set; }
    public string DamageHealShortString { get; set; }

    [JsonIgnore]
    public BitmapImage Icon => Application.Current.Dispatcher.Invoke(() => _icon ??= ImageController.GetSpellImage(UniqueName));
}