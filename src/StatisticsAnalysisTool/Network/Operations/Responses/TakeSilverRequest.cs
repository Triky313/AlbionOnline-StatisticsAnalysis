using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class TakeSilverRequest
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public readonly long BuildingObjectId;
    public readonly long Costs;

    public TakeSilverRequest(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLine(new ConsoleFragment(GetType().Name, parameters, ConsoleColorType.EventColor));

        try
        {
            if (parameters.ContainsKey(1))
            {
                BuildingObjectId = parameters[1].ObjectToLong() ?? -1;
            }

            if (parameters.ContainsKey(4))
            {
                Costs = parameters[4].ObjectToLong() ?? 0;
            }

        }
        catch (Exception e)
        {
            Log.Error(nameof(ReadMailResponse), e);
        }
    }
}