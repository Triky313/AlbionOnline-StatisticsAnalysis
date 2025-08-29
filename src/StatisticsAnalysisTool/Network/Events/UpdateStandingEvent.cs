using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class UpdateStandingEvent
{
    public CityFaction CityFaction;
    public FixPoint PointsWithoutPremium;
    public FixPoint PremiumBonus;
    public FixPoint TotalPoints;

    public UpdateStandingEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out object totalPoints))
            {
                TotalPoints = FixPoint.FromInternalValue(totalPoints.ObjectToLong() ?? 0);
            }

            if (parameters.TryGetValue(1, out object premiumBonus))
            {
                PremiumBonus = FixPoint.FromInternalValue(premiumBonus.ObjectToLong() ?? 0);
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}