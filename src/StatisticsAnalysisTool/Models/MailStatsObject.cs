using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models;

public class MailStatsObject : INotifyPropertyChanged
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
    private Mail _mostExpensiveSaleItem;
    private Mail _mostExpensivePurchasedItem;

    #region Stat calculations

    public void SetMailStats(ObservableCollection<Mail> mails)
    {
        SetMailStats(mails.ToList());
    }

    public void SetMailStats(List<Mail> mails)
    {
        var currentUtc = DateTime.UtcNow;
        SoldToday = mails.Where(x => x.Timestamp.Date == DateTime.UtcNow.Date && x.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        SoldThisWeek = mails.Where(x => x.Timestamp.IsDateInWeekOfYear(currentUtc) && x.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        SoldLastWeek = mails.Where(x => x.Timestamp.IsDateInWeekOfYear(currentUtc.AddDays(-7)) && x.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        SoldMonth = mails.Where(x => x.Timestamp.Year == currentUtc.Year && x.Timestamp.Month == currentUtc.Month && x.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        SoldYear = mails.Where(x => x.Timestamp.Year == currentUtc.Year && x.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);

        BoughtToday = mails.Where(x => x.Timestamp.Date == DateTime.UtcNow.Date && x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPriceWithDeductedTaxes.IntegerValue);
        BoughtThisWeek = mails.Where(x => x.Timestamp.IsDateInWeekOfYear(currentUtc) && x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPriceWithDeductedTaxes.IntegerValue);
        BoughtLastWeek = mails.Where(x => x.Timestamp.IsDateInWeekOfYear(currentUtc.AddDays(-7)) && x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPriceWithDeductedTaxes.IntegerValue);
        BoughtMonth = mails.Where(x => x.Timestamp.Year == currentUtc.Year && x.Timestamp.Month == currentUtc.Month && x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPriceWithDeductedTaxes.IntegerValue);
        BoughtYear = mails.Where(x => x.Timestamp.Year == currentUtc.Year && x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPriceWithDeductedTaxes.IntegerValue);

        TaxesToday = mails.Where(x => x.Timestamp.Date == DateTime.UtcNow.Date).Sum(x => x.MailContent.TaxSetupPrice.IntegerValue + x.MailContent.TaxPrice.IntegerValue);
        TaxesThisWeek = mails.Where(x => x.Timestamp.IsDateInWeekOfYear(currentUtc)).Sum(x => x.MailContent.TaxSetupPrice.IntegerValue + x.MailContent.TaxPrice.IntegerValue);
        TaxesLastWeek = mails.Where(x => x.Timestamp.IsDateInWeekOfYear(currentUtc.AddDays(-7))).Sum(x => x.MailContent.TaxSetupPrice.IntegerValue + x.MailContent.TaxPrice.IntegerValue);
        TaxesMonth = mails.Where(x => x.Timestamp.Year == currentUtc.Year && x.Timestamp.Month == currentUtc.Month).Sum(x => x.MailContent.TaxSetupPrice.IntegerValue + x.MailContent.TaxPrice.IntegerValue);
        TaxesYear = mails.Where(x => x.Timestamp.Year == currentUtc.Year).Sum(x => x.MailContent.TaxSetupPrice.IntegerValue + x.MailContent.TaxPrice.IntegerValue);

        SoldTotal = mails.Where(x => x.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        BoughtTotal = mails.Where(x => x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        TaxesTotal = mails.Sum(x => x.MailContent.TaxSetupPrice.IntegerValue + x.MailContent.TaxPrice.IntegerValue);

        SalesToday = SoldToday - (BoughtToday + TaxesToday);
        SalesThisWeek = SoldThisWeek - (BoughtThisWeek + TaxesThisWeek);
        SalesLastWeek = SoldLastWeek - (BoughtLastWeek + TaxesLastWeek);
        SalesMonth = SoldMonth - (BoughtMonth + TaxesMonth);
        SalesYear = SoldYear - (BoughtYear + TaxesYear);
        SalesTotal = SoldTotal - (BoughtTotal + TaxesTotal);

        MostExpensiveSaleItem = mails.Where(x => x.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired).MaxBy(x => x.MailContent.TotalPrice.IntegerValue);
        MostExpensivePurchasedItem = mails.Where(x => x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).MaxBy(x => x.MailContent.TotalPrice.IntegerValue);
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
    
    public Mail MostExpensiveSaleItem
    {
        get => _mostExpensiveSaleItem;
        set
        {
            _mostExpensiveSaleItem = value;
            OnPropertyChanged();
        }
    }

    public Mail MostExpensivePurchasedItem
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