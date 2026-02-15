using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Trade.Mails;
using StatisticsAnalysisTool.Trade.Market;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Trade;

public class Trade : BaseViewModel
{
    public long Id { get; init; }
    public long Ticks { get; init; }
    public DateTime Timestamp => new(Ticks);
    public string ClusterIndex { get; init; }
    public TradeType Type { get; init; }
    public string Description { get; init; } = string.Empty;
    public int ItemIndex { get; init; } = -1;
    public string UniqueClusterName => WorldData.GetUniqueNameOrDefault(ClusterIndex);

    private bool? _isSelectedForDeletion = false;

    public bool? IsSelectedForDeletion
    {
        get => _isSelectedForDeletion;
        set
        {
            _isSelectedForDeletion = value;
            OnPropertyChanged();
        }
    }

    public Item Item => ItemController.GetItemByIndex(ItemIndex) ?? ItemController.GetItemByUniqueName(MailContent?.UniqueItemName) ?? ItemController.GetItemByUniqueName(AuctionEntry?.ItemTypeId);

    #region Mail

    public Guid Guid { get; init; }

    public string MailTypeText { get; init; }

    public MarketLocation Location => ClusterIndex.GetMarketLocationByLocationNameOrId();

    public string LocationName
    {
        get
        {
            var location = Locations.GetMarketLocationByIndex(ClusterIndex);
            if (location == MarketLocation.Unknown && !string.IsNullOrEmpty(ClusterIndex) && ClusterIndex.Contains("HIDEOUT"))
            {
                var parts = ClusterIndex.Split('_');
                var hideoutName = parts.Length > 1 ? parts[1] : ClusterIndex;
                return $"{hideoutName} ({LocalizationController.Translation("HIDEOUT")})";
            }

            if (location == MarketLocation.BlackMarket)
            {
                return "Black Market";
            }

            if (location == MarketLocation.SmugglersDen)
            {
                return "Smuggler's Den";
            }

            return WorldData.GetUniqueNameOrDefault(ClusterIndex);
        }
    }

    public MailType MailType => MailController.ConvertToMailType(MailTypeText);
    public MailContent MailContent { get; init; } = new();
    public string MailTypeDescription
    {
        get
        {
            return MailType switch
            {
                MailType.MarketplaceBuyOrderFinished => LocalizationController.Translation("BOUGHT"),
                MailType.MarketplaceSellOrderFinished => LocalizationController.Translation("SOLD"),
                MailType.MarketplaceSellOrderExpired => LocalizationController.Translation("SELL_EXPIRED"),
                MailType.MarketplaceBuyOrderExpired => LocalizationController.Translation("BUY_EXPIRED"),
                _ => LocalizationController.Translation("MAIL")
            };
        }
    }

    public string TradeNotificationTitleText
    {
        get
        {
            switch (Type)
            {
                case TradeType.InstantBuy:
                    return LocalizationController.Translation("ADDED_PURCHASE");
                case TradeType.InstantSell:
                    return LocalizationController.Translation("ADDED_SALE");
                case TradeType.Mail:
                    return MailType switch
                    {
                        MailType.MarketplaceBuyOrderFinished => LocalizationController.Translation("ADDED_BUY_ORDER"),
                        MailType.MarketplaceSellOrderFinished => LocalizationController.Translation("ADDED_SELL_ORDER"),
                        MailType.MarketplaceSellOrderExpired => LocalizationController.Translation("ADDED_EXPIRED_SELL_ORDER"),
                        MailType.MarketplaceBuyOrderExpired => LocalizationController.Translation("ADDED_EXPIRED_BUY_ORDER"),
                        _ => LocalizationController.Translation("ADDED_UNKNOWN_TRADE")
                    };

                case TradeType.Crafting:
                    return LocalizationController.Translation("ADDED_CRAFTING");
                case TradeType.Unknown:
                default:
                    return LocalizationController.Translation("ADDED_UNKNOWN_TRADE");
            }
        }
    }

    #endregion

    #region Instant buy / sell

    public AuctionEntry AuctionEntry { get; init; }
    public InstantBuySellContent InstantBuySellContent { get; init; } = new();
    public string TypeDescription => Type switch
    {
        TradeType.InstantSell => LocalizationController.Translation("INSTANT_SELL"),
        TradeType.InstantBuy => LocalizationController.Translation("INSTANT_BUY"),
        TradeType.ManualSell => LocalizationController.Translation("MANUAL_SELL"),
        TradeType.ManualBuy => LocalizationController.Translation("MANUAL_BUY"),
        TradeType.Crafting => LocalizationController.Translation("CRAFTING"),
        TradeType.Mail => LocalizationController.Translation("MAIL"),
        _ => LocalizationController.Translation("UNKNOWN_TRADE")
    };

    #endregion

    public int CompareTo(Trade other)
    {
        if (ReferenceEquals(this, other))
            return 0;
        if (ReferenceEquals(null, other))
            return 1;
        var tickComparison = Ticks.CompareTo(other.Ticks);
        if (tickComparison != 0)
            return tickComparison;

        return Id.CompareTo(other.Id);
    }

    public static string TranslationSilver => LocalizationController.Translation("SILVER");
    public static string TranslationCostPerItem => LocalizationController.Translation("COST_PER_ITEM");
    public static string TranslationTotalCost => LocalizationController.Translation("TOTAL_COST");
    public static string TranslationTotalDistanceFee => LocalizationController.Translation("TOTAL_DISTANCE_FEE");
    public static string TranslationTotalRevenue => LocalizationController.Translation("TOTAL_REVENUE");
    public static string TranslationTax => LocalizationController.Translation("TAX");
    public static string TranslationSetupTax => LocalizationController.Translation("SETUP_TAX");
    public static string TranslationSelectToDelete => LocalizationController.Translation("SELECT_TO_DELETE");
    public static string TranslationFrom => LocalizationController.Translation("FROM");
    public static string TranslationTotalPriceWithDeductedTaxes => LocalizationController.Translation("TOTAL_PRICE_WITH_DEDUCTED_TAXES");
    public static string TranslationTotalIncomeWithoutTaxDeductions => LocalizationController.Translation("TOTAL_INCOME_WITHOUT_TAX_DEDUCTIONS");
    public static string TranslationTotalCostWithoutAddedTaxes => LocalizationController.Translation("TOTAL_COST_WITHOUT_ADDED_TAXES");

    #region Commands

    public void OpenItemWindow(object value)
    {
        MainWindowViewModel.OpenItemWindow(Item);
    }

    private ICommand _openItemWindowCommand;

    public ICommand OpenItemWindowCommand => _openItemWindowCommand ??= new CommandHandler(OpenItemWindow, true);

    #endregion
}