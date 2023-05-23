using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Properties;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace StatisticsAnalysisTool.EventLogging;

public class LoggingBindings : INotifyPropertyChanged
{
    private ListCollectionView _gameLoggingCollectionView;
    private ObservableCollection<TrackingNotification> _trackingNotifications = new();
    private ObservableCollection<TopLooterObject> _topLooters = new();
    private bool _isTrackingSilver;
    private bool _isTrackingFame;
    private bool _isTrackingMobLoot;
    private ObservableCollection<LoggingFilterObject> _filters = new();
    private ListCollectionView _topLootersCollectionView;
    
    public void Init()
    {
        TopLootersCollectionView = CollectionViewSource.GetDefaultView(TopLooters) as ListCollectionView;
        if (TopLootersCollectionView != null)
        {
            TopLootersCollectionView.IsLiveSorting = true;
            TopLootersCollectionView.CustomSort = new TopLooterComparer();
        }

        Filters.Add(new LoggingFilterObject(LoggingFilterType.Fame)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterFame,
            Name = MainWindowTranslation.Fame
        });

        Filters.Add(new LoggingFilterObject(LoggingFilterType.Silver)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterSilver,
            Name = MainWindowTranslation.Silver
        });

        Filters.Add(new LoggingFilterObject(LoggingFilterType.Faction)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterFaction,
            Name = MainWindowTranslation.Faction
        });

        Filters.Add(new LoggingFilterObject(LoggingFilterType.ConsumableLoot)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterConsumableLoot,
            Name = MainWindowTranslation.ConsumableLoot
        });

        Filters.Add(new LoggingFilterObject(LoggingFilterType.EquipmentLoot)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterEquipmentLoot,
            Name = MainWindowTranslation.EquipmentLoot
        });

        Filters.Add(new LoggingFilterObject(LoggingFilterType.SimpleLoot)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterSimpleLoot,
            Name = MainWindowTranslation.SimpleLoot
        });

        Filters.Add(new LoggingFilterObject(LoggingFilterType.UnknownLoot)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterUnknownLoot,
            Name = MainWindowTranslation.UnknownLoot
        });

        Filters.Add(new LoggingFilterObject(LoggingFilterType.ShowLootFromMob)
        {
            IsSelected = SettingsController.CurrentSettings.IsLootFromMobShown,
            Name = MainWindowTranslation.ShowLootFromMobs
        });

        Filters.Add(new LoggingFilterObject(LoggingFilterType.Kill)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterKill,
            Name = MainWindowTranslation.ShowKills
        });
    }

    #region Bindings

    public ListCollectionView GameLoggingCollectionView
    {
        get => _gameLoggingCollectionView;
        set
        {
            _gameLoggingCollectionView = value;
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