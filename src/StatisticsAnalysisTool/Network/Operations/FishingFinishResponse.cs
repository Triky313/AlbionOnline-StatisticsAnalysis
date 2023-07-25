using StatisticsAnalysisTool.Common;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Operations;

public class FishingFinishResponse
{
    public bool Succeeded { get; set; }

    public FishingFinishResponse(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);
    }
}