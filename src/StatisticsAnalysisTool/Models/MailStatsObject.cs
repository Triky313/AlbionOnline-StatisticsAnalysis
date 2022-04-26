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

        BoughtToday = mails.Where(x => x.Timestamp.Date == DateTime.UtcNow.Date && x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        BoughtThisWeek = mails.Where(x => x.Timestamp.IsDateInWeekOfYear(currentUtc) && x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        BoughtLastWeek = mails.Where(x => x.Timestamp.IsDateInWeekOfYear(currentUtc.AddDays(-7)) && x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        BoughtMonth = mails.Where(x => x.Timestamp.Year == currentUtc.Year && x.Timestamp.Month == currentUtc.Month && x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        BoughtYear = mails.Where(x => x.Timestamp.Year == currentUtc.Year && x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);

        SoldTotal = mails.Where(x => x.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        BoughtTotal = mails.Where(x => x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);

        SalesToday = SoldToday - BoughtToday;
        SalesThisWeek = SoldThisWeek - BoughtThisWeek;
        SalesLastWeek = SoldLastWeek - BoughtLastWeek;
        SalesMonth = SoldMonth - BoughtMonth;
        SalesYear = SoldYear - BoughtYear;
        SalesTotal = SoldTotal - BoughtTotal;
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
    public static string TranslationNetProfitToday => LanguageController.Translation("NET_PROFIT_TODAY");
    public static string TranslationNetProfitThisWeek => LanguageController.Translation("NET_PROFIT_THIS_WEEK");
    public static string TranslationNetProfitLastWeek => LanguageController.Translation("NET_PROFIT_LAST_WEEK");
    public static string TranslationNetProfitMonth => LanguageController.Translation("NET_PROFIT_MONTH");
    public static string TranslationNetProfitYear => LanguageController.Translation("NET_PROFIT_YEAR");
    public static string TranslationNetProfitTotal => LanguageController.Translation("NET_PROFIT_TOTAL");

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