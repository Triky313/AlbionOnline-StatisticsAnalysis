using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Operations;

public class FishingCatchRequest
{
    public long ActionId { get; }

    public FishingCatchRequest(Dictionary<byte, object> parameters)
    {
        if (parameters.TryGetValue(0, out object actionId))
        {
            ActionId = actionId.ObjectToLong() ?? -1;
        }
    }
}