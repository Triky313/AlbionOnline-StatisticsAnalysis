using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Trade.Market;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public sealed class GoldMarketTradeResponse
{
    public GoldMarketTradeResponse(Dictionary<byte, object> parameters)
    {
        var internalGoldAmount = GetLong(parameters, 0);
        var internalTotalPrice = GetLong(parameters, 2);
        if (internalTotalPrice <= 0)
        {
            internalTotalPrice = GetLong(parameters, 1);
        }

        Trade = new GoldMarketTrade
        {
            Quantity = GetQuantity(internalGoldAmount),
            InternalTotalPrice = internalTotalPrice
        };
    }

    public GoldMarketTrade Trade { get; }

    private static long GetLong(IReadOnlyDictionary<byte, object> parameters, byte key)
    {
        return parameters.TryGetValue(key, out var value) ? value.ObjectToLong() ?? 0 : 0;
    }

    private static int GetQuantity(long internalGoldAmount)
    {
        if (internalGoldAmount <= 0 || internalGoldAmount % FixPoint.InternalFactor != 0)
        {
            return 0;
        }

        var quantity = internalGoldAmount / FixPoint.InternalFactor;
        return quantity <= int.MaxValue ? (int) quantity : 0;
    }
}