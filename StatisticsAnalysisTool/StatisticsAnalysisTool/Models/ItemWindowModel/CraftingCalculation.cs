using StatisticsAnalysisTool.Common;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models.ItemWindowModel
{
    public class CraftingCalculation : INotifyPropertyChanged
    {
        private double _possibleItemCrafting;
        private double _craftingTax;
        private double _setupFee;
        private double _auctionsHouseTax;
        private double _totalJournalCosts;
        private double _totalCosts;
        private long _totalResourceCosts;
        private double _totalIncomeFromSales;

        private double GetTotalCosts()
        {
            return CraftingTax + SetupFee + AuctionsHouseTax + TotalJournalCosts + TotalResourceCosts;
        }

        public double PossibleItemCrafting
        {
            get => _possibleItemCrafting;
            set
            {
                _possibleItemCrafting = value;
                TotalCosts = GetTotalCosts();
                OnPropertyChanged();
            }
        }

        public double CraftingTax
        {
            get => _craftingTax;
            set
            {
                _craftingTax = value;
                TotalCosts = GetTotalCosts();
                OnPropertyChanged();
            }
        }

        public double SetupFee
        {
            get => _setupFee;
            set
            {
                _setupFee = value;
                TotalCosts = GetTotalCosts();
                OnPropertyChanged();
            }
        }

        public double AuctionsHouseTax
        {
            get => _auctionsHouseTax;
            set
            {
                _auctionsHouseTax = value;
                TotalCosts = GetTotalCosts();
                OnPropertyChanged();
            }
        }
        
        public double TotalJournalCosts
        {
            get => _totalJournalCosts;
            set
            {
                _totalJournalCosts = value;
                TotalCosts = GetTotalCosts();
                OnPropertyChanged();
            }
        }

        public long TotalResourceCosts
        {
            get => _totalResourceCosts;
            set
            {
                _totalResourceCosts = value;
                TotalCosts = GetTotalCosts();
                OnPropertyChanged();
            }
        }

        public double TotalCosts
        {
            get => _totalCosts;
            set
            {
                _totalCosts = value;
                OnPropertyChanged();
            }
        }

        public double TotalIncomeFromSales
        {
            get => _totalIncomeFromSales;
            set
            {
                _totalIncomeFromSales = value;
                OnPropertyChanged();
            }
        }

        public string TranslationCalculation => LanguageController.Translation("CALCULATION");
        public string TranslationPossibleCrafting => LanguageController.Translation("POSSIBLE_CRAFTING");
        public string TranslationPossibleItemCrafting => LanguageController.Translation("POSSIBLE_ITEM_CRAFTING");
        public string TranslationStatementOfCost => LanguageController.Translation("STATEMENT_OF_COST");
        public string TranslationCraftingTax => LanguageController.Translation("CRAFTING_TAX");
        public string TranslationSetupFee => LanguageController.Translation("SETUP_FEE");
        public string TranslationAuctionsHouseTax => LanguageController.Translation("AUCTIONS_HOUSE_TAX");
        public string TranslationTotalJournalCosts => LanguageController.Translation("TOTAL_JOURNAL_COSTS");
        public string TranslationTotalCosts => LanguageController.Translation("TOTAL_COSTS");
        public string TranslationTotalResourceCosts => LanguageController.Translation("TOTAL_RESOURCE_COSTS");
        public string TranslationTotalIncomeFromSales => LanguageController.Translation("TOTAL_INCOME_FROM_SALES");

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}