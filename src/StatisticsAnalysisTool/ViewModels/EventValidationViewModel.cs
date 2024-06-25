using StatisticsAnalysisTool.EventValidations;
using StatisticsAnalysisTool.Localization;
using System.Windows.Data;

namespace StatisticsAnalysisTool.ViewModels;

public class EventValidationViewModel : BaseViewModel
{
    private ListCollectionView _eventValidationCollectionView;

    public EventValidationViewModel()
    {
        Init();
    }

    private void Init()
    {
        EventValidator.Init();

        EventValidationCollectionView = CollectionViewSource.GetDefaultView(EventValidator.ValidatingEvents) as ListCollectionView;
        if (EventValidationCollectionView != null)
        {
            EventValidationCollectionView.IsLiveSorting = true;
            EventValidationCollectionView.IsLiveFiltering = true;
        }
    }

    public ListCollectionView EventValidationCollectionView
    {
        get => _eventValidationCollectionView;
        set
        {
            _eventValidationCollectionView = value;
            OnPropertyChanged();
        }
    }

    public static string TranslationReset => $"{LanguageController.Translation("RESET")}";
    public static string TranslationTitle => $"{LanguageController.Translation("EVENT_VALIDATION")}";
}