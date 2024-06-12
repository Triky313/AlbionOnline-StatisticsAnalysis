using StatisticsAnalysisTool.DamageMeter;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.ViewModels;

public class DamageMeterWindowViewModel : BaseViewModel
{
    private ObservableCollection<DamageMeterFragment> _damageMeter;
    private DamageMeterWindowTranslation _translation;

    public DamageMeterWindowViewModel(ObservableCollection<DamageMeterFragment> damageMeter)
    {
        DamageMeter = damageMeter;
        Init();
    }

    private void Init()
    {
        Translation = new DamageMeterWindowTranslation();
    }

    public ObservableCollection<DamageMeterFragment> DamageMeter
    {
        get => _damageMeter;
        set
        {
            _damageMeter = value;
            OnPropertyChanged();
        }
    }

    public DamageMeterWindowTranslation Translation
    {
        get => _translation;
        set
        {
            _translation = value;
            OnPropertyChanged();
        }
    }
}