using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

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

        public RequiredResource(ItemWindowViewModel itemWindowViewModel)
        {
            _itemWindowViewModel = itemWindowViewModel;
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
                TotalCost = ResourceCost * CraftingQuantity;
                OnPropertyChanged();
            }
        }

        public string TranslationCost => LanguageController.Translation("COST");
        public string TranslationOneProductionAmount => LanguageController.Translation("ONE_PRODUCTION_AMOUNT");
        public string TranslationTotalQuantity => LanguageController.Translation("TOTAL_QUANTITY");
        public string TranslationTotalCost => LanguageController.Translation("TOTAL_COST");

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}