using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models.ItemWindowModel
{
    public class EssentialCraftingValuesTemplate : INotifyPropertyChanged
    {
        private readonly ItemWindowViewModel _itemWindowViewModel;
        private long _sellPricePerItem;
        private int _craftingItemQuantity;
        private double _setupFee;
        private double _auctionHouseTax;
        private int _usageFeePerHundredFood;
        private int _craftingBonus;
        private bool _isCraftingWithFocus;
        private int _otherCosts;
        public List<MarketResponse> CurrentCityPrices { get; set; }

        public string UniqueName;
        private List<MarketResponse> _marketResponse = new();
        private Location _itemPricesLocationSelected;
        private DateTime _lastUpdate = DateTime.UtcNow.AddDays(-100);

        public EssentialCraftingValuesTemplate(ItemWindowViewModel itemWindowViewModel)
        {
            _itemWindowViewModel = itemWindowViewModel;
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
                    SellPricePerItem = (long)buyPriceMax;
                }

                return;
            }

            var sellPriceMin = _marketResponse?.FirstOrDefault(x => string.Equals(x?.City, Locations.GetParameterName(location), StringComparison.CurrentCultureIgnoreCase))?.SellPriceMin;
            if (sellPriceMin != null)
            {
                SellPricePerItem = (long)sellPriceMin;
            }
        }

        public KeyValuePair<Location, string>[] ItemPricesLocations { get; } =
        {
            new (Location.BlackMarket, Locations.GetName(Location.BlackMarket)),
            new (Location.Martlock, Locations.GetName(Location.Martlock)),
            new (Location.Thetford, Locations.GetName(Location.Thetford)),
            new (Location.FortSterling, Locations.GetName(Location.FortSterling)),
            new (Location.Lymhurst, Locations.GetName(Location.Lymhurst)),
            new (Location.Bridgewatch, Locations.GetName(Location.Bridgewatch)),
            new (Location.Caerleon, Locations.GetName(Location.Caerleon)),
            new (Location.MerlynsRest, Locations.GetName(Location.MerlynsRest)),
            new (Location.MorganasRest, Locations.GetName(Location.MorganasRest)),
            new (Location.ArthursRest, Locations.GetName(Location.ArthursRest))
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

        public int CraftingItemQuantity
        {
            get => _craftingItemQuantity;
            set
            {
                _craftingItemQuantity = value;
                _itemWindowViewModel.UpdateCraftingValues();
                OnPropertyChanged();
            }
        }

        public long SellPricePerItem
        {
            get => _sellPricePerItem;
            set
            {
                _sellPricePerItem = value;
                _itemWindowViewModel.UpdateCraftingValues();
                OnPropertyChanged();
            }
        }

        public double SetupFee
        {
            get => _setupFee;
            set
            {
                _setupFee = value;
                _itemWindowViewModel.UpdateCraftingValues();
                OnPropertyChanged();
            }
        }

        public double AuctionHouseTax
        {
            get => _auctionHouseTax;
            set
            {
                _auctionHouseTax = value;
                _itemWindowViewModel.UpdateCraftingValues();
                OnPropertyChanged();
            }
        }

        public int UsageFeePerHundredFood
        {
            get => _usageFeePerHundredFood;
            set
            {
                _usageFeePerHundredFood = value;
                _itemWindowViewModel.UpdateCraftingValues();
                OnPropertyChanged();
            }
        }

        public int CraftingBonus
        {
            get => _craftingBonus;
            set
            {
                _craftingBonus = value;
                _itemWindowViewModel.UpdateCraftingValues();
                OnPropertyChanged();
            }
        }

        public int OtherCosts
        {
            get => _otherCosts;
            set
            {
                _otherCosts = value;
                _itemWindowViewModel.UpdateCraftingValues();
                OnPropertyChanged();
            }
        }

        public bool IsCraftingWithFocus
        {
            get => _isCraftingWithFocus;
            set
            {
                _isCraftingWithFocus = value;
                _itemWindowViewModel.UpdateCraftingValues();
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
}