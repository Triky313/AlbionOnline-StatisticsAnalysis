using Newtonsoft.Json;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.DamageMeter;

public class SpellsSnapshotFragment : BaseViewModel
{
    private BitmapImage _icon;

    public int SpellIndex { get; set; }
    public int ItemIndex { get; set; }
    public string UniqueName { get; set; }
    public long DamageHealValue { get; set; }
    public string DamageHealShortString { get; set; }
    public string Target { get; set; }
    public string Category { get; set; }
    public int Ticks { get; set; }
    public double DamageInPercent { get; set; }
    public double DamagePercentage { get; set; }

    [JsonIgnore]
    public BitmapImage Icon => Application.Current.Dispatcher.Invoke(() => _icon ??= ImageController.GetSpellImage(UniqueName));
}