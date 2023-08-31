using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.Comparer;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Dungeon.Models;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
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
    private DungeonStatsFilterStruct _statsTimeSelection;
    private DungeonStats _dungeonStatsSelection;
    private DungeonOptionsObject _dungeonOptionsObject = new();

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

    public DungeonStatsFilterStruct StatsTimeSelection
    {
        get => _statsTimeSelection;
        set
        {
            _statsTimeSelection = value;
            UpdateStats();
            OnPropertyChanged();
        }
    }

    private void UpdateStats()
    {
        var trackingController = ServiceLocator.Resolve<TrackingController>();
        trackingController.DungeonController.UpdateStatsUi();
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
        bool isTypeOkay = false;

        if (IsLevelOkay(obj))
        {
            isLevelOkay = true;
        }

        if (IsTierOkay(obj))
        {
            isTierOkay = true;
        }

        if (IsTypeOkay(obj))
        {
            isTypeOkay = true;
        }

        return isLevelOkay && isTierOkay && isTypeOkay;
    }

    private bool IsTypeOkay(object obj)
    {
        switch (obj)
        {
            case RandomDungeonFragment {Mode: DungeonMode.Solo} when DungeonStatsFilter.SoloCheckbox == true:
            case RandomDungeonFragment {Mode: DungeonMode.Standard} when DungeonStatsFilter.StandardCheckbox == true:
            case RandomDungeonFragment {Mode: DungeonMode.Avalon} when DungeonStatsFilter.AvaCheckbox == true:
            case CorruptedFragment when DungeonStatsFilter.CorruptedCheckbox == true:
            case HellGateFragment when DungeonStatsFilter.HgCheckbox == true:
            case ExpeditionFragment when DungeonStatsFilter.ExpeditionCheckbox == true:
            case MistsFragment when DungeonStatsFilter.MistsCheckbox == true:
            case MistsDungeonFragment when DungeonStatsFilter.MistsDungeonCheckbox == true:
                return true;
            default:
                return false;
        }
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

    #endregion
}