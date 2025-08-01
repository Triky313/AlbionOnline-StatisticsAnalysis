using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Notification;

public class NotificationFilter(NotificationFilterType notificationFilterType) : BaseViewModel
{
    private bool? _isSelected;
    private string _name;

    public NotificationFilterType NotificationFilterType { get; } = notificationFilterType;

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