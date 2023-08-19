using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class TrackingActivityBindings : BaseViewModel
{
    private TrackingIconType _trackingActivityType = TrackingIconType.Off;
    private string _trackingActiveText = MainWindowTranslation.TrackingIsNotActive;
    private Visibility _characterIsNotTrackedInfoVisibility;

    public TrackingIconType TrackingActivityType
    {
        get => _trackingActivityType;
        set
        {
            _trackingActivityType = value;
            OnPropertyChanged();
        }
    }

    public string TrackingActiveText
    {
        get => _trackingActiveText;
        set
        {
            _trackingActiveText = value;
            OnPropertyChanged();
        }
    }

    public Visibility CharacterIsNotTrackedInfoVisibility
    {
        get => _characterIsNotTrackedInfoVisibility;
        set
        {
            _characterIsNotTrackedInfoVisibility = value;
            OnPropertyChanged();
        }
    }
}