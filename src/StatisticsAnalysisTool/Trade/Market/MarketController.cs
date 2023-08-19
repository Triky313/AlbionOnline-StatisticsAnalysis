using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Trade.Market;

public class MarketController
{
    private readonly TrackingController _trackingController;
    private ObservableCollection<AuctionEntry> _tempOffers = new();
    private ObservableCollection<AuctionEntry> _tempBuyOrders = new();
    private ObservableCollection<int> _tempNumberToBuyList = new();

    public MarketController(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    #region Buy from market

    public void AddOffers(IEnumerable<AuctionEntry> auctionOffers)
    {
        if (!SettingsController.CurrentSettings.IsTradeMonitoringActive)
        {
            return;
        }

        _tempOffers = new ObservableCollection<AuctionEntry>(auctionOffers);
    }

    public void AddOffers(IEnumerable<AuctionEntry> auctionOffers, IEnumerable<int> numberToBuyList)
    {
        if (!SettingsController.CurrentSettings.IsTradeMonitoringActive)
        {
            return;
        }

        _tempOffers = new ObservableCollection<AuctionEntry>(auctionOffers);
        _tempNumberToBuyList = new ObservableCollection<int>(numberToBuyList);
    }

    public async Task AddBuyAsync(Purchase purchase)
    {
        if (!SettingsController.CurrentSettings.IsTradeMonitoringActive)
        {
            return;
        }

        var tempOffer = _tempOffers.FirstOrDefault(x => x.Id == purchase.AuctionId);
        if (tempOffer != null)
        {
            var instantBuySellContent = new InstantBuySellContent()
            {
                Quantity = purchase.Amount,
                InternalUnitPrice = tempOffer.UnitPriceSilver,
                TaxRate = SettingsController.CurrentSettings.TradeMonitoringMarketTaxRate
            };

            var trade = new Trade()
            {
                Ticks = DateTime.UtcNow.Ticks,
                Type = TradeType.InstantBuy,
                Id = purchase.AuctionId,
                ClusterIndex = ClusterController.CurrentCluster.MainClusterIndex ?? ClusterController.CurrentCluster.Index,
                AuctionEntry = tempOffer,
                InstantBuySellContent = instantBuySellContent
            };

            _ = _trackingController.TradeController.AddTradeToBindingCollectionAsync(trade);
            await _trackingController.TradeController.SaveInFileAfterExceedingLimit(20);
        }
    }

    public async Task AddBuyAsync(List<long> purchaseIds)
    {
        if (!SettingsController.CurrentSettings.IsTradeMonitoringActive)
        {
            return;
        }

        foreach (long purchaseId in purchaseIds)
        {
            var tempOffer = _tempOffers.FirstOrDefault(x => x.Id == purchaseId);
            if (tempOffer == null)
            {
                continue;
            }

            var quantity = GetQuantityOfTempNumberToBuyList(purchaseIds, purchaseId);

            if (quantity <= 0)
            {
                continue;
            }

            var instantBuySellContent = new InstantBuySellContent()
            {
                Quantity = quantity,
                InternalUnitPrice = tempOffer.UnitPriceSilver,
                TaxRate = SettingsController.CurrentSettings.TradeMonitoringMarketTaxRate
            };

            var trade = new Trade()
            {
                Ticks = DateTime.UtcNow.Ticks,
                Type = TradeType.InstantBuy,
                Id = purchaseId,
                ClusterIndex = ClusterController.CurrentCluster.MainClusterIndex ?? ClusterController.CurrentCluster.Index,
                AuctionEntry = tempOffer,
                InstantBuySellContent = instantBuySellContent
            };

            _ = _trackingController.TradeController.AddTradeToBindingCollectionAsync(trade);
        }

        await _trackingController.TradeController.SaveInFileAfterExceedingLimit(20);
    }

    public void ResetTempOffers()
    {
        _tempOffers.Clear();
    }

    public void ResetTempNumberToBuyList()
    {
        _tempNumberToBuyList.Clear();
    }

    private int GetQuantityOfTempNumberToBuyList(IList<long> purchaseIds, long currentPurchaseId)
    {
        try
        {
            var index = purchaseIds.IndexOf(currentPurchaseId);
            return _tempNumberToBuyList[index];
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }

        return 0;
    }

    #endregion

    #region Sell to market

    public void AddBuyOrders(IEnumerable<AuctionEntry> auctionOrders)
    {
        if (!SettingsController.CurrentSettings.IsTradeMonitoringActive)
        {
            return;
        }

        _tempBuyOrders = new ObservableCollection<AuctionEntry>(auctionOrders);
    }

    public async Task AddSaleAsync(Sale sale)
    {
        if (!SettingsController.CurrentSettings.IsTradeMonitoringActive)
        {
            return;
        }

        var tempBuyOrder = _tempBuyOrders.FirstOrDefault(x => x.Id == sale.AuctionId);
        if (tempBuyOrder != null)
        {
            var instantBuySellContent = new InstantBuySellContent()
            {
                Quantity = sale.Amount,
                InternalUnitPrice = tempBuyOrder.UnitPriceSilver,
                TaxRate = SettingsController.CurrentSettings.TradeMonitoringMarketTaxRate
            };

            var trade = new Trade()
            {
                Ticks = DateTime.UtcNow.Ticks,
                Type = TradeType.InstantSell,
                Id = sale.AuctionId,
                ClusterIndex = ClusterController.CurrentCluster.MainClusterIndex ?? ClusterController.CurrentCluster.Index,
                AuctionEntry = tempBuyOrder,
                InstantBuySellContent = instantBuySellContent
            };

            _ = _trackingController.TradeController.AddTradeToBindingCollectionAsync(trade);
            await _trackingController.TradeController.SaveInFileAfterExceedingLimit(10);
        }
    }

    public void ResetTempBuyOrders()
    {
        _tempBuyOrders.Clear();
    }

    #endregion
}