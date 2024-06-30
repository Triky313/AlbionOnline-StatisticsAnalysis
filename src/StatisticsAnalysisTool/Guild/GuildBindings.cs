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
    public ListCollectionView SiphonedEnergyCollectionView { get; set; }
    private ManuallySiphonedEnergy _manuallySiphonedEnergy;
    private GridLength _gridSplitterPosition;
    private ObservableRangeCollection<SiphonedEnergyItem> _siphonedEnergyList = new();
    private ObservableRangeCollection<SiphonedEnergyItem> _siphonedEnergyOverviewList = new();
    private Visibility _guildPopupVisibility = Visibility.Collapsed;
    private bool _isDeleteEntriesButtonEnabled = true;
    private DateTime _siphonedEnergyLastUpdate;
    private Visibility _siphonedEnergyLastUpdateVisibility = Visibility.Collapsed;
    private long _totalSiphonedEnergyQuantity;


    public GuildBindings()
    {
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

    public string TranslationSiphonedEnergy => LocalizationController.Translation("SIPHONED_ENERGY");
    public string TranslationDeleteSelectedEntries => LocalizationController.Translation("DELETE_SELECTED_ENTRIES");
    public string TranslationSelectDeselectAll => LocalizationController.Translation("SELECT_DESELECT_ALL");
    public string TranslationLastUpdate => LocalizationController.Translation("LAST_UPDATE");
    public string TranslationLootLogChecker => LocalizationController.Translation("LOOT_LOG_CHECKER");
    public string TranslationTotal => LocalizationController.Translation("TOTAL");
}