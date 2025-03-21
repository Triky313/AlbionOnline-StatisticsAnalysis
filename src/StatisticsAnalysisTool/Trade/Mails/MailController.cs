using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Trade.Mails;

public class MailController
{
    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;

    public readonly List<MailNetworkObject> CurrentMailInfos = new();

    public MailController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
    }

    public void SetMailInfos(IEnumerable<MailNetworkObject> currentMailInfos)
    {
        CurrentMailInfos.Clear();
        CurrentMailInfos.AddRange(currentMailInfos);
    }

    public async Task AddMailAsync(long mailId, string content)
    {
        if (!SettingsController.CurrentSettings.IsTradeMonitoringActive)
        {
            return;
        }

        var mailArray = _mainWindowViewModel.TradeMonitoringBindings.Trades.ToArray();
        if (mailArray.Any(mailObject => mailObject.Id == mailId))
        {
            return;
        }

        if (_mainWindowViewModel.TradeMonitoringBindings.Trades.ToArray().Any(x => x.Id == mailId))
        {
            return;
        }

        var mailInfo = CurrentMailInfos.FirstOrDefault(x => x.MailId == mailId);

        if (mailInfo == null)
        {
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

        _ = _trackingController.TradeController.AddTradeToBindingCollectionAsync(trade);
        await _trackingController.TradeController.SaveInFileAfterExceedingLimit(10);
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
        _ = long.TryParse(contentObject[2], out var totalPriceWithoutTaxLong);
        _ = long.TryParse(contentObject[3], out var unitPricePaidWithOverpaymentLong);
        _ = long.TryParse(contentObject[4], out var totalDistanceFeeLong);

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
        var contentObject = content.Split("|");

        if (contentObject.Length < 3)
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
        var contentObject = content.Split("|");

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
        var contentExpiredObject = content.Split("|");

        if (contentExpiredObject.Length < 4)
        {
            return new MailContent();
        }

        _ = int.TryParse(contentExpiredObject[0], out var usedExpiredQuantity);
        _ = int.TryParse(contentExpiredObject[1], out var expiredQuantity);
        _ = long.TryParse(contentExpiredObject[2], out var totalRecoveredSilverLong);
        _ = long.TryParse(contentExpiredObject[4], out var totalDistanceFeeLong);
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

    #endregion
}