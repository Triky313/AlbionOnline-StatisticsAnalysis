using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.ObjectModel;
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

    public string TranslationSiphonedEnergy => LanguageController.Translation("SIPHONED_ENERGY");
    public string TranslationDeleteSelectedEntries => LanguageController.Translation("DELETE_SELECTED_ENTRIES");
    public string TranslationSelectDeselectAll => LanguageController.Translation("SELECT_DESELECT_ALL");
    public string TranslationLastUpdate => LanguageController.Translation("LAST_UPDATE");
    public string TranslationLootLogChecker => LanguageController.Translation("LOOT_LOG_CHECKER");
}