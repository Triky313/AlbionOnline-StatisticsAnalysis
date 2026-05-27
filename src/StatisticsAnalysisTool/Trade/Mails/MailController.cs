using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace StatisticsAnalysisTool.Trade.Mails;

public class MailController
{
    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly Dictionary<long, MailNetworkObject> _mailInfoById = new();
    private readonly SemaphoreSlim _mailProcessingLock = new(1, 1);

    public readonly List<MailNetworkObject> CurrentMailInfos = new();

    public MailController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
    }

    public void SetMailInfos(IEnumerable<MailNetworkObject> currentMailInfos)
    {
        if (currentMailInfos == null)
        {
            return;
        }

        lock (_mailInfoById)
        {
            foreach (var mailInfo in currentMailInfos.Where(x => x?.MailId > 0))
            {
                _mailInfoById[mailInfo.MailId] = mailInfo;
            }

            CurrentMailInfos.Clear();
            CurrentMailInfos.AddRange(_mailInfoById.Values.OrderByDescending(x => x.Tick));
        }
    }

    public async Task AddMailAsync(long mailId, string content)
    {
        if (!SettingsController.CurrentSettings.IsTradeMonitoringActive)
        {
            return;
        }

        if (mailId <= 0)
        {
            return;
        }

        await _mailProcessingLock.WaitAsync();
        try
        {
            var mailArray = _mainWindowViewModel.TradeMonitoringBindings.Trades.ToArray();
            if (mailArray.Any(mailObject => mailObject.Type == TradeType.Mail && mailObject.Id == mailId))
            {
                return;
            }

            var mailInfo = GetMailInfo(mailId);

            if (mailInfo == null)
            {
                Log.Warning("Mail content was received without matching mail metadata. MailId: {mailId}", mailId);
                return;
            }

            var mailContent = ContentToObject(mailInfo.MailType, content, SettingsController.CurrentSettings.TradeMonitoringMarketTaxRate, SettingsController.CurrentSettings.TradeMonitoringMarketTaxSetupRate);

            if (SettingsController.CurrentSettings.IgnoreMailsWithZeroValues && mailContent.IsMailWithoutValues)
            {
                return;
            }

            var trade = new Trade()
            {
                Ticks = mailInfo.Tick,
                Type = TradeType.Mail,
                Guid = mailInfo.Guid ?? default,
                Id = mailId,
                ClusterIndex = mailInfo.Subject,
                MailTypeText = mailInfo.MailTypeText,
                MailContent = mailContent
            };

            if (trade.MailType == MailType.Unknown)
            {
                return;
            }

            if (await _trackingController.TradeController.AddTradeToBindingCollectionAsync(trade))
            {
                await _trackingController.TradeController.SaveInFileAfterExceedingLimit(10);
            }
        }
        finally
        {
            _mailProcessingLock.Release();
        }
    }

    private MailNetworkObject GetMailInfo(long mailId)
    {
        lock (_mailInfoById)
        {
            return _mailInfoById.GetValueOrDefault(mailId);
        }
    }

    internal static MailContent ContentToObject(MailType type, string content, double taxRate, double taxSetupRate)
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
        var contentObject = SplitMailContent(content);

        if (contentObject.Length < 4)
        {
            return new MailContent();
        }

        _ = int.TryParse(contentObject[0], out var quantity);
        var uniqueItemName = contentObject[1];
        _ = long.TryParse(contentObject[2], out var totalPriceWithoutTaxLong);
        _ = long.TryParse(contentObject[3], out var unitPricePaidWithOverpaymentLong);
        var totalDistanceFeeLong = ParseOptionalLong(contentObject, 4);

        return new MailContent()
        {
            UsedQuantity = quantity,
            Quantity = quantity,
            InternalTotalPriceWithoutTax = totalPriceWithoutTaxLong,
            InternalUnitPricePaidWithOverpayment = unitPricePaidWithOverpaymentLong,
            InternalTotalDistanceFee = totalDistanceFeeLong,
            UniqueItemName = uniqueItemName,
            TaxSetupRate = taxSetupRate
        };
    }

    private static MailContent MarketplaceSellOrderFinishedToMailContent(string content, double taxRate, double taxSetupRate)
    {
        var contentObject = SplitMailContent(content);

        if (contentObject.Length < 4)
        {
            return new MailContent();
        }

        _ = int.TryParse(contentObject[0], out var quantity);
        var uniqueItemName = contentObject[1];
        _ = long.TryParse(contentObject[2], out var totalPriceWithoutTaxLong);
        _ = long.TryParse(contentObject[3], out var unitPricePaidWithOverpaymentLong);

        return new MailContent()
        {
            UsedQuantity = quantity,
            Quantity = quantity,
            InternalTotalPriceWithoutTax = totalPriceWithoutTaxLong,
            InternalUnitPricePaidWithOverpayment = unitPricePaidWithOverpaymentLong,
            UniqueItemName = uniqueItemName,
            TaxRate = taxRate,
            TaxSetupRate = taxSetupRate
        };
    }

    private static MailContent MarketplaceSellOrderExpiredToMailContent(string content, double taxRate, double taxSetupRate)
    {
        var contentObject = SplitMailContent(content);

        if (contentObject.Length < 4)
        {
            return new MailContent();
        }

        _ = int.TryParse(contentObject[0], out var usedQuantity);
        _ = int.TryParse(contentObject[1], out var quantity);
        _ = long.TryParse(contentObject[2], out var totalPriceWithoutTaxLong);
        var uniqueItemName = contentObject[3];

        return new MailContent()
        {
            UsedQuantity = usedQuantity,
            Quantity = quantity,
            InternalTotalPriceWithoutTax = totalPriceWithoutTaxLong,
            InternalUnitPricePaidWithOverpayment = 0,
            UniqueItemName = uniqueItemName,
            TaxRate = taxRate,
            TaxSetupRate = taxSetupRate
        };
    }

    private static MailContent MarketplaceBuyOrderExpiredToMailContent(string content, double taxSetupRate)
    {
        var contentExpiredObject = SplitMailContent(content);

        if (contentExpiredObject.Length < 4)
        {
            return new MailContent();
        }

        _ = int.TryParse(contentExpiredObject[0], out var usedExpiredQuantity);
        _ = int.TryParse(contentExpiredObject[1], out var expiredQuantity);
        _ = long.TryParse(contentExpiredObject[2], out var totalRecoveredSilverLong);
        var totalDistanceFeeLong = ParseOptionalLong(contentExpiredObject, 4);
        var uniqueItemExpiredName = contentExpiredObject[3];

        var totalRecoveredSilver = FixPoint.FromInternalValue(totalRecoveredSilverLong);

        // Calculation of costs
        var totalNotPurchased = expiredQuantity - usedExpiredQuantity;
        if (totalNotPurchased <= 0)
        {
            return new MailContent();
        }

        var unitPrice = FixPoint.FromFloatingPointValue(totalRecoveredSilver.DoubleValue / totalNotPurchased);
        var totalPrice = FixPoint.FromFloatingPointValue(unitPrice.DoubleValue * usedExpiredQuantity);

        return new MailContent()
        {
            UsedQuantity = usedExpiredQuantity,
            Quantity = expiredQuantity,
            InternalTotalPriceWithoutTax = FixPoint.FromFloatingPointValue(totalPrice.DoubleValue / 100 * taxSetupRate + totalPrice.DoubleValue).InternalValue,
            InternalUnitPricePaidWithOverpayment = 0,
            InternalTotalDistanceFee = totalDistanceFeeLong,
            UniqueItemName = uniqueItemExpiredName,
            TaxSetupRate = taxSetupRate
        };
    }

    public static MailType ConvertToMailType(string typeString)
    {
        return typeString switch
        {
            "MARKETPLACE_BUYORDER_FINISHED_SUMMARY" => MailType.MarketplaceBuyOrderFinished,
            "MARKETPLACE_SELLORDER_FINISHED_SUMMARY" => MailType.MarketplaceSellOrderFinished,
            "MARKETPLACE_SELLORDER_EXPIRED_SUMMARY" or "BLACKMARKET_SELLORDER_EXPIRED_SUMMARY" => MailType.MarketplaceSellOrderExpired,
            "MARKETPLACE_BUYORDER_EXPIRED_SUMMARY" => MailType.MarketplaceBuyOrderExpired,
            _ => MailType.Unknown
        };
    }

    private static long ParseOptionalLong(IReadOnlyList<string> values, int index)
    {
        return values.Count > index && long.TryParse(values[index], out var value)
            ? value
            : 0;
    }

    private static string[] SplitMailContent(string content)
    {
        return string.IsNullOrEmpty(content)
            ? []
            : content.Split("|");
    }

    #endregion
}
