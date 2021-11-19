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
    public class UpdateCurrencyEventHandler : EventPacketHandler<UpdateCurrencyEvent>
    {
        private readonly TrackingController _trackingController;
        private readonly CountUpTimer _countUpTimer;

        public UpdateCurrencyEventHandler(TrackingController trackingController) : base((int)EventCodes.UpdateCurrency)
        {
            _trackingController = trackingController;
            _countUpTimer = _trackingController?.CountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateCurrencyEvent value)
        {
            if (value.CityFaction != CityFaction.Unknown)
            {
                await _trackingController.AddNotificationAsync(SetFactionPointsNotification(value.CityFaction, value.GainedFactionCoins.DoubleValue, value.BonusPremiumGainedFractionFlagPoints.DoubleValue));
            }
            
            _countUpTimer.Add(ValueType.FactionPoints, value.GainedFactionCoins.DoubleValue, value.CityFaction);
            _trackingController.DungeonController?.AddValueToDungeon(value.GainedFactionCoins.DoubleValue, ValueType.FactionPoints, value.CityFaction);
        }

        private TrackingNotification SetFactionPointsNotification(CityFaction cityFaction, double GainedFractionPoints, double BonusPremiumGainedFractionPoints)
        {
            return new TrackingNotification(DateTime.Now, new List<LineFragment>
            {
                new FactionPointsNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, cityFaction, GainedFractionPoints, 
                    BonusPremiumGainedFractionPoints, LanguageController.Translation("FACTION_POINTS"), LanguageController.Translation("GAINED")),
            }, NotificationType.Faction);
        }
    }
}