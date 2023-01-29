using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Trade;

public class TradeStatsObject : INotifyPropertyChanged
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

        //SoldTotal = trades.Where(x => x.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        //BoughtTotal = trades.Where(x => x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        //TaxesTotal = trades.Sum(x => x.MailContent.TaxSetupPrice.IntegerValue + x.MailContent.TaxPrice.IntegerValue);

        //SalesToday = SoldToday - (BoughtToday + TaxesToday);
        //SalesThisWeek = SoldThisWeek - (BoughtThisWeek + TaxesThisWeek);
        //SalesLastWeek = SoldLastWeek - (BoughtLastWeek + TaxesLastWeek);
        //SalesMonth = SoldMonth - (BoughtMonth + TaxesMonth);
        //SalesYear = SoldYear - (BoughtYear + TaxesYear);
        //SalesTotal = SoldTotal - (BoughtTotal + TaxesTotal);

        //MostExpensiveSaleItem = trades.Where(x => x.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired).MaxBy(x => x.MailContent.TotalPrice.IntegerValue);
        //MostExpensivePurchasedItem = trades.Where(x => x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).MaxBy(x => x.MailContent.TotalPrice.IntegerValue);
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
                    case TradeStatType.SoldToday or TradeStatType.SoldThisWeek or TradeStatType.SoldLastWeek or TradeStatType.SoldMonth or TradeStatType.SoldYear:
                        switch (trade.Type)
                        {
                            case TradeType.Mail when trade.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired:
                            case TradeType.InstantSell:
                                return true;
                        }
                        break;
                    case TradeStatType.BoughtToday or TradeStatType.BoughtThisWeek or TradeStatType.BoughtLastWeek or TradeStatType.BoughtMonth or TradeStatType.BoughtYear:
                        switch (trade.Type)
                        {
                            case TradeType.Mail when trade.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired:
                            case TradeType.InstantBuy:
                                return true;
                        }
                        break;
                    case TradeStatType.TaxesToday or TradeStatType.TaxesThisWeek or TradeStatType.TaxesLastWeek or TradeStatType.TaxesMonth or TradeStatType.TaxesYear:
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
                        _ => 0
                    },
                    TradeStatType.BoughtToday or TradeStatType.BoughtThisWeek or TradeStatType.BoughtLastWeek or TradeStatType.BoughtMonth or TradeStatType.BoughtYear => trade.Type switch
                    {
                        TradeType.Mail => trade.MailContent.TotalPriceWithDeductedTaxes.IntegerValue,
                        TradeType.InstantBuy => trade.InstantBuySellContent.TotalPrice.IntegerValue,
                        _ => 0
                    },
                    TradeStatType.TaxesToday or TradeStatType.TaxesThisWeek or TradeStatType.TaxesLastWeek or TradeStatType.TaxesMonth or TradeStatType.TaxesYear => trade.Type switch
                    {
                        TradeType.Mail => trade.MailContent.TaxSetupPrice.IntegerValue + trade.MailContent.TaxPrice.IntegerValue,
                        TradeType.InstantSell => trade.InstantBuySellContent.TaxPrice.IntegerValue,
                        _ => 0
                    },
                    _ => 0
                };
            });
    }

    #endregion

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

    public static string TranslationSoldToday => LanguageController.Translation("SOLD_TODAY");
    public static string TranslationSoldThisWeek => LanguageController.Translation("SOLD_THIS_WEEK");
    public static string TranslationSoldLastWeek => LanguageController.Translation("SOLD_LAST_WEEK");
    public static string TranslationSoldMonth => LanguageController.Translation("SOLD_MONTH");
    public static string TranslationSoldYear => LanguageController.Translation("SOLD_YEAR");
    public static string TranslationBoughtToday => LanguageController.Translation("BOUGHT_TODAY");
    public static string TranslationBoughtThisWeek => LanguageController.Translation("BOUGHT_THIS_WEEK");
    public static string TranslationBoughtLastWeek => LanguageController.Translation("BOUGHT_LAST_WEEK");
    public static string TranslationBoughtMonth => LanguageController.Translation("BOUGHT_MONTH");
    public static string TranslationBoughtYear => LanguageController.Translation("BOUGHT_YEAR");
    public static string TranslationSoldTotal => LanguageController.Translation("SOLD_TOTAL");
    public static string TranslationBoughtTotal => LanguageController.Translation("BOUGHT_TOTAL");
    public static string TranslationTaxesToday => LanguageController.Translation("TAXES_TODAY");
    public static string TranslationTaxesThisWeek => LanguageController.Translation("TAXES_THIS_WEEK");
    public static string TranslationTaxesLastWeek => LanguageController.Translation("TAXES_LAST_WEEK");
    public static string TranslationTaxesMonth => LanguageController.Translation("TAXES_MONTH");
    public static string TranslationTaxesYear => LanguageController.Translation("TAXES_YEAR");
    public static string TranslationTaxesTotal => LanguageController.Translation("TAXES_TOTAL");
    public static string TranslationNetProfitToday => LanguageController.Translation("NET_PROFIT_TODAY");
    public static string TranslationNetProfitThisWeek => LanguageController.Translation("NET_PROFIT_THIS_WEEK");
    public static string TranslationNetProfitLastWeek => LanguageController.Translation("NET_PROFIT_LAST_WEEK");
    public static string TranslationNetProfitMonth => LanguageController.Translation("NET_PROFIT_MONTH");
    public static string TranslationNetProfitYear => LanguageController.Translation("NET_PROFIT_YEAR");
    public static string TranslationNetProfitTotal => LanguageController.Translation("NET_PROFIT_TOTAL");
    public static string TranslationMostExpensiveSale => LanguageController.Translation("MOST_EXPENSIVE_SALE");
    public static string TranslationMostExpensivePurchase => LanguageController.Translation("MOST_EXPENSIVE_PURCHASE");

    public static string TranslationSilver => LanguageController.Translation("SILVER");

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
    {
        if (!Equals(field, newValue))
        {
            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        return false;
    }
}