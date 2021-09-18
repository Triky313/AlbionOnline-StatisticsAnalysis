using StatisticsAnalysisTool.Common;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Models.ItemWindowModel
{
    public class RequiredResource
    {
        private string _craftingResourceName;
        private int _resourceCost;
        private BitmapImage _icon;
        private int _quantity;

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

        public int ResourceCost
        {
            get => _resourceCost;
            set
            {
                _resourceCost = value;
                OnPropertyChanged();
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }

        public string TranslationCost => LanguageController.Translation("COST");
        public string TranslationQuantity => LanguageController.Translation("QUANTITY");

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}