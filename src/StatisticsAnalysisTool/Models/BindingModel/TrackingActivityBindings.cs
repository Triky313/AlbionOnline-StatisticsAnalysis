using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using StatisticsAnalysisTool.Models.TranslationModel;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class TrackingActivityBindings : INotifyPropertyChanged
{
    private TrackingIconType _trackingActivityType = TrackingIconType.Off;
    private string _trackingActiveText = MainWindowTranslation.TrackingIsNotActive;


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


    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}