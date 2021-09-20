using StatisticsAnalysisTool.Common;
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
        private string _totalQuantityString;
        private string _totalCostString;
        private long _oneProductionAmount;

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
                TotalCost = ResourceCost * CraftingQuantity;
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
                TotalCost = ResourceCost * CraftingQuantity;
                OnPropertyChanged();
            }
        }

        public long TotalQuantity
        {
            get => _totalQuantity;
            set
            {
                _totalQuantity = value;
                TotalQuantityString = Utilities.LongNumberToString(_totalQuantity);
                OnPropertyChanged();
            }
        }

        public long TotalCost
        {
            get => _totalCost;
            set
            {
                _totalCost = value;
                TotalCostString = Utilities.LongNumberToString(_totalCost);
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

        #region String value bindings

        public string TotalQuantityString
        {
            get => _totalQuantityString;
            private set
            {
                _totalQuantityString = value;
                OnPropertyChanged();
            }
        }

        public string TotalCostString
        {
            get => _totalCostString;
            private set
            {
                _totalCostString = value;
                OnPropertyChanged();
            }
        }

        #endregion

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