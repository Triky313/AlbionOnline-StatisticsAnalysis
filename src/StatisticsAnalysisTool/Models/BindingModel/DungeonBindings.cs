using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.Comparer;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class DungeonBindings : INotifyPropertyChanged
{
    private ObservableCollection<DungeonNotificationFragment> _trackingDungeons = new();
    private ListCollectionView _trackingDungeonsCollectionView;
    private DungeonCloseTimer _dungeonCloseTimer = new();
    private DungeonStatsFilter _dungeonStatsFilter;
    private DungeonStats _dungeonStatsDay = new();
    private DungeonStats _dungeonStatsWeek = new();
    private DungeonStats _dungeonStatsMonth = new();
    private DungeonStats _dungeonStatsYear = new();
    private DungeonStats _dungeonStatsTotal = new();
    private GridLength _gridSplitterPosition;
    private DungeonsTranslation _translation = new();
    private DungeonStatsFilterStruct _dungeonStatTimeSelection;
    private DungeonStats _dungeonStatsSelection;
    private DungeonOptionsObject _dungeonOptionsObject = new ();

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

    public DungeonStats DungeonStatsWeek
    {
        get => _dungeonStatsWeek;
        set
        {
            _dungeonStatsWeek = value;
            OnPropertyChanged();
        }
    }

    public DungeonStats DungeonStatsMonth
    {
        get => _dungeonStatsMonth;
        set
        {
            _dungeonStatsMonth = value;
            OnPropertyChanged();
        }
    }

    public DungeonStats DungeonStatsYear
    {
        get => _dungeonStatsYear;
        set
        {
            _dungeonStatsYear = value;
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

    public DungeonOptionsObject DungeonOptionsObject
    {
        get => _dungeonOptionsObject;
        set
        {
            _dungeonOptionsObject = value;
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

    public List<DungeonStatsFilterStruct> DungeonStatTimeTypes { get; } = new()
    {
        new DungeonStatsFilterStruct
        {
            Name = LanguageController.Translation("TODAY"),
            DungeonStatTimeType = DungeonStatTimeType.Today
        },
        new DungeonStatsFilterStruct
        {
            Name = LanguageController.Translation("LAST_7_DAYS"),
            DungeonStatTimeType = DungeonStatTimeType.Last7Days
        },
        new DungeonStatsFilterStruct
        {
            Name = LanguageController.Translation("LAST_30_DAYS"),
            DungeonStatTimeType = DungeonStatTimeType.Last30Days
        },
        new DungeonStatsFilterStruct
        {
            Name = LanguageController.Translation("LAST_365_DAYS"),
            DungeonStatTimeType = DungeonStatTimeType.Last365Days
        },
        new DungeonStatsFilterStruct
        {
            Name = LanguageController.Translation("TOTAL"),
            DungeonStatTimeType = DungeonStatTimeType.Total
        }
    };

    public DungeonStatsFilterStruct DungeonStatTimeSelection
    {
        get => _dungeonStatTimeSelection;
        set
        {
            _dungeonStatTimeSelection = value;

            DungeonStatsSelection = _dungeonStatTimeSelection.DungeonStatTimeType switch
            {
                DungeonStatTimeType.Today => DungeonStatsDay,
                DungeonStatTimeType.Last7Days => DungeonStatsWeek,
                DungeonStatTimeType.Last30Days => DungeonStatsMonth,
                DungeonStatTimeType.Last365Days => DungeonStatsYear,
                DungeonStatTimeType.Total => DungeonStatsTotal,
                _ => new DungeonStats()
            };

            OnPropertyChanged();
        }
    }
    
    public DungeonStats DungeonStatsSelection
    {
        get => _dungeonStatsSelection;
        set
        {
            _dungeonStatsSelection = value;
            OnPropertyChanged();
        }
    }

    public DungeonsTranslation Translation
    {
        get => _translation;
        set
        {
            _translation = value;
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