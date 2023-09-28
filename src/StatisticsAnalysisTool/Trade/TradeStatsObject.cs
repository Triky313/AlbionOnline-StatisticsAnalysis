using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Trade;

public class TradeStatsObject : BaseViewModel
{
    private long _soldToday;
    private long _soldMonth;
    private long _soldYear;
    private long _boughtToday;
    private long _boughtMonth;
    private long _boughtYear;
    private long _soldTotal;
    private long _boughtTotal;
    private long _salesToday;
    private long _salesMonth;
    private long _salesYear;
    private long _salesTotal;
    private long _soldThisWeek;
    private long _boughtThisWeek;
    private long _salesThisWeek;
    private long _soldLastWeek;
    private long _boughtLastWeek;
    private long _salesLastWeek;
    private long _taxesToday;
    private long _taxesThisWeek;
    private long _taxesLastWeek;
    private long _taxesMonth;
    private long _taxesYear;
    private long _taxesTotal;
    private Trade _mostExpensiveSaleItem;
    private Trade _mostExpensivePurchasedItem;
    private bool _isMostExpensivePurchasedItemFromMail;
    private bool _isMostExpensiveSaleItemFromMail;

    #region Stat calculations

    public void SetTradeStats(IEnumerable<Trade> trades)
    {
        SetTradeStats(trades.ToList());
    }

    public void SetTradeStats(List<Trade> trades)
    {
        var currentUtc = DateTime.UtcNow;

        SoldToday = GetStatByType(trades, currentUtc, TradeStatType.SoldToday);
        SoldThisWeek = GetStatByType(trades, currentUtc, TradeStatType.SoldThisWeek);
        SoldLastWeek = GetStatByType(trades, currentUtc, TradeStatType.SoldLastWeek);
        SoldMonth = GetStatByType(trades, currentUtc, TradeStatType.SoldMonth);
        SoldYear = GetStatByType(trades, currentUtc, TradeStatType.SoldYear);

        BoughtToday = GetStatByType(trades, currentUtc, TradeStatType.BoughtToday);
        BoughtThisWeek = GetStatByType(trades, currentUtc, TradeStatType.BoughtThisWeek);
        BoughtLastWeek = GetStatByType(trades, currentUtc, TradeStatType.BoughtLastWeek);
        BoughtMonth = GetStatByType(trades, currentUtc, TradeStatType.BoughtMonth);
        BoughtYear = GetStatByType(trades, currentUtc, TradeStatType.BoughtYear);

        TaxesToday = GetStatByType(trades, currentUtc, TradeStatType.TaxesToday);
        TaxesThisWeek = GetStatByType(trades, currentUtc, TradeStatType.TaxesThisWeek);
        TaxesLastWeek = GetStatByType(trades, currentUtc, TradeStatType.TaxesLastWeek);
        TaxesMonth = GetStatByType(trades, currentUtc, TradeStatType.TaxesMonth);
        TaxesYear = GetStatByType(trades, currentUtc, TradeStatType.TaxesYear);

        SoldTotal = GetStatByType(trades, currentUtc, TradeStatType.SoldTotal);
        BoughtTotal = GetStatByType(trades, currentUtc, TradeStatType.BoughtTotal);
        TaxesTotal = GetStatByType(trades, currentUtc, TradeStatType.TaxesTotal);

        SalesToday = SoldToday - (BoughtToday + TaxesToday);
        SalesThisWeek = SoldThisWeek - (BoughtThisWeek + TaxesThisWeek);
        SalesLastWeek = SoldLastWeek - (BoughtLastWeek + TaxesLastWeek);
        SalesMonth = SoldMonth - (BoughtMonth + TaxesMonth);
        SalesYear = SoldYear - (BoughtYear + TaxesYear);
        SalesTotal = SoldTotal - (BoughtTotal + TaxesTotal);

        MostExpensiveSaleItem = trades
            .Where(x => x.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired || x.Type == TradeType.InstantSell)
            .MaxBy(x =>
            {
                switch (x.Type)
                {
                    case TradeType.Mail:
                        IsMostExpensiveSaleItemFromMail = true;
                        return x.MailContent.TotalPrice.IntegerValue;
                    case TradeType.InstantBuy:
                        IsMostExpensiveSaleItemFromMail = false;
                        return x.InstantBuySellContent.TotalPrice.IntegerValue;
                    default:
                        return 0;
                }
            });

        MostExpensivePurchasedItem = trades
            .Where(x => x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired || x.Type == TradeType.InstantBuy)
            .MaxBy(x =>
            {
                switch (x.Type)
                {
                    case TradeType.Mail:
                        IsMostExpensivePurchasedItemFromMail = true;
                        return x.MailContent.TotalPrice.IntegerValue;
                    case TradeType.InstantBuy:
                        IsMostExpensivePurchasedItemFromMail = false;
                        return x.InstantBuySellContent.TotalPrice.IntegerValue;
                    default:
                        return 0;
                }
            });
    }

    private static long GetStatByType(IEnumerable<Trade> trades, DateTime datetime, TradeStatType type)
    {
        return trades.Where(trade =>
            {
                switch (type)
                {
                    case TradeStatType.SoldToday when trade.Timestamp.Date != DateTime.UtcNow.Date:
                    case TradeStatType.BoughtToday when trade.Timestamp.Date != DateTime.UtcNow.Date:
                    case TradeStatType.TaxesToday when trade.Timestamp.Date != DateTime.UtcNow.Date:
                    case TradeStatType.SoldThisWeek when !trade.Timestamp.Date.IsDateInWeekOfYear(datetime):
                    case TradeStatType.BoughtThisWeek when !trade.Timestamp.Date.IsDateInWeekOfYear(datetime):
                    case TradeStatType.TaxesThisWeek when !trade.Timestamp.Date.IsDateInWeekOfYear(datetime):
                    case TradeStatType.SoldLastWeek when !trade.Timestamp.Date.IsDateInWeekOfYear(datetime.AddDays(-7)):
                    case TradeStatType.BoughtLastWeek when !trade.Timestamp.Date.IsDateInWeekOfYear(datetime.AddDays(-7)):
                    case TradeStatType.TaxesLastWeek when !trade.Timestamp.Date.IsDateInWeekOfYear(datetime.AddDays(-7)):
                    case TradeStatType.SoldMonth when trade.Timestamp.Year != datetime.Year || trade.Timestamp.Month != datetime.Month:
                    case TradeStatType.BoughtMonth when trade.Timestamp.Year != datetime.Year || trade.Timestamp.Month != datetime.Month:
                    case TradeStatType.TaxesMonth when trade.Timestamp.Year != datetime.Year || trade.Timestamp.Month != datetime.Month:
                    case TradeStatType.SoldYear when trade.Timestamp.Year != datetime.Year:
                    case TradeStatType.BoughtYear when trade.Timestamp.Year != datetime.Year:
                    case TradeStatType.TaxesYear when trade.Timestamp.Year != datetime.Year:
                        return false;
                }

                switch (type)
                {
                    case TradeStatType.SoldToday or TradeStatType.SoldThisWeek or TradeStatType.SoldLastWeek or TradeStatType.SoldMonth or TradeStatType.SoldYear
                        or TradeStatType.SoldTotal:
                        switch (trade.Type)
                        {
                            case TradeType.Mail when trade.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired:
                            case TradeType.InstantSell:
                            case TradeType.ManualSell:
                                return true;
                        }
                        break;
                    case TradeStatType.BoughtToday or TradeStatType.BoughtThisWeek or TradeStatType.BoughtLastWeek or TradeStatType.BoughtMonth or TradeStatType.BoughtYear
                        or TradeStatType.BoughtTotal:
                        switch (trade.Type)
                        {
                            case TradeType.Mail when trade.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired:
                            case TradeType.InstantBuy:
                            case TradeType.ManualBuy:
                                return true;
                        }
                        break;
                    case TradeStatType.TaxesToday or TradeStatType.TaxesThisWeek or TradeStatType.TaxesLastWeek or TradeStatType.TaxesMonth or TradeStatType.TaxesYear
                        or TradeStatType.TaxesTotal:
                        switch (trade.Type)
                        {
                            case TradeType.Mail when trade.MailType
                                is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired
                                or MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired:
                            case TradeType.InstantSell:
                                return true;
                        }
                        break;
                }

                return false;
            })
            .Sum(trade =>
            {
                return type switch
                {
                    TradeStatType.SoldToday or TradeStatType.SoldThisWeek or TradeStatType.SoldLastWeek or TradeStatType.SoldMonth or TradeStatType.SoldYear => trade.Type switch
                    {
                        TradeType.Mail => trade.MailContent.TotalPrice.IntegerValue,
                        TradeType.InstantSell => trade.InstantBuySellContent.TotalPrice.IntegerValue,
                        TradeType.ManualSell => trade.InstantBuySellContent.TotalPrice.IntegerValue,
                        _ => 0
                    },
                    TradeStatType.BoughtToday or TradeStatType.BoughtThisWeek or TradeStatType.BoughtLastWeek or TradeStatType.BoughtMonth or TradeStatType.BoughtYear => trade.Type switch
                    {
                        TradeType.Mail => trade.MailContent.TotalPriceWithDeductedTaxes.IntegerValue,
                        TradeType.InstantBuy => trade.InstantBuySellContent.TotalPrice.IntegerValue,
                        TradeType.ManualBuy => trade.InstantBuySellContent.TotalPrice.IntegerValue,
                        _ => 0
                    },
                    TradeStatType.TaxesToday or TradeStatType.TaxesThisWeek or TradeStatType.TaxesLastWeek or TradeStatType.TaxesMonth or TradeStatType.TaxesYear
                        or TradeStatType.TaxesTotal => trade.Type switch
                        {
                            TradeType.Mail => trade.MailContent.TaxSetupPrice.IntegerValue + trade.MailContent.TaxPrice.IntegerValue,
                            TradeType.InstantSell => trade.InstantBuySellContent.TaxPrice.IntegerValue,
                            _ => 0
                        },
                    TradeStatType.SoldTotal => trade.Type switch
                    {
                        TradeType.Mail => trade.MailContent.TotalPrice.IntegerValue,
                        TradeType.InstantSell => trade.InstantBuySellContent.TotalPrice.IntegerValue,
                        TradeType.ManualSell => trade.InstantBuySellContent.TotalPrice.IntegerValue,
                        _ => 0
                    },
                    TradeStatType.BoughtTotal => trade.Type switch
                    {
                        TradeType.Mail => trade.MailContent.TotalPrice.IntegerValue,
                        TradeType.InstantBuy => trade.InstantBuySellContent.TotalPrice.IntegerValue,
                        TradeType.ManualBuy => trade.InstantBuySellContent.TotalPrice.IntegerValue,
                        _ => 0
                    },
                    _ => 0
                };
            });
    }

    #endregion

    public bool IsMostExpensiveSaleItemFromMail
    {
        get => _isMostExpensiveSaleItemFromMail;
        set
        {
            _isMostExpensiveSaleItemFromMail = value;
            OnPropertyChanged();
        }
    }

    public bool IsMostExpensivePurchasedItemFromMail
    {
        get => _isMostExpensivePurchasedItemFromMail;
        set
        {
            _isMostExpensivePurchasedItemFromMail = value;
            OnPropertyChanged();
        }
    }

    public long SoldToday
    {
        get => _soldToday;
        set
        {
            _soldToday = value;
            OnPropertyChanged();
        }
    }

    public long SoldThisWeek
    {
        get => _soldThisWeek;
        set
        {
            _soldThisWeek = value;
            OnPropertyChanged();
        }
    }

    public long SoldLastWeek
    {
        get => _soldLastWeek;
        set
        {
            _soldLastWeek = value;
            OnPropertyChanged();
        }
    }

    public long SoldMonth
    {
        get => _soldMonth;
        set
        {
            _soldMonth = value;
            OnPropertyChanged();
        }
    }

    public long SoldYear
    {
        get => _soldYear;
        set
        {
            _soldYear = value;
            OnPropertyChanged();
        }
    }

    public long BoughtToday
    {
        get => _boughtToday;
        set
        {
            _boughtToday = value;
            OnPropertyChanged();
        }
    }

    public long BoughtThisWeek
    {
        get => _boughtThisWeek;
        set
        {
            _boughtThisWeek = value;
            OnPropertyChanged();
        }
    }

    public long BoughtLastWeek
    {
        get => _boughtLastWeek;
        set
        {
            _boughtLastWeek = value;
            OnPropertyChanged();
        }
    }

    public long BoughtMonth
    {
        get => _boughtMonth;
        set
        {
            _boughtMonth = value;
            OnPropertyChanged();
        }
    }

    public long BoughtYear
    {
        get => _boughtYear;
        set
        {
            _boughtYear = value;
            OnPropertyChanged();
        }
    }

    public long SoldTotal
    {
        get => _soldTotal;
        set
        {
            _soldTotal = value;
            OnPropertyChanged();
        }
    }

    public long BoughtTotal
    {
        get => _boughtTotal;
        set
        {
            _boughtTotal = value;
            OnPropertyChanged();
        }
    }

    public long SalesToday
    {
        get => _salesToday;
        set
        {
            _salesToday = value;
            OnPropertyChanged();
        }
    }

    public long SalesThisWeek
    {
        get => _salesThisWeek;
        set
        {
            _salesThisWeek = value;
            OnPropertyChanged();
        }
    }

    public long SalesLastWeek
    {
        get => _salesLastWeek;
        set
        {
            _salesLastWeek = value;
            OnPropertyChanged();
        }
    }

    public long SalesMonth
    {
        get => _salesMonth;
        set
        {
            _salesMonth = value;
            OnPropertyChanged();
        }
    }

    public long SalesYear
    {
        get => _salesYear;
        set
        {
            _salesYear = value;
            OnPropertyChanged();
        }
    }

    public long SalesTotal
    {
        get => _salesTotal;
        set
        {
            _salesTotal = value;
            OnPropertyChanged();
        }
    }

    public long TaxesToday
    {
        get => _taxesToday;
        set
        {
            _taxesToday = value;
            OnPropertyChanged();
        }
    }

    public long TaxesThisWeek
    {
        get => _taxesThisWeek;
        set
        {
            _taxesThisWeek = value;
            OnPropertyChanged();
        }
    }

    public long TaxesLastWeek
    {
        get => _taxesLastWeek;
        set
        {
            _taxesLastWeek = value;
            OnPropertyChanged();
        }
    }

    public long TaxesMonth
    {
        get => _taxesMonth;
        set
        {
            _taxesMonth = value;
            OnPropertyChanged();
        }
    }

    public long TaxesYear
    {
        get => _taxesYear;
        set
        {
            _taxesYear = value;
            OnPropertyChanged();
        }
    }

    public long TaxesTotal
    {
        get => _taxesTotal;
        set
        {
            _taxesTotal = value;
            OnPropertyChanged();
        }
    }

    public Trade MostExpensiveSaleItem
    {
        get => _mostExpensiveSaleItem;
        set
        {
            _mostExpensiveSaleItem = value;
            OnPropertyChanged();
        }
    }

    public Trade MostExpensivePurchasedItem
    {
        get => _mostExpensivePurchasedItem;
        set
        {
            _mostExpensivePurchasedItem = value;
            OnPropertyChanged();
        }
    }

    public static string TranslationSold => LanguageController.Translation("SOLD");
    public static string TranslationToday => LanguageController.Translation("TODAY");
    public static string TranslationThisWeek => LanguageController.Translation("THIS_WEEK");
    public static string TranslationLastWeek => LanguageController.Translation("LAST_WEEK");
    public static string TranslationMonth => LanguageController.Translation("MONTH");
    public static string TranslationYear => LanguageController.Translation("YEAR");
    public static string TranslationTotal => LanguageController.Translation("TOTAL");
    public static string TranslationBought => LanguageController.Translation("BOUGHT");
    public static string TranslationTax => LanguageController.Translation("TAX");
    public static string TranslationNetProfit => LanguageController.Translation("NET_PROFIT");
    public static string TranslationMostExpensiveSale => LanguageController.Translation("MOST_EXPENSIVE_SALE");
    public static string TranslationMostExpensivePurchase => LanguageController.Translation("MOST_EXPENSIVE_PURCHASE");

    public static string TranslationSilver => LanguageController.Translation("SILVER");
}