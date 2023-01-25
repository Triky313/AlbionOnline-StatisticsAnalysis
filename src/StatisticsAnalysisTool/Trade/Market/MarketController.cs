using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using StatisticsAnalysisTool.ViewModels;

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
        if (_tempOffers.Any(x => x.Id == purchase.AuctionId))
        {
            var trade = new InstantBuy();
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _mainWindowViewModel?.TradeMonitoringBindings?.Trade.Add(trade);
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
        if (_tempBuyOrders.Any(x => x.Id == sale.AuctionId))
        {
            var trade = new InstantSell();
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _mainWindowViewModel?.TradeMonitoringBindings?.Trade.Add(trade);
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