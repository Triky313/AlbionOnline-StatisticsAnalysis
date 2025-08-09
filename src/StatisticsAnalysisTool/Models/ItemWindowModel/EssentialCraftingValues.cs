using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class EssentialCraftingValuesTemplate : BaseViewModel
{
    private readonly ItemWindowViewModel _itemWindowViewModel;
    private int _amountCrafted = 1;
    private long _sellPricePerItem;
    private int _craftingItemQuantity = 1;
    private double _setupFee = 2.5d;
    private double _auctionHouseTax = 4.0d;
    private int _usageFeePerHundredFood;
    private int _craftingBonus = 133;
    private bool _isCraftingWithFocus;
    private int _otherCosts;
    private bool _isAmountCraftedRelevant;

    public List<MarketResponse> CurrentCityPrices { get; set; }

    public readonly string UniqueName;
    private List<MarketResponse> _marketResponse = new();
    private MarketLocation _itemPricesLocationSelected;
    private DateTime _lastUpdate = DateTime.UtcNow.AddDays(-100);
    private bool _isCraftingWithFocusCheckboxEnabled;
    private bool _isCraftingBonusEnabled;

    public EssentialCraftingValuesTemplate(ItemWindowViewModel itemWindowViewModel, List<MarketResponse> currentCityPrices, string uniqueName)
    {
        _itemWindowViewModel = itemWindowViewModel;
        CurrentCityPrices = currentCityPrices;
        UniqueName = uniqueName;
    }

    private async void LoadSellPriceAsync(MarketLocation location)
    {
        if (_lastUpdate.AddMilliseconds(SettingsController.CurrentSettings.RefreshRate) < DateTime.UtcNow)
        {
            _marketResponse = await ApiController.GetCityItemPricesFromJsonAsync(UniqueName);
            _lastUpdate = DateTime.UtcNow;
        }

        if (location == MarketLocation.BlackMarket)
        {
            var buyPriceMax = _marketResponse?.FirstOrDefault(x => string.Equals(x?.City, Locations.GetParameterName(location), StringComparison.CurrentCultureIgnoreCase))?.BuyPriceMax;
            if (buyPriceMax != null)
            {
                SellPricePerItem = (long) buyPriceMax;
            }

            return;
        }

        var sellPriceMin = _marketResponse?.FirstOrDefault(x => string.Equals(x?.City, Locations.GetParameterName(location), StringComparison.CurrentCultureIgnoreCase))?.SellPriceMin;
        if (sellPriceMin != null)
        {
            SellPricePerItem = (long) sellPriceMin;
        }
    }

    public KeyValuePair<MarketLocation, string>[] ItemPricesLocations { get; } =
    [
        new (MarketLocation.BlackMarket, "Black Market"),
        new (MarketLocation.MartlockMarket, WorldData.GetUniqueNameOrDefault("3004")),
        new (MarketLocation.ThetfordMarket, WorldData.GetUniqueNameOrDefault("0000")),
        new (MarketLocation.FortSterlingMarket, WorldData.GetUniqueNameOrDefault("4000")),
        new (MarketLocation.LymhurstMarket, WorldData.GetUniqueNameOrDefault("1000")),
        new (MarketLocation.BridgewatchMarket, WorldData.GetUniqueNameOrDefault("2000")),
        new (MarketLocation.CaerleonMarket, WorldData.GetUniqueNameOrDefault("3003")),
        new (MarketLocation.BrecilienMarket, WorldData.GetUniqueNameOrDefault("5000")),
        new (MarketLocation.SmugglersDen, "Smuggler's Den")
    ];

    public MarketLocation ItemPricesLocationSelected
    {
        get => _itemPricesLocationSelected;
        set
        {
            _itemPricesLocationSelected = value;
            LoadSellPriceAsync(value);
            OnPropertyChanged();
        }
    }

    #region Bindings

    public int AmountCrafted
    {
        get => _amountCrafted;
        set
        {
            _amountCrafted = value;
            IsAmountCraftedRelevant = _amountCrafted > 1;
            _itemWindowViewModel.UpdateCraftingCalculationTab();
            OnPropertyChanged();
        }
    }

    public bool IsAmountCraftedRelevant
    {
        get => _isAmountCraftedRelevant;
        set
        {
            _isAmountCraftedRelevant = value;
            OnPropertyChanged();
        }
    }

    public int CraftingItemQuantity
    {
        get => _craftingItemQuantity;
        set
        {
            _craftingItemQuantity = value;
            _itemWindowViewModel.UpdateCraftingCalculationTab();
            OnPropertyChanged();
        }
    }

    public long SellPricePerItem
    {
        get => _sellPricePerItem;
        set
        {
            _sellPricePerItem = value;
            _itemWindowViewModel.UpdateCraftingCalculationTab();
            OnPropertyChanged();
        }
    }

    public double SetupFee
    {
        get => _setupFee;
        set
        {
            _setupFee = value;
            _itemWindowViewModel.UpdateCraftingCalculationTab();
            OnPropertyChanged();
        }
    }

    public double AuctionHouseTax
    {
        get => _auctionHouseTax;
        set
        {
            _auctionHouseTax = value;
            _itemWindowViewModel.UpdateCraftingCalculationTab();
            OnPropertyChanged();
        }
    }

    public int UsageFeePerHundredFood
    {
        get => _usageFeePerHundredFood;
        set
        {
            _usageFeePerHundredFood = value;
            _itemWindowViewModel.UpdateCraftingCalculationTab();
            OnPropertyChanged();
        }
    }

    public int CraftingBonus
    {
        get => _craftingBonus;
        set
        {
            _craftingBonus = value;
            _itemWindowViewModel.UpdateCraftingCalculationTab();
            OnPropertyChanged();
        }
    }

    public bool IsCraftingBonusEnabled
    {
        get => _isCraftingBonusEnabled;
        set
        {
            _isCraftingBonusEnabled = value;
            OnPropertyChanged();
        }
    }

    public int OtherCosts
    {
        get => _otherCosts;
        set
        {
            _otherCosts = value;
            _itemWindowViewModel.UpdateCraftingCalculationTab();
            OnPropertyChanged();
        }
    }

    public bool IsCraftingWithFocus
    {
        get => _isCraftingWithFocus;
        set
        {
            _isCraftingWithFocus = value;
            _itemWindowViewModel.UpdateCraftingCalculationTab();
            OnPropertyChanged();
        }
    }

    public bool IsCraftingWithFocusCheckboxEnabled
    {
        get => _isCraftingWithFocusCheckboxEnabled;
        set
        {
            _isCraftingWithFocusCheckboxEnabled = value;
            OnPropertyChanged();
        }
    }

    public string TranslationSellPricePerItem => LocalizationController.Translation("SELL_PRICE_PER_ITEM");
    public string TranslationItemQuantity => LocalizationController.Translation("ITEM_QUANTITY");
    public string TranslationSetupFeePercent => LocalizationController.Translation("SETUP_FEE_PERCENT");
    public string TranslationAuctionsHouseTaxPercent => LocalizationController.Translation("AUCTIONS_HOUSE_TAX_PERCENT");
    public string TranslationUsageFeePerHundredFood => LocalizationController.Translation("USAGE_FEE_PER_HUNDRED_FOOD");
    public string TranslationCraftingBonusPercent => LocalizationController.Translation("CRAFTING_BONUS_PERCENT");
    public string TranslationCraftingWithFocus => LocalizationController.Translation("CRAFTING_WITH_FOCUS");
    public string TranslationOtherCosts => LocalizationController.Translation("OTHER_COSTS");

    #endregion
}