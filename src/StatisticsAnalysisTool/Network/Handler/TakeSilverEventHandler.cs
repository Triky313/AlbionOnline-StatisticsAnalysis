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
    private readonly IGameEventWrapper _gameEventWrapper;

    public TakeSilverEventHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.TakeSilver)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(TakeSilverEvent value)
    {
        var localEntity = _gameEventWrapper.EntityController.GetLocalEntity()?.Value;

        var isObjectLocalEntity = value.ObjectId != null && localEntity?.ObjectId == value.ObjectId;
        var isObjectPartyEntityAndNotTargetEntity = value.ObjectId != null && _gameEventWrapper.EntityController.IsEntityInParty((long) value.ObjectId) && value.ObjectId != value.TargetEntityId;
        var isObjectLocalEntityAndTargetEntity = value.ObjectId != null && localEntity?.ObjectId == value.ObjectId && value.ObjectId == value.TargetEntityId;

        if (isObjectLocalEntity || isObjectPartyEntityAndNotTargetEntity || isObjectLocalEntityAndTargetEntity)
        {
            // Set guild tax % to local player
            if (isObjectLocalEntity && !isObjectLocalEntityAndTargetEntity)
            {
                _gameEventWrapper.EntityController.SetLastLocalEntityGuildTax(value.YieldPreTax, value.GuildTax);
                _gameEventWrapper.EntityController.SetLastLocalEntityClusterTax(value.YieldPreTax, value.ClusterTax);
            }

            // Include guild + cluster tax if a party member takes silver
            if (isObjectPartyEntityAndNotTargetEntity && !isObjectLocalEntity)
            {
                value.GuildTax = _gameEventWrapper.EntityController.GetLastLocalEntityGuildTax(value.YieldPreTax);
                var yieldAfterGuildTax = value.YieldPreTax - value.GuildTax;
                value.ClusterTax = _gameEventWrapper.EntityController.GetLastLocalEntityClusterTax(yieldAfterGuildTax);

                var yieldAfterGuildTaxAndClusterTax = yieldAfterGuildTax - value.ClusterTax;
                value.YieldAfterTax = yieldAfterGuildTaxAndClusterTax;
            }

            await _gameEventWrapper.TrackingController.AddNotificationAsync(SetNotification(value.YieldAfterTax, value.ClusterYieldAfterTax, value.PremiumAfterTax, value.ClusterTax));
            _gameEventWrapper.LiveStatsTracker.Add(ValueType.Silver, value.YieldAfterTax.DoubleValue);
            _gameEventWrapper.DungeonController?.AddValueToDungeon(value.YieldAfterTax.DoubleValue, ValueType.Silver);
            _gameEventWrapper.StatisticController?.AddValue(ValueType.Silver, value.YieldAfterTax.DoubleValue);
        }
    }

    private TrackingNotification SetNotification(FixPoint totalGainedSilver, FixPoint cluster, FixPoint premium, FixPoint clusterTax)
    {
        return new TrackingNotification(DateTime.Now, new SilverNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, totalGainedSilver,
            LanguageController.Translation("SILVER"), cluster, premium, clusterTax, LanguageController.Translation("GAINED")), LoggingFilterType.Silver);
    }
}