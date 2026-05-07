using FontAwesome5;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace StatisticsAnalysisTool.DamageMeter;

public class DamageMeterBindings : BaseViewModel, IAsyncInitialization
{
    private List<DamageMeterSortStruct> _damageMeterSort = new();
    private DamageMeterSortStruct _damageMeterSortSelection;
    private EFontAwesomeIcon _damageMeterActivationToggleIcon = EFontAwesomeIcon.Solid_ToggleOff;
    private Brush _damageMeterActivationToggleColor;
    private ObservableCollection<DamageMeterFragment> _damageMeter = new();
    private List<DamageMeterSnapshot> _damageMeterSnapshots = new();
    private DamageMeterSnapshot _damageMeterSnapshotSelection;
    private DamageMeterSortStruct _damageMeterSnapshotSortSelection;
    private List<DamageMeterSortStruct> _damageMeterSnapshotSort = new();
    private bool _isDamageMeterResetByMapChangeActive;
    private bool _isSnapshotAfterMapChangeActive;
    private GridLength _gridSplitterPosition;
    private bool _isDamageMeterResetBeforeCombatActive;
    private bool _shortDamageMeterToClipboard;
    private bool _onlyDamageToPlayersCounts;
    private DamageStatsSnapshot _currentDamageStats = DamageStatsSnapshot.Empty;
    private DamageStatsSnapshot _snapshotDamageStats = DamageStatsSnapshot.Empty;
    private DamageMeterYourStatsSnapshot _currentYourStats = DamageMeterYourStatsSnapshot.Empty;
    private DamageMeterYourStatsSnapshot _snapshotYourStats = DamageMeterYourStatsSnapshot.Empty;
    private Guid? _localPlayerGuid;
    private string _localPlayerName = string.Empty;
    private ObservableCollection<DamageStatsEntry> _topSingleHits = [];
    private ObservableCollection<DamageStatsEntry> _topSingleHeals = [];
    private ObservableCollection<DamageStatsEntry> _topLastHits = [];
    private ObservableCollection<DamageStatsEntry> _topOverheals = [];
    private ObservableCollection<DamageStatsEntry> _topTakenDamage = [];
    private ObservableCollection<DamageStatsEntry> _topBurstDamageFiveSeconds = [];
    private ObservableCollection<DamageStatsEntry> _topBurstDamageTenSeconds = [];
    private ObservableCollection<DamageStatsEntry> _topAttackedTargets = [];
    public Task Initialization { get; init; }

    public DamageMeterBindings()
    {
        var sortByDamageStruct = new DamageMeterSortStruct
        {
            Name = TranslationSortByDamage,
            DamageMeterSortType = DamageMeterSortType.Damage
        };
        var sortByDpsStruct = new DamageMeterSortStruct
        {
            Name = TranslationSortByDps,
            DamageMeterSortType = DamageMeterSortType.Dps
        };
        var sortByNameStruct = new DamageMeterSortStruct
        {
            Name = TranslationSortByName,
            DamageMeterSortType = DamageMeterSortType.Name
        };
        var sortByHealStruct = new DamageMeterSortStruct
        {
            Name = TranslationSortByHeal,
            DamageMeterSortType = DamageMeterSortType.Heal
        };
        var sortByHpsStruct = new DamageMeterSortStruct
        {
            Name = TranslationSortByHps,
            DamageMeterSortType = DamageMeterSortType.Hps
        };
        var takenDamageStruct = new DamageMeterSortStruct
        {
            Name = TranslationTakenDamage,
            DamageMeterSortType = DamageMeterSortType.TakenDamage
        };

        DamageMeterSort.Clear();
        DamageMeterSort.Add(sortByDamageStruct);
        DamageMeterSort.Add(sortByDpsStruct);
        DamageMeterSort.Add(sortByNameStruct);
        DamageMeterSort.Add(sortByHealStruct);
        DamageMeterSort.Add(sortByHpsStruct);
        DamageMeterSort.Add(takenDamageStruct);
        DamageMeterSortSelection = sortByDamageStruct;

        DamageMeterSnapshotSort.Clear();
        DamageMeterSnapshotSort.Add(sortByDamageStruct);
        DamageMeterSnapshotSort.Add(sortByDpsStruct);
        DamageMeterSnapshotSort.Add(sortByNameStruct);
        DamageMeterSnapshotSort.Add(sortByHealStruct);
        DamageMeterSnapshotSort.Add(sortByHpsStruct);
        DamageMeterSnapshotSort.Add(takenDamageStruct);
        DamageMeterSnapshotSortSelection = sortByDamageStruct;

        IsSnapshotAfterMapChangeActive = SettingsController.CurrentSettings.IsSnapshotAfterMapChangeActive;
        IsDamageMeterResetByMapChangeActive = SettingsController.CurrentSettings.IsDamageMeterResetByMapChangeActive;
        IsDamageMeterResetBeforeCombatActive = SettingsController.CurrentSettings.IsDamageMeterResetBeforeCombatActive;
        ShortDamageMeterToClipboard = SettingsController.CurrentSettings.ShortDamageMeterToClipboard;

        Initialization = LoadLocalFileAsync();
    }

    #region Generally

    public GridLength GridSplitterPosition
    {
        get => _gridSplitterPosition;
        set
        {
            _gridSplitterPosition = value;
            SettingsController.CurrentSettings.DamageMeterGridSplitterPosition = _gridSplitterPosition.Value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region Damage meter

    public ObservableCollection<DamageMeterFragment> DamageMeter
    {
        get => _damageMeter;
        set
        {
            _damageMeter = value;
            OnPropertyChanged();
        }
    }

    public EFontAwesomeIcon DamageMeterActivationToggleIcon
    {
        get => _damageMeterActivationToggleIcon;
        set
        {
            _damageMeterActivationToggleIcon = value;
            OnPropertyChanged();
        }
    }

    public Brush DamageMeterActivationToggleColor
    {
        get => _damageMeterActivationToggleColor ?? new SolidColorBrush((Color) Application.Current.Resources["Color.Text.1"]);
        set
        {
            _damageMeterActivationToggleColor = value;
            OnPropertyChanged();
        }
    }

    public DamageMeterSortStruct DamageMeterSortSelection
    {
        get => _damageMeterSortSelection;
        set
        {
            _damageMeterSortSelection = value;
            SetDamageMeterSort();

            OnPropertyChanged();
        }
    }

    public List<DamageMeterSortStruct> DamageMeterSort
    {
        get => _damageMeterSort;
        set
        {
            _damageMeterSort = value;
            OnPropertyChanged();
        }
    }

    public void SetDamageMeterSort()
    {
        switch (DamageMeterSortSelection.DamageMeterSortType)
        {
            case DamageMeterSortType.Damage:
                SetIsDamageMeterShowing(DamageMeter, DamageMeterStyleFragmentType.Damage);
                DamageMeter.OrderByReference(DamageMeter.OrderByDescending(x => x.DamageInPercent).ToList());
                return;
            case DamageMeterSortType.Dps:
                SetIsDamageMeterShowing(DamageMeter, DamageMeterStyleFragmentType.Damage);
                DamageMeter.OrderByReference(DamageMeter.OrderByDescending(x => x.Dps).ToList());
                return;
            case DamageMeterSortType.Name:
                SetIsDamageMeterShowing(DamageMeter, DamageMeterStyleFragmentType.Damage);
                DamageMeter.OrderByReference(DamageMeter.OrderBy(x => x.Name).ToList());
                return;
            case DamageMeterSortType.Heal:
                SetIsDamageMeterShowing(DamageMeter, DamageMeterStyleFragmentType.Heal);
                DamageMeter.OrderByReference(DamageMeter.OrderByDescending(x => x.HealInPercent).ToList());
                return;
            case DamageMeterSortType.Hps:
                SetIsDamageMeterShowing(DamageMeter, DamageMeterStyleFragmentType.Heal);
                DamageMeter.OrderByReference(DamageMeter.OrderByDescending(x => x.Hps).ToList());
                break;
            case DamageMeterSortType.TakenDamage:
                SetIsDamageMeterShowing(DamageMeter, DamageMeterStyleFragmentType.TakenDamage);
                DamageMeter.OrderByReference(DamageMeter.OrderByDescending(x => x.TakenDamage).ToList());
                return;
        }
    }

    private static void SetIsDamageMeterShowing(IEnumerable<DamageMeterFragment> damageMeter, DamageMeterStyleFragmentType damageMeterStyleFragmentType)
    {
        foreach (var fragment in damageMeter)
        {
            fragment.DamageMeterStyleFragmentType = damageMeterStyleFragmentType;
        }
    }

    public bool IsDamageMeterResetByMapChangeActive
    {
        get => _isDamageMeterResetByMapChangeActive;
        set
        {
            _isDamageMeterResetByMapChangeActive = value;
            SettingsController.CurrentSettings.IsDamageMeterResetByMapChangeActive = _isDamageMeterResetByMapChangeActive;
            OnPropertyChanged();
        }
    }

    public bool IsDamageMeterResetBeforeCombatActive
    {
        get => _isDamageMeterResetBeforeCombatActive;
        set
        {
            _isDamageMeterResetBeforeCombatActive = value;
            SettingsController.CurrentSettings.IsDamageMeterResetBeforeCombatActive = _isDamageMeterResetBeforeCombatActive;
            OnPropertyChanged();
        }
    }

    public bool ShortDamageMeterToClipboard
    {
        get => _shortDamageMeterToClipboard;
        set
        {
            _shortDamageMeterToClipboard = value;
            SettingsController.CurrentSettings.ShortDamageMeterToClipboard = ShortDamageMeterToClipboard;
            OnPropertyChanged();
        }
    }

    public bool OnlyDamageToPlayersCounts
    {
        get => _onlyDamageToPlayersCounts;
        set
        {
            _onlyDamageToPlayersCounts = value;
            SettingsController.CurrentSettings.OnlyDamageToPlayersCounts = OnlyDamageToPlayersCounts;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Damage Stats

    public ObservableCollection<DamageStatsEntry> TopSingleHits
    {
        get => _topSingleHits;
        set
        {
            _topSingleHits = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DamageStatsEntry> TopSingleHeals
    {
        get => _topSingleHeals;
        set
        {
            _topSingleHeals = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DamageStatsEntry> TopLastHits
    {
        get => _topLastHits;
        set
        {
            _topLastHits = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DamageStatsEntry> TopOverheals
    {
        get => _topOverheals;
        set
        {
            _topOverheals = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DamageStatsEntry> TopTakenDamage
    {
        get => _topTakenDamage;
        set
        {
            _topTakenDamage = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DamageStatsEntry> TopBurstDamageFiveSeconds
    {
        get => _topBurstDamageFiveSeconds;
        set
        {
            _topBurstDamageFiveSeconds = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DamageStatsEntry> TopBurstDamageTenSeconds
    {
        get => _topBurstDamageTenSeconds;
        set
        {
            _topBurstDamageTenSeconds = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DamageStatsEntry> TopAttackedTargets
    {
        get => _topAttackedTargets;
        set
        {
            _topAttackedTargets = value;
            OnPropertyChanged();
        }
    }

    public void SetDamageStats(DamageStatsSnapshot snapshot)
    {
        snapshot ??= DamageStatsSnapshot.Empty;
        CurrentDamageStats = snapshot;

        TopSingleHits = new ObservableCollection<DamageStatsEntry>(snapshot.TopSingleHits);
        TopSingleHeals = new ObservableCollection<DamageStatsEntry>(snapshot.TopSingleHeals);
        TopLastHits = new ObservableCollection<DamageStatsEntry>(snapshot.TopLastHits);
        TopOverheals = new ObservableCollection<DamageStatsEntry>(snapshot.TopOverheals);
        TopTakenDamage = new ObservableCollection<DamageStatsEntry>(snapshot.TopTakenDamage);
        TopBurstDamageFiveSeconds = new ObservableCollection<DamageStatsEntry>(snapshot.TopBurstDamageFiveSeconds);
        TopBurstDamageTenSeconds = new ObservableCollection<DamageStatsEntry>(snapshot.TopBurstDamageTenSeconds);
        TopAttackedTargets = new ObservableCollection<DamageStatsEntry>(snapshot.TopAttackedTargets);
    }

    public void ClearDamageStats()
    {
        SetDamageStats(DamageStatsSnapshot.Empty);
        CurrentYourStats = DamageMeterYourStatsSnapshot.Empty;
    }

    public DamageStatsSnapshot CurrentDamageStats
    {
        get => _currentDamageStats;
        set
        {
            _currentDamageStats = value ?? DamageStatsSnapshot.Empty;
            OnPropertyChanged();
        }
    }

    public DamageStatsSnapshot SnapshotDamageStats
    {
        get => _snapshotDamageStats;
        set
        {
            _snapshotDamageStats = value ?? DamageStatsSnapshot.Empty;
            OnPropertyChanged();
        }
    }

    public DamageMeterYourStatsSnapshot CurrentYourStats
    {
        get => _currentYourStats;
        set
        {
            _currentYourStats = value ?? DamageMeterYourStatsSnapshot.Empty;
            OnPropertyChanged();
        }
    }

    public DamageMeterYourStatsSnapshot SnapshotYourStats
    {
        get => _snapshotYourStats;
        set
        {
            _snapshotYourStats = value ?? DamageMeterYourStatsSnapshot.Empty;
            OnPropertyChanged();
        }
    }

    public void SetLocalPlayer(Guid? localPlayerGuid, string localPlayerName)
    {
        _localPlayerGuid = localPlayerGuid;
        _localPlayerName = localPlayerName ?? string.Empty;
        SetSnapshotYourStats();
    }

    public void SetYourStats(DamageMeterYourStatsSnapshot snapshot)
    {
        CurrentYourStats = snapshot ?? DamageMeterYourStatsSnapshot.Empty;
    }

    #endregion

    #region Damage Meter Snapshot

    public List<DamageMeterSnapshot> DamageMeterSnapshots
    {
        get => _damageMeterSnapshots;
        set
        {
            _damageMeterSnapshots = value;
            OnPropertyChanged();
        }
    }

    public DamageMeterSnapshot DamageMeterSnapshotSelection
    {
        get => _damageMeterSnapshotSelection;
        set
        {
            _damageMeterSnapshotSelection = value;
            SetDamageMeterSnapshotSort();
            SetSnapshotDamageStats();
            OnPropertyChanged();
        }
    }

    public DamageMeterSortStruct DamageMeterSnapshotSortSelection
    {
        get => _damageMeterSnapshotSortSelection;
        set
        {
            _damageMeterSnapshotSortSelection = value;
            SetDamageMeterSnapshotSort();

            OnPropertyChanged();
        }
    }

    public List<DamageMeterSortStruct> DamageMeterSnapshotSort
    {
        get => _damageMeterSnapshotSort;
        set
        {
            _damageMeterSnapshotSort = value;
            OnPropertyChanged();
        }
    }

    public void GetSnapshot(bool takeSnapshot = true, string location = null, bool isAutoSave = false)
    {
        if (!takeSnapshot)
        {
            return;
        }

        if (!DamageMeter.Any(x => x.Damage > 0 || x.Heal > 0))
        {
            return;
        }

        var snapshots = DamageMeterSnapshots;

        var damageMeterSnapshot = new DamageMeterSnapshot
        {
            Location = string.IsNullOrWhiteSpace(location) ? DamageMeterSnapshotLocationResolver.Resolve(ClusterController.CurrentCluster) : location,
            IsAutoSave = isAutoSave
        };

        foreach (var damageMeterFragment in DamageMeter)
        {
            damageMeterSnapshot.DamageMeter.Add(new DamageMeterSnapshotFragment(damageMeterFragment));
        }

        damageMeterSnapshot.DamageStats = DamageStatsSnapshotFactory.Clone(CurrentDamageStats);
        damageMeterSnapshot.YourStats = DamageMeterYourStatsSnapshotFactory.Clone(CurrentYourStats);
        DamageMeterSnapshots?.Add(damageMeterSnapshot);

        Application.Current.Dispatcher.Invoke(() =>
        {
            DamageMeterSnapshots = snapshots.OrderByDescending(x => x.Timestamp).ToList();
            DamageMeterSnapshotSelection = DamageMeterSnapshots.FirstOrDefault();
        });
    }

    public bool IsSnapshotAfterMapChangeActive
    {
        get => _isSnapshotAfterMapChangeActive;
        set
        {
            _isSnapshotAfterMapChangeActive = value;
            SettingsController.CurrentSettings.IsSnapshotAfterMapChangeActive = _isSnapshotAfterMapChangeActive;
            OnPropertyChanged();
        }
    }

    public void DeleteSelectedSnapshot()
    {
        var damageMeterSnapshotSelection = DamageMeterSnapshotSelection;
        if (damageMeterSnapshotSelection != null)
        {
            DamageMeterSnapshots?.Remove(damageMeterSnapshotSelection);
        }

        DamageMeterSnapshots = DamageMeterSnapshots?.ToList();
        DamageMeterSnapshotSelection = DamageMeterSnapshots?.FirstOrDefault();
        SetSnapshotDamageStats();
    }

    public void DeleteAllSnapshots()
    {
        if (DamageMeterSnapshots?.Count <= 0)
        {
            return;
        }

        var dialog = new DialogWindow(LocalizationController.Translation("DELETE_ALL_SNAPSHOTS"), LocalizationController.Translation("SURE_YOU_WANT_TO_DELETE_ALL_SNAPSHOTS"));
        var dialogResult = dialog.ShowDialog();

        if (dialogResult is true)
        {
            DamageMeterSnapshots = new List<DamageMeterSnapshot>();
            DamageMeterSnapshotSelection = null;
        }
    }

    public void SetDamageMeterSnapshotSort()
    {
        switch (DamageMeterSnapshotSortSelection.DamageMeterSortType)
        {
            case DamageMeterSortType.Damage:
                SetIsDamageMeterSnapshotShowing(DamageMeterSnapshotSelection?.DamageMeter, DamageMeterStyleFragmentType.Damage);
                if (DamageMeterSnapshotSelection != null)
                {
                    DamageMeterSnapshotSelection.DamageMeter = DamageMeterSnapshotSelection?.DamageMeter?.OrderByDescending(x => x.DamageInPercent).ToList();
                }
                return;
            case DamageMeterSortType.Dps:
                SetIsDamageMeterSnapshotShowing(DamageMeterSnapshotSelection?.DamageMeter, DamageMeterStyleFragmentType.Damage);
                if (DamageMeterSnapshotSelection != null)
                {
                    DamageMeterSnapshotSelection.DamageMeter = DamageMeterSnapshotSelection?.DamageMeter?.OrderByDescending(x => x.Dps).ToList();
                }
                return;
            case DamageMeterSortType.Name:
                SetIsDamageMeterSnapshotShowing(DamageMeterSnapshotSelection?.DamageMeter, DamageMeterStyleFragmentType.Damage);
                if (DamageMeterSnapshotSelection != null)
                {
                    DamageMeterSnapshotSelection.DamageMeter = DamageMeterSnapshotSelection?.DamageMeter?.OrderBy(x => x.Name).ToList();
                }
                return;
            case DamageMeterSortType.Heal:
                SetIsDamageMeterSnapshotShowing(DamageMeterSnapshotSelection?.DamageMeter, DamageMeterStyleFragmentType.Heal);
                if (DamageMeterSnapshotSelection != null)
                {
                    DamageMeterSnapshotSelection.DamageMeter = DamageMeterSnapshotSelection?.DamageMeter?.OrderByDescending(x => x.HealInPercent).ToList();
                }
                return;
            case DamageMeterSortType.Hps:
                SetIsDamageMeterSnapshotShowing(DamageMeterSnapshotSelection?.DamageMeter, DamageMeterStyleFragmentType.Heal);
                if (DamageMeterSnapshotSelection != null)
                {
                    DamageMeterSnapshotSelection.DamageMeter = DamageMeterSnapshotSelection?.DamageMeter?.OrderByDescending(x => x.Hps).ToList();
                }
                break;
            case DamageMeterSortType.TakenDamage:
                SetIsDamageMeterSnapshotShowing(DamageMeterSnapshotSelection?.DamageMeter, DamageMeterStyleFragmentType.TakenDamage);
                if (DamageMeterSnapshotSelection != null)
                {
                    DamageMeterSnapshotSelection.DamageMeter = DamageMeterSnapshotSelection?.DamageMeter?.OrderByDescending(x => x.TakenDamage).ToList();
                }
                break;
        }
    }
    
    private static void SetIsDamageMeterSnapshotShowing(IEnumerable<DamageMeterSnapshotFragment> damageMeter, DamageMeterStyleFragmentType damageMeterStyleFragmentType)
    {
        foreach (var fragment in damageMeter ?? new List<DamageMeterSnapshotFragment>())
        {
            fragment.DamageMeterStyleFragmentType = damageMeterStyleFragmentType;
        }
    }

    private void SetSnapshotDamageStats()
    {
        SnapshotDamageStats = DamageMeterSnapshotSelection?.DamageStats
                              ?? DamageStatsSnapshotFactory.FromSnapshotFragments(DamageMeterSnapshotSelection?.DamageMeter);
        SetSnapshotYourStats();
    }

    private void SetSnapshotYourStats()
    {
        var snapshotYourStats = DamageMeterSnapshotSelection?.YourStats;
        SnapshotYourStats = snapshotYourStats?.HasData == true
            ? snapshotYourStats
            : DamageMeterYourStatsSnapshotFactory.FromSnapshotFragments(DamageMeterSnapshotSelection?.DamageMeter, _localPlayerGuid, _localPlayerName);
    }

    #endregion

    #region Translations

    public static string TranslationSortByDamage => LocalizationController.Translation("SORT_BY_DAMAGE");
    public static string TranslationSortByDps => LocalizationController.Translation("SORT_BY_DPS");
    public static string TranslationSortByName => LocalizationController.Translation("SORT_BY_NAME");
    public static string TranslationSortByHeal => LocalizationController.Translation("SORT_BY_HEAL");
    public static string TranslationSortByHps => LocalizationController.Translation("SORT_BY_HPS");
    public static string TranslationTakenDamage => LocalizationController.Translation("TAKEN_DAMAGE");
    public static string TranslationSnapshots => LocalizationController.Translation("SNAPSHOTS");
    public static string TranslationDeleteSelectedSnapshot => LocalizationController.Translation("DELETE_SELECTED_SNAPSHOT");
    public static string TranslationDeleteAllSnapshots => LocalizationController.Translation("DELETE_ALL_SNAPSHOTS");
    public static string TranslationTakeASnapshotOfDamageMeterDescription => LocalizationController.Translation("TAKE_A_SNAPSHOT_OF_DAMAGE_METER_DESCRIPTION");
    public static string TranslationDamageStats => LocalizationController.Translation("DAMAGE_STATS");
    public static string TranslationLive => LocalizationController.Translation("LIVE");
    public static string TranslationMeter => LocalizationController.Translation("METER");
    public static string TranslationStats => LocalizationController.Translation("STATS");
    public static string TranslationTopStats => LocalizationController.Translation("TOP_STATS");
    public static string TranslationYourStats => LocalizationController.Translation("YOUR_STATS");
    public static string TranslationNoLocalDamageStatsAvailable => LocalizationController.Translation("NO_LOCAL_DAMAGE_STATS_AVAILABLE");
    public static string TranslationYourStatsDamageSection => LocalizationController.Translation("YOUR_STATS_DAMAGE_SECTION");
    public static string TranslationYourStatsHealingSection => LocalizationController.Translation("YOUR_STATS_HEALING_SECTION");
    public static string TranslationYourStatsDefenseSection => LocalizationController.Translation("YOUR_STATS_DEFENSE_SECTION");
    public static string TranslationYourStatsCombatSection => LocalizationController.Translation("YOUR_STATS_COMBAT_SECTION");
    public static string TranslationTotalDamage => LocalizationController.Translation("TOTAL_DAMAGE");
    public static string TranslationTotalDps => LocalizationController.Translation("TOTAL_DPS");
    public static string TranslationPeakDpsThreeSeconds => LocalizationController.Translation("PEAK_DPS_3_SECONDS");
    public static string TranslationPeakDpsFiveSeconds => LocalizationController.Translation("PEAK_DPS_5_SECONDS");
    public static string TranslationPeakDpsTenSeconds => LocalizationController.Translation("PEAK_DPS_10_SECONDS");
    public static string TranslationBiggestHit => LocalizationController.Translation("BIGGEST_HIT");
    public static string TranslationTotalHealing => LocalizationController.Translation("TOTAL_HEALING");
    public static string TranslationEffectiveHealing => LocalizationController.Translation("EFFECTIVE_HEALING");
    public static string TranslationOverhealPercent => LocalizationController.Translation("OVERHEAL_PERCENT");
    public static string TranslationDamageTaken => LocalizationController.Translation("DAMAGE_TAKEN");
    public static string TranslationBiggestDamageTakenBySpellTopThree => LocalizationController.Translation("BIGGEST_DAMAGE_TAKEN_BY_SPELL_TOP_3");
    public static string TranslationDtps => LocalizationController.Translation("DTPS");
    public static string TranslationDtpsTooltip => LocalizationController.Translation("DTPS_TOOLTIP");
    public static string TranslationCombatTime => LocalizationController.Translation("COMBAT_TIME");
    public static string TranslationFightCount => LocalizationController.Translation("FIGHT_COUNT");
    public static string TranslationAverageFightDuration => LocalizationController.Translation("AVERAGE_FIGHT_DURATION");
    public static string TranslationPveDamage => LocalizationController.Translation("PVE_DAMAGE");
    public static string TranslationPvpDamage => LocalizationController.Translation("PVP_DAMAGE");
    public static string TranslationHealingTargetsTopThree => LocalizationController.Translation("HEALING_TARGETS_TOP_3");
    public static string TranslationBiggestSingleHitTop5 => LocalizationController.Translation("BIGGEST_SINGLE_HIT_TOP_5");
    public static string TranslationBiggestSingleHealTop5 => LocalizationController.Translation("BIGGEST_SINGLE_HEAL_TOP_5");
    public static string TranslationLastHitsTop5 => LocalizationController.Translation("LAST_HITS_TOP_5");
    public static string TranslationOverhealTop5 => LocalizationController.Translation("OVERHEAL_TOP_5");
    public static string TranslationDamageTakenTop5 => LocalizationController.Translation("DAMAGE_TAKEN_TOP_5");
    public static string TranslationBurstDamageFiveSecondsTop5 => LocalizationController.Translation("BURST_DAMAGE_5_SECONDS_TOP_5");
    public static string TranslationBurstDamageTenSecondsTop5 => LocalizationController.Translation("BURST_DAMAGE_10_SECONDS_TOP_5");
    public static string TranslationAttackedMobsPlayersTop5 => LocalizationController.Translation("ATTACKED_MOBS_PLAYERS_TOP_5");

    #endregion

    #region Load file

    private async Task LoadLocalFileAsync()
    {
        DamageMeterSnapshots = await FileController.LoadAsync<List<DamageMeterSnapshot>>(
            AppDataPaths.UserDataFile(Settings.Default.DamageMeterSnapshotsFileName));
    }

    #endregion
}