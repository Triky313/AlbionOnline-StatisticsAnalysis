using System;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Network.Manager;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
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
        _tempOffers = new ObservableCollection<AuctionEntry>(auctionOffers);
    }

    public async Task AddBuyAsync(Purchase purchase)
    {
        var tempOffer = _tempOffers.FirstOrDefault(x => x.Id == purchase.AuctionId);
        if (tempOffer != null)
        {
            var instantBuy = new InstantBuy()
            {
                Ticks = DateTime.UtcNow.Ticks,
                Id = purchase.AuctionId,
                Amount = purchase.Amount,
                AuctionEntry = tempOffer,
                TaxRate = SettingsController.CurrentSettings.TradeMonitoringMarketTaxRate
            };

            _trackingController.TradeController.AddTradeToBindingCollection(instantBuy);
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
        _tempBuyOrders = new ObservableCollection<AuctionEntry>(auctionOrders);
    }

    public async Task AddSaleAsync(Sale sale)
    {
        var tempBuyOrder = _tempBuyOrders.FirstOrDefault(x => x.Id == sale.AuctionId);
        if (tempBuyOrder != null)
        {
            var instantSell = new InstantSell()
            {
                Ticks = DateTime.UtcNow.Ticks,
                Id = sale.AuctionId,
                Amount = sale.Amount,
                AuctionEntry = tempBuyOrder,
                TaxRate = SettingsController.CurrentSettings.TradeMonitoringMarketTaxRate
            };

            _trackingController.TradeController.AddTradeToBindingCollection(instantSell);
            await _trackingController.TradeController.SaveInFileAfterExceedingLimit(10);
        }
    }

    public void ResetTempBuyOrders()
    {
        _tempBuyOrders.Clear();
    }

    #endregion
}