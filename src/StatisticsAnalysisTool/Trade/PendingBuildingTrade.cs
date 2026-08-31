using StatisticsAnalysisTool.Trade.Market;
using System;

namespace StatisticsAnalysisTool.Trade;

internal sealed class PendingBuildingTrade
{
    public long Id { get; init; }
    public long Ticks { get; init; }
    public TradeType Type { get; set; }
    public bool IsMerchantPurchaseConfirmed { get; init; }
    public string ClusterIndex { get; init; }
    public Guid Guid { get; init; }
    public int ItemIndex { get; init; }
    public long InternalUnitPrice { get; init; }
    public int Quantity { get; init; }

    public Trade CreateTrade()
    {
        return new Trade
        {
            Id = Id,
            Ticks = Ticks,
            Type = Type,
            ClusterIndex = ClusterIndex,
            Guid = Guid,
            ItemIndex = ItemIndex,
            InstantBuySellContent = new InstantBuySellContent
            {
                InternalUnitPrice = InternalUnitPrice,
                Quantity = Quantity,
                TaxRate = 0
            }
        };
    }
}