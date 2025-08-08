using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class CraftingCalculation : BaseViewModel
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
    private int _amountCrafted;
    private bool _isAmountCraftedRelevant;
    private double _totalResourcesWeight;
    private double _totalRequiredJournalWeight;
    private double _totalCraftedItemWeight;
    private double _totalUnfinishedCraftingWeight;
    private double _totalFinishedCraftingWeight;
    private double _returnOnInvestment;
    private double _breakEvenPrice;
    private double _profitPerItem;
    private Visibility _weightValuesVisibility = Visibility.Collapsed;
    private bool _isReturnOnInvestmentNegative;
    private bool _isBreakEvenPriceNegative;
    private bool _isProfitPerItemNegative;

    public int AmountCrafted
    {
        get => _amountCrafted;
        set
        {
            _amountCrafted = value;
            IsAmountCraftedRelevant = _amountCrafted > 1;
            OnPropertyChanged();
        }
    }

    public bool IsAmountCraftedRelevant
    {
        get => _isAmountCraftedRelevant;
        set
        {
            _isAmountCraftedRelevant = value;
            OnPropertyChanged();
        }
    }

    public double PossibleItemCrafting
    {
        get => _possibleItemCrafting;
        set
        {
            _possibleItemCrafting = value;
            OnPropertyChanged();
        }
    }

    public double CraftingTax
    {
        get => _craftingTax;
        set
        {
            _craftingTax = value;
            OnPropertyChanged();
        }
    }

    public double SetupFee
    {
        get => _setupFee;
        set
        {
            _setupFee = value;
            OnPropertyChanged();
        }
    }

    public double AuctionsHouseTax
    {
        get => _auctionsHouseTax;
        set
        {
            _auctionsHouseTax = value;
            OnPropertyChanged();
        }
    }

    public double OtherCosts
    {
        get => _otherCosts;
        set
        {
            _otherCosts = value;
            OnPropertyChanged();
        }
    }

    public double TotalJournalCosts
    {
        get => _totalJournalCosts;
        set
        {
            _totalJournalCosts = value;
            OnPropertyChanged();
        }
    }

    public double TotalResourceCosts
    {
        get => _totalResourceCosts;
        set
        {
            _totalResourceCosts = value;
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
            OnPropertyChanged();
        }
    }

    public double TotalJournalSells
    {
        get => _totalJournalSells;
        set
        {
            _totalJournalSells = value;
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

    public double TotalResourcesWeight
    {
        get => _totalResourcesWeight;
        set
        {
            _totalResourcesWeight = value;
            OnPropertyChanged();
        }
    }

    public double TotalRequiredJournalWeight
    {
        get => _totalRequiredJournalWeight;
        set
        {
            _totalRequiredJournalWeight = value;
            OnPropertyChanged();
        }
    }

    public double TotalCraftedItemWeight
    {
        get => _totalCraftedItemWeight;
        set
        {
            _totalCraftedItemWeight = value;
            OnPropertyChanged();
        }
    }

    public double TotalFinishedCraftingWeight
    {
        get => _totalFinishedCraftingWeight;
        set
        {
            _totalFinishedCraftingWeight = value;
            OnPropertyChanged();
        }
    }

    public double TotalUnfinishedCraftingWeight
    {
        get => _totalUnfinishedCraftingWeight;
        set
        {
            _totalUnfinishedCraftingWeight = value;
            OnPropertyChanged();
        }
    }

    public Visibility WeightValuesVisibility
    {
        get => _weightValuesVisibility;
        set
        {
            _weightValuesVisibility = value;
            OnPropertyChanged();
        }
    }

    public double ReturnOnInvestment
    {
        get => _returnOnInvestment;
        set
        {
            _returnOnInvestment = value;
            IsReturnOnInvestmentNegative = ReturnOnInvestment < 0;
            OnPropertyChanged();
        }
    }

    public double BreakEvenPrice
    {
        get => _breakEvenPrice;
        set
        {
            _breakEvenPrice = value;
            IsBreakEvenPriceNegative = BreakEvenPrice < 0;
            OnPropertyChanged();
        }
    }
    
    public double ProfitPerItem
    {
        get => _profitPerItem;
        set
        {
            _profitPerItem = value;
            IsProfitPerItemNegative = ProfitPerItem < 0;
            OnPropertyChanged();
        }
    }
    
    public bool IsReturnOnInvestmentNegative
    {
        get => _isReturnOnInvestmentNegative;
        set
        {
            _isReturnOnInvestmentNegative = value;
            OnPropertyChanged();
        }
    }

    public bool IsBreakEvenPriceNegative
    {
        get => _isBreakEvenPriceNegative;
        set
        {
            _isBreakEvenPriceNegative = value;
            OnPropertyChanged();
        }
    }

    public bool IsProfitPerItemNegative
    {
        get => _isProfitPerItemNegative;
        set
        {
            _isProfitPerItemNegative = value;
            OnPropertyChanged();
        }
    }

    #region Commands

    public void FoldUnfoldWeightValues(object value)
    {
        WeightValuesVisibility = WeightValuesVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    private ICommand _foldUnfoldWeightValuesCommand;

    public ICommand FoldUnfoldWeightValuesCommand => _foldUnfoldWeightValuesCommand ??= new CommandHandler(FoldUnfoldWeightValues, true);

    #endregion

    public static string TranslationCalculation => LocalizationController.Translation("CALCULATION");
    public static string TranslationPossibleCrafting => LocalizationController.Translation("POSSIBLE_CRAFTING");
    public static string TranslationPossibleItemCrafting => LocalizationController.Translation("POSSIBLE_ITEM_CRAFTING");
    public static string TranslationStatementOfCost => LocalizationController.Translation("STATEMENT_OF_COST");
    public static string TranslationCraftingTax => LocalizationController.Translation("CRAFTING_TAX");
    public static string TranslationSetupFee => LocalizationController.Translation("SETUP_FEE");
    public static string TranslationAuctionsHouseTax => LocalizationController.Translation("AUCTIONS_HOUSE_TAX");
    public static string TranslationTotalJournalCosts => LocalizationController.Translation("TOTAL_JOURNAL_COSTS");
    public static string TranslationTotalCosts => LocalizationController.Translation("TOTAL_COSTS");
    public static string TranslationTotalResourceCosts => LocalizationController.Translation("TOTAL_RESOURCE_COSTS");
    public static string TranslationOtherCosts => LocalizationController.Translation("OTHER_COSTS");
    public static string TranslationTotalItemSells => LocalizationController.Translation("TOTAL_ITEM_SELLS");
    public static string TranslationTotalJournalSells => LocalizationController.Translation("TOTAL_JOURNAL_SELLS");
    public static string TranslationTotalSells => LocalizationController.Translation("TOTAL_SELLS");
    public static string TranslationGrandTotal => LocalizationController.Translation("GRAND_TOTAL");
    public static string TranslationWeight => LocalizationController.Translation("WEIGHT");
    public static string TranslationTotalResourcesWeight => LocalizationController.Translation("TOTAL_RESOURCES_WEIGHT");
    public static string TranslationTotalJournalWeight => LocalizationController.Translation("TOTAL_JOURNAL_WEIGHT");
    public static string TranslationTotalCraftedItemWeight => LocalizationController.Translation("TOTAL_CRAFTED_WEIGHT");
    public static string TranslationTotalUnfinishedCraftingWeight => LocalizationController.Translation("TOTAL_UNFINISHED_CRAFTING_WEIGHT");
    public static string TranslationTotalFinishedCraftingWeight => LocalizationController.Translation("TOTAL_FINISHED_CRAFTING_WEIGHT");
    public static string TranslationProfitAnalysis => LocalizationController.Translation("PROFIT_ANALYSIS");
    public static string TranslationBreakEvenPrice => LocalizationController.Translation("BREAK_EVEN_PRICE");
    public static string TranslationReturnOnInvestment => LocalizationController.Translation("RETURN_ON_INVESTMENT");
    public static string TranslationProfitPerItem => LocalizationController.Translation("PROFIT_PER_ITEM");
    public static string TranslationReturnOnInvestmentToolTipDescription => LocalizationController.Translation("RETURN_ON_INVESTMENT_TOOL_TIP_DESCRIPTION");
    public static string TranslationProfitPerItemToolTipDescription => LocalizationController.Translation("PROFIT_PER_ITEM_TOOL_TIP_DESCRIPTION");
    public static string TranslationBreakEvenPriceToolTipDescription => LocalizationController.Translation("BREAK_EVEN_PRICE_TOOL_TIP_DESCRIPTION");
}