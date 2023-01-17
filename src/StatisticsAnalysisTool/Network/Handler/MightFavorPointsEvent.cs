using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Handler;

public class MightFavorPointsEvent
{
    public FixPoint Might;
    public FixPoint PremiumOfMight;
    public FixPoint Favor;
    public FixPoint PremiumOfFavor;
    public FixPoint TotalFavor;

    public MightFavorPointsEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);
        try
        {
            if (parameters.ContainsKey(0))
            {
                var might = parameters[0].ObjectToLong();
                Might = FixPoint.FromInternalValue(might ?? 0);
            }

            if (parameters.ContainsKey(2))
            {
                var premiumOfMight = parameters[2].ObjectToLong();
                PremiumOfMight = FixPoint.FromInternalValue(premiumOfMight ?? 0);
            }

            if (parameters.ContainsKey(3))
            {
                var favor = parameters[3].ObjectToLong();
                Favor = FixPoint.FromInternalValue(favor ?? 0);
            }

            if (parameters.ContainsKey(5))
            {
                var premiumOfFavor = parameters[5].ObjectToLong();
                PremiumOfFavor = FixPoint.FromInternalValue(premiumOfFavor ?? 0);
            }

            if (parameters.ContainsKey(6))
            {
                var totalFavor = parameters[6].ObjectToLong();
                TotalFavor = FixPoint.FromInternalValue(totalFavor ?? 0);
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}