using Serilog;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public sealed class AuctionGetItemAverageStatsRequest
{
    public AuctionGetItemAverageStatsRequest(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters == null)
            {
                return;
            }

            if (parameters.TryGetValue(1, out var itemIndex))
            {
                ItemIndex = itemIndex.ObjectToInt();
            }

            if (parameters.TryGetValue(2, out var qualityLevel))
            {
                QualityLevel = qualityLevel.ObjectToInt();
            }

            if (parameters.TryGetValue(3, out var timeRange))
            {
                TimeRange = timeRange.ObjectToInt();
            }

            if (parameters.TryGetValue(255, out var requestId))
            {
                RequestId = requestId.ObjectToInt();
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    public int ItemIndex { get; }

    public int QualityLevel { get; }

    public int TimeRange { get; }

    public int RequestId { get; }
}