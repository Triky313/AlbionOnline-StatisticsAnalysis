using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Crafting;

public class CraftingBindings : BaseViewModel
{
    private readonly CraftingCalculator _calculator = new();
    private readonly CraftingRecipeResolver _recipeResolver = new();
    private readonly CraftingController _controller = new();
    private string _itemSearchText;
    private string _craftingLocationSearchText;
    private int _amountCrafted = 1;
    private bool _isLoading;

    public CraftingBindings()
    {
        CraftableItems = new ObservableCollection<Item>(ItemController.Items
            .Where(_recipeResolver.IsCraftable)
            .OrderBy(x => x.LocalizedName)
            .ToList());
        CraftableItemsView = CollectionViewSource.GetDefaultView(CraftableItems);
        CraftableItemsView.Filter = FilterCraftableItem;
        SelectedDailyBonus = DailyBonusOptions.First();
        SelectedHideoutBonus = HideoutBonusOptions.First();
        RefreshCraftingLocations(null);

        _ = LoadAsync();
    }

    public ObservableCollection<Item> CraftableItems
    {
        get;
    }
    = [];

    public ICollectionView CraftableItemsView
    {
        get;
    }

    public ObservableCollection<CraftingResourceEntry> Resources
    {
        get;
    }
    = [];

    public ObservableCollection<SavedCrafting> SavedCraftings
    {
        get;
    }
    = [];

    public ObservableCollection<CraftingItemSearchResult> ListBoxItemSearchItems
    {
        get;
    }
    = [];

    public ObservableCollection<CraftingLocationOption> CraftingLocations
    {
        get;
    }
    = [];

    public ObservableCollection<CraftingLocationOption> ListBoxCraftingLocationItems
    {
        get;
    }
    = [];

    public CraftingDailyBonusOption[] DailyBonusOptions
    {
        get;
    }
    =
    [
        new CraftingDailyBonusOption
        {
            Name = "None",
            BonusPercent = 0m
        }
        ,
        new CraftingDailyBonusOption
        {
            Name = "10%",
            BonusPercent = 10m
        }
        ,
        new CraftingDailyBonusOption
        {
            Name = "20%",
            BonusPercent = 20m
        }
    ];

    public CraftingHideoutBonusOption[] HideoutBonusOptions
    {
        get;
    }
    = HideoutData.GetHideoutBonusOptions();

    public KeyValuePair<MarketLocation, string>[] MarketLocations
    {
        get;
    }
    =
    [
        new(MarketLocation.BlackMarket, "Black Market"),
        .. Locations.OnceMarketLocations
    ];

    public Dictionary<ItemTier, string> ItemTiers => FrequentlyValues.ItemTiers;

    public Dictionary<ItemLevel, string> ItemLevels => FrequentlyValues.ItemLevels;

    public Item SelectedItem
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
            _ = ApplySelectedItemAsync(value);
        }
    }

    public SavedCrafting SelectedSavedCrafting
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string ItemSearchText
    {
        get => _itemSearchText;
        set
        {
            _itemSearchText = value;
            CraftableItemsView?.Refresh();
            UpdateItemSearchListBox(value);
            OnPropertyChanged();
        }
    }

    public bool IsItemSearchPopupOpen
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string CraftingLocationSearchText
    {
        get => _craftingLocationSearchText;
        set
        {
            _craftingLocationSearchText = value;
            UpdateCraftingLocationListBox(value);
            OnPropertyChanged();
        }
    }

    public bool IsCraftingLocationPopupOpen
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ItemTier SelectedItemTier
    {
        get;
        set
        {
            field = value;
            CraftableItemsView?.Refresh();
            UpdateItemSearchListBox(ItemSearchText);
            OnPropertyChanged();
        }
    }
    = ItemTier.Unknown;

    public ItemLevel SelectedItemLevel
    {
        get;
        set
        {
            field = value;
            CraftableItemsView?.Refresh();
            UpdateItemSearchListBox(ItemSearchText);
            OnPropertyChanged();
        }
    }
    = ItemLevel.Unknown;

    public int CraftingRuns
    {
        get;
        set
        {
            field = Math.Max(1, value);
            Recalculate();
            OnPropertyChanged();
        }
    }
    = 1;

    public bool UsesFocus
    {
        get;
        set
        {
            field = value;
            Recalculate();
            OnPropertyChanged();
        }
    }

    public decimal ReturnRatePercent
    {
        get;
        set
        {
            field = Math.Clamp(value, 0m, 100m);
            Recalculate();
            OnPropertyChanged();
        }
    }

    public CraftingLocationOption SelectedCraftingLocation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedCraftingLocationBonusSummary));
        }
    }

    public CraftingDailyBonusOption SelectedDailyBonus
    {
        get;
        set
        {
            field = value ?? DailyBonusOptions.First();
            ApplySelectedCraftingLocationReturnRate();
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedCraftingLocationBonusSummary));
        }
    }

    public CraftingHideoutBonusOption SelectedHideoutBonus
    {
        get;
        set
        {
            field = value ?? HideoutBonusOptions.First();
            ApplySelectedCraftingLocationReturnRate();
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedCraftingLocationBonusSummary));
        }
    }

    public string SelectedCraftingLocationBonusSummary => SelectedCraftingLocation == null
        ? string.Empty
        : "Bonus "
          + EffectiveCraftingBonusPercent.ToString("N2")
          + "% | expected RRR "
          + GetSelectedCraftingLocationReturnRate().ToString("N2")
          + "%";

    public decimal EffectiveCraftingBonusPercent => (SelectedCraftingLocation?.TotalProductionBonusPercent ?? 0m)
                                                    + (SelectedDailyBonus?.BonusPercent ?? 0m)
                                                    + GetSelectedHideoutBonusPercent();

    public MarketLocation SelectedMarketLocation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
    = MarketLocation.CaerleonMarket;

    public decimal StationFee
    {
        get;
        set
        {
            field = Math.Max(0m, value);
            Recalculate();
            OnPropertyChanged();
        }
    }

    public decimal SalesTaxPercent
    {
        get;
        set
        {
            field = Math.Clamp(value, 0m, 100m);
            Recalculate();
            OnPropertyChanged();
        }
    }
    = 4m;

    public decimal OtherCosts
    {
        get;
        set
        {
            field = Math.Max(0m, value);
            Recalculate();
            OnPropertyChanged();
        }
    }

    public decimal OutputUnitPrice
    {
        get;
        set
        {
            field = Math.Max(0m, value);
            Recalculate();
            OnPropertyChanged();
        }
    }

    public string Notes
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public CraftingJournalEntry Journal
    {
        get;
        set
        {
            if (field != null)
            {
                field.ValuesChanged = null;
            }

            field = value;

            if (field != null)
            {
                field.ValuesChanged = Recalculate;
            }

            Recalculate();
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasJournal));
        }
    }

    public CraftingCalculationResult Calculation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
    = new();

    public string StatusText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ICommand NewCommand => field ??= new CommandHandler(_ => NewCrafting(), true);

    public ICommand SaveCommand => field ??= new CommandHandler(_ => SaveCurrentAsync(), true);

    public ICommand DeleteCommand => field ??= new CommandHandler(_ => DeleteSelectedAsync(), true);

    public ICommand ResetCommand => field ??= new CommandHandler(_ => ResetEditor(), true);

    public ICommand LoadSelectedCommand => field ??= new CommandHandler(LoadSelectedCrafting, true);

    public ICommand LoadPricesCommand => field ??= new CommandHandler(_ => LoadPricesAsync(), true);

    public bool HasJournal => Journal != null;

    public void SelectItemSearchResult(CraftingItemSearchResult searchResult)
    {
        if (searchResult?.Value == null)
        {
            return;
        }

        _itemSearchText = searchResult.Name;
        OnPropertyChanged(nameof(ItemSearchText));
        SelectedItem = searchResult.Value;
        IsItemSearchPopupOpen = false;
    }

    public void OpenCraftingLocationSearch()
    {
        UpdateCraftingLocationListBox(CraftingLocationSearchText);
    }

    public void SelectCraftingLocation(CraftingLocationOption location)
    {
        if (location == null)
        {
            return;
        }

        SelectedCraftingLocation = location;
        _craftingLocationSearchText = location.DisplayName;
        OnPropertyChanged(nameof(CraftingLocationSearchText));
        IsCraftingLocationPopupOpen = false;
        ApplySelectedCraftingLocationReturnRate();
    }

    public async Task LoadAsync()
    {
        var craftings = await _controller.LoadAsync();
        SavedCraftings.Clear();

        foreach (var crafting in craftings)
        {
            PrepareSavedCrafting(crafting);
            SavedCraftings.Add(crafting);
        }
    }

    private bool FilterCraftableItem(object value)
    {
        if (value is not Item item)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(ItemSearchText))
        {
            return true;
        }

        return ItemMatchesFilter(item)
               && ItemMatchesSearchText(item, ItemSearchText);
    }

    private void UpdateItemSearchListBox(string searchText)
    {
        ListBoxItemSearchItems.Clear();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            IsItemSearchPopupOpen = false;
            return;
        }

        foreach (var item in CraftableItems
                     .Where(x => ItemMatchesFilter(x) && ItemMatchesSearchText(x, searchText))
                     .Take(25))
        {
            ListBoxItemSearchItems.Add(new CraftingItemSearchResult
            {
                Name = item.LocalizedName,
                Icon = item.Icon,
                Value = item
            }
            );
        }

        IsItemSearchPopupOpen = ListBoxItemSearchItems.Count > 0;
    }

    private void UpdateCraftingLocationListBox(string searchText)
    {
        ListBoxCraftingLocationItems.Clear();

        var locations = CraftingLocations
            .Where(x => CraftingLocationMatchesSearchText(x, searchText))
            .Take(20)
            .ToList();

        foreach (var location in locations)
        {
            ListBoxCraftingLocationItems.Add(location);
        }

        IsCraftingLocationPopupOpen = ListBoxCraftingLocationItems.Count > 0;
    }

    private static bool ItemMatchesSearchText(Item item, string searchText)
    {
        return (item.LocalizedNameAndEnglish ?? item.UniqueName ?? string.Empty)
            .Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }

    private static bool CraftingLocationMatchesSearchText(CraftingLocationOption location, string searchText)
    {
        if (location == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        return (location.DisplayName ?? string.Empty).Contains(searchText, StringComparison.OrdinalIgnoreCase)
               || (location.ClusterId ?? string.Empty).Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }

    private bool ItemMatchesFilter(Item item)
    {
        var tierMatch = SelectedItemTier == ItemTier.Unknown || (ItemTier) item.Tier == SelectedItemTier;
        var levelMatch = SelectedItemLevel == ItemLevel.Unknown || (ItemLevel) item.Level == SelectedItemLevel;

        return tierMatch && levelMatch;
    }

    private async Task ApplySelectedItemAsync(Item item)
    {
        if (_isLoading || item == null)
        {
            return;
        }

        _amountCrafted = _recipeResolver.GetAmountCrafted(item);
        RefreshCraftingLocations(item, SelectedCraftingLocation?.ClusterId);
        Resources.Clear();

        foreach (var resource in _recipeResolver.GetResources(item))
        {
            AddResource(resource);
        }

        Journal = _recipeResolver.GetJournal(item);
        Notes = await Common.CraftingTabController.GetNoteAsync(item.UniqueName);
        Recalculate();
    }

    private void RefreshCraftingLocations(Item item, string selectedClusterId = null)
    {
        var locations = CraftingLocationData.GetCraftingLocations(item);
        var selectedId = selectedClusterId ?? SelectedCraftingLocation?.ClusterId;

        CraftingLocations.Clear();

        foreach (var location in locations)
        {
            CraftingLocations.Add(location);
        }

        SelectedCraftingLocation = CraftingLocations.FirstOrDefault(x => string.Equals(x.ClusterId, selectedId, StringComparison.OrdinalIgnoreCase))
                                   ?? CraftingLocations.FirstOrDefault(x => string.Equals(x.ClusterId, "3003", StringComparison.OrdinalIgnoreCase))
                                   ?? CraftingLocations.FirstOrDefault();
        _craftingLocationSearchText = SelectedCraftingLocation?.DisplayName;
        OnPropertyChanged(nameof(CraftingLocationSearchText));
        ApplySelectedCraftingLocationReturnRate();
        UpdateCraftingLocationListBox(_craftingLocationSearchText);
        IsCraftingLocationPopupOpen = false;
    }

    private void ApplySelectedCraftingLocationReturnRate()
    {
        if (_isLoading)
        {
            OnPropertyChanged(nameof(SelectedCraftingLocationBonusSummary));
            return;
        }

        ReturnRatePercent = GetSelectedCraftingLocationReturnRate();
        OnPropertyChanged(nameof(SelectedCraftingLocationBonusSummary));
    }

    private decimal GetSelectedCraftingLocationReturnRate()
    {
        return CraftingLocationData.GetExpectedReturnRatePercent(EffectiveCraftingBonusPercent);
    }

    private decimal GetSelectedHideoutBonusPercent()
    {
        if (SelectedHideoutBonus == null || !IsHideoutCraftingLocation())
        {
            return 0m;
        }

        return SelectedHideoutBonus.GetBonusPercent(ShouldApplySpecialistHideoutBonus());
    }

    private bool IsHideoutCraftingLocation()
    {
        var clusterType = SelectedCraftingLocation?.ClusterType ?? string.Empty;
        var clusterId = SelectedCraftingLocation?.ClusterId ?? string.Empty;

        return clusterType.Contains("OPENPVP_BLACK", StringComparison.OrdinalIgnoreCase)
               || clusterType.Contains("TUNNEL", StringComparison.OrdinalIgnoreCase)
               || clusterId.StartsWith("TNL-", StringComparison.OrdinalIgnoreCase);
    }

    private bool ShouldApplySpecialistHideoutBonus()
    {
        return (SelectedCraftingLocation?.MatchingModifierPercent ?? 0m) > 0m;
    }

    private void AddResource(CraftingResourceEntry resource)
    {
        resource.ValuesChanged = Recalculate;
        Resources.Add(resource);
    }

    private void Recalculate()
    {
        if (_isLoading)
        {
            return;
        }

        var calculationInput = CreateCalculationInput();
        Calculation = _calculator.Calculate(calculationInput);
        ApplyCalculationToBindings(Calculation);
    }

    private CraftingCalculationInput CreateCalculationInput()
    {
        return new CraftingCalculationInput
        {
            ItemUniqueName = SelectedItem?.UniqueName,
            ItemName = SelectedItem?.LocalizedName,
            CraftingRuns = Math.Max(1, CraftingRuns),
            AmountCrafted = Math.Max(1, _amountCrafted),
            ReturnRatePercent = ReturnRatePercent,
            UsesFocus = UsesFocus,
            OutputUnitPrice = OutputUnitPrice,
            StationFee = StationFee,
            SalesTaxPercent = SalesTaxPercent,
            OtherCosts = OtherCosts,
            OutputUnitWeight = ItemController.GetWeight(SelectedItem?.FullItemInformation),
            Resources = Resources
                .Select(x =>
                {
                    return new CraftingResourceInput
                    {
                        UniqueName = x.UniqueName,
                        DisplayName = x.DisplayName,
                        QuantityPerRun = x.QuantityPerRun,
                        UnitPrice = x.UnitPrice,
                        UnitWeight = x.UnitWeight,
                        IsReturnable = x.IsReturnable,
                        MaxReturnQuantityPerRun = x.MaxReturnQuantityPerRun,
                        ResourceKind = x.ResourceKind
                    }
                    ;
                }
                )
                .ToList(),
            Journal = Journal == null
                ? null
                : new CraftingJournalInput
                {
                    EmptyJournalUniqueName = Journal.EmptyJournalUniqueName,
                    FullJournalUniqueName = Journal.FullJournalUniqueName,
                    DisplayName = Journal.DisplayName,
                    FamePerRun = Journal.FamePerRun,
                    MaxFamePerJournal = Journal.MaxFamePerJournal,
                    EmptyJournalPrice = Journal.EmptyJournalPrice,
                    FullJournalPrice = Journal.FullJournalPrice,
                    UnitWeight = Journal.UnitWeight
                }
        }
        ;
    }

    private void ApplyCalculationToBindings(CraftingCalculationResult result)
    {
        foreach (var resourceResult in result.Resources)
        {
            var resource = Resources.FirstOrDefault(x => x.UniqueName == resourceResult.UniqueName);
            if (resource == null)
            {
                continue;
            }

            resource.GrossQuantity = resourceResult.GrossQuantity;
            resource.ExpectedReturnQuantity = resourceResult.ExpectedReturnQuantity;
            resource.NetQuantity = resourceResult.NetQuantity;
            resource.GrossCost = resourceResult.GrossCost;
            resource.NetCost = resourceResult.NetCost;
        }

        if (Journal != null && result.Journal != null)
        {
            Journal.RequiredEmptyJournals = result.Journal.RequiredEmptyJournals;
            Journal.ExpectedFullJournals = result.Journal.ExpectedFullJournals;
            Journal.PartialJournalPercent = result.Journal.PartialJournalPercent;
            Journal.TotalEmptyJournalCosts = result.Journal.TotalEmptyJournalCosts;
            Journal.TotalFullJournalRevenue = result.Journal.TotalFullJournalRevenue;
        }
    }

    private async void SaveCurrentAsync()
    {
        try
        {
            if (SelectedItem == null)
            {
                StatusText = "Select an item before saving.";
                return;
            }

            var savedCrafting = CreateSavedCrafting();
            var existing = SavedCraftings.FirstOrDefault(x => x.Id == savedCrafting.Id);
            if (existing != null)
            {
                var index = SavedCraftings.IndexOf(existing);
                SavedCraftings[index] = savedCrafting;
            }
            else
            {
                SavedCraftings.Insert(0, savedCrafting);
            }

            await _controller.SaveAsync(SavedCraftings);
            SelectedSavedCrafting = savedCrafting;
            StatusText = "Crafting saved.";
        }
        catch (Exception e)
        {
            Log.Error(e, "Error saving crafting");
        }
    }

    private SavedCrafting CreateSavedCrafting()
    {
        var id = SelectedSavedCrafting?.ItemUniqueName == SelectedItem?.UniqueName ? SelectedSavedCrafting.Id : Guid.NewGuid();

        var savedCrafting = new SavedCrafting
        {
            Id = id,
            ItemUniqueName = SelectedItem?.UniqueName,
            ItemName = SelectedItem?.LocalizedName,
            CraftingRuns = Math.Max(1, Calculation.CraftingRuns),
            AmountCrafted = Math.Max(1, _amountCrafted),
            UsesFocus = UsesFocus,
            ReturnRatePercent = ReturnRatePercent,
            DailyBonusPercent = SelectedDailyBonus?.BonusPercent ?? 0m,
            HideoutBonusLevel = SelectedHideoutBonus?.Level ?? 0,
            HideoutGeneralistBonusPercent = SelectedHideoutBonus?.GeneralistBonusPercent ?? 0m,
            HideoutSpecialistBonusPercent = SelectedHideoutBonus?.SpecialistBonusPercent ?? 0m,
            CraftingLocationId = SelectedCraftingLocation?.ClusterId,
            CraftingLocationName = SelectedCraftingLocation?.DisplayName,
            CraftingContext = Locations.GetParameterName(SelectedMarketLocation),
            StationFee = StationFee,
            SalesTaxPercent = SalesTaxPercent,
            OtherCosts = OtherCosts,
            OutputUnitPrice = OutputUnitPrice,
            Notes = Notes,
            Resources = Resources.Select(CloneResource).ToList(),
            Journal = CloneJournal(Journal),
            LastChangedUtc = DateTime.UtcNow,
            NetMaterialCosts = Calculation.NetMaterialCosts,
            Profit = Calculation.Profit,
            Icon = SelectedItem?.Icon
        }
        ;

        PrepareSavedCrafting(savedCrafting);
        return savedCrafting;
    }

    private async void DeleteSelectedAsync()
    {
        try
        {
            if (SelectedSavedCrafting == null)
            {
                return;
            }

            SavedCraftings.Remove(SelectedSavedCrafting);
            SelectedSavedCrafting = null;
            await _controller.SaveAsync(SavedCraftings);
            StatusText = "Crafting deleted.";
        }
        catch (Exception e)
        {
            Log.Error(e, "Error deleting crafting");
        }
    }

    private void LoadSelectedCrafting(object value)
    {
        if (value is SavedCrafting savedCrafting)
        {
            SelectedSavedCrafting = savedCrafting;
        }

        if (SelectedSavedCrafting == null)
        {
            return;
        }

        LoadSavedCrafting(SelectedSavedCrafting);
    }

    private void LoadSavedCrafting(SavedCrafting savedCrafting)
    {
        _isLoading = true;

        SelectedItem = ItemController.GetItemByUniqueName(savedCrafting.ItemUniqueName);
        CraftingRuns = Math.Max(1, savedCrafting.CraftingRuns);
        _amountCrafted = Math.Max(1, savedCrafting.AmountCrafted);
        UsesFocus = savedCrafting.UsesFocus;
        SelectedDailyBonus = DailyBonusOptions.FirstOrDefault(x => x.BonusPercent == savedCrafting.DailyBonusPercent)
                             ?? DailyBonusOptions.First();
        SelectedHideoutBonus = HideoutBonusOptions.FirstOrDefault(x => x.Level == savedCrafting.HideoutBonusLevel)
                               ?? HideoutBonusOptions.First();
        ReturnRatePercent = savedCrafting.ReturnRatePercent;
        RefreshCraftingLocations(SelectedItem, savedCrafting.CraftingLocationId);
        SelectedMarketLocation = (savedCrafting.CraftingContext ?? string.Empty).GetMarketLocationByLocationNameOrId();
        StationFee = savedCrafting.StationFee;
        SalesTaxPercent = savedCrafting.SalesTaxPercent;
        OtherCosts = savedCrafting.OtherCosts;
        OutputUnitPrice = savedCrafting.OutputUnitPrice;
        Notes = savedCrafting.Notes;
        Resources.Clear();

        foreach (var resource in savedCrafting.Resources ?? [])
        {
            PrepareResource(resource);
            AddResource(CloneResource(resource));
        }

        var journal = CloneJournal(savedCrafting.Journal);
        PrepareJournal(journal);
        Journal = journal;

        _isLoading = false;
        Recalculate();
        StatusText = "Crafting loaded.";
    }

    private void NewCrafting()
    {
        SelectedSavedCrafting = null;
        ResetEditor();
        StatusText = "New crafting started.";
    }

    private void ResetEditor()
    {
        _isLoading = true;

        CraftingRuns = 1;
        UsesFocus = false;
        SelectedDailyBonus = DailyBonusOptions.First();
        SelectedHideoutBonus = HideoutBonusOptions.First();
        ReturnRatePercent = 0m;
        StationFee = 0m;
        SalesTaxPercent = 4m;
        OtherCosts = 0m;
        OutputUnitPrice = 0m;
        Notes = string.Empty;

        _isLoading = false;
        _ = ApplySelectedItemAsync(SelectedItem);
    }

    private async void LoadPricesAsync()
    {
        try
        {
            if (SelectedItem == null)
            {
                StatusText = "Select an item before loading prices.";
                return;
            }

            try
            {
                OutputUnitPrice = await LoadPriceAsync(SelectedItem.UniqueName, SelectedMarketLocation, SelectedMarketLocation == MarketLocation.BlackMarket);

                foreach (var resource in Resources)
                {
                    resource.UnitPrice = await LoadPriceAsync(resource.UniqueName, SelectedMarketLocation, false);
                }

                if (Journal != null)
                {
                    Journal.EmptyJournalPrice = await LoadPriceAsync(Journal.EmptyJournalUniqueName, SelectedMarketLocation, false);
                    Journal.FullJournalPrice = await LoadPriceAsync(Journal.FullJournalUniqueName, SelectedMarketLocation, false);
                }

                StatusText = "Market prices loaded.";
            }
            catch (Exception e)
            {
                DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(e, "Crafting market prices could not be loaded");
                StatusText = "Market prices could not be loaded.";
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Error loading market prices");
        }
    }

    private static async Task<decimal> LoadPriceAsync(string uniqueName, MarketLocation location, bool useBuyPrice)
    {
        var prices = await ApiController.GetCityItemPricesFromJsonAsync(uniqueName).ConfigureAwait(true);
        var locationName = Locations.GetParameterName(location);
        var price = prices?
            .Where(x => string.Equals(x?.City, locationName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => useBuyPrice ? x.BuyPriceMaxDate : x.SellPriceMinDate)
            .FirstOrDefault();

        if (price == null)
        {
            return 0m;
        }

        return useBuyPrice ? price.BuyPriceMax : price.SellPriceMin;
    }

    private static CraftingResourceEntry CloneResource(CraftingResourceEntry resource)
    {
        if (resource == null)
        {
            return null;
        }

        return new CraftingResourceEntry
        {
            UniqueName = resource.UniqueName,
            DisplayName = resource.DisplayName,
            QuantityPerRun = resource.QuantityPerRun,
            UnitPrice = resource.UnitPrice,
            UnitWeight = resource.UnitWeight,
            IsReturnable = resource.IsReturnable,
            MaxReturnQuantityPerRun = resource.MaxReturnQuantityPerRun,
            ResourceKind = resource.ResourceKind,
            Icon = resource.Icon
        }
        ;
    }

    private static CraftingJournalEntry CloneJournal(CraftingJournalEntry journal)
    {
        if (journal == null)
        {
            return null;
        }

        return new CraftingJournalEntry
        {
            EmptyJournalUniqueName = journal.EmptyJournalUniqueName,
            FullJournalUniqueName = journal.FullJournalUniqueName,
            DisplayName = journal.DisplayName,
            FamePerRun = journal.FamePerRun,
            MaxFamePerJournal = journal.MaxFamePerJournal,
            EmptyJournalPrice = journal.EmptyJournalPrice,
            FullJournalPrice = journal.FullJournalPrice,
            UnitWeight = journal.UnitWeight,
            Icon = journal.Icon
        }
        ;
    }

    private void PrepareSavedCrafting(SavedCrafting crafting)
    {
        var item = ItemController.GetItemByUniqueName(crafting.ItemUniqueName);
        crafting.Icon = item?.Icon;
        crafting.ItemName = string.IsNullOrWhiteSpace(crafting.ItemName) ? item?.LocalizedName : crafting.ItemName;

        foreach (var resource in crafting.Resources ?? [])
        {
            PrepareResource(resource);
        }

        PrepareJournal(crafting.Journal);
    }

    private static void PrepareResource(CraftingResourceEntry resource)
    {
        if (resource == null)
        {
            return;
        }

        var item = ItemController.GetItemByUniqueName(resource.UniqueName);
        resource.Icon = item?.Icon;
        resource.DisplayName = string.IsNullOrWhiteSpace(resource.DisplayName) ? item?.LocalizedName : resource.DisplayName;
    }

    private static void PrepareJournal(CraftingJournalEntry journal)
    {
        if (journal == null)
        {
            return;
        }

        var item = ItemController.GetItemByUniqueName(journal.EmptyJournalUniqueName);
        journal.Icon = item?.Icon;
        journal.DisplayName = string.IsNullOrWhiteSpace(journal.DisplayName) ? item?.LocalizedName : journal.DisplayName;
    }
}
