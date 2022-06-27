using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Properties;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using StatisticsAnalysisTool.Common.Comparer;
using StatisticsAnalysisTool.Network.Notification;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class LoggingBindings : INotifyPropertyChanged
{
    private ListCollectionView _notificationsCollectionView;
    private ObservableCollection<TrackingNotification> _trackingNotifications = new();
    private ObservableCollection<TopLooterObject> _topLooters = new();
    private bool _isTrackingSilver;
    private bool _isTrackingFame;
    private bool _isTrackingMobLoot;
    private ObservableCollection<LoggingFilterObject> _filters = new();
    private ListCollectionView _topLootersCollectionView;

    public LoggingBindings()
    {
        TopLootersCollectionView = CollectionViewSource.GetDefaultView(TopLooters) as ListCollectionView;
        if (TopLootersCollectionView != null)
        {
            TopLootersCollectionView.IsLiveSorting = true;
            TopLootersCollectionView.CustomSort = new TopLooterComparer();
        }
    }

    #region Bindings

    public ListCollectionView NotificationsCollectionView
    {
        get => _notificationsCollectionView;
        set
        {
            _notificationsCollectionView = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<TrackingNotification> TrackingNotifications
    {
        get => _trackingNotifications;
        set
        {
            _trackingNotifications = value;
            OnPropertyChanged();
        }
    }

    public ListCollectionView TopLootersCollectionView
    {
        get => _topLootersCollectionView;
        set
        {
            _topLootersCollectionView = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<TopLooterObject> TopLooters
    {
        get => _topLooters;
        set
        {
            _topLooters = value;
            OnPropertyChanged();
        }
    }

    public bool IsTrackingSilver
    {
        get => _isTrackingSilver;
        set
        {
            _isTrackingSilver = value;

            SettingsController.CurrentSettings.IsTrackingSilver = _isTrackingSilver;
            OnPropertyChanged();
        }
    }

    public bool IsTrackingFame
    {
        get => _isTrackingFame;
        set
        {
            _isTrackingFame = value;

            SettingsController.CurrentSettings.IsTrackingFame = _isTrackingFame;
            OnPropertyChanged();
        }
    }

    public bool IsTrackingMobLoot
    {
        get => _isTrackingMobLoot;
        set
        {
            _isTrackingMobLoot = value;

            SettingsController.CurrentSettings.IsTrackingMobLoot = _isTrackingMobLoot;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<LoggingFilterObject> Filters
    {
        get => _filters;
        set
        {
            _filters = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}