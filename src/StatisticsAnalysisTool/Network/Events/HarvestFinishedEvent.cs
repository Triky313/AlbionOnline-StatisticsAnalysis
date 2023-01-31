using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class HarvestFinishedEvent
{
    public HarvestFinishedObject HarvestFinishedObject = new();

    public HarvestFinishedEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            long userObjectId = 0;
            long objectId = 0;
            int standardAmount = 0;
            int itemId = 0;
            int collectorBonusAmount = 0;
            int premiumBonusAmount = 0;
            int currentPossibleDegradationProcesses = 0;

            if (parameters.ContainsKey(0))
            {
                userObjectId = parameters[0].ObjectToLong() ?? 0;
            }

            if (parameters.ContainsKey(3))
            {
                objectId = parameters[3].ObjectToLong() ?? 0;
            }

            if (parameters.ContainsKey(4))
            {
                itemId = parameters[4].ObjectToInt();
            }

            if (parameters.ContainsKey(5))
            {
                standardAmount = parameters[5].ObjectToInt();
            }

            if (parameters.ContainsKey(6))
            {
                collectorBonusAmount = parameters[6].ObjectToInt();
            }

            if (parameters.ContainsKey(7))
            {
                premiumBonusAmount = parameters[7].ObjectToInt();
            }

            if (parameters.ContainsKey(8))
            {
                currentPossibleDegradationProcesses = parameters[8].ObjectToInt();
            }

            HarvestFinishedObject = new HarvestFinishedObject()
            {
                UserObjectId = userObjectId,
                ObjectId = objectId,
                StandardAmount = standardAmount,
                ItemId = itemId,
                CollectorBonusAmount = collectorBonusAmount,
                PremiumBonusAmount = premiumBonusAmount,
                CurrentPossibleDegradationProcesses = currentPossibleDegradationProcesses
            };
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}