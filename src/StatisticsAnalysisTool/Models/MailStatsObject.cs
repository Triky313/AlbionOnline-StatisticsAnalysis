using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using StatisticsAnalysisTool.Enumerations;

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

    #region Stat calculations

    public void SetMailStats(ObservableCollection<Mail> mails)
    {
        SetMailStats(mails.ToList());
    }

    public void SetMailStats(List<Mail> mails)
    {
        var currentUtc = DateTime.UtcNow;
        SoldToday = mails.Where(x => x.Timestamp.Date == DateTime.UtcNow.Date && x.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        SoldMonth = mails.Where(x => x.Timestamp.Year == currentUtc.Year && x.Timestamp.Month == currentUtc.Month && x.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        SoldYear = mails.Where(x => x.Timestamp.Year == currentUtc.Year && x.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);

        BoughtToday = mails.Where(x => x.Timestamp.Date == DateTime.UtcNow.Date && x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        BoughtMonth = mails.Where(x => x.Timestamp.Year == currentUtc.Year && x.Timestamp.Month == currentUtc.Month && x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        BoughtYear = mails.Where(x => x.Timestamp.Year == currentUtc.Year && x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);

        SoldTotal = mails.Where(x => x.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        BoughtTotal = mails.Where(x => x.MailType is MailType.MarketplaceBuyOrderFinished or MailType.MarketplaceBuyOrderExpired).Sum(x => x.MailContent.TotalPrice.IntegerValue);
        
        SalesToday = SoldToday - BoughtToday;
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

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}