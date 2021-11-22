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
        private double _totalResourceCosts;
        private double _totalItemSells;
        private double _totalJournalSells;
        private double _totalSells;
        private double _grandTotal;
        private double _otherCosts;

        private double GetTotalCosts()
        {
            return CraftingTax + SetupFee + AuctionsHouseTax + TotalJournalCosts + TotalResourceCosts + OtherCosts;
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

        public double OtherCosts
        {
            get => _otherCosts;
            set
            {
                _otherCosts = value;
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

        public double TotalResourceCosts
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
                GrandTotal = TotalSells - TotalCosts;
                OnPropertyChanged();
            }
        }

        public double TotalItemSells
        {
            get => _totalItemSells;
            set
            {
                _totalItemSells = value;
                TotalSells = TotalItemSells + TotalJournalSells;
                OnPropertyChanged();
            }
        }

        public double TotalJournalSells
        {
            get => _totalJournalSells;
            set
            {
                _totalJournalSells = value;
                TotalSells = TotalItemSells + TotalJournalSells;
                OnPropertyChanged();
            }
        }

        public double TotalSells
        {
            get => _totalSells;
            set
            {
                _totalSells = value;
                GrandTotal = TotalSells - TotalCosts;
                OnPropertyChanged();
            }
        }

        public double GrandTotal
        {
            get => _grandTotal;
            set
            {
                _grandTotal = value;
                OnPropertyChanged();
            }
        }

        public static string TranslationCalculation => LanguageController.Translation("CALCULATION");
        public static string TranslationPossibleCrafting => LanguageController.Translation("POSSIBLE_CRAFTING");
        public static string TranslationPossibleItemCrafting => LanguageController.Translation("POSSIBLE_ITEM_CRAFTING");
        public static string TranslationStatementOfCost => LanguageController.Translation("STATEMENT_OF_COST");
        public static string TranslationCraftingTax => LanguageController.Translation("CRAFTING_TAX");
        public static string TranslationSetupFee => LanguageController.Translation("SETUP_FEE");
        public static string TranslationAuctionsHouseTax => LanguageController.Translation("AUCTIONS_HOUSE_TAX");
        public static string TranslationTotalJournalCosts => LanguageController.Translation("TOTAL_JOURNAL_COSTS");
        public static string TranslationTotalCosts => LanguageController.Translation("TOTAL_COSTS");
        public static string TranslationTotalResourceCosts => LanguageController.Translation("TOTAL_RESOURCE_COSTS");
        public static string TranslationOtherCosts => LanguageController.Translation("OTHER_COSTS");
        public static string TranslationTotalItemSells => LanguageController.Translation("TOTAL_ITEM_SELLS");
        public static string TranslationTotalJournalSells => LanguageController.Translation("TOTAL_JOURNAL_SELLS");
        public static string TranslationTotalSells => LanguageController.Translation("TOTAL_SELLS");
        public static string TranslationGrandTotal => LanguageController.Translation("GRAND_TOTAL");

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}