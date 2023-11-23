using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Guild;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace StatisticsAnalysisTool.ViewModels;

public class GuildViewModel : BaseViewModel
{
    private readonly MainWindowViewModelOld _mainWindowViewModel;
    private readonly IGuildController _guildController;
    private ManuallySiphonedEnergy _manuallySiphonedEnergy;
    private GridLength _gridSplitterPosition;
    private ObservableRangeCollection<SiphonedEnergyItem> _siphonedEnergyList = new();
    private ObservableRangeCollection<SiphonedEnergyItem> _siphonedEnergyOverviewList = new();
    private Visibility _guildPopupVisibility = Visibility.Collapsed;
    private bool _isDeleteEntriesButtonEnabled = true;
    private DateTime _siphonedEnergyLastUpdate;
    private Visibility _siphonedEnergyLastUpdateVisibility = Visibility.Collapsed;
    private long _totalSiphonedEnergyQuantity;
    private bool _isSelectAllActive;

    public ListCollectionView SiphonedEnergyCollectionView { get; set; }
    public ICommand DeleteSelectedSiphonedEnergyEntriesCommand { get; }
    public ICommand OpenSiphonedEnergyInfoPopupCommand { get; }
    public ICommand CloseSiphonedEnergyInfoPopupCommand { get; }
    public ICommand SelectSwitchAllSiphonedEnergyEntriesCommand { get; }

    public GuildViewModel(MainWindowViewModelOld mainWindowViewModel, IGuildController guildController)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _guildController = guildController;

        GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.GuildGridSplitterPosition);

        DeleteSelectedSiphonedEnergyEntriesCommand = new AsyncCommandHandler(ExecuteDeleteSelectedSiphonedEnergyEntriesCommand, true);
        OpenSiphonedEnergyInfoPopupCommand = new CommandHandler(ExecuteOpenSiphonedEnergyInfoPopupCommand, true);
        CloseSiphonedEnergyInfoPopupCommand = new CommandHandler(ExecuteCloseSiphonedEnergyInfoPopupCommand, true);
        SelectSwitchAllSiphonedEnergyEntriesCommand = new CommandHandler(ExecuteSelectSwitchAllSiphonedEnergyEntriesCommand, true);

        SiphonedEnergyCollectionView = CollectionViewSource.GetDefaultView(SiphonedEnergyList) as ListCollectionView;

        if (SiphonedEnergyCollectionView != null)
        {
            SiphonedEnergyCollectionView.IsLiveSorting = true;
            SiphonedEnergyCollectionView.IsLiveFiltering = true;
            SiphonedEnergyCollectionView.CustomSort = new SiphonedEnergyItemComparer();

            SiphonedEnergyCollectionView?.Refresh();
        }

        ManuallySiphonedEnergy = new ManuallySiphonedEnergy();
    }

    private async Task ExecuteDeleteSelectedSiphonedEnergyEntriesCommand(object parameter)
    {
        var dialog = new DialogWindow(LanguageController.Translation("DELETE_SELECTED_ENTRIES"), LanguageController.Translation("SURE_YOU_WANT_TO_DELETE_SELECTED_ENTRIES"));
        var dialogResult = dialog.ShowDialog();

        if (_mainWindowViewModel == null)
        {
            return;
        }

        if (dialogResult is true)
        {
            var selectedEntries = SiphonedEnergyCollectionView?.Cast<SiphonedEnergyItem>()
                .ToList()
                .Where(x => x?.IsSelectedForDeletion ?? false)
                .Select(x => x.GetHashCode());

            IsDeleteEntriesButtonEnabled = false;
            await _guildController?.RemoveTradesByIdsAsync(selectedEntries)!;
            IsDeleteEntriesButtonEnabled = true;
        }
    }

    private void ExecuteOpenSiphonedEnergyInfoPopupCommand(object parameter)
    {
        GuildPopupVisibility = Visibility.Visible;
    }

    private void ExecuteCloseSiphonedEnergyInfoPopupCommand(object parameter)
    {
        GuildPopupVisibility = Visibility.Collapsed;
    }

    private void ExecuteSelectSwitchAllSiphonedEnergyEntriesCommand(object parameter)
    {
        if (SiphonedEnergyList is null)
        {
            return;
        }

        foreach (var item in SiphonedEnergyList)
        {
            item.IsSelectedForDeletion = !_isSelectAllActive;
        }

        _isSelectAllActive = !_isSelectAllActive;
    }

    public ObservableRangeCollection<SiphonedEnergyItem> SiphonedEnergyList
    {
        get => _siphonedEnergyList;
        set
        {
            _siphonedEnergyList = value;
            OnPropertyChanged();
        }
    }

    public ObservableRangeCollection<SiphonedEnergyItem> SiphonedEnergyOverviewList
    {
        get => _siphonedEnergyOverviewList;
        set
        {
            _siphonedEnergyOverviewList = value;
            OnPropertyChanged();
        }
    }

    public GridLength GridSplitterPosition
    {
        get => _gridSplitterPosition;
        set
        {
            _gridSplitterPosition = value;
            SettingsController.CurrentSettings.GuildGridSplitterPosition = _gridSplitterPosition.Value;
            OnPropertyChanged();
        }
    }

    public ManuallySiphonedEnergy ManuallySiphonedEnergy
    {
        get => _manuallySiphonedEnergy;
        set
        {
            _manuallySiphonedEnergy = value;
            OnPropertyChanged();
        }
    }

    public long TotalSiphonedEnergyQuantity
    {
        get => _totalSiphonedEnergyQuantity;
        set
        {
            _totalSiphonedEnergyQuantity = value;
            OnPropertyChanged();
        }
    }

    public Visibility GuildPopupVisibility
    {
        get => _guildPopupVisibility;
        set
        {
            _guildPopupVisibility = value;
            OnPropertyChanged();
        }
    }

    public bool IsDeleteEntriesButtonEnabled
    {
        get => _isDeleteEntriesButtonEnabled;
        set
        {
            _isDeleteEntriesButtonEnabled = value;
            OnPropertyChanged();
        }
    }

    public DateTime SiphonedEnergyLastUpdate
    {
        get => _siphonedEnergyLastUpdate;
        set
        {
            _siphonedEnergyLastUpdate = value;
            OnPropertyChanged();
        }
    }

    public Visibility SiphonedEnergyLastUpdateVisibility
    {
        get => _siphonedEnergyLastUpdateVisibility;
        set
        {
            _siphonedEnergyLastUpdateVisibility = value;
            OnPropertyChanged();
        }
    }

    public static string TranslationSiphonedEnergy => LanguageController.Translation("SIPHONED_ENERGY");
    public static string TranslationDeleteSelectedEntries => LanguageController.Translation("DELETE_SELECTED_ENTRIES");
    public static string TranslationSelectDeselectAll => LanguageController.Translation("SELECT_DESELECT_ALL");
    public static string TranslationLastUpdate => LanguageController.Translation("LAST_UPDATE");
    public static string TranslationLootLogChecker => LanguageController.Translation("LOOT_LOG_CHECKER");
    public static string TranslationTotal => LanguageController.Translation("TOTAL");
    public static string TranslationSiphonedEnergyDescription1 => LanguageController.Translation("SIPHONED_ENERGY_DESCRIPTION1");
    public static string TranslationSiphonedEnergyDescription2 => LanguageController.Translation("SIPHONED_ENERGY_DESCRIPTION2");
    public static string TranslationSiphonedEnergyDescription3 => LanguageController.Translation("SIPHONED_ENERGY_DESCRIPTION3");
    public static string TranslationSiphonedEnergyDescription4 => LanguageController.Translation("SIPHONED_ENERGY_DESCRIPTION4");
}