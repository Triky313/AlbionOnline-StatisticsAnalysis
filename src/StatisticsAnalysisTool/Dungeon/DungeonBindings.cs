using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.Comparer;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Dungeon.Models;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Dungeon;

public class DungeonBindings : BaseViewModel
{
    private ObservableRangeCollection<DungeonBaseFragment> _dungeons = new();
    private ListCollectionView _dungeonsCollectionView;
    private DungeonCloseTimer _dungeonCloseTimer = new();
    private DungeonStatsFilter _dungeonStatsFilter;
    private DungeonStats _stats = new();
    private GridLength _gridSplitterPosition;
    private DungeonsTranslation _translation = new();
    private DungeonStatsFilterStruct _selectedStatsTimeType;
    private DungeonStats _dungeonStatsSelection;
    private DungeonOptionsObject _dungeonOptionsObject = new();
    private StatsTypeFilterStruct _selectedDungeonStatsType;

    public DungeonBindings()
    {
        DungeonStatTimeTypes = new List<DungeonStatsFilterStruct>
        {
            new ()
            {
                Name = LanguageController.Translation("TODAY"),
                StatTimeType = DungeonStatTimeType.Today
            },
            new ()
            {
                Name = LanguageController.Translation("LAST_7_DAYS"),
                StatTimeType = DungeonStatTimeType.Last7Days
            },
            new ()
            {
                Name = LanguageController.Translation("LAST_30_DAYS"),
                StatTimeType = DungeonStatTimeType.Last30Days
            },
            new ()
            {
                Name = LanguageController.Translation("LAST_365_DAYS"),
                StatTimeType = DungeonStatTimeType.Last365Days
            },
            new ()
            {
                Name = LanguageController.Translation("TOTAL"),
                StatTimeType = DungeonStatTimeType.Total
            }
        };

        SelectedStatsTimeType = DungeonStatTimeTypes.FirstOrDefault(x => x.StatTimeType == DungeonStatTimeType.Total);

        DungeonStatsType = new List<StatsTypeFilterStruct>
        {
            new ()
            {
                Name = LanguageController.Translation("SOLO_DUNGEON"),
                StatsViewType = DungeonMode.Solo
            },
            new ()
            {
                Name = LanguageController.Translation("STANDARD_DUNGEON"),
                StatsViewType = DungeonMode.Standard
            },
            new ()
            {
                Name = LanguageController.Translation("AVALONIAN_DUNGEON"),
                StatsViewType = DungeonMode.Avalon
            },
            new ()
            {
                Name = LanguageController.Translation("CORRUPTED"),
                StatsViewType = DungeonMode.Corrupted
            },
            new ()
            {
                Name = LanguageController.Translation("HELLGATE"),
                StatsViewType = DungeonMode.HellGate
            },
            new ()
            {
                Name = LanguageController.Translation("HCE_EXPEDITION"),
                StatsViewType = DungeonMode.Expedition
            },
            new ()
            {
                Name = LanguageController.Translation("MISTS"),
                StatsViewType = DungeonMode.Mists
            },
            new ()
            {
                Name = LanguageController.Translation("MISTS_DUNGEON"),
                StatsViewType = DungeonMode.MistsDungeon
            }
        };

        SelectedDungeonStatsType = DungeonStatsType.FirstOrDefault(x => x.StatsViewType == DungeonMode.Solo);
    }

    public void InitListCollectionView()
    {
        DungeonsCollectionView = CollectionViewSource.GetDefaultView(Dungeons) as ListCollectionView;
        if (DungeonsCollectionView != null)
        {
            DungeonsCollectionView.IsLiveSorting = true;
            DungeonsCollectionView.IsLiveFiltering = true;
            DungeonsCollectionView.CustomSort = new DungeonTrackingNumberComparer();
            DungeonsCollectionView.Refresh();
        }
    }

    public ObservableRangeCollection<DungeonBaseFragment> Dungeons
    {
        get => _dungeons;
        set
        {
            _dungeons = value;
            OnPropertyChanged();
        }
    }

    public ListCollectionView DungeonsCollectionView
    {
        get => _dungeonsCollectionView;
        set
        {
            _dungeonsCollectionView = value;
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

    public DungeonStats Stats
    {
        get => _stats;
        set
        {
            _stats = value;
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

    public List<DungeonStatsFilterStruct> DungeonStatTimeTypes { get; }

    public DungeonStatsFilterStruct SelectedStatsTimeType
    {
        get => _selectedStatsTimeType;
        set
        {
            _selectedStatsTimeType = value;
            _ = UpdateFilteredDungeonsAsync();
            OnPropertyChanged();
        }
    }

    public List<StatsTypeFilterStruct> DungeonStatsType { get; }

    public StatsTypeFilterStruct SelectedDungeonStatsType
    {
        get => _selectedDungeonStatsType;
        set
        {
            _selectedDungeonStatsType = value;
            _ = UpdateFilteredDungeonsAsync();
            UpdateStatsView();
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

    #region Filter

    private CancellationTokenSource _cancellationTokenSource;

    public async Task UpdateFilteredDungeonsAsync()
    {
        if (Dungeons?.Count <= 0 && DungeonsCollectionView?.Count <= 0)
        {
            return;
        }

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var filteredDungeons = await Task.Run(ParallelTradeFilterProcess, _cancellationTokenSource.Token);

            DungeonsCollectionView = CollectionViewSource.GetDefaultView(filteredDungeons) as ListCollectionView;
            Stats?.Set(DungeonsCollectionView?.Cast<DungeonBaseFragment>().ToList());
        }
        catch (TaskCanceledException)
        {
            // ignored
        }
    }

    public List<DungeonBaseFragment> ParallelTradeFilterProcess()
    {
        var partitioner = Partitioner.Create(Dungeons, EnumerablePartitionerOptions.NoBuffering);
        var result = new ConcurrentBag<DungeonBaseFragment>();

        Parallel.ForEach(partitioner, (tradeBatch, state) =>
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                state.Stop();
            }

            if (Filter(tradeBatch))
            {
                result.Add(tradeBatch);
            }
        });

        return result.ToList();
    }

    private bool Filter(object obj)
    {
        if (obj is not DungeonBaseFragment)
        {
            return false;
        }

        bool isLevelOkay = false;
        bool isTierOkay = false;
        bool isModeOkay = false;
        bool isTimestampOkay = false;

        if (IsLevelOkay(obj))
        {
            isLevelOkay = true;
        }

        if (IsTierOkay(obj))
        {
            isTierOkay = true;
        }

        if (IsModeOkay(obj))
        {
            isModeOkay = true;
        }

        if (IsTimestampOkay(obj))
        {
            isTimestampOkay = true;
        }

        return isLevelOkay && isTierOkay && isModeOkay && isTimestampOkay;
    }

    private bool IsTierOkay(object obj)
    {
        if (obj is not DungeonBaseFragment baseDun)
        {
            return false;
        }

        if (DungeonStatsFilter.IsTierUnknown == true && baseDun.Tier < 0)
        {
            return true;
        }

        if (DungeonStatsFilter.TierFilters.Contains(baseDun.Tier))
        {
            return true;
        }

        if (DungeonStatsFilter.IsTierUnknown == false && DungeonStatsFilter.TierFilters.Count <= 0)
        {
            return true;
        }

        return false;
    }

    private bool IsLevelOkay(object obj)
    {
        if (obj is not RandomDungeonFragment ranDun)
        {
            return true;
        }

        if (DungeonStatsFilter.IsLevelUnknown == true && ranDun.Level < 0)
        {
            return true;
        }

        if (DungeonStatsFilter.IsLevel0 == true && ranDun.Level == 0)
        {
            return true;
        }

        if (DungeonStatsFilter.IsLevel1 == true && ranDun.Level == 1)
        {
            return true;
        }

        if (DungeonStatsFilter.IsLevel2 == true && ranDun.Level == 2)
        {
            return true;
        }

        if (DungeonStatsFilter.IsLevel3 == true && ranDun.Level == 3)
        {
            return true;
        }

        if (DungeonStatsFilter.IsLevel4 == true && ranDun.Level == 4)
        {
            return true;
        }

        if (DungeonStatsFilter.IsLevelUnknown == false
            && DungeonStatsFilter.IsLevel0 == false
            && DungeonStatsFilter.IsLevel1 == false
            && DungeonStatsFilter.IsLevel2 == false
            && DungeonStatsFilter.IsLevel3 == false
            && DungeonStatsFilter.IsLevel4 == false)
        {
            return true;
        }

        return false;
    }

    private bool IsTimestampOkay(object obj)
    {
        if (obj is not DungeonBaseFragment dungeon)
        {
            return false;
        }

        if (SelectedStatsTimeType.StatTimeType == DungeonStatTimeType.Total)
        {
            return true;
        }

        if (SelectedStatsTimeType.StatTimeType == DungeonStatTimeType.Last365Days && dungeon.EnterDungeonFirstTime > DateTime.UtcNow.AddDays(-365))
        {
            return true;
        }

        if (SelectedStatsTimeType.StatTimeType == DungeonStatTimeType.Last30Days && dungeon.EnterDungeonFirstTime > DateTime.UtcNow.AddDays(-30))
        {
            return true;
        }

        if (SelectedStatsTimeType.StatTimeType == DungeonStatTimeType.Last7Days && dungeon.EnterDungeonFirstTime > DateTime.UtcNow.AddDays(-7))
        {
            return true;
        }

        if (SelectedStatsTimeType.StatTimeType == DungeonStatTimeType.Today && dungeon.EnterDungeonFirstTime.Date == DateTime.UtcNow.Date)
        {
            return true;
        }

        if (SelectedStatsTimeType.StatTimeType == DungeonStatTimeType.Unknown)
        {
            return true;
        }

        return false;
    }

    private bool IsModeOkay(object obj)
    {
        if (obj is not DungeonBaseFragment dungeon)
        {
            return false;
        }

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.Solo && dungeon.Mode == DungeonMode.Solo)
        {
            return true;
        }

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.Standard && dungeon.Mode == DungeonMode.Standard)
        {
            return true;
        }

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.Avalon && dungeon.Mode == DungeonMode.Avalon)
        {
            return true;
        }

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.Corrupted && dungeon.Mode == DungeonMode.Corrupted)
        {
            return true;
        }

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.HellGate && dungeon.Mode == DungeonMode.HellGate)
        {
            return true;
        }

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.Expedition && dungeon.Mode == DungeonMode.Expedition)
        {
            return true;
        }

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.Mists && dungeon.Mode == DungeonMode.Mists)
        {
            return true;
        }

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.MistsDungeon && dungeon.Mode == DungeonMode.MistsDungeon)
        {
            return true;
        }

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.Unknown)
        {
            return true;
        }

        return false;
    }

    #endregion

    private void UpdateStatsView()
    {
        SetAllStatViewsToCollapsed();

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.Solo)
        {
            Stats.StatsSolo.Visibility = Visibility.Visible;
            return;
        }

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.Standard)
        {
            Stats.StatsStandard.Visibility = Visibility.Visible;
            return;
        }

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.Avalon)
        {
            Stats.StatsAvalonian.Visibility = Visibility.Visible;
            return;
        }

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.Corrupted)
        {
            Stats.StatsCorrupted.Visibility = Visibility.Visible;
            return;
        }

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.HellGate)
        {
            Stats.StatsHellGate.Visibility = Visibility.Visible;
            return;
        }

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.Expedition)
        {
            Stats.StatsExpedition.Visibility = Visibility.Visible;
            return;
        }

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.Mists)
        {
            Stats.StatsMists.Visibility = Visibility.Visible;
            return;
        }

        if (SelectedDungeonStatsType.StatsViewType == DungeonMode.MistsDungeon)
        {
            Stats.StatsMistsDungeon.Visibility = Visibility.Visible;
        }
    }

    private void SetAllStatViewsToCollapsed()
    {
        Stats.StatsSolo.Visibility = Visibility.Collapsed;
        Stats.StatsStandard.Visibility = Visibility.Collapsed;
        Stats.StatsAvalonian.Visibility = Visibility.Collapsed;
        Stats.StatsExpedition.Visibility = Visibility.Collapsed;
        Stats.StatsCorrupted.Visibility = Visibility.Collapsed;
        Stats.StatsHellGate.Visibility = Visibility.Collapsed;
        Stats.StatsMists.Visibility = Visibility.Collapsed;
        Stats.StatsMistsDungeon.Visibility = Visibility.Collapsed;
    }
}