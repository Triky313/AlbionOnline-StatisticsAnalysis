using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Trade.Mails;
using StatisticsAnalysisTool.Trade.Market;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Trade;

public class Trade : INotifyPropertyChanged
{
    public long Id { get; init; }
    public long Ticks { get; init; }
    public DateTime Timestamp => new(Ticks);
    public string ClusterIndex { get; init; }
    public TradeType Type { get; init; }
    public string Description { get; init; } = string.Empty;
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

    public Item Item => ItemController.GetItemByUniqueName(MailContent?.UniqueItemName) ?? ItemController.GetItemByUniqueName(AuctionEntry?.ItemTypeId);

    #region Mail

    public Guid Guid { get; init; }

    public string MailTypeText { get; init; }

    public MarketLocation Location => Locations.GetMarketLocationByIndex(ClusterIndex);

    public string LocationName
    {
        get
        {
            if (Location == MarketLocation.Unknown && ClusterIndex != null && ClusterIndex.Contains("HIDEOUT"))
            {
                return $"{ClusterIndex.Split("_")[1]} ({LanguageController.Translation("HIDEOUT")})";
            }

            return Location switch
            {
                MarketLocation.BlackMarket => "Black Market",
                MarketLocation.Unknown => LanguageController.Translation("UNKNOWN"),
                _ => WorldData.GetUniqueNameOrDefault((int) Location)
            };
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
                MailType.MarketplaceBuyOrderFinished => LanguageController.Translation("BOUGHT"),
                MailType.MarketplaceSellOrderFinished => LanguageController.Translation("SOLD"),
                MailType.MarketplaceSellOrderExpired => LanguageController.Translation("SELL_EXPIRED"),
                MailType.MarketplaceBuyOrderExpired => LanguageController.Translation("BUY_EXPIRED"),
                _ => LanguageController.Translation("MAIL")
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
                    return LanguageController.Translation("ADDED_PURCHASE");
                case TradeType.InstantSell:
                    return LanguageController.Translation("ADDED_SALE");
                case TradeType.Mail:
                    return MailType switch
                    {
                        MailType.MarketplaceBuyOrderFinished => LanguageController.Translation("ADDED_BUY_ORDER"),
                        MailType.MarketplaceSellOrderFinished => LanguageController.Translation("ADDED_SELL_ORDER"),
                        MailType.MarketplaceSellOrderExpired => LanguageController.Translation("ADDED_EXPIRED_SELL_ORDER"),
                        MailType.MarketplaceBuyOrderExpired => LanguageController.Translation("ADDED_EXPIRED_BUY_ORDER"),
                        _ => LanguageController.Translation("ADDED_UNKNOWN_TRADE")
                    };
                case TradeType.Unknown:
                default:
                    return LanguageController.Translation("ADDED_UNKNOWN_TRADE");
            }
        }
    }

    #endregion

    #region Instant buy / sell

    public AuctionEntry AuctionEntry { get; init; }
    public InstantBuySellContent InstantBuySellContent { get; init; } = new();
    public string TypeDescription => Type switch
    {
        TradeType.InstantSell => LanguageController.Translation("INSTANT_SELL"),
        TradeType.InstantBuy => LanguageController.Translation("INSTANT_BUY"),
        TradeType.ManualSell => LanguageController.Translation("MANUAL_SELL"),
        TradeType.ManualBuy => LanguageController.Translation("MANUAL_BUY"),
        TradeType.Mail => LanguageController.Translation("MAIL"),
        _ => LanguageController.Translation("UNKNOWN_TRADE")
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

    public static string TranslationSilver => LanguageController.Translation("SILVER");
    public static string TranslationCostPerItem => LanguageController.Translation("COST_PER_ITEM");
    public static string TranslationTotalCost => LanguageController.Translation("TOTAL_COST");
    public static string TranslationTotalRevenue => LanguageController.Translation("TOTAL_REVENUE");
    public static string TranslationTax => LanguageController.Translation("TAX");
    public static string TranslationSetupTax => LanguageController.Translation("SETUP_TAX");
    public static string TranslationSelectToDelete => LanguageController.Translation("SELECT_TO_DELETE");
    public static string TranslationFrom => LanguageController.Translation("FROM");
    public static string TranslationTotalPriceWithDeductedTaxes => LanguageController.Translation("TOTAL_PRICE_WITH_DEDUCTED_TAXES");

    #region Commands

    public void OpenItemWindow(object value)
    {
        MainWindowViewModel.OpenItemWindow(Item);
    }

    private ICommand _openItemWindowCommand;

    public ICommand OpenItemWindowCommand => _openItemWindowCommand ??= new CommandHandler(OpenItemWindow, true);

    #endregion

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}