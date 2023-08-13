using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class EssentialCraftingValuesTemplate : INotifyPropertyChanged
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
    private Location _itemPricesLocationSelected;
    private DateTime _lastUpdate = DateTime.UtcNow.AddDays(-100);
    private bool _isCraftingWithFocusCheckboxEnabled;
    private bool _isCraftingBonusEnabled;

    public EssentialCraftingValuesTemplate(ItemWindowViewModel itemWindowViewModel, List<MarketResponse> currentCityPrices, string uniqueName)
    {
        _itemWindowViewModel = itemWindowViewModel;
        CurrentCityPrices = currentCityPrices;
        UniqueName = uniqueName;
    }

    private async void LoadSellPriceAsync(Location location)
    {
        if (_lastUpdate.AddMilliseconds(SettingsController.CurrentSettings.RefreshRate) < DateTime.UtcNow)
        {
            _marketResponse = await ApiController.GetCityItemPricesFromJsonAsync(UniqueName);
            _lastUpdate = DateTime.UtcNow;
        }

        if (location == Location.BlackMarket)
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

    public KeyValuePair<Location, string>[] ItemPricesLocations { get; } =
    {
        new (Location.BlackMarket, WorldData.GetUniqueNameOrDefault((int)Location.BlackMarket)),
        new (Location.Martlock, WorldData.GetUniqueNameOrDefault((int)Location.Martlock)),
        new (Location.Thetford, WorldData.GetUniqueNameOrDefault((int)Location.Thetford)),
        new (Location.FortSterling, WorldData.GetUniqueNameOrDefault((int)Location.FortSterling)),
        new (Location.Lymhurst, WorldData.GetUniqueNameOrDefault((int)Location.Lymhurst)),
        new (Location.Bridgewatch, WorldData.GetUniqueNameOrDefault((int)Location.Bridgewatch)),
        new (Location.Caerleon, WorldData.GetUniqueNameOrDefault((int)Location.Caerleon)),
        new (Location.Brecilien, WorldData.GetUniqueNameOrDefault((int)Location.Brecilien)),
        new (Location.MerlynsRest, WorldData.GetUniqueNameOrDefault((int)Location.MerlynsRest)),
        new (Location.MorganasRest, WorldData.GetUniqueNameOrDefault((int)Location.MorganasRest)),
        new (Location.ArthursRest, WorldData.GetUniqueNameOrDefault((int)Location.ArthursRest))
    };

    public Location ItemPricesLocationSelected
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

    public string TranslationSellPricePerItem => LanguageController.Translation("SELL_PRICE_PER_ITEM");
    public string TranslationItemQuantity => LanguageController.Translation("ITEM_QUANTITY");
    public string TranslationSetupFeePercent => LanguageController.Translation("SETUP_FEE_PERCENT");
    public string TranslationAuctionsHouseTaxPercent => LanguageController.Translation("AUCTIONS_HOUSE_TAX_PERCENT");
    public string TranslationUsageFeePerHundredFood => LanguageController.Translation("USAGE_FEE_PER_HUNDRED_FOOD");
    public string TranslationCraftingBonusPercent => LanguageController.Translation("CRAFTING_BONUS_PERCENT");
    public string TranslationCraftingWithFocus => LanguageController.Translation("CRAFTING_WITH_FOCUS");
    public string TranslationOtherCosts => LanguageController.Translation("OTHER_COSTS");

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}