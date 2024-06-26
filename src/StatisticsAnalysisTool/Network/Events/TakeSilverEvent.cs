using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.EventValidations;

namespace StatisticsAnalysisTool.Network.Events;

public class TakeSilverEvent
{
    public long? ObjectId;
    public FixPoint ClusterTax;
    public FixPoint GuildTax;
    public FixPoint Multiplier;
    public bool IsPremiumBonus;
    public long? TargetEntityId;
    public long TimeStamp;

    public FixPoint YieldAfterTax;
    public FixPoint YieldPreTax;
    public FixPoint ClusterYieldPreTax;
    public FixPoint PremiumAfterTax;
    public FixPoint ClusterYieldAfterTax;

    public TakeSilverEvent(Dictionary<byte, object> parameters)
    {
        EventValidator.IsEventValid(EventCodes.TakeSilver, parameters);
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.TryGetValue(0, out object objectId))
            {
                ObjectId = objectId.ObjectToLong();
            }

            if (parameters.TryGetValue(1, out object timeStamp))
            {
                TimeStamp = timeStamp.ObjectToLong() ?? 0;
            }

            if (parameters.TryGetValue(2, out object targetEntityId))
            {
                TargetEntityId = targetEntityId.ObjectToLong();
            }

            if (parameters.TryGetValue(3, out object yieldPreTaxObject))
            {
                var yieldPreTax = yieldPreTaxObject.ObjectToLong();
                YieldPreTax = FixPoint.FromInternalValue(yieldPreTax ?? 0);
            }

            if (parameters.TryGetValue(5, out object guildTaxObject))
            {
                var guildTax = guildTaxObject.ObjectToLong();
                GuildTax = FixPoint.FromInternalValue(guildTax ?? 0);
            }

            if (parameters.TryGetValue(6, out object clusterTaxObject))
            {
                var clusterTax = clusterTaxObject.ObjectToLong();
                ClusterTax = FixPoint.FromInternalValue(clusterTax ?? 0);
            }

            if (parameters.TryGetValue(7, out object isPremiumBonus))
            {
                IsPremiumBonus = isPremiumBonus as bool? ?? false;
            }

            if (parameters.TryGetValue(8, out object multiplierObject))
            {
                var multiplier = multiplierObject.ObjectToLong();
                Multiplier = FixPoint.FromInternalValue(multiplier ?? 0);
            }

            YieldAfterTax = YieldPreTax - GuildTax;
            ClusterYieldPreTax = FixPoint.FromFloatingPointValue(YieldPreTax.DoubleValue - (YieldPreTax.DoubleValue / Multiplier.DoubleValue));
            PremiumAfterTax = ClusterYieldPreTax - ClusterTax;
            ClusterYieldAfterTax = FixPoint.FromFloatingPointValue((ClusterYieldPreTax.DoubleValue / Multiplier.DoubleValue) - ClusterTax.DoubleValue);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}