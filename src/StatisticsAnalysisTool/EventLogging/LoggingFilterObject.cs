using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.EventLogging;

public class LoggingFilterObject : INotifyPropertyChanged
{
    private bool? _isSelected;
    private string _name;

    public LoggingFilterObject(LoggingFilterType loggingFilterType)
    {
        LoggingFilterType = loggingFilterType;
    }

    public LoggingFilterType LoggingFilterType { get; }

    private void SetFilter()
    {
        try
        {

            var loggingSearchText = ServiceLocator.Resolve<MainWindowViewModel>()?.LoggingSearchText;
            var trackingController = ServiceLocator.Resolve<TrackingController>();

            trackingController?.UpdateFilterType(LoggingFilterType, IsSelected ?? false);
            trackingController?.NotificationUiFilteringAsync(loggingSearchText);
        }
        catch (KeyNotFoundException e)
        {

        }
    }

    public bool? IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            SetFilter();
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
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