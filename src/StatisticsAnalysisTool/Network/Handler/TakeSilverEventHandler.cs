using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Handler
{
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

            var IsObjectLocalEntity = value.ObjectId != null && localEntity?.ObjectId == value.ObjectId;
            var IsObjectPartyEntityAndNotTargetEntity = value.ObjectId != null && _trackingController.EntityController.IsEntityInParty((long) value.ObjectId) && value.ObjectId != value.TargetEntityId;
            var IsObjectLocalEntityAndTargetEntity = value.ObjectId != null && localEntity?.ObjectId == value.ObjectId && value.ObjectId == value.TargetEntityId;

            if (IsObjectLocalEntity || IsObjectPartyEntityAndNotTargetEntity || IsObjectLocalEntityAndTargetEntity)
            {
                // Set guild tax % to local player
                if (IsObjectLocalEntity && !IsObjectLocalEntityAndTargetEntity)
                {
                    _trackingController.EntityController.SetLastLocalEntityGuildTax(value.YieldPreTax, value.GuildTax);
                    _trackingController.EntityController.SetLastLocalEntityClusterTax(value.YieldPreTax, value.ClusterTax);
                }

                // Include guild + cluster tax if a party member takes silver
                if (IsObjectPartyEntityAndNotTargetEntity && !IsObjectLocalEntity)
                {
                    value.GuildTax = _trackingController.EntityController.GetLastLocalEntityGuildTax(value.YieldPreTax);
                    var YieldAfterGuildTax = value.YieldPreTax - value.GuildTax;
                    value.ClusterTax = _trackingController.EntityController.GetLastLocalEntityClusterTax(YieldAfterGuildTax);
                    
                    var yieldAfterGuildTaxAndClusterTax = YieldAfterGuildTax - value.ClusterTax;
                    value.YieldAfterTax = yieldAfterGuildTaxAndClusterTax;
                }

                await _trackingController.AddNotificationAsync(SetNotification(value.YieldAfterTax, value.ClusterYieldAfterTax, value.PremiumAfterTax, value.ClusterTax));
                _trackingController.CountUpTimer.Add(ValueType.Silver, value.YieldAfterTax.DoubleValue);
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
}