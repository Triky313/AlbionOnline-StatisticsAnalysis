using System.Collections.ObjectModel;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Notification;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class DungeonBindings : INotifyPropertyChanged
{
    private ObservableCollection<DungeonNotificationFragment> _trackingDungeons = new();
    private ListCollectionView _trackingDungeonsCollectionView;
    private DungeonCloseTimer _dungeonCloseTimer = new();
    private DungeonStatsFilter _dungeonStatsFilter;
    private DungeonStats _dungeonStatsDay = new();
    private DungeonStats _dungeonStatsTotal = new();
    private GridLength _gridSplitterPosition;

    public DungeonBindings()
    {
        TrackingDungeonsCollectionView = CollectionViewSource.GetDefaultView(TrackingDungeons) as ListCollectionView;
        if (TrackingDungeonsCollectionView != null)
        {
            TrackingDungeonsCollectionView.IsLiveSorting = true;
            TrackingDungeonsCollectionView.CustomSort = new DungeonTrackingNumberComparer();
        }
    }

    public ObservableCollection<DungeonNotificationFragment> TrackingDungeons
    {
        get => _trackingDungeons;
        set
        {
            _trackingDungeons = value;
            OnPropertyChanged();
        }
    }

    public ListCollectionView TrackingDungeonsCollectionView
    {
        get => _trackingDungeonsCollectionView;
        set
        {
            _trackingDungeonsCollectionView = value;
            OnPropertyChanged();
        }
    }

    public DungeonCloseTimer DungeonCloseTimer
    {
        get => _dungeonCloseTimer;
        set
        {
            _dungeonCloseTimer = value;
            OnPropertyChanged();
        }
    }

    public DungeonStats DungeonStatsDay
    {
        get => _dungeonStatsDay;
        set
        {
            _dungeonStatsDay = value;
            OnPropertyChanged();
        }
    }

    public DungeonStats DungeonStatsTotal
    {
        get => _dungeonStatsTotal;
        set
        {
            _dungeonStatsTotal = value;
            OnPropertyChanged();
        }
    }

    public DungeonStatsFilter DungeonStatsFilter
    {
        get => _dungeonStatsFilter;
        set
        {
            _dungeonStatsFilter = value;
            OnPropertyChanged();
        }
    }

    public GridLength GridSplitterPosition
    {
        get => _gridSplitterPosition;
        set
        {
            _gridSplitterPosition = value;
            SettingsController.CurrentSettings.DungeonsGridSplitterPosition = _gridSplitterPosition.Value;
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