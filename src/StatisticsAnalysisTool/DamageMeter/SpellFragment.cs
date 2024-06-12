using System.Windows;
using StatisticsAnalysisTool.ViewModels;
using System.Windows.Media.Imaging;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.DamageMeter;

public class SpellFragment : BaseViewModel
{
    private string _uniqueName;
    private long _damageHealValue;
    private BitmapImage _icon;
    private string _damageHealShortString;

    public int Index { get; set; }

    public string UniqueName
    {
        get => _uniqueName;
        set
        {
            _uniqueName = value;
            OnPropertyChanged();
        }
    }

    public long DamageHealValue
    {
        get => _damageHealValue;
        set
        {
            _damageHealValue = value;
            DamageHealShortString = _damageHealValue.ToShortNumberString();
            OnPropertyChanged();
        }
    }

    public string DamageHealShortString
    {
        get => _damageHealShortString;
        private set
        {
            _damageHealShortString = value;
            OnPropertyChanged();
        }
    }
    
    public BitmapImage Icon => Application.Current.Dispatcher.Invoke(() => _icon ??= ImageController.GetSpellImage(UniqueName));
}