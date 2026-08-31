using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.EventValidations;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class TakeSilverEvent
{
    private const double PremiumBonusMultiplier = 1.5;

    public long? ObjectId;
    public FixPoint AlliancePenalty;
    public FixPoint ClusterTax;
    public FixPoint GuildTax;
    public FixPoint Multiplier = FixPoint.One;
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

            if (parameters.TryGetValue(4, out object clusterTaxObject))
            {
                var clusterTax = clusterTaxObject.ObjectToLong();
                ClusterTax = FixPoint.FromInternalValue(clusterTax ?? 0);
            }

            if (parameters.TryGetValue(5, out object guildTaxObject))
            {
                var guildTax = guildTaxObject.ObjectToLong();
                GuildTax = FixPoint.FromInternalValue(guildTax ?? 0);
            }

            if (parameters.TryGetValue(6, out object alliancePenaltyObject))
            {
                var alliancePenalty = alliancePenaltyObject.ObjectToLong();
                AlliancePenalty = FixPoint.FromInternalValue(alliancePenalty ?? 0);
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

            RecalculateDerivedValues();
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    public void ApplyTaxes(FixPoint clusterTax, FixPoint guildTax, FixPoint alliancePenalty)
    {
        ClusterTax = clusterTax;
        GuildTax = guildTax;
        AlliancePenalty = alliancePenalty;

        RecalculateDerivedValues();
    }

    private void RecalculateDerivedValues()
    {
        var totalTaxInternal = ClusterTax.InternalValue + GuildTax.InternalValue + AlliancePenalty.InternalValue;
        var yieldAfterTaxInternal = Math.Max(0, YieldPreTax.InternalValue - totalTaxInternal);
        YieldAfterTax = FixPoint.FromInternalValue(yieldAfterTaxInternal);

        if (YieldPreTax.InternalValue <= 0)
        {
            ClusterYieldPreTax = default;
            ClusterYieldAfterTax = default;
            PremiumAfterTax = default;
            return;
        }

        var premiumMultiplier = IsPremiumBonus ? PremiumBonusMultiplier : 1;
        var clusterMultiplier = Math.Max(1, Multiplier.DoubleValue);
        var yieldBeforePremium = YieldPreTax.DoubleValue / premiumMultiplier;
        var baseYield = yieldBeforePremium / clusterMultiplier;
        var clusterYieldPreTax = Math.Max(0, yieldBeforePremium - baseYield);
        var premiumYieldPreTax = Math.Max(0, YieldPreTax.DoubleValue - yieldBeforePremium);
        var yieldAfterTaxFactor = YieldAfterTax.DoubleValue / YieldPreTax.DoubleValue;

        ClusterYieldPreTax = FixPoint.FromFloatingPointValue(clusterYieldPreTax);
        ClusterYieldAfterTax = FixPoint.FromFloatingPointValue(clusterYieldPreTax * yieldAfterTaxFactor);
        PremiumAfterTax = FixPoint.FromFloatingPointValue(premiumYieldPreTax * yieldAfterTaxFactor);
    }
}