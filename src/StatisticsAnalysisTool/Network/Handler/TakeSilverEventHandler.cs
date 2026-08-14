using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Threading.Tasks;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Handler;

public class TakeSilverEventHandler : EventPacketHandler<TakeSilverEvent>
{
    private readonly TrackingController _trackingController;

    public TakeSilverEventHandler(TrackingController trackingController) : base((int) EventCodes.TakeSilver)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(TakeSilverEvent value)
    {


        var localEntity = _trackingController.EntityController.GetLocalEntity()?.Value;

        var isObjectLocalEntity = value.ObjectId != null && localEntity?.ObjectId == value.ObjectId;
        var isObjectPartyEntityAndNotTargetEntity = value.ObjectId != null && _trackingController.EntityController.IsEntityInParty((long) value.ObjectId) && value.ObjectId != value.TargetEntityId;
        var isObjectLocalEntityAndTargetEntity = value.ObjectId != null && localEntity?.ObjectId == value.ObjectId && value.ObjectId == value.TargetEntityId;

        if (isObjectLocalEntity || isObjectPartyEntityAndNotTargetEntity || isObjectLocalEntityAndTargetEntity)
        {
            // Set tax percentages based on the local player's event for party member estimates
            if (isObjectLocalEntity && !isObjectLocalEntityAndTargetEntity)
            {
                _trackingController.EntityController.SetLastLocalEntityGuildTax(value.YieldPreTax, value.GuildTax);
                _trackingController.EntityController.SetLastLocalEntityClusterTax(value.YieldPreTax, value.ClusterTax);
                _trackingController.EntityController.SetLastLocalEntityAlliancePenalty(value.YieldPreTax, value.AlliancePenalty);
            }

            // Include the local player's tax percentages if a party member takes silver
            if (isObjectPartyEntityAndNotTargetEntity && !isObjectLocalEntity)
            {
                value.ApplyTaxes(
                    _trackingController.EntityController.GetLastLocalEntityClusterTax(value.YieldPreTax),
                    _trackingController.EntityController.GetLastLocalEntityGuildTax(value.YieldPreTax),
                    _trackingController.EntityController.GetLastLocalEntityAlliancePenalty(value.YieldPreTax));
            }

            await _trackingController.AddNotificationAsync(SetNotification(value.YieldAfterTax, value.ClusterYieldAfterTax, value.PremiumAfterTax, value.ClusterTax));
            _trackingController.LiveStatsTracker.Add(ValueType.Silver, value.YieldAfterTax.DoubleValue);
            _trackingController.DungeonController?.AddValueToDungeon(value.YieldAfterTax.DoubleValue, ValueType.Silver);
            _trackingController.StatisticController?.AddValue(ValueType.Silver, value.YieldAfterTax.DoubleValue);
        }
    }

    private TrackingNotification SetNotification(FixPoint totalGainedSilver, FixPoint cluster, FixPoint premium, FixPoint clusterTax)
    {
        return new TrackingNotification(DateTime.Now, new SilverNotificationFragment(LocalizationController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, totalGainedSilver,
            LocalizationController.Translation("SILVER"), cluster, premium, clusterTax, LocalizationController.Translation("GAINED")), LoggingFilterType.Silver);
    }
}