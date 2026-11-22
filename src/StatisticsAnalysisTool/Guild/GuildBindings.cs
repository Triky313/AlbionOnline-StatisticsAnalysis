using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Guild;

public class GuildBindings : BaseViewModel
{
    public ListCollectionView SiphonedEnergyCollectionView { get; }
    public ListCollectionView SiphonedEnergyOverviewView { get; }
    private ManuallySiphonedEnergy _manuallySiphonedEnergy;
    private GridLength _gridSplitterPosition;
    private string _overviewNameFilter = string.Empty;
    private string _detailsNameFilter = string.Empty;
    private Visibility _guildPopupVisibility = Visibility.Collapsed;
    private bool _isDeleteEntriesButtonEnabled = true;
    private DateTime _siphonedEnergyLastUpdate;
    private Visibility _siphonedEnergyLastUpdateVisibility = Visibility.Collapsed;
    private long _totalSiphonedEnergyQuantity;


    public GuildBindings()
    {
        SiphonedEnergyCollectionView = new ListCollectionView(SiphonedEnergyList)
        {
            IsLiveSorting = true,
            CustomSort = new SiphonedEnergyItemComparer(),
            Filter = item => MatchesCharacterName(item, DetailsNameFilter)
        };
        SiphonedEnergyOverviewView = new ListCollectionView(SiphonedEnergyOverviewList)
        {
            Filter = item => MatchesCharacterName(item, OverviewNameFilter)
        };

        ManuallySiphonedEnergy = new ManuallySiphonedEnergy();
    }

    public ObservableRangeCollection<SiphonedEnergyItem> SiphonedEnergyList { get; } = new();
    public ObservableRangeCollection<SiphonedEnergyItem> SiphonedEnergyOverviewList { get; } = new();

    public string OverviewNameFilter
    {
        get => _overviewNameFilter;
        set
        {
            _overviewNameFilter = value;
            SiphonedEnergyOverviewView.Refresh();
            OnPropertyChanged();
        }
    }

    public string DetailsNameFilter
    {
        get => _detailsNameFilter;
        set
        {
            _detailsNameFilter = value;
            SiphonedEnergyCollectionView.Refresh();
            OnPropertyChanged();
        }
    }

    private static bool MatchesCharacterName(object item, string filter)
    {
        return string.IsNullOrWhiteSpace(filter)
               || (item as SiphonedEnergyItem)?.CharacterName?.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase) == true;
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

    public string TranslationSiphonedEnergy => LocalizationController.Translation("SIPHONED_ENERGY");
    public string TranslationTotalOverview => LocalizationController.Translation("TOTAL_OVERVIEW");
    public string TranslationDetails => LocalizationController.Translation("DETAILS");
    public string TranslationDeleteSelectedEntries => LocalizationController.Translation("DELETE_SELECTED_ENTRIES");
    public string TranslationSelectDeselectAll => LocalizationController.Translation("SELECT_DESELECT_ALL");
    public string TranslationLastUpdate => LocalizationController.Translation("LAST_UPDATE");
    public string TranslationLootLogChecker => LocalizationController.Translation("LOOT_LOG_CHECKER");
    public string TranslationTotal => LocalizationController.Translation("TOTAL");
}
