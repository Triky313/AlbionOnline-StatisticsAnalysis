using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class GatheringBindings : INotifyPropertyChanged
{
    private bool _isGatheringActive = true;
    private GatheringStats _gatheringStats = new();
    private ObservableRangeCollection<Gathered> _gatheredCollection = new();
    private readonly ListCollectionView _gatheredCollectionView;
    private GridLength _gridSplitterPosition = GridLength.Auto;
    private Dictionary<ItemTier, string> _tierFilter = FrequentlyValues.ItemTiers;
    private ItemTier _selectedTierFilter = ItemTier.T8;
    private Dictionary<GatheringFilterType, string> _gatheringFilter = new()
    {
        { GatheringFilterType.Generally, LanguageController.Translation("GENERALLY") },
        { GatheringFilterType.Wood, LanguageController.Translation("WOOD") },
        { GatheringFilterType.Fiber, LanguageController.Translation("FIBER") },
        { GatheringFilterType.Hide, LanguageController.Translation("HIDE") },
        { GatheringFilterType.Ore, LanguageController.Translation("ORE") },
        { GatheringFilterType.Rock, LanguageController.Translation("ROCK") }
    };
    private GatheringFilterType _selectedGatheringFilter = GatheringFilterType.Generally;

    public GatheringBindings()
    {
        IsGatheringActive = SettingsController.CurrentSettings.IsGatheringActive;

        GatheredCollectionView = CollectionViewSource.GetDefaultView(GatheredCollection) as ListCollectionView;

        if (GatheredCollectionView != null)
        {
            GatheredCollectionView.IsLiveSorting = true;
            GatheredCollectionView.IsLiveFiltering = true;
            GatheredCollectionView.CustomSort = new GatheredComparer();

            GatheredCollectionView?.Refresh();
        }

        GatheredCollection.CollectionChanged += UpdateStats;
    }
    
    public void UpdateStats(object sender, NotifyCollectionChangedEventArgs e)
    {
        var hide = GatheredCollection?.Where(x => x?.Item?.ShopShopSubCategory1 == ShopSubCategory.Hide) ?? new List<Gathered>();
        GatheringStats.GatheredHide = new ObservableRangeCollection<Gathered>(hide);
    }

    #region Bindings

    public ListCollectionView GatheredCollectionView
    {
        get => _gatheredCollectionView;
        init
        {
            _gatheredCollectionView = value;
            OnPropertyChanged();
        }
    }

    public ObservableRangeCollection<Gathered> GatheredCollection
    {
        get => _gatheredCollection;
        set
        {
            _gatheredCollection = value;
            OnPropertyChanged();
        }
    }

    public GridLength GridSplitterPosition
    {
        get => _gridSplitterPosition;
        set
        {
            _gridSplitterPosition = value;
            SettingsController.CurrentSettings.GatheringGridSplitterPosition = _gridSplitterPosition.Value;
            OnPropertyChanged();
        }
    }

    public GatheringStats GatheringStats
    {
        get => _gatheringStats;
        set
        {
            _gatheringStats = value;
            OnPropertyChanged();
        }
    }

    public bool IsGatheringActive
    {
        get => _isGatheringActive;
        set
        {
            _isGatheringActive = value;
            SettingsController.CurrentSettings.IsGatheringActive = _isGatheringActive;
            OnPropertyChanged();
        }
    }

    public Dictionary<ItemTier, string> TierFilter
    {
        get => _tierFilter;
        set
        {
            _tierFilter = value;
            OnPropertyChanged();
        }
    }

    public ItemTier SelectedTierFilter
    {
        get => _selectedTierFilter;
        set
        {
            _selectedTierFilter = value;
            OnPropertyChanged();
        }
    }

    public Dictionary<GatheringFilterType, string> GatheringFilter
    {
        get => _gatheringFilter;
        set
        {
            _gatheringFilter = value;
            OnPropertyChanged();
        }
    }

    public GatheringFilterType SelectedGatheringFilter
    {
        get => _selectedGatheringFilter;
        set
        {
            _selectedGatheringFilter = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public static string TranslationGatheringActive => LanguageController.Translation("GATHERING_ACTIVE");

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}