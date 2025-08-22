using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.BindingModel;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using StatisticsAnalysisTool.Models.ItemWindowModel;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.ViewModels;

public class ItemWindowViewModel : BaseViewModel
{
    private readonly ItemWindow _itemWindow;
    private Item _item;
    private string _titleName;
    private string _itemTierLevel;
    private BitmapImage _icon;
    private Visibility _errorBarVisibility;
    private ItemWindowTranslation _translation = new();
    private bool _refreshSpin;
    private bool _isAutoUpdateActive;
    private readonly Timer _timer = new();
    private double _taskProgressbarMinimum;
    private double _taskProgressbarMaximum = 100;
    private double _taskProgressbarValue;
    private bool _isTaskProgressbarIndeterminate;
    private XmlLanguage _itemListViewLanguage = XmlLanguage.GetLanguage(CultureInfo.DefaultThreadCurrentCulture?.IetfLanguageTag ?? string.Empty);
    private double _refreshRateInMilliseconds = 10;
    private RequiredJournal _requiredJournal;
    private EssentialCraftingValuesTemplate _essentialCraftingValues;
    private ObservableCollection<RequiredResource> _requiredResources = new();
    private Visibility _requiredJournalVisibility = Visibility.Collapsed;
    private Visibility _craftingTabVisibility = Visibility.Collapsed;
    private string _craftingNotes;
    private List<MarketResponse> _currentItemPrices = new();
    private ExtraItemInformation _extraItemInformation = new();
    private string _errorBarText;
    private string _refreshIconTooltipText;
    private ObservableCollection<MainTabLocationFilterObject> _locationFilters;
    private int _tabControlSelectedIndex = -1;
    private ItemWindowMainTabBindings _mainTabBindings;
    private ItemWindowQualityTabBindings _qualityTabBindings;
    private ItemWindowHistoryTabBindings _historyTabBindings;
    private ItemWindowRealMoneyTabBindings _realMoneyTabBindings;
    private ItemWindowCraftingTabBindings _craftingTabBindings;

    private CraftingCalculation _craftingCalculation = new()
    {
        AuctionsHouseTax = 0.0d,
        CraftingTax = 0.0d,
        PossibleItemCrafting = 0.0d,
        SetupFee = 0.0d,
        TotalCosts = 0.0,
        TotalJournalCosts = 0.0d,
        TotalItemSells = 0.0d,
        TotalJournalSells = 0.0d,
        TotalResourceCosts = 0.0d,
        GrandTotal = 0.0d
    };

    public enum Error
    {
        NoPrices,
        NoItemInfo,
        GeneralError,
        ToManyRequests
    }

    public ItemWindowViewModel(ItemWindow itemWindow, Item item)
    {
        _itemWindow = itemWindow;

        ErrorBarVisibility = Visibility.Hidden;

        Item = item;

        Translation = new ItemWindowTranslation();
        _ = InitAsync(item);

        ItemListViewLanguage = XmlLanguage.GetLanguage(CultureInfo.DefaultThreadCurrentCulture?.IetfLanguageTag ?? string.Empty);
    }

    #region Inits

    private async Task InitAsync(Item item)
    {
        IsTaskProgressbarIndeterminate = true;
        Icon = null;
        TitleName = "-";
        ItemTierLevel = string.Empty;
        TabControlSelectedIndex = 0;

        Item = item;

        if (item == null)
        {
            SetErrorValues(Error.NoItemInfo);
            return;
        }

        InitBindings();
        InitMainTabLocationFiltering();
        InitQualityFiltering();
        InitExtraItemInformation();
        await InitCraftingTabAsync();

        if (Application.Current.Dispatcher == null)
        {
            SetErrorValues(Error.GeneralError);
            return;
        }

        ChangeHeaderValues(item);
        ChangeWindowValuesAsync(item);

        await InitTimerAsync();
        IsAutoUpdateActive = true;

        IsTaskProgressbarIndeterminate = false;
    }

    private void InitBindings()
    {
        MainTabBindings = new ItemWindowMainTabBindings(this);
        QualityTabBindings = new ItemWindowQualityTabBindings();
        HistoryTabBindings = new ItemWindowHistoryTabBindings(this);
        RealMoneyTabBindings = new ItemWindowRealMoneyTabBindings(this);
        CraftingTabBindings = new ItemWindowCraftingTabBindings();
    }

    private void InitMainTabLocationFiltering()
    {
        var locationFilters = new List<MainTabLocationFilterObject>
        {
            new (MarketLocation.CaerleonMarket, Locations.GetParameterName(MarketLocation.CaerleonMarket), true),
            new (MarketLocation.ThetfordMarket, Locations.GetParameterName(MarketLocation.ThetfordMarket), true),
            new (MarketLocation.FortSterlingMarket, Locations.GetParameterName(MarketLocation.FortSterlingMarket), true),
            new (MarketLocation.LymhurstMarket, Locations.GetParameterName(MarketLocation.LymhurstMarket), true),
            new (MarketLocation.BridgewatchMarket, Locations.GetParameterName(MarketLocation.BridgewatchMarket), true),
            new (MarketLocation.MartlockMarket, Locations.GetParameterName(MarketLocation.MartlockMarket), true),
            new (MarketLocation.BrecilienMarket, Locations.GetParameterName(MarketLocation.BrecilienMarket), true),
            new (MarketLocation.BlackMarket, Locations.GetParameterName(MarketLocation.BlackMarket), true),
            new (MarketLocation.ForestCross, Locations.GetParameterName(MarketLocation.ForestCross), true),
            new (MarketLocation.SwampCross, Locations.GetParameterName(MarketLocation.SwampCross), true),
            new (MarketLocation.SteppeCross, Locations.GetParameterName(MarketLocation.SteppeCross), true),
            new (MarketLocation.HighlandCross, Locations.GetParameterName(MarketLocation.HighlandCross), true),
            new (MarketLocation.MountainCross, Locations.GetParameterName(MarketLocation.MountainCross), true),
            new (MarketLocation.SmugglersDen, Locations.GetParameterName(MarketLocation.SmugglersDen), true)
        };

        foreach (var itemWindowMainTabLocationFilter in SettingsController.CurrentSettings.ItemWindowMainTabLocationFilters)
        {
            var filter = locationFilters.FirstOrDefault(x => x.Location == itemWindowMainTabLocationFilter?.Location);
            if (filter != null)
            {
                filter.IsChecked = itemWindowMainTabLocationFilter.IsChecked;
            }
        }

        LocationFilters = new ObservableCollection<MainTabLocationFilterObject>(locationFilters.OrderBy(x => x.Name));

        AddLocationFiltersEvents();
    }

    private void AddLocationFiltersEvents()
    {
        foreach (var cityFilterObject in LocationFilters)
        {
            cityFilterObject.OnCheckedChanged += UpdateMainTabItemPrices;
            cityFilterObject.OnCheckedChanged += UpdateQualityTabItemPrices;
            cityFilterObject.OnCheckedChanged += UpdateHistoryTabChartPricesAsync;
        }
    }

    public void RemoveLocationFiltersEvents()
    {
        foreach (var cityFilterObject in LocationFilters)
        {
            cityFilterObject.OnCheckedChanged -= UpdateMainTabItemPrices;
            cityFilterObject.OnCheckedChanged -= UpdateQualityTabItemPrices;
            cityFilterObject.OnCheckedChanged -= UpdateHistoryTabChartPricesAsync;
        }
    }

    private void InitQualityFiltering()
    {
        var normalQuality = new ItemWindowMainTabBindings.QualityStruct() { Name = LocalizationController.Translation("NORMAL"), Quality = 1 };
        var goodQuality = new ItemWindowMainTabBindings.QualityStruct() { Name = LocalizationController.Translation("GOOD"), Quality = 2 };
        var outstandingQuality = new ItemWindowMainTabBindings.QualityStruct() { Name = LocalizationController.Translation("OUTSTANDING"), Quality = 3 };
        var excellentQuality = new ItemWindowMainTabBindings.QualityStruct() { Name = LocalizationController.Translation("EXCELLENT"), Quality = 4 };
        var masterpieceQuality = new ItemWindowMainTabBindings.QualityStruct() { Name = LocalizationController.Translation("MASTERPIECE"), Quality = 5 };

        MainTabBindings.Qualities.Add(normalQuality);
        MainTabBindings.Qualities.Add(goodQuality);
        MainTabBindings.Qualities.Add(outstandingQuality);
        MainTabBindings.Qualities.Add(excellentQuality);
        MainTabBindings.Qualities.Add(masterpieceQuality);

        if (MainTabBindings.Qualities != null)
        {
            MainTabBindings.QualitiesSelection = MainTabBindings.Qualities.FirstOrDefault();
        }

        HistoryTabBindings.Qualities.Add(normalQuality);
        HistoryTabBindings.Qualities.Add(goodQuality);
        HistoryTabBindings.Qualities.Add(outstandingQuality);
        HistoryTabBindings.Qualities.Add(excellentQuality);
        HistoryTabBindings.Qualities.Add(masterpieceQuality);

        if (HistoryTabBindings.Qualities != null)
        {
            HistoryTabBindings.QualitiesSelection = HistoryTabBindings.Qualities.FirstOrDefault();
        }
    }

    private void InitExtraItemInformation()
    {
        switch (Item?.FullItemInformation)
        {
            case Weapon weapon:
                ExtraItemInformation.ShopCategory = weapon.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = weapon.ShopSubCategory1;
                ExtraItemInformation.CanBeOvercharged = weapon.CanBeOvercharged.SetYesOrNo();
                ExtraItemInformation.Durability = weapon.Durability;
                ExtraItemInformation.ShowInMarketPlace = weapon.ShowInMarketPlace.SetYesOrNo();
                ExtraItemInformation.Weight = weapon.Weight;
                break;
            case TransformationWeapon transformationWeapon:
                ExtraItemInformation.ShopCategory = transformationWeapon.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = transformationWeapon.ShopSubCategory1;
                ExtraItemInformation.Weight = transformationWeapon.Weight;
                break;
            case HideoutItem hideoutItem:
                ExtraItemInformation.ShopCategory = hideoutItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = hideoutItem.ShopSubCategory1;
                ExtraItemInformation.Weight = hideoutItem.Weight;
                break;
            case FarmableItem farmableItem:
                ExtraItemInformation.ShopCategory = farmableItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = farmableItem.ShopSubCategory1;
                ExtraItemInformation.ShowInMarketPlace = farmableItem.ShowInMarketPlace.SetYesOrNo();
                ExtraItemInformation.Weight = farmableItem.Weight;
                break;
            case SimpleItem simpleItem:
                ExtraItemInformation.ShopCategory = simpleItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = simpleItem.ShopSubCategory1;
                ExtraItemInformation.Weight = simpleItem.Weight;
                break;
            case ConsumableItem consumableItem:
                ExtraItemInformation.ShopCategory = consumableItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = consumableItem.ShopSubCategory1;
                ExtraItemInformation.Weight = consumableItem.Weight;
                break;
            case ConsumableFromInventoryItem consumableFromInventoryItem:
                ExtraItemInformation.ShopCategory = consumableFromInventoryItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = consumableFromInventoryItem.ShopSubCategory1;
                ExtraItemInformation.Weight = consumableFromInventoryItem.Weight;
                break;
            case EquipmentItem equipmentItem:
                ExtraItemInformation.ShopCategory = equipmentItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = equipmentItem.ShopSubCategory1;
                ExtraItemInformation.CanBeOvercharged = equipmentItem.CanBeOvercharged.SetYesOrNo();
                ExtraItemInformation.Durability = equipmentItem.Durability;
                ExtraItemInformation.ShowInMarketPlace = equipmentItem.ShowInMarketPlace.SetYesOrNo();
                ExtraItemInformation.Weight = equipmentItem.Weight;
                break;
            case Mount mount:
                ExtraItemInformation.ShopCategory = mount.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = mount.ShopSubCategory1;
                ExtraItemInformation.Durability = mount.Durability;
                ExtraItemInformation.ShowInMarketPlace = mount.ShowInMarketPlace.SetYesOrNo();
                ExtraItemInformation.Weight = mount.Weight;
                break;
            case FurnitureItem furnitureItem:
                ExtraItemInformation.ShopCategory = furnitureItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = furnitureItem.ShopSubCategory1;
                ExtraItemInformation.Durability = furnitureItem.Durability;
                ExtraItemInformation.ShowInMarketPlace = furnitureItem.ShowInMarketPlace.SetYesOrNo();
                ExtraItemInformation.Weight = furnitureItem.Weight;
                break;
            case JournalItem journalItem:
                ExtraItemInformation.ShopCategory = journalItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = journalItem.ShopSubCategory1;
                ExtraItemInformation.Weight = journalItem.Weight;
                break;
            case LabourerContract labourerContract:
                ExtraItemInformation.ShopCategory = labourerContract.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = labourerContract.ShopSubCategory1;
                ExtraItemInformation.Weight = labourerContract.Weight;
                break;
            case CrystalLeagueItem crystalLeagueItem:
                ExtraItemInformation.ShopCategory = crystalLeagueItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = crystalLeagueItem.ShopSubCategory1;
                ExtraItemInformation.Weight = crystalLeagueItem.Weight;
                break;
            case TrackingItem trackingItem:
                ExtraItemInformation.ShopCategory = trackingItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = trackingItem.ShopSubCategory1;
                ExtraItemInformation.Weight = trackingItem.Weight;
                break;
            case KillTrophyItem killTrophyItem:
                ExtraItemInformation.ShopCategory = killTrophyItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = killTrophyItem.ShopSubCategory1;
                ExtraItemInformation.Weight = killTrophyItem.Weight;
                break;
        }
    }

    #endregion

    #region Saving

    public void SaveSettings()
    {
        SettingsController.CurrentSettings.ItemWindowMainTabLocationFilters = LocationFilters?.Select(x => new MainTabLocationFilterSettingsObject()
        {
            IsChecked = x.IsChecked ?? false,
            Location = x.Location
        }).ToList();
    }

    #endregion

    #region Ui

    private async void ChangeWindowValuesAsync(Item item)
    {
        var localizedName = ItemController.LocalizedName(item?.LocalizedNames, null, item?.UniqueName);

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            _itemWindow.Icon = item?.Icon;
            _itemWindow.Title = $"{localizedName} (T{item?.Tier})";
        });
    }

    private void ChangeHeaderValues(Item item)
    {
        var localizedName = ItemController.LocalizedName(item?.LocalizedNames, null, item?.UniqueName);

        Icon = item?.Icon;
        TitleName = localizedName;
        ItemTierLevel = item?.Tier != -1 && item?.Level != -1 ? $"T{item?.Tier}.{item?.Level}" : string.Empty;
    }

    #endregion

    #region Timer

    private async Task InitTimerAsync()
    {
        await UpdateMarketPricesAsync();
        UpdateMainTabItemPrices(null, null);
        UpdateQualityTabItemPrices(null, null);
        UpdateHistoryTabChartPrices(null, null);

        _timer.Interval = SettingsController.CurrentSettings.RefreshRate;
        _timer.Elapsed += UpdateInterval;
        _timer.Elapsed += UpdateMarketPricesAsync;
        _timer.Elapsed += UpdateMainTabItemPrices;
        _timer.Elapsed += UpdateQualityTabItemPrices;
    }

    public void RemoveTimerAsync()
    {
        _timer.Elapsed -= UpdateInterval;
        _timer.Elapsed -= UpdateMarketPricesAsync;
        _timer.Elapsed -= UpdateMainTabItemPrices;
        _timer.Elapsed -= UpdateQualityTabItemPrices;
    }

    private void UpdateInterval(object sender, EventArgs e)
    {
        if (Math.Abs(_refreshRateInMilliseconds - SettingsController.CurrentSettings.RefreshRate) <= 0)
        {
            return;
        }

        _refreshRateInMilliseconds = SettingsController.CurrentSettings.RefreshRate;
        _timer.Interval = _refreshRateInMilliseconds;
    }

    public void AutoUpdateSwitcher()
    {
        IsAutoUpdateActive = !IsAutoUpdateActive;
    }

    #endregion

    #region Crafting tab

    private async Task InitCraftingTabAsync()
    {
        var areResourcesAvailable = false;

        switch (Item?.FullItemInformation)
        {
            case Weapon weapon when weapon.CraftingRequirements?.FirstOrDefault()?.CraftResource.Count > 0:
            case TransformationWeapon transformationWeapon when transformationWeapon.CraftingRequirements?.FirstOrDefault()?.CraftResource.Count > 0:
            case EquipmentItem equipmentItem when equipmentItem.CraftingRequirements?.FirstOrDefault()?.CraftResource.Count > 0:
            case Mount mount when mount.CraftingRequirements?.FirstOrDefault()?.CraftResource != null && mount.CraftingRequirements?.FirstOrDefault()?.CraftResource.Count > 0:
            case TrackingItem trackingItem when trackingItem.CraftingRequirements?.FirstOrDefault()?.CraftResource.Count > 0:
            case ConsumableItem consumableItem when consumableItem.CraftingRequirements?.FirstOrDefault()?.CraftResource.Count > 0:
                areResourcesAvailable = true;
                break;
        }

        if (areResourcesAvailable)
        {
            CraftingTabVisibility = Visibility.Visible;

            EssentialCraftingValues = new EssentialCraftingValuesTemplate(this, CurrentItemPrices, Item?.UniqueName);
            SetJournalInfo();
            await SetRequiredResourcesAsync();
            CraftingNotes = await CraftingTabController.GetNoteAsync(Item?.UniqueName);
            IsFocusCheckboxEnabled(Item?.FullItemInformation);
        }
    }

    private void IsFocusCheckboxEnabled(object fullItemInfo)
    {
        if (fullItemInfo is EquipmentItem equipmentItem)
        {
            if (int.TryParse(equipmentItem.CraftingRequirements?.FirstOrDefault()?.CraftingFocus, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out int craftingFocusNumber) && (craftingFocusNumber <= 0))
            {
                EssentialCraftingValues.IsCraftingWithFocusCheckboxEnabled = false;
                EssentialCraftingValues.IsCraftingBonusEnabled = false;
                EssentialCraftingValues.CraftingBonus = 100;
            }
            else
            {
                EssentialCraftingValues.IsCraftingWithFocusCheckboxEnabled = true;
                EssentialCraftingValues.IsCraftingBonusEnabled = true;
            }
        }
        else
        {
            EssentialCraftingValues.IsCraftingWithFocusCheckboxEnabled = true;
            EssentialCraftingValues.IsCraftingBonusEnabled = true;
            EssentialCraftingValues.CraftingBonus = 133;
        }
    }

    private void SetJournalInfo()
    {
        var craftingJournalType = Item?.FullItemInformation switch
        {
            Weapon weapon => CraftingController.GetCraftingJournalItem(Item.Tier, weapon.CraftingJournalType),
            TransformationWeapon transformationWeapon => CraftingController.GetCraftingJournalItem(Item.Tier, transformationWeapon.CraftingJournalType),
            EquipmentItem equipmentItem => CraftingController.GetCraftingJournalItem(Item.Tier, equipmentItem.CraftingJournalType),
            TrackingItem trackingItem => CraftingController.GetCraftingJournalItem(Item.Tier, trackingItem.CraftingJournalType),
            _ => null
        };

        if (craftingJournalType == null)
        {
            return;
        }

        RequiredJournalVisibility = Visibility.Visible;

        var fullItemInformation = ItemController.GetItemByUniqueName(ItemController.GetGeneralJournalName(craftingJournalType.UniqueName))?.FullItemInformation;

        RequiredJournal = new RequiredJournal(this)
        {
            UniqueName = craftingJournalType.UniqueName,
            CostsPerJournal = 0,
            CraftingResourceName = craftingJournalType.LocalizedName,
            Icon = craftingJournalType.Icon,
            Weight = ItemController.GetWeight(fullItemInformation),
            RequiredJournalAmount = CraftingController.GetRequiredJournalAmount(Item, CraftingCalculation.PossibleItemCrafting),
            SellPricePerJournal = 0
        };
    }

    private async Task SetRequiredResourcesAsync()
    {
        var currentItemEnchantmentLevel = Item.Level;
        List<CraftingRequirements> craftingRequirements = null;

        var enchantments = Item?.FullItemInformation switch
        {
            Weapon weapon => weapon.Enchantments,
            EquipmentItem equipmentItem => equipmentItem.Enchantments,
            ConsumableItem consumableItem => consumableItem.Enchantments,
            TransformationWeapon transformationWeapon => transformationWeapon.Enchantments,
            _ => null
        };

        var enchantment = enchantments?.Enchantment?.FirstOrDefault(x => x.EnchantmentLevelInteger == currentItemEnchantmentLevel);

        if (enchantment != null)
        {
            craftingRequirements = enchantment.CraftingRequirements;
        }

        craftingRequirements ??= Item?.FullItemInformation switch
        {
            Weapon weapon => weapon.CraftingRequirements,
            TransformationWeapon transformationWeapon => transformationWeapon.CraftingRequirements,
            EquipmentItem equipmentItem => equipmentItem.CraftingRequirements,
            Mount mount => mount.CraftingRequirements,
            ConsumableItem consumableItem => consumableItem.CraftingRequirements,
            TrackingItem trackingItem => trackingItem.CraftingRequirements,
            _ => null
        };

        if (craftingRequirements?.FirstOrDefault()?.CraftResource == null)
        {
            return;
        }

        if (int.TryParse(craftingRequirements.FirstOrDefault()?.AmountCrafted, out var amountCrafted))
        {
            EssentialCraftingValues.AmountCrafted = amountCrafted;
        }

        await foreach (var craftResource in craftingRequirements
                           .SelectMany(x => x.CraftResource)
                           .ToList()
                           .GroupBy(x => x.UniqueName)
                           .Select(grp => grp.FirstOrDefault())
                           .ToAsyncEnumerable())
        {
            var item = GetSuitableResourceItem(craftResource.UniqueName);
            var craftingQuantity = (long) Math.Round(item?.UniqueName?.ToUpper().Contains("ARTEFACT") ?? false
            ? CraftingCalculation.PossibleItemCrafting
                : EssentialCraftingValues.CraftingItemQuantity, MidpointRounding.ToPositiveInfinity);

            RequiredResources.Add(new RequiredResource(this)
            {
                CraftingResourceName = item?.LocalizedName,
                UniqueName = item?.UniqueName,
                OneProductionAmount = craftResource.Count,
                Icon = item?.Icon,
                ResourceCost = 0,
                Weight = ItemController.GetWeight(item?.FullItemInformation),
                CraftingQuantity = craftingQuantity,
                ResourceType = GetResourceType(item),
                IsTomeOfInsightResource = item?.UniqueName?.ToUpper().Contains("SKILLBOOK_STANDARD") ?? false,
                IsAvalonianEnergy = item?.UniqueName?.ToUpper().Contains("QUESTITEM_TOKEN_AVALON") ?? false
            });
        }
    }

    private static ResourceType GetResourceType(Item item)
    {
        if (item?.FullItemInformation is SimpleItem { ResourceType: "ESSENCE" })
        {
            return ResourceType.Essence;
        }

        if (item?.UniqueName?.ToUpper().Contains("ARTEFACT") ?? false)
        {
            return ResourceType.Artefact;
        }

        return ResourceType.Unknown;
    }

    private Item GetSuitableResourceItem(string uniqueName)
    {
        var item = ItemController.GetItemByUniqueName(uniqueName);

        if (item == null)
        {
            var suitableUniqueName = $"{uniqueName}_LEVEL{Item.Level}@{Item.Level}";
            item = ItemController.GetItemByUniqueName(suitableUniqueName);
        }

        return item;
    }

    public void UpdateCraftingCalculationTab()
    {
        if (EssentialCraftingValues == null || CraftingCalculation == null)
        {
            return;
        }

        // PossibleItem crafting
        var possibleItemCrafting = EssentialCraftingValues.CraftingItemQuantity / 100d * EssentialCraftingValues.CraftingBonus * ((EssentialCraftingValues.IsCraftingWithFocus)
            ? ((23.1d / 100d) + 1d) : 1d);

        // Crafting quantity
        if (RequiredResources?.Count > 0)
        {
            foreach (var requiredResource in RequiredResources.ToList())
            {
                if (requiredResource.ResourceType is ResourceType.Artefact or ResourceType.Essence || requiredResource.IsTomeOfInsightResource || requiredResource.IsAvalonianEnergy)
                {
                    requiredResource.CraftingQuantity = (long) Math.Round(possibleItemCrafting, MidpointRounding.ToNegativeInfinity);
                    continue;
                }

                requiredResource.CraftingQuantity = EssentialCraftingValues.CraftingItemQuantity;
            }
        }

        CraftingCalculation.PossibleItemCrafting = Math.Round(possibleItemCrafting, MidpointRounding.ToNegativeInfinity);

        // Crafting (Usage) tax
        CraftingCalculation.CraftingTax = CraftingController.GetCraftingTax(EssentialCraftingValues.UsageFeePerHundredFood, Item, CraftingCalculation.PossibleItemCrafting);

        // Setup fee
        CraftingCalculation.SetupFee = CraftingController.GetSetupFeeCalculation(EssentialCraftingValues.CraftingItemQuantity, EssentialCraftingValues.SetupFee, EssentialCraftingValues.SellPricePerItem);

        // Auctions house tax
        CraftingCalculation.AuctionsHouseTax =
            EssentialCraftingValues.SellPricePerItem * Convert.ToInt64(EssentialCraftingValues.CraftingItemQuantity) / 100.00 * Convert.ToInt64(EssentialCraftingValues.AuctionHouseTax);

        // Total resource costs
        CraftingCalculation.TotalResourceCosts = RequiredResources?.Sum(x => x.TotalCost) ?? 0;

        // Other costs
        CraftingCalculation.OtherCosts = EssentialCraftingValues.OtherCosts;

        // Total item sells
        CraftingCalculation.TotalItemSells = EssentialCraftingValues.SellPricePerItem * (CraftingCalculation.PossibleItemCrafting * EssentialCraftingValues.AmountCrafted);

        if (RequiredJournal != null)
        {
            // Required journal amount
            RequiredJournal.RequiredJournalAmount = CraftingController.GetRequiredJournalAmount(Item, Math.Round(possibleItemCrafting, MidpointRounding.ToNegativeInfinity));

            // Total journal costs
            CraftingCalculation.TotalJournalCosts = RequiredJournal.CostsPerJournal * RequiredJournal.RequiredJournalAmount;

            // Total journal sells
            CraftingCalculation.TotalJournalSells = RequiredJournal.RequiredJournalAmount * RequiredJournal.SellPricePerJournal;
        }

        // Amount crafted
        CraftingCalculation.AmountCrafted = EssentialCraftingValues.AmountCrafted;
        
        // Total sells
        CraftingCalculation.TotalSells = CraftingCalculation.TotalItemSells + CraftingCalculation.TotalJournalSells;

        // Total costs
        CraftingCalculation.TotalCosts = CraftingCalculation.TotalResourceCosts + CraftingCalculation.CraftingTax + CraftingCalculation.SetupFee
                                         + CraftingCalculation.AuctionsHouseTax + CraftingCalculation.TotalJournalCosts + CraftingCalculation.OtherCosts;

        // Weight
        var requiredResourcesWeights = RequiredResources?.Sum(x => x.TotalWeight) ?? 0;
        var possibleItemCraftingWeights = CraftingCalculation?.PossibleItemCrafting * ItemController.GetWeight(Item?.FullItemInformation) ?? 0;

        if (CraftingCalculation != null)
        {
            CraftingCalculation.TotalResourcesWeight = requiredResourcesWeights;
            CraftingCalculation.TotalRequiredJournalWeight = RequiredJournal?.TotalWeight ?? 0;
            CraftingCalculation.TotalUnfinishedCraftingWeight = CraftingCalculation.TotalResourcesWeight + CraftingCalculation.TotalRequiredJournalWeight;

            CraftingCalculation.TotalCraftedItemWeight = possibleItemCraftingWeights;
            CraftingCalculation.TotalFinishedCraftingWeight = CraftingCalculation.TotalCraftedItemWeight;

            // Return on investment
            CraftingCalculation.ReturnOnInvestment = (CraftingCalculation.TotalCosts > 0) ? ((CraftingCalculation.TotalSells - CraftingCalculation.TotalCosts) / CraftingCalculation.TotalCosts) * 100.0 : 0.0;

            // Break even price
            var totalCraftedItems = CraftingCalculation.PossibleItemCrafting * CraftingCalculation.AmountCrafted;
            if (totalCraftedItems == 0)
            {
                totalCraftedItems = 1;
            }

            // All fixed costs excluding AH tax
            var costsWithoutAhTax = CraftingCalculation.TotalResourceCosts + CraftingCalculation.CraftingTax + CraftingCalculation.SetupFee + CraftingCalculation.TotalJournalCosts + CraftingCalculation.OtherCosts;

            // tax rate
            var taxRate = EssentialCraftingValues.AuctionHouseTax / 100.0;

            // Break-even-price
            CraftingCalculation.BreakEvenPrice = (costsWithoutAhTax - CraftingCalculation.TotalJournalSells) / (totalCraftedItems * (1 - taxRate));

            // Profit per item
            CraftingCalculation.ProfitPerItem = (CraftingCalculation.TotalSells - CraftingCalculation.TotalCosts) / CraftingCalculation.PossibleItemCrafting;
        }
    }

    #endregion Crafting tab

    #region Error methods

    private void SetErrorValues(Error error)
    {
        switch (error)
        {
            case Error.NoItemInfo:
                Icon = new BitmapImage(new Uri(@"pack://application:,,,/"
                                               + Assembly.GetExecutingAssembly().GetName().Name + ";component/"
                                               + "Resources/Trash.png", UriKind.Absolute));
                SetLoadingImageToError();
                SetErrorBar(Visibility.Visible, LocalizationController.Translation("ERROR_NO_ITEM_INFO"));
                return;

            case Error.NoPrices:
                SetLoadingImageToError();
                SetErrorBar(Visibility.Visible, LocalizationController.Translation("ERROR_PRICES_CAN_NOT_BE_LOADED"));
                return;

            case Error.GeneralError:
                SetLoadingImageToError();
                SetErrorBar(Visibility.Visible, LocalizationController.Translation("ERROR_GENERAL_ERROR"));
                return;

            case Error.ToManyRequests:
                SetLoadingImageToError();
                SetErrorBar(Visibility.Visible, LocalizationController.Translation("TOO_MANY_REQUESTS_CLOSE_WINDOWS_OR_WAIT"));
                return;

            default:
                SetLoadingImageToError();
                SetErrorBar(Visibility.Visible, LocalizationController.Translation("ERROR_GENERAL_ERROR"));
                return;
        }
    }

    private void ErrorBarReset()
    {
        IsTaskProgressbarIndeterminate = false;
        SetErrorBar(Visibility.Hidden, string.Empty);
    }

    private void SetLoadingImageToError()
    {
        IsTaskProgressbarIndeterminate = true;
    }

    private void SetErrorBar(Visibility visibility, string errorMessage)
    {
        ErrorBarText = errorMessage;
        ErrorBarVisibility = visibility;
    }

    #endregion

    #region Prices

    public async void UpdateMarketPricesAsync(object sender, ElapsedEventArgs e)
    {
        await UpdateMarketPricesAsync();
    }

    public async Task UpdateMarketPricesAsync()
    {
        try
        {
            var trackingController = ServiceLocator.Resolve<TrackingController>();
            var localList = await trackingController.MarketController.GetResponsesForItem(Item?.UniqueName);
            var apiList = await ApiController.GetCityItemPricesFromJsonAsync(Item?.UniqueName);

            CurrentItemPrices = MergeMarketResponses(localList, apiList);

            RefreshIconTooltipText = $"{LocalizationController.Translation("LAST_UPDATE")}: {DateTime.UtcNow.CurrentDateTimeFormat()}";
            ErrorBarReset();
        }
        catch (TooManyRequestsException ex)
        {
            SetErrorValues(Error.ToManyRequests);
            Log.Warning(ex, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private static List<MarketResponse> MergeMarketResponses(IEnumerable<MarketResponse> localList, IEnumerable<MarketResponse> apiList)
    {
        var result = new Dictionary<(string ItemTypeId, string City, int QualityLevel), MarketResponse>();

        void AddOrUpdate(MarketResponse response)
        {
            var key = (response.ItemTypeId, response.City, response.QualityLevel);

            if (!result.TryGetValue(key, out var existing))
            {
                result[key] = response;
                return;
            }

            var existingDate = MaxDate(existing.SellPriceMaxDate, existing.BuyPriceMaxDate);
            var responseDate = MaxDate(response.SellPriceMaxDate, response.BuyPriceMaxDate);

            if (responseDate > existingDate)
            {
                result[key] = response;
            }
        }

        foreach (var response in localList)
        {
            AddOrUpdate(response);
        }
        foreach (var response in apiList)
        {
            AddOrUpdate(response);
        }

        return result.Values.ToList();
    }

    private static DateTime MaxDate(DateTime a, DateTime b)
    {
        return a > b ? a : b;
    }

    private static void FindBestPrice(IReadOnlyCollection<ItemPricesObject> list)
    {
        if (list == null || list.Count == 0)
            return;

        for (var i = 1; i <= 5; i++)
        {
            var max = GetMaxPrice(list, i);

            var itemPricesObjectBuyPriceMax = list.Where(x => x.Visibility == Visibility.Visible && x.QualityLevel == i).FirstOrDefault(s => s.BuyPriceMax == max);
            if (itemPricesObjectBuyPriceMax != null)
            {
                itemPricesObjectBuyPriceMax.IsBestBuyMaxPrice = true;
            }

            var min = GetMinPrice(list, i);

            var itemPricesObjectSellPriceMin = list.Where(x => x.Visibility == Visibility.Visible && x.QualityLevel == i).FirstOrDefault(s => s.SellPriceMin == min);
            if (itemPricesObjectSellPriceMin != null)
            {
                itemPricesObjectSellPriceMin.IsBestSellMinPrice = true;
            }
        }
    }

    private static ulong GetMaxPrice(IEnumerable<ItemPricesObject> list, int quality)
    {
        var max = ulong.MinValue;
        foreach (var type in list.Where(x => x.QualityLevel == quality))
        {
            if (type.BuyPriceMax == 0)
                continue;

            if (type.BuyPriceMax > max)
                max = type.BuyPriceMax;
        }

        return max;
    }

    private static ulong GetMinPrice(IEnumerable<ItemPricesObject> list, int quality)
    {
        var min = ulong.MaxValue;
        foreach (var type in list.Where(x => x.QualityLevel == quality))
        {
            if (type.SellPriceMin == 0)
                continue;

            if (type.SellPriceMin < min)
                min = type.SellPriceMin;
        }

        return min;
    }

    #endregion Prices

    #region Main tab

    public void UpdateMainTabItemPrices(object sender, ElapsedEventArgs e)
    {
        UpdateMainTabItemPrices();
    }

    public void UpdateMainTabItemPrices()
    {
        var currentItemPrices = CurrentItemPrices?.Select(x => new ItemPricesObject(x)).ToList();
        UpdateMainTabItemPricesObjects(currentItemPrices);
        SetItemPricesObjectVisibility(MainTabBindings.ItemPrices);
    }

    private void UpdateMainTabItemPricesObjects(List<ItemPricesObject> newPrices)
    {
        foreach (var newItemPricesObject in newPrices ?? new List<ItemPricesObject>())
        {
            if (MainTabBindings.ItemPrices == null)
            {
                break;
            }

            lock (MainTabBindings.ItemPrices)
            {
                var currentItemPricesObject = MainTabBindings.ItemPrices?.FirstOrDefault(x => x.MarketLocation == newItemPricesObject.MarketLocation && x.QualityLevel == newItemPricesObject.QualityLevel);

                if (currentItemPricesObject == null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MainTabBindings.ItemPrices?.Add(newItemPricesObject);
                    });
                }

                if (newItemPricesObject?.SellPriceMinDate > currentItemPricesObject?.SellPriceMinDate)
                {
                    currentItemPricesObject.SellPriceMin = newItemPricesObject.SellPriceMin;
                    currentItemPricesObject.SellPriceMinDate = newItemPricesObject.SellPriceMinDate;
                }

                if (newItemPricesObject?.SellPriceMaxDate > currentItemPricesObject?.SellPriceMaxDate)
                {
                    currentItemPricesObject.SellPriceMax = newItemPricesObject.SellPriceMax;
                    currentItemPricesObject.SellPriceMaxDate = newItemPricesObject.SellPriceMaxDate;
                }

                if (newItemPricesObject?.BuyPriceMinDate > currentItemPricesObject?.BuyPriceMinDate)
                {
                    currentItemPricesObject.BuyPriceMin = newItemPricesObject.BuyPriceMin;
                    currentItemPricesObject.BuyPriceMinDate = newItemPricesObject.BuyPriceMinDate;
                }

                if (newItemPricesObject?.BuyPriceMaxDate > currentItemPricesObject?.BuyPriceMaxDate)
                {
                    currentItemPricesObject.BuyPriceMax = newItemPricesObject.BuyPriceMax;
                    currentItemPricesObject.BuyPriceMaxDate = newItemPricesObject.BuyPriceMaxDate;
                }
            }
        }
    }

    private void SetItemPricesObjectVisibility(ObservableCollection<ItemPricesObject> prices)
    {
        foreach (var currentItemPricesObject in prices?.ToList() ?? new List<ItemPricesObject>())
        {
            if (GetCheckedLocations().Contains(currentItemPricesObject.MarketLocation) && currentItemPricesObject.QualityLevel == MainTabBindings.QualitiesSelection.Quality)
            {
                currentItemPricesObject.Visibility = Visibility.Visible;
            }
            else
            {
                currentItemPricesObject.Visibility = Visibility.Collapsed;
            }
        }

        FindBestPrice(prices?.Where(x => GetCheckedLocations().Contains(x.MarketLocation)).ToList());
    }

    private List<MarketLocation> GetCheckedLocations()
    {
        return LocationFilters?.Where(x => x?.IsChecked == true).Select(x => x.Location).ToList() ?? new List<MarketLocation>();
    }

    #endregion

    #region Quality tab / Real money tab

    private void UpdateQualityTabItemPrices(object sender, ElapsedEventArgs e)
    {
        UpdateQualityTabItemPrices();
    }

    public void UpdateQualityTabItemPrices()
    {
        var marketResponse = CurrentItemPrices?.ToList();
        UpdateQualityTabItemPricesObjects(marketResponse);
        SetMarketQualityObjectVisibility(QualityTabBindings?.Prices);
    }

    private void UpdateQualityTabItemPricesObjects(List<MarketResponse> newPrices)
    {
        if (QualityTabBindings?.Prices == null)
        {
            return;
        }

        var existingPrices = QualityTabBindings.Prices.ToDictionary(x => x.MarketLocation);

        foreach (var marketResponse in newPrices ?? new List<MarketResponse>())
        {
            if (existingPrices.TryGetValue(marketResponse.MarketLocation, out var currentPriceObject))
            {
                currentPriceObject.SetValues(marketResponse);
                existingPrices[marketResponse.MarketLocation] = currentPriceObject;
            }
            else
            {
                currentPriceObject = new MarketQualityObject(marketResponse);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    QualityTabBindings?.Prices?.Add(currentPriceObject);
                });
                existingPrices.Add(marketResponse.MarketLocation, currentPriceObject);
            }
        }
    }

    private void SetMarketQualityObjectVisibility(List<MarketQualityObject> prices)
    {
        foreach (var currentItemPricesObject in prices?.ToList() ?? new List<MarketQualityObject>())
        {
            if (GetCheckedLocations().Contains(currentItemPricesObject.MarketLocation))
            {
                currentItemPricesObject.Visibility = Visibility.Visible;
            }
            else
            {
                currentItemPricesObject.Visibility = Visibility.Collapsed;
            }
        }
    }

    #endregion

    #region History tab

    public void UpdateHistoryTabChartPrices(object sender, ElapsedEventArgs e)
    {
        UpdateHistoryTabChartPricesAsync();
    }

    public async void UpdateHistoryTabChartPricesAsync()
    {
        List<MarketHistoriesResponse> historyItemPrices;

        try
        {
            var locations = GetCheckedLocations();
            historyItemPrices = await ApiController.GetHistoryItemPricesFromJsonAsync(Item.UniqueName, locations, DateTime.Now.AddDays(-30), HistoryTabBindings.QualitiesSelection.Quality).ConfigureAwait(true);

            if (historyItemPrices == null)
            {
                return;
            }
        }
        catch (TooManyRequestsException)
        {
            ConsoleManager.WriteLineForWarning(MethodBase.GetCurrentMethod()?.DeclaringType, new TooManyRequestsException());
            SetErrorValues(Error.ToManyRequests);
            return;
        }

        SetHistoryChart(historyItemPrices);
    }

    private void SetHistoryChart(List<MarketHistoriesResponse> historyItemPrices)
    {
        var date = new List<string>();
        var seriesCollectionHistory = new ObservableCollection<ISeries>();
        var xAxes = new ObservableCollection<Axis>();

        foreach (var marketHistory in historyItemPrices)
        {
            if (marketHistory == null)
            {
                continue;
            }

            var amount = new ObservableCollection<ObservablePoint>();
            var counter = 0;
            foreach (var data in marketHistory.Data?.OrderBy(x => x.Timestamp).ToList() ?? new List<MarketHistoryResponse>())
            {
                if (!date.Exists(x => x.Contains(data.Timestamp.ToString("g", CultureInfo.CurrentCulture))))
                {
                    date.Add(data.Timestamp.ToString("g", CultureInfo.CurrentCulture));
                }

                amount.Add(new ObservablePoint(counter++, data.AveragePrice));
            }

            var lineSeries = new LineSeries<ObservablePoint>
            {
                Name = WorldData.GetUniqueNameOrDefault(marketHistory.Location),
                Values = amount,
                Fill = Locations.GetLocationBrush(marketHistory.Location.GetMarketLocationByLocationNameOrId(), true),
                Stroke = Locations.GetLocationBrush(marketHistory.Location.GetMarketLocationByLocationNameOrId(), false),
                GeometryStroke = Locations.GetLocationBrush(marketHistory.Location.GetMarketLocationByLocationNameOrId(), false),
                GeometryFill = Locations.GetLocationBrush(marketHistory.Location.GetMarketLocationByLocationNameOrId(), true),
                GeometrySize = 7
            };

            seriesCollectionHistory.Add(lineSeries);
        }

        xAxes.Add(new Axis()
        {
            LabelsRotation = 15,
            Labels = date,
            Labeler = (value) => new DateTime((long) value).ToString("g", CultureInfo.CurrentCulture),
            UnitWidth = TimeSpan.FromDays(1).Ticks
        });

        HistoryTabBindings.XAxesHistory = xAxes.ToArray();
        HistoryTabBindings.SeriesHistory = seriesCollectionHistory;
    }

    #endregion

    #region Bindings

    public Item Item
    {
        get => _item;
        set
        {
            _item = value;
            OnPropertyChanged();
        }
    }

    public string TitleName
    {
        get => _titleName;
        set
        {
            _titleName = value;
            OnPropertyChanged();
        }
    }

    public string ItemTierLevel
    {
        get => _itemTierLevel;
        set
        {
            _itemTierLevel = value;
            OnPropertyChanged();
        }
    }

    public BitmapImage Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }

    public bool RefreshSpin
    {
        get => _refreshSpin;
        set
        {
            _refreshSpin = value;
            OnPropertyChanged();
        }
    }

    public XmlLanguage ItemListViewLanguage
    {
        get => _itemListViewLanguage;
        set
        {
            _itemListViewLanguage = value;
            OnPropertyChanged();
        }
    }

    public ItemWindowMainTabBindings MainTabBindings
    {
        get => _mainTabBindings;
        set
        {
            _mainTabBindings = value;
            OnPropertyChanged();
        }
    }

    public ItemWindowQualityTabBindings QualityTabBindings
    {
        get => _qualityTabBindings;
        set
        {
            _qualityTabBindings = value;
            OnPropertyChanged();
        }
    }

    public ItemWindowHistoryTabBindings HistoryTabBindings
    {
        get => _historyTabBindings;
        set
        {
            _historyTabBindings = value;
            OnPropertyChanged();
        }
    }

    public ItemWindowRealMoneyTabBindings RealMoneyTabBindings
    {
        get => _realMoneyTabBindings;
        set
        {
            _realMoneyTabBindings = value;
            OnPropertyChanged();
        }
    }

    public ItemWindowCraftingTabBindings CraftingTabBindings
    {
        get => _craftingTabBindings;
        set
        {
            _craftingTabBindings = value;
            OnPropertyChanged();
        }
    }

    public RequiredJournal RequiredJournal
    {
        get => _requiredJournal;
        set
        {
            _requiredJournal = value;
            OnPropertyChanged();
        }
    }

    public CraftingCalculation CraftingCalculation
    {
        get => _craftingCalculation;
        set
        {
            _craftingCalculation = value;
            OnPropertyChanged();
        }
    }

    public EssentialCraftingValuesTemplate EssentialCraftingValues
    {
        get => _essentialCraftingValues;
        set
        {
            _essentialCraftingValues = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<RequiredResource> RequiredResources
    {
        get => _requiredResources;
        set
        {
            _requiredResources = value;
            OnPropertyChanged();
        }
    }

    public string CraftingNotes
    {
        get => _craftingNotes;
        set
        {
            _craftingNotes = value;
            OnPropertyChanged();
        }
    }

    public Visibility RequiredJournalVisibility
    {
        get => _requiredJournalVisibility;
        set
        {
            _requiredJournalVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility CraftingTabVisibility
    {
        get => _craftingTabVisibility;
        set
        {
            _craftingTabVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility ErrorBarVisibility
    {
        get => _errorBarVisibility;
        set
        {
            _errorBarVisibility = value;
            OnPropertyChanged();
        }
    }

    public string ErrorBarText
    {
        get => _errorBarText;
        set
        {
            _errorBarText = value;
            OnPropertyChanged();
        }
    }

    public List<MarketResponse> CurrentItemPrices
    {
        get => _currentItemPrices;
        set
        {
            _currentItemPrices = value;
            if (EssentialCraftingValues != null)
            {
                EssentialCraftingValues.CurrentCityPrices = value;
            }

            OnPropertyChanged();
        }
    }

    public bool IsAutoUpdateActive
    {
        get => _isAutoUpdateActive;
        set
        {
            _isAutoUpdateActive = value;

            _timer.Enabled = _isAutoUpdateActive;
            RefreshSpin = _isAutoUpdateActive;
            OnPropertyChanged();
        }
    }

    public double TaskProgressbarMinimum
    {
        get => _taskProgressbarMinimum;
        set
        {
            _taskProgressbarMinimum = value;
            OnPropertyChanged();
        }
    }

    public double TaskProgressbarMaximum
    {
        get => _taskProgressbarMaximum;
        set
        {
            _taskProgressbarMaximum = value;
            OnPropertyChanged();
        }
    }

    public double TaskProgressbarValue
    {
        get => _taskProgressbarValue;
        set
        {
            _taskProgressbarValue = value;
            OnPropertyChanged();
        }
    }

    public bool IsTaskProgressbarIndeterminate
    {
        get => _isTaskProgressbarIndeterminate;
        set
        {
            _isTaskProgressbarIndeterminate = value;
            OnPropertyChanged();
        }
    }

    public string RefreshIconTooltipText
    {
        get => _refreshIconTooltipText;
        set
        {
            _refreshIconTooltipText = value;
            OnPropertyChanged();
        }
    }

    public int TabControlSelectedIndex
    {
        get => _tabControlSelectedIndex;
        set
        {
            _tabControlSelectedIndex = value;

            // 2 is History tab
            if (_tabControlSelectedIndex == 2)
            {
                UpdateHistoryTabChartPricesAsync();
            }

            OnPropertyChanged();
        }
    }

    public ObservableCollection<MainTabLocationFilterObject> LocationFilters
    {
        get => _locationFilters;
        set
        {
            _locationFilters = value;
            OnPropertyChanged();
        }
    }

    public ExtraItemInformation ExtraItemInformation
    {
        get => _extraItemInformation;
        set
        {
            _extraItemInformation = value;
            OnPropertyChanged();
        }
    }

    public ItemWindowTranslation Translation
    {
        get => _translation;
        set
        {
            _translation = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Helper

    public ulong Sum(params ulong[] values)
    {
        return values.Aggregate(0UL, (current, t) => current + t);
    }

    public ulong Average(params ulong[] values)
    {
        if (values.Length == 0) return 0;

        var sum = Sum(values);
        var result = sum / (ulong) values.Length;
        return result;
    }

    #endregion
}