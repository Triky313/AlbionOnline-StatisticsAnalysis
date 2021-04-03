using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            _trackingController.AddNotification(SetFactionFlagPointsNotification(value.CityFaction, value.GainedFactionFlagPoints.DoubleValue, value.BonusPremiumGainedFractionFlagPoints.DoubleValue));
            await Task.CompletedTask;
        }

        private TrackingNotification SetFactionFlagPointsNotification(CityFaction cityFaction, double GainedFractionPoints, double BonusPremiumGainedFractionPoints)
        {
            return new TrackingNotification(DateTime.Now, new List<LineFragment>
            {
                new FactionFlagPointsNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, cityFaction, GainedFractionPoints, BonusPremiumGainedFractionPoints, LanguageController.Translation("FACTION_FLAG_POINTS"), FameTypeOperator.Pve, LanguageController.Translation("GAINED")),
            });
        }
    }
}