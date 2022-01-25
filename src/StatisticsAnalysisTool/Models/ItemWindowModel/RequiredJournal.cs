using System;
using System.Collections.Generic;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using StatisticsAnalysisTool.Common.UserSettings;

namespace StatisticsAnalysisTool.Models.ItemWindowModel
{
    public class RequiredJournal : INotifyPropertyChanged
    {
        private BitmapImage _icon;
        private string _craftingResourceName;
        private long _costsPerJournal;
        private double _requiredJournalAmount;
        private readonly ItemWindowViewModel _itemWindowViewModel;
        private double _sellPricePerJournal;
        private List<MarketResponse> _marketResponseEmptyJournal = new();
        private List<MarketResponse> _marketResponseFullJournal = new();
        private DateTime _lastUpdateEmptyJournal = DateTime.UtcNow.AddDays(-100);
        private DateTime _lastUpdateFullJournal = DateTime.UtcNow.AddDays(-100);
        private Location _itemPricesLocationEmptyJournalSelected;
        private Location _itemPricesLocationFullJournalSelected;

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

        public string UniqueName { get; set; }
        
        private async void LoadSellPriceEmptyJournalAsync(Location location)
        {
            if (_lastUpdateEmptyJournal.AddMilliseconds(SettingsController.CurrentSettings.RefreshRate) < DateTime.UtcNow)
            {
                _marketResponseEmptyJournal = await ApiController.GetCityItemPricesFromJsonAsync(UniqueName);
                _lastUpdateEmptyJournal = DateTime.UtcNow;
            }

            var sellPriceMin = _marketResponseEmptyJournal?.FirstOrDefault(x => string.Equals(x?.City, Locations.GetParameterName(location), StringComparison.CurrentCultureIgnoreCase))?.SellPriceMin;
            if (sellPriceMin != null)
            {
                CostsPerJournal = (long)sellPriceMin;
            }
        }

        private async void LoadSellPriceFullJournalAsync(Location location)
        {
            var uniqueNameFullJournal = UniqueName.Replace("EMPTY", "FULL");
            if (_lastUpdateFullJournal.AddMilliseconds(SettingsController.CurrentSettings.RefreshRate) < DateTime.UtcNow)
            {
                _marketResponseFullJournal = await ApiController.GetCityItemPricesFromJsonAsync(uniqueNameFullJournal);
                _lastUpdateFullJournal = DateTime.UtcNow;
            }

            var sellPriceMin = _marketResponseFullJournal?.FirstOrDefault(x => string.Equals(x?.City, Locations.GetParameterName(location), StringComparison.CurrentCultureIgnoreCase))?.SellPriceMin;
            if (sellPriceMin != null)
            {
                SellPricePerJournal = (long)sellPriceMin;
            }
        }

        public KeyValuePair<Location, string>[] ItemPricesLocationsEmptyJournal => _itemPricesLocations;
        public KeyValuePair<Location, string>[] ItemPricesLocationsFullJournal => _itemPricesLocations;

        public Location ItemPricesLocationEmptyJournalSelected
        {
            get => _itemPricesLocationEmptyJournalSelected;
            set
            {
                _itemPricesLocationEmptyJournalSelected = value;
                LoadSellPriceEmptyJournalAsync(value);
                OnPropertyChanged();
            }
        }

        public Location ItemPricesLocationFullJournalSelected
        {
            get => _itemPricesLocationFullJournalSelected;
            set
            {
                _itemPricesLocationFullJournalSelected = value;
                LoadSellPriceFullJournalAsync(value);
                OnPropertyChanged();
            }
        }

        public RequiredJournal(ItemWindowViewModel itemWindowViewModel)
        {
            _itemWindowViewModel = itemWindowViewModel;
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

        public string CraftingResourceName
        {
            get => _craftingResourceName;
            set
            {
                _craftingResourceName = value;
                OnPropertyChanged();
            }
        }

        public long CostsPerJournal
        {
            get => _costsPerJournal;
            set
            {
                _costsPerJournal = value;
                _itemWindowViewModel.CraftingCalculation.TotalJournalCosts = CostsPerJournal * RequiredJournalAmount;
                _itemWindowViewModel.UpdateCraftingValues();
                OnPropertyChanged();
            }
        }

        public double RequiredJournalAmount
        {
            get => _requiredJournalAmount;
            set
            {
                _requiredJournalAmount = value;
                OnPropertyChanged();
            }
        }

        public double SellPricePerJournal
        {
            get => _sellPricePerJournal;
            set
            {
                _sellPricePerJournal = value;
                _itemWindowViewModel.UpdateCraftingValues();
                OnPropertyChanged();
            }
        }

        public string TranslationRequiredJournals => LanguageController.Translation("REQUIRED_JOURNALS");
        public string TranslationCostsPerJournal => LanguageController.Translation("COSTS_PER_JOURNAL");
        public string TranslationRequiredJournalAmount => LanguageController.Translation("REQUIRED_JOURNAL_AMOUNT");
        public string TranslationSellPricePerJournal => LanguageController.Translation("SELL_PRICE_PER_JOURNAL");
        public string TranslationGetPrice => LanguageController.Translation("GET_PRICE");

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}