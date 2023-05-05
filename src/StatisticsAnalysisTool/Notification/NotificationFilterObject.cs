using Notifications.Wpf.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Notification;

public class NotificationFilter : INotifyPropertyChanged
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

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}