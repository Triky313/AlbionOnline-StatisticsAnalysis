using System.Windows;
using StatisticsAnalysisTool.ViewModels;
using System.Windows.Media.Imaging;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.DamageMeter;

public class SpellsFragment : BaseViewModel
{
    private string _uniqueName;
    private long _damage;
    private long _heal;
    private BitmapImage _icon;
    private string _damageShortString;

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

    public long Damage
    {
        get => _damage;
        set
        {
            _damage = value;
            DamageShortString = _damage.ToShortNumberString();
            OnPropertyChanged();
        }
    }

    public string DamageShortString
    {
        get => _damageShortString;
        private set
        {
            _damageShortString = value;
            OnPropertyChanged();
        }
    }

    public long Heal
    {
        get => _heal;
        set
        {
            _heal = value;
            OnPropertyChanged();
        }
    }

    public BitmapImage Icon => Application.Current.Dispatcher.Invoke(() => _icon ??= ImageController.GetSpellImage(UniqueName));
}