using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace StatisticsAnalysisTool.Network.Manager;

public class MailController
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private int _addMailCounter;

    public List<MailInfoObject> CurrentMailInfos = new();

    public MailController(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;

        if (_mainWindowViewModel?.MailMonitoringBindings?.Mails != null)
        {
            _mainWindowViewModel.MailMonitoringBindings.Mails.CollectionChanged += OnCollectionChanged;
        }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        _mainWindowViewModel?.MailMonitoringBindings?.MailStatsObject.SetMailStats(_mainWindowViewModel?.MailMonitoringBindings?.Mails);
    }

    public void SetMailInfos(List<MailInfoObject> currentMailInfos)
    {
        CurrentMailInfos.Clear();
        CurrentMailInfos.AddRange(currentMailInfos);
    }

    public async Task AddMailAsync(long mailId, string content)
    {
        if (!SettingsController.CurrentSettings.IsMailMonitoringActive)
        {
            return;
        }

        lock (_mainWindowViewModel.MailMonitoringBindings.Mails)
        {
            var list = _mainWindowViewModel.MailMonitoringBindings.Mails.ToList();
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].MailId == mailId)
                {
                    return;
                }
            }
        }

        if (_mainWindowViewModel.MailMonitoringBindings.Mails.ToArray().Any(x => x.MailId == mailId))
        {
            return;
        }

        var mailInfo = CurrentMailInfos.FirstOrDefault(x => x.MailId == mailId);

        if (mailInfo == null)
        {
            return;
        }

        var mailContent = ContentToObject(mailInfo.MailType, content, SettingsController.CurrentSettings.MailMonitoringMarketTaxRate, SettingsController.CurrentSettings.MailMonitoringMarketTaxSetupRate);

        if (SettingsController.CurrentSettings.IgnoreMailsWithZeroValues && mailContent.IsMailWithoutValues)
        {
            return;
        }

        var mail = new Mail()
        {
            Tick = mailInfo.Tick,
            Guid = mailInfo.Guid ?? default,
            MailId = mailId,
            ClusterIndex = mailInfo.Subject,
            MailTypeText = mailInfo.MailTypeText,
            MailContent = mailContent
        };

        if (mail.MailType == MailType.Unknown)
        {
            return;
        }

        AddMailToListAndSort(mail);
        await SaveInFileAfterExceedingLimit(10);
    }

    public async void AddMailToListAndSort(Mail mail)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            _mainWindowViewModel?.MailMonitoringBindings?.Mails.Add(mail);
            _mainWindowViewModel?.MailMonitoringBindings?.MailCollectionView?.Refresh();
        });
    }

    public async Task RemoveMailsByIdsAsync(IEnumerable<long> mailIds)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            foreach (var mail in _mainWindowViewModel?.MailMonitoringBindings?.Mails?.ToList().Where(x => mailIds.Contains(x.MailId)) ?? new List<Mail>())
            {
                _mainWindowViewModel?.MailMonitoringBindings?.Mails?.Remove(mail);
            }
            _mainWindowViewModel?.MailMonitoringBindings?.MailStatsObject?.SetMailStats(_mainWindowViewModel?.MailMonitoringBindings?.MailCollectionView?.Cast<Mail>().ToList());

            _mainWindowViewModel?.MailMonitoringBindings?.UpdateTotalMailsUi(null, null);
            _mainWindowViewModel?.MailMonitoringBindings?.UpdateCurrentMailsUi(null, null);
        });
    }

    public async Task RemoveMailsByDaysInSettingsAsync()
    {
        var deleteAfterDays = SettingsController.CurrentSettings?.DeleteMailsOlderThanSpecifiedDays ?? 0;
        if (deleteAfterDays <= 0)
        {
            return;
        }

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            foreach (var mail in _mainWindowViewModel?.MailMonitoringBindings?.Mails?.ToList()
                         .Where(x => x?.Timestamp.AddDays(deleteAfterDays) < DateTime.UtcNow)!)
            {
                _mainWindowViewModel?.MailMonitoringBindings?.Mails?.Remove(mail);
            }
            _mainWindowViewModel?.MailMonitoringBindings?.MailStatsObject?.SetMailStats(_mainWindowViewModel?.MailMonitoringBindings?.MailCollectionView?.Cast<Mail>().ToList());

            _mainWindowViewModel?.MailMonitoringBindings?.UpdateTotalMailsUi(null, null);
            _mainWindowViewModel?.MailMonitoringBindings?.UpdateCurrentMailsUi(null, null);
        });
    }

    private static MailContent ContentToObject(MailType type, string content, double taxRate, double taxSetupRate)
    {
        switch (type)
        {
            case MailType.MarketplaceBuyOrderFinished:
                return MarketplaceBuyOrderFinishedToMailContent(content, taxSetupRate);
            case MailType.MarketplaceSellOrderFinished:
                return MarketplaceSellOrderFinishedToMailContent(content, taxRate, taxSetupRate);
            case MailType.MarketplaceSellOrderExpired:
                return MarketplaceSellOrderExpiredToMailContent(content, taxRate, taxSetupRate);
            case MailType.MarketplaceBuyOrderExpired:
                return MarketplaceBuyOrderExpiredToMailContent(content, taxSetupRate);
            case MailType.Unknown:
            default:
                return new MailContent();
        }
    }

    #region Mail values converter

    private static MailContent MarketplaceBuyOrderFinishedToMailContent(string content, double taxSetupRate)
    {
        var contentObject = content.Split("|");

        if (contentObject.Length < 3)
        {
            return new MailContent();
        }

        _ = int.TryParse(contentObject[0], out var quantity);
        var uniqueItemName = contentObject[1];
        _ = long.TryParse(contentObject[2], out var totalPriceLong);
        _ = long.TryParse(contentObject[3], out var unitPriceLong);

        var totalPrice = FixPoint.FromInternalValue(totalPriceLong);
        var unitPrice = FixPoint.FromInternalValue(unitPriceLong);

        return new MailContent()
        {
            UsedQuantity = quantity,
            Quantity = quantity,
            InternalTotalPrice = FixPoint.FromFloatingPointValue((totalPrice.DoubleValue / 100 * taxSetupRate) + totalPrice.DoubleValue).InternalValue,
            InternalUnitPrice = FixPoint.FromFloatingPointValue((unitPrice.DoubleValue / 100 * taxSetupRate) + unitPrice.DoubleValue).InternalValue,
            UniqueItemName = uniqueItemName,
            TaxSetupRate = taxSetupRate
        };
    }

    private static MailContent MarketplaceSellOrderFinishedToMailContent(string content, double taxRate, double taxSetupRate)
    {
        var contentObject = content.Split("|");

        if (contentObject.Length < 3)
        {
            return new MailContent();
        }

        _ = int.TryParse(contentObject[0], out var quantity);
        var uniqueItemName = contentObject[1];
        _ = long.TryParse(contentObject[2], out var totalPriceLong);
        _ = long.TryParse(contentObject[3], out var unitPriceLong);

        return new MailContent()
        {
            UsedQuantity = quantity,
            Quantity = quantity,
            InternalTotalPrice = totalPriceLong,
            InternalUnitPrice = unitPriceLong,
            UniqueItemName = uniqueItemName,
            TaxRate = taxRate,
            TaxSetupRate = taxSetupRate
        };
    }

    private static MailContent MarketplaceSellOrderExpiredToMailContent(string content, double taxRate, double taxSetupRate)
    {
        var contentObject = content.Split("|");

        if (contentObject.Length < 4)
        {
            return new MailContent();
        }

        _ = int.TryParse(contentObject[0], out var usedQuantity);
        _ = int.TryParse(contentObject[1], out var quantity);
        _ = long.TryParse(contentObject[2], out var totalPriceLong);
        var uniqueItemName = contentObject[3];

        var totalPrice = FixPoint.FromInternalValue(totalPriceLong);

        // Calculation of costs
        var singlePrice = totalPrice.DoubleValue / usedQuantity;

        return new MailContent()
        {
            UsedQuantity = usedQuantity,
            Quantity = quantity,
            InternalTotalPrice = totalPriceLong,
            InternalUnitPrice = FixPoint.FromFloatingPointValue(singlePrice).InternalValue,
            UniqueItemName = uniqueItemName,
            TaxRate = taxRate,
            TaxSetupRate = taxSetupRate
        };
    }

    private static MailContent MarketplaceBuyOrderExpiredToMailContent(string content, double taxSetupRate)
    {
        var contentExpiredObject = content.Split("|");

        if (contentExpiredObject.Length < 4)
        {
            return new MailContent();
        }

        _ = int.TryParse(contentExpiredObject[0], out var usedExpiredQuantity);
        _ = int.TryParse(contentExpiredObject[1], out var expiredQuantity);
        _ = long.TryParse(contentExpiredObject[2], out var totalRecoveredSilverLong);
        var uniqueItemExpiredName = contentExpiredObject[3];

        var totalRecoveredSilver = FixPoint.FromInternalValue(totalRecoveredSilverLong);

        // Calculation of costs
        var totalNotPurchased = expiredQuantity - usedExpiredQuantity;

        var unitPrice = FixPoint.FromFloatingPointValue(totalRecoveredSilver.DoubleValue / totalNotPurchased);
        var totalPrice = FixPoint.FromFloatingPointValue(unitPrice.DoubleValue * usedExpiredQuantity);

        return new MailContent()
        {
            UsedQuantity = usedExpiredQuantity,
            Quantity = expiredQuantity,
            InternalTotalPrice = FixPoint.FromFloatingPointValue((totalPrice.DoubleValue / 100 * taxSetupRate) + totalPrice.DoubleValue).InternalValue,
            InternalUnitPrice = FixPoint.FromFloatingPointValue((unitPrice.DoubleValue / 100 * taxSetupRate) + unitPrice.DoubleValue).InternalValue,
            UniqueItemName = uniqueItemExpiredName,
            TaxSetupRate = taxSetupRate
        };
    }

    #endregion

    public async Task SetMailsAsync(List<Mail> mails)
    {
        await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
        {
            foreach (var item in mails)
            {
                // The if block convert data from old versions to new version
                if (item.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceBuyOrderFinished && item.MailContent.UsedQuantity != item.MailContent.Quantity)
                {
                    item.MailContent.UsedQuantity = item.MailContent.Quantity;
                }

                // The if block convert data from old versions to new version
                if (item.MailType is MailType.MarketplaceSellOrderFinished or MailType.MarketplaceSellOrderExpired
                    && item.MailContent?.TaxRate != null && item.Timestamp < new DateTime(2022, 12, 1))
                {
                    item.MailContent.TaxRate = item.Timestamp < new DateTime(2022, 9, 14) ? 3 : 4; // Into the Fray Patch 6 tax increase
                }

                // The if block convert data from old versions to new version
                if (item.MailContent?.TaxSetupRate != null && item.Timestamp < new DateTime(2022, 12, 1))
                {
                    item.MailContent.TaxSetupRate = item.Timestamp < new DateTime(2022, 9, 14) ? 1.5 : 2.5; // Into the Fray Patch 6 tax increase
                }
            }

            _mainWindowViewModel?.MailMonitoringBindings?.Mails?.AddRange(mails.AsEnumerable());
            _mainWindowViewModel?.MailMonitoringBindings?.MailCollectionView?.Refresh();
            _mainWindowViewModel?.MailMonitoringBindings?.MailStatsObject?.SetMailStats(mails);
        }, DispatcherPriority.Background, CancellationToken.None);
    }

    /// <summary>
    /// Converted a string to MailType.
    /// </summary>
    /// <param name="typeString"></param>
    /// <returns>Returns a enum as MailType.</returns>
    public static MailType ConvertToMailType(string typeString)
    {
        return typeString switch
        {
            "MARKETPLACE_BUYORDER_FINISHED_SUMMARY" => MailType.MarketplaceBuyOrderFinished,
            "MARKETPLACE_SELLORDER_FINISHED_SUMMARY" => MailType.MarketplaceSellOrderFinished,
            "MARKETPLACE_SELLORDER_EXPIRED_SUMMARY" => MailType.MarketplaceSellOrderExpired,
            "MARKETPLACE_BUYORDER_EXPIRED_SUMMARY" => MailType.MarketplaceBuyOrderExpired,
            _ => MailType.Unknown
        };
    }

    #region Load / Save local file data

    public async Task LoadFromFileAsync()
    {
        await SetMailsAsync(await FileController.LoadAsync<List<Mail>>($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.MailsFileName}"));
    }

    private async Task SaveInFileAfterExceedingLimit(int limit)
    {
        if (++_addMailCounter < limit)
        {
            return;
        }

        if (_mainWindowViewModel?.MailMonitoringBindings?.Mails == null)
        {
            return;
        }

        List<Mail> mails;
        lock (_mainWindowViewModel.MailMonitoringBindings.Mails)
        {
            mails = _mainWindowViewModel?.MailMonitoringBindings?.Mails?.ToList();
        }

        if (mails == null)
        {
            return;
        }

        await FileController.SaveAsync(mails, $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.MailsFileName}");
        _addMailCounter = 0;
    }

    #endregion
}