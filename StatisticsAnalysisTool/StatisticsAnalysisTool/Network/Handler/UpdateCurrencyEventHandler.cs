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
    public class UpdateCurrencyEventHandler : EventPacketHandler<UpdateCurrencyEvent>
    {
        private readonly TrackingController _trackingController;
        private readonly FactionPointsCountUpTimer _factionPointsCountUpTimer;

        public UpdateCurrencyEventHandler(TrackingController trackingController, FactionPointsCountUpTimer factionPointsCountUpTimer) : base((int)EventCodes.UpdateCurrency)
        {
            _trackingController = trackingController;
            _factionPointsCountUpTimer = factionPointsCountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateCurrencyEvent value)
        {
            _trackingController.AddNotification(SetFactionPointsNotification(value.CityFaction, value.GainedFactionPoints.DoubleValue, value.BonusPremiumGainedFractionFlagPoints.DoubleValue));
            _factionPointsCountUpTimer.Add(value.CityFaction, value.GainedFactionPoints.DoubleValue);
            await Task.CompletedTask;
        }

        private TrackingNotification SetFactionPointsNotification(CityFaction cityFaction, double GainedFractionPoints, double BonusPremiumGainedFractionPoints)
        {
            return new TrackingNotification(DateTime.Now, new List<LineFragment>
            {
                new FactionPointsNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, cityFaction, GainedFractionPoints, BonusPremiumGainedFractionPoints, LanguageController.Translation("FACTION_FLAG_POINTS"), FameTypeOperator.Pve, LanguageController.Translation("GAINED")),
            });
        }
    }
}