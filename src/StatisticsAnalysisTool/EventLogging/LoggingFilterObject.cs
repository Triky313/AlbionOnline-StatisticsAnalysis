using Microsoft.Extensions.DependencyInjection;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.EventLogging;

public class LoggingFilterObject : BaseViewModel
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
            var loggingSearchText = App.ServiceProvider.GetService<MainWindowViewModelOld>()?.LoggingSearchText;
            var trackingController = App.ServiceProvider.GetService<TrackingController>();

            trackingController?.UpdateFilterType(LoggingFilterType, IsSelected ?? false);
            trackingController?.NotificationUiFilteringAsync(loggingSearchText);
        }
        catch (KeyNotFoundException)
        {
            // ignored
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
}