using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Trade.Market;

public class MarketController
{
    private readonly TrackingController _trackingController;
    private ObservableCollection<AuctionEntry> _tempOffers = new();
    private ObservableCollection<AuctionEntry> _tempBuyOrders = new();

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

            _trackingController.TradeController.AddTradeToBindingCollection(trade);
            await _trackingController.TradeController.SaveInFileAfterExceedingLimit(10);
        }
    }

    public void ResetTempOffers()
    {
        _tempOffers.Clear();
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

            _trackingController.TradeController.AddTradeToBindingCollection(trade);
            await _trackingController.TradeController.SaveInFileAfterExceedingLimit(10);
        }
    }

    public void ResetTempBuyOrders()
    {
        _tempBuyOrders.Clear();
    }

    #endregion
}