using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class UpdateBrecilienStandingEvent
{
    public CityFaction CityFaction;
    public FixPoint PointsWithoutPremium;
    public FixPoint PremiumBonus;
    public FixPoint TotalPoints;

    public UpdateBrecilienStandingEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

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
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}