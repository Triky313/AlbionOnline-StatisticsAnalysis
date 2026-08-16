using StatisticsAnalysisTool.DamageMeter;

namespace StatisticsAnalysisTool.ViewModels;

public class DamageMeterWindowViewModel : BaseViewModel
{
    private DamageMeterBindings _damageMeterBindings;
    private DamageMeterWindowTranslation _translation;

    public DamageMeterWindowViewModel(DamageMeterBindings damageMeterBindings)
    {
        DamageMeterBindings = damageMeterBindings;
        Init();
    }

    private void Init()
    {
        Translation = new DamageMeterWindowTranslation();
    }

    public DamageMeterBindings DamageMeterBindings
    {
        get => _damageMeterBindings;
        set
        {
            _damageMeterBindings = value;
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