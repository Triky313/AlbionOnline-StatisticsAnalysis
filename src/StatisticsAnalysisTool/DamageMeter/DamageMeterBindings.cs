using FontAwesome5;
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

    public void GetSnapshot(bool takeSnapshot = true)
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

        var damageMeterSnapshot = new DamageMeterSnapshot();
        foreach (var damageMeterFragment in DamageMeter)
        {
            damageMeterSnapshot.DamageMeter.Add(new DamageMeterSnapshotFragment(damageMeterFragment));
        }

        DamageMeterSnapshots?.Add(damageMeterSnapshot);

        Application.Current.Dispatcher.Invoke(() =>
        {
            DamageMeterSnapshots = snapshots.OrderByDescending(x => x.Timestamp).ToList();
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

    #endregion

    #region Load file

    private async Task LoadLocalFileAsync()
    {
        FileController.TransferFileIfExistFromOldPathToUserDataDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.DamageMeterSnapshotsFileName));
        DamageMeterSnapshots = await FileController.LoadAsync<List<DamageMeterSnapshot>>(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.DamageMeterSnapshotsFileName));
    }

    #endregion
}