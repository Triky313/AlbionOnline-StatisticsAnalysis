using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

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

        public string UniqueName { get; set; }

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
                _ = _itemWindowViewModel.UpdateCraftingValuesAsync();
                OnPropertyChanged();
            }
        }

        public string TranslationRequiredJournals => LanguageController.Translation("REQUIRED_JOURNALS");
        public string TranslationCostsPerJournal => LanguageController.Translation("COSTS_PER_JOURNAL");
        public string TranslationRequiredJournalAmount => LanguageController.Translation("REQUIRED_JOURNAL_AMOUNT");
        public string TranslationSellPricePerJournal => LanguageController.Translation("SELL_PRICE_PER_JOURNAL");

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}