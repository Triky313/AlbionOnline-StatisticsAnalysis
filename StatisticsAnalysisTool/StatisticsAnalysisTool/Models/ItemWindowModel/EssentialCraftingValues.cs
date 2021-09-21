using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using System.ComponentModel;
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
        private short _craftingTax;
        private short _craftingBonus;

        public EssentialCraftingValuesTemplate(ItemWindowViewModel itemWindowViewModel)
        {
            _itemWindowViewModel = itemWindowViewModel;
        }

        public int CraftingItemQuantity
        {
            get => _craftingItemQuantity;
            set
            {
                _craftingItemQuantity = value;
                _itemWindowViewModel.UpdateCraftingValues(CraftingItemQuantity);
                OnPropertyChanged();
            }
        }

        public long SellPricePerItem
        {
            get => _sellPricePerItem;
            set
            {
                _sellPricePerItem = value;
                _itemWindowViewModel.UpdateCraftingValues(CraftingItemQuantity);
                OnPropertyChanged();
            }
        }
        
        public double SetupFee
        {
            get => _setupFee;
            set
            {
                _setupFee = value;
                _itemWindowViewModel.UpdateCraftingValues(CraftingItemQuantity);
                OnPropertyChanged();
            }
        }

        public double AuctionHouseTax
        {
            get => _auctionHouseTax;
            set
            {
                _auctionHouseTax = value;
                _itemWindowViewModel.UpdateCraftingValues(CraftingItemQuantity);
                OnPropertyChanged();
            }
        }

        public short CraftingTax
        {
            get => _craftingTax;
            set
            {
                _craftingTax = value;
                _itemWindowViewModel.UpdateCraftingValues(CraftingItemQuantity);
                OnPropertyChanged();
            }
        }

        public short CraftingBonus
        {
            get => _craftingBonus;
            set
            {
                _craftingBonus = value;
                _itemWindowViewModel.UpdateCraftingValues(CraftingItemQuantity);
                OnPropertyChanged();
            }
        }


        public string TranslationSellPricePerItem => LanguageController.Translation("SELL_PRICE_PER_ITEM");
        public string TranslationItemQuantity => LanguageController.Translation("ITEM_QUANTITY");
        public string TranslationSetupFeePercent => LanguageController.Translation("SETUP_FEE_PERCENT");
        public string TranslationAuctionsHouseTaxPercent => LanguageController.Translation("AUCTIONS_HOUSE_TAX_PERCENT");
        public string TranslationCraftingTaxPercent => LanguageController.Translation("CRAFTING_TAX_PERCENT");
        public string TranslationCraftingBonusPercent => LanguageController.Translation("CRAFTING_BONUS_PERCENT");


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}