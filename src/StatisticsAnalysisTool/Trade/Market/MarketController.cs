using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Trade.Market;

public class MarketController
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private ObservableCollection<AuctionEntry> _tempOffers = new();
    private ObservableCollection<AuctionEntry> _tempBuyOrders = new();

    public MarketController(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
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
                Id = purchase.AuctionId,
                Amount = purchase.Amount,
                AuctionEntry = tempOffer
            };
            
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _mainWindowViewModel?.TradeMonitoringBindings?.Trade.Add(instantBuy);
                _mainWindowViewModel?.TradeMonitoringBindings?.TradeCollectionView?.Refresh();
            });
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
                Id = sale.AuctionId,
                Amount = sale.Amount,
                AuctionEntry = tempBuyOrder
            };

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _mainWindowViewModel?.TradeMonitoringBindings?.Trade.Add(instantSell);
                _mainWindowViewModel?.TradeMonitoringBindings?.TradeCollectionView?.Refresh();
            });
        }
    }

    public void ResetTempBuyOrders()
    {
        _tempBuyOrders.Clear();
    }

    #endregion
}