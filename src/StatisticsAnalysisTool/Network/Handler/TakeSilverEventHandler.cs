using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Handler;

public class TakeSilverEventHandler
{
    private readonly TrackingController _trackingController;

    public TakeSilverEventHandler(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    public async Task OnActionAsync(TakeSilverEvent value)
    {
        var localEntity = _trackingController.EntityController.GetLocalEntity()?.Value;

        var isObjectLocalEntity = value.ObjectId != null && localEntity?.ObjectId == value.ObjectId;
        var isObjectPartyEntityAndNotTargetEntity = value.ObjectId != null && _trackingController.EntityController.IsEntityInParty((long)value.ObjectId) && value.ObjectId != value.TargetEntityId;
        var isObjectLocalEntityAndTargetEntity = value.ObjectId != null && localEntity?.ObjectId == value.ObjectId && value.ObjectId == value.TargetEntityId;

        if (isObjectLocalEntity || isObjectPartyEntityAndNotTargetEntity || isObjectLocalEntityAndTargetEntity)
        {
            // Set guild tax % to local player
            if (isObjectLocalEntity && !isObjectLocalEntityAndTargetEntity)
            {
                _trackingController.EntityController.SetLastLocalEntityGuildTax(value.YieldPreTax, value.GuildTax);
                _trackingController.EntityController.SetLastLocalEntityClusterTax(value.YieldPreTax, value.ClusterTax);
            }

            // Include guild + cluster tax if a party member takes silver
            if (isObjectPartyEntityAndNotTargetEntity && !isObjectLocalEntity)
            {
                value.GuildTax = _trackingController.EntityController.GetLastLocalEntityGuildTax(value.YieldPreTax);
                var yieldAfterGuildTax = value.YieldPreTax - value.GuildTax;
                value.ClusterTax = _trackingController.EntityController.GetLastLocalEntityClusterTax(yieldAfterGuildTax);

                var yieldAfterGuildTaxAndClusterTax = yieldAfterGuildTax - value.ClusterTax;
                value.YieldAfterTax = yieldAfterGuildTaxAndClusterTax;
            }

            await _trackingController.AddNotificationAsync(SetNotification(value.YieldAfterTax, value.ClusterYieldAfterTax, value.PremiumAfterTax, value.ClusterTax));
            _trackingController.LiveStatsTracker.Add(ValueType.Silver, value.YieldAfterTax.DoubleValue);
            _trackingController.DungeonController?.AddValueToDungeon(value.YieldAfterTax.DoubleValue, ValueType.Silver);
            _trackingController.StatisticController?.AddValue(ValueType.Silver, value.YieldAfterTax.DoubleValue);
        }
    }

    private TrackingNotification SetNotification(FixPoint totalGainedSilver, FixPoint cluster, FixPoint premium, FixPoint clusterTax)
    {
        return new TrackingNotification(DateTime.Now, new SilverNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, totalGainedSilver,
            LanguageController.Translation("SILVER"), cluster, premium, clusterTax, LanguageController.Translation("GAINED")), NotificationType.Silver);
    }
}