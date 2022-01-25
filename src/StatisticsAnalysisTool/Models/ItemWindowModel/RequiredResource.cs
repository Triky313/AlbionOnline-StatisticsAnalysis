using System;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using StatisticsAnalysisTool.Common.UserSettings;

namespace StatisticsAnalysisTool.Models.ItemWindowModel
{
    public class RequiredResource : INotifyPropertyChanged
    {
        private string _craftingResourceName;
        private long _resourceCost;
        private BitmapImage _icon;
        private long _totalQuantity;
        private long _totalCost;
        private long _craftingQuantity;
        private long _oneProductionAmount;
        private readonly ItemWindowViewModel _itemWindowViewModel;
        private bool _isArtifactResource;
        private string _uniqueName;
        private List<MarketResponse> _marketResponse = new ();
        private Location _itemPricesLocationSelected;
        private DateTime _lastUpdate = DateTime.UtcNow.AddDays(-100);

        private static readonly KeyValuePair<Location, string>[] _itemPricesLocations = {
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

        public KeyValuePair<Location, string>[] ItemPricesLocations => _itemPricesLocations;

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

        public string UniqueName
        {
            get => _uniqueName;
            set
            {
                _uniqueName = value;
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