using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using StatisticsAnalysisTool.Common.UserSettings;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class MailMonitoringBindings : INotifyPropertyChanged
{
    private ListCollectionView _mailCollectionView;
    private ObservableCollection<Mail> _mails = new();
    private string _mailsSearchText;
    private DateTime _datePickerMailsFrom = new(2017, 1, 1);
    private DateTime _datePickerMailsTo = DateTime.UtcNow.AddDays(1);
    private MailStatsObject _mailStatsObject = new();
    private Visibility _isMailMonitoringPopupVisible = Visibility.Collapsed;
    private GridLength _gridSplitterPosition = GridLength.Auto;
    private int _totalMails;
    private int _currentMails;

    public MailMonitoringBindings()
    {
        MailCollectionView = CollectionViewSource.GetDefaultView(Mails) as ListCollectionView;

        if (MailCollectionView != null)
        {
            MailCollectionView.CurrentChanged += UpdateCurrentMailsUi;
            Mails.CollectionChanged += UpdateTotalMailsUi;

            MailCollectionView.IsLiveSorting = true;
            MailCollectionView.IsLiveFiltering = true;
            MailCollectionView.SortDescriptions.Add(new SortDescription("Tick", ListSortDirection.Descending));

            MailCollectionView?.Refresh();
        }
    }

    public ListCollectionView MailCollectionView
    {
        get => _mailCollectionView;
        set
        {
            _mailCollectionView = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Mail> Mails
    {
        get => _mails;
        set
        {
            _mails = value;
            OnPropertyChanged();
        }
    }

    public string MailsSearchText
    {
        get => _mailsSearchText;
        set
        {
            _mailsSearchText = value;

            if (_mailsSearchText.Length >= 2)
            {
                MailCollectionView.Filter = Filter;
                MailStatsObject.SetMailStats(MailCollectionView.Cast<Mail>().ToList());
            }

            OnPropertyChanged();
        }
    }

    public DateTime DatePickerMailsFrom
    {
        get => _datePickerMailsFrom;
        set
        {
            _datePickerMailsFrom = value;
            MailCollectionView.Filter = Filter;
            MailStatsObject.SetMailStats(MailCollectionView.Cast<Mail>().ToList());
            OnPropertyChanged();
        }
    }

    public DateTime DatePickerMailsTo
    {
        get => _datePickerMailsTo;
        set
        {
            _datePickerMailsTo = value;
            MailCollectionView.Filter = Filter;
            MailStatsObject.SetMailStats(MailCollectionView.Cast<Mail>().ToList());
            OnPropertyChanged();
        }
    }

    public MailStatsObject MailStatsObject
    {
        get => _mailStatsObject;
        set
        {
            _mailStatsObject = value;
            OnPropertyChanged();
        }
    }

    public int TotalMails
    {
        get => _totalMails;
        set
        {
            _totalMails = value;
            OnPropertyChanged();
        }
    }

    public int CurrentMails
    {
        get => _currentMails;
        set
        {
            _currentMails = value;
            OnPropertyChanged();
        }
    }

    public Visibility IsMailMonitoringPopupVisible
    {
        get => _isMailMonitoringPopupVisible;
        set
        {
            _isMailMonitoringPopupVisible = value;
            OnPropertyChanged();
        }
    }

    public GridLength GridSplitterPosition
    {
        get => _gridSplitterPosition;
        set
        {
            _gridSplitterPosition = value;
            SettingsController.CurrentSettings.MailMonitoringGridSplitterPosition = _gridSplitterPosition.Value;
            OnPropertyChanged();
        }
    }

    #region Update ui

    private void UpdateTotalMailsUi(object sender, NotifyCollectionChangedEventArgs e)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            TotalMails = Mails.Count;
        });
    }

    private void UpdateCurrentMailsUi(object sender, EventArgs e)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            CurrentMails = MailCollectionView.Count;
        });
    }

    #endregion

    #region Filter

    private bool Filter(object obj)
    {
        return obj is Mail mail
               && mail.Timestamp.Date >= DatePickerMailsFrom.Date
               && mail.Timestamp.Date <= DatePickerMailsTo.Date && (
                   (mail.LocationName != null && mail.LocationName.ToLower().Contains(MailsSearchText?.ToLower() ?? string.Empty))
                   || ($"T{mail.Item.Tier}.{mail.Item.Level}".ToLower().Contains(MailsSearchText?.ToLower() ?? string.Empty))
                   || mail.MailTypeDescription.ToLower().Contains(MailsSearchText?.ToLower() ?? string.Empty)
                   || (mail.Item != null && mail.Item.LocalizedName.ToLower().Contains(MailsSearchText?.ToLower() ?? string.Empty))
                   || mail.MailContent.ActualUnitPrice.ToString().Contains(MailsSearchText?.ToLower() ?? string.Empty)
                   || mail.MailContent.TotalPrice.ToString().Contains(MailsSearchText?.ToLower() ?? string.Empty));
    }

    #endregion

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}