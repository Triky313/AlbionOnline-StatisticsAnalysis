using Albion.Network;
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
    public class UpdateFactionStandingEventHandler : EventPacketHandler<UpdateFactionStandingEvent>
    {
        private readonly TrackingController _trackingController;

        public UpdateFactionStandingEventHandler(TrackingController trackingController) : base((int)EventCodes.UpdateFactionStanding)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(UpdateFactionStandingEvent value)
        {
            await _trackingController.AddNotificationAsync(SetFactionFlagPointsNotification(value.CityFaction, value.GainedFactionFlagPoints.DoubleValue, value.BonusPremiumGainedFractionFlagPoints.DoubleValue));
            _trackingController.DungeonController?.AddValueToDungeon(value.GainedFactionFlagPoints.DoubleValue, ValueType.FactionFame);
        }

        private TrackingNotification SetFactionFlagPointsNotification(CityFaction cityFaction, double GainedFractionPoints, double BonusPremiumGainedFractionPoints)
        {
            return new TrackingNotification(DateTime.Now, new List<LineFragment>
            {
                new FactionFlagPointsNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, cityFaction, GainedFractionPoints, 
                    BonusPremiumGainedFractionPoints, LanguageController.Translation("FACTION_FLAG_POINTS"), LanguageController.Translation("GAINED")),
            }, NotificationType.Faction);
        }
    }
}