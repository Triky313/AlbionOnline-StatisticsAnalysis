using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Trade.Market;

public interface IMarketController
{
    void AddOffers(IEnumerable<AuctionEntry> auctionOffers);
    void AddOffers(IEnumerable<AuctionEntry> auctionOffers, IEnumerable<int> numberToBuyList);
    Task AddBuyAsync(Purchase purchase);
    Task AddBuyAsync(List<long> purchaseIds);
    void ResetTempOffers();
    void ResetTempNumberToBuyList();
    void AddBuyOrders(IEnumerable<AuctionEntry> auctionOrders);
    Task AddSaleAsync(Sale sale);
    public void ResetTempBuyOrders();
}