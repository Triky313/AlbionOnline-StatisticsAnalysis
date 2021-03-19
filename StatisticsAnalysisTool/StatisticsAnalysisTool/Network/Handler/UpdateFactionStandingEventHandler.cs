using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
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
        private readonly FactionPointsCountUpTimer _factionPointsCountUpTimer;

        public UpdateFactionStandingEventHandler(TrackingController trackingController, FactionPointsCountUpTimer factionPointsCountUpTimer) : base((int) EventCodes.UpdateFactionStanding)
        {
            _trackingController = trackingController;
            _factionPointsCountUpTimer = factionPointsCountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateFactionStandingEvent value)
        {
            _trackingController.AddNotification(SetFactionPointsNotification(value.CurrentFactionID, value.GainedFactionPoints.DoubleValue, value.BonusPremiumGainedFractionPoints.DoubleValue));
            _factionPointsCountUpTimer.Add(value.GainedFactionPoints.DoubleValue);
           // _trackingController.AddValueToDungeon(value.GainedFractionPoints.DoubleValue, ValueType.Fame);

            _trackingController.SetTotalPlayerFactionPoints(value.TotalPlayerFactionPoints.DoubleValue);
            await Task.CompletedTask;
        }




        private TrackingNotification SetFactionPointsNotification(double CurrentFactionID, double GainedFractionPoints, double BonusPremiumGainedFractionPoints)
        {
            return new TrackingNotification(DateTime.Now, new List<LineFragment>
            {
                new FactionNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, CurrentFactionID, GainedFractionPoints, BonusPremiumGainedFractionPoints, LanguageController.Translation("FACTION_POINTS"), FameTypeOperator.Pve, LanguageController.Translation("GAINED")),
            });
        }
    }
}