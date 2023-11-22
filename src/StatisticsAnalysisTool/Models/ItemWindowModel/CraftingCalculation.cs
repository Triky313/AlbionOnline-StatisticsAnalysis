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
    private Visibility _weightValuesVisibility = Visibility.Collapsed;

    private double GetTotalCosts()
    {
        return CraftingTax + SetupFee + AuctionsHouseTax + TotalJournalCosts + TotalResourceCosts + OtherCosts;
    }

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

    #region Commands

    public void FoldUnfoldWeightValues(object value)
    {
        WeightValuesVisibility = WeightValuesVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    private ICommand _foldUnfoldWeightValuesCommand;

    public ICommand FoldUnfoldWeightValuesCommand => _foldUnfoldWeightValuesCommand ??= new CommandHandler(FoldUnfoldWeightValues, true);

    #endregion

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
    public static string TranslationWeight => LanguageController.Translation("WEIGHT");
    public static string TranslationTotalResourcesWeight => LanguageController.Translation("TOTAL_RESOURCES_WEIGHT");
    public static string TranslationTotalJournalWeight => LanguageController.Translation("TOTAL_JOURNAL_WEIGHT");
    public static string TranslationTotalCraftedItemWeight => LanguageController.Translation("TOTAL_CRAFTED_WEIGHT");
    public static string TranslationTotalUnfinishedCraftingWeight => LanguageController.Translation("TOTAL_UNFINISHED_CRAFTING_WEIGHT");
    public static string TranslationTotalFinishedCraftingWeight => LanguageController.Translation("TOTAL_FINISHED_CRAFTING_WEIGHT");
}