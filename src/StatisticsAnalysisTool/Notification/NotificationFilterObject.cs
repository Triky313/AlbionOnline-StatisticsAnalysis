using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Notification;

public class NotificationFilter : BaseViewModel
{
    private bool? _isSelected;
    private string _name;

    public NotificationFilter(NotificationFilterType notificationFilterType)
    {
        NotificationFilterType = notificationFilterType;
    }

    public NotificationFilterType NotificationFilterType { get; }

    public bool? IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
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