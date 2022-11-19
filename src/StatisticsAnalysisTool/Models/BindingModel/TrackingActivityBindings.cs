using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class TrackingActivityBindings : INotifyPropertyChanged
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

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}