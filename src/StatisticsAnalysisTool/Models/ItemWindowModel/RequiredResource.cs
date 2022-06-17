using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Models.ItemWindowModel
{
    public class RequiredResource : INotifyPropertyChanged
    {
        public string UniqueName { get; set; }
        private string _craftingResourceName;
        private long _resourceCost;
        private BitmapImage _icon;
        private long _totalQuantity;
        private long _totalCost;
        private long _craftingQuantity;
        private long _oneProductionAmount;
        private readonly ItemWindowViewModel _itemWindowViewModel;
        private bool _isArtifactResource;
        private List<MarketResponse> _marketResponse = new();
        private Location _itemPricesLocationSelected;
        private DateTime _lastUpdate = DateTime.UtcNow.AddDays(-100);

        public RequiredResource(ItemWindowViewModel itemWindowViewModel)
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

            var sellPriceMin = _marketResponse?.FirstOrDefault(x => string.Equals(x?.City, Locations.GetParameterName(location), StringComparison.CurrentCultureIgnoreCase))?.SellPriceMin;
            if (sellPriceMin != null)
            {
                ResourceCost = (long)sellPriceMin;
            }
        }

        public KeyValuePair<Location, string>[] ItemPricesLocations { get; } =
        {
            new (Location.Martlock, WorldData.GetUniqueNameOrDefault((int)Location.Martlock)),
            new (Location.Thetford, WorldData.GetUniqueNameOrDefault((int)Location.Thetford)),
            new (Location.FortSterling, WorldData.GetUniqueNameOrDefault((int)Location.FortSterling)),
            new (Location.Lymhurst, WorldData.GetUniqueNameOrDefault((int)Location.Lymhurst)),
            new (Location.Bridgewatch, WorldData.GetUniqueNameOrDefault((int)Location.Bridgewatch)),
            new (Location.Caerleon, WorldData.GetUniqueNameOrDefault((int)Location.Caerleon)),
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

        public string CraftingResourceName
        {
            get => _craftingResourceName;
            set
            {
                _craftingResourceName = value;
                OnPropertyChanged();
            }
        }

        public bool IsArtifactResource
        {
            get => _isArtifactResource;
            set
            {
                _isArtifactResource = value;
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

        public long ResourceCost
        {
            get => _resourceCost;
            set
            {
                _resourceCost = value;

                TotalQuantity = OneProductionAmount * CraftingQuantity;
                TotalCost = ResourceCost * TotalQuantity;
                OnPropertyChanged();
            }
        }

        public long OneProductionAmount
        {
            get => _oneProductionAmount;
            set
            {
                _oneProductionAmount = value;

                TotalQuantity = OneProductionAmount * CraftingQuantity;
                TotalCost = ResourceCost * TotalQuantity;
                OnPropertyChanged();
            }
        }

        public long TotalQuantity
        {
            get => _totalQuantity;
            set
            {
                _totalQuantity = value;
                OnPropertyChanged();
            }
        }

        public long TotalCost
        {
            get => _totalCost;
            set
            {
                _totalCost = value;
                _itemWindowViewModel.UpdateCraftingCalculationTotalResourceCosts();
                OnPropertyChanged();
            }
        }

        public long CraftingQuantity
        {
            get => _craftingQuantity;
            set
            {
                _craftingQuantity = value;

                TotalQuantity = OneProductionAmount * CraftingQuantity;
                TotalCost = ResourceCost * TotalQuantity;
                OnPropertyChanged();
            }
        }

        public string TranslationCost => LanguageController.Translation("COST");
        public string TranslationOneProductionAmount => LanguageController.Translation("ONE_PRODUCTION_AMOUNT");
        public string TranslationTotalQuantity => LanguageController.Translation("TOTAL_QUANTITY");
        public string TranslationTotalCost => LanguageController.Translation("TOTAL_COST");
        public string TranslationGetPrice => LanguageController.Translation("GET_PRICE");

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}