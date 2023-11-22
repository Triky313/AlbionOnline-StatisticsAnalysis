using StatisticsAnalysisTool.Models.TranslationModel;

namespace StatisticsAnalysisTool.ViewModels;

public class ThanksViewModel : BaseViewModel
{
    private ThanksTranslation _translation;

    public ThanksViewModel()
    {
        Translation = new ThanksTranslation();
    }

    public ThanksTranslation Translation
    {
        get => _translation;
        set
        {
            _translation = value;
            OnPropertyChanged();
        }
    }
}