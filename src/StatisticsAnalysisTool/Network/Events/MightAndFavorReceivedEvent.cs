using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class MightAndFavorReceivedEvent
{
    public FixPoint Might;
    public FixPoint PremiumOfMight;
    public FixPoint BonusOfMight;
    public FixPoint Favor;
    public FixPoint PremiumOfFavor;
    public FixPoint BonusOfFavor;
    public FixPoint TotalFavor;

    public MightAndFavorReceivedEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.ContainsKey(1))
            {
                var might = parameters[1].ObjectToLong();
                Might = FixPoint.FromInternalValue(might ?? 0);
            }

            if (parameters.ContainsKey(2))
            {
                var bonusOfMight = parameters[2].ObjectToLong();
                BonusOfMight = FixPoint.FromInternalValue(bonusOfMight ?? 0);
            }

            if (parameters.ContainsKey(3))
            {
                var premiumOfMight = parameters[3].ObjectToLong();
                PremiumOfMight = FixPoint.FromInternalValue(premiumOfMight ?? 0);
            }

            if (parameters.ContainsKey(4))
            {
                var favor = parameters[4].ObjectToLong();
                Favor = FixPoint.FromInternalValue(favor ?? 0);
            }

            if (parameters.ContainsKey(5))
            {
                var bonusOfFavor = parameters[5].ObjectToLong();
                BonusOfFavor = FixPoint.FromInternalValue(bonusOfFavor ?? 0);
            }

            if (parameters.ContainsKey(6))
            {
                var premiumOfFavor = parameters[6].ObjectToLong();
                PremiumOfFavor = FixPoint.FromInternalValue(premiumOfFavor ?? 0);
            }

            if (parameters.ContainsKey(7))
            {
                var totalFavor = parameters[7].ObjectToLong();
                TotalFavor = FixPoint.FromInternalValue(totalFavor ?? 0);
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}