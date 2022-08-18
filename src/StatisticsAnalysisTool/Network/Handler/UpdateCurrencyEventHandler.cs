using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Threading.Tasks;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateCurrencyEventHandler
    {
        private readonly TrackingController _trackingController;
        private readonly LiveStatsTracker _liveStatsTracker;

        public UpdateCurrencyEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
            _liveStatsTracker = _trackingController?.LiveStatsTracker;
        }

        public async Task OnActionAsync(UpdateCurrencyEvent value)
        {
            if (value.CityFaction != CityFaction.Unknown)
            {
                await _trackingController.AddNotificationAsync(SetFactionPointsNotification(value.CityFaction, value.GainedFactionCoins.DoubleValue, value.BonusPremiumGainedFractionFlagPoints.DoubleValue));
            }

            _liveStatsTracker.Add(ValueType.FactionPoints, value.GainedFactionCoins.DoubleValue, value.CityFaction);
            _trackingController.DungeonController?.AddValueToDungeon(value.GainedFactionCoins.DoubleValue, ValueType.FactionPoints, value.CityFaction);
            _trackingController.StatisticController?.AddValue(ValueType.FactionPoints, value.GainedFactionCoins.DoubleValue);
        }

        private TrackingNotification SetFactionPointsNotification(CityFaction cityFaction, double GainedFractionPoints, double BonusPremiumGainedFractionPoints)
        {
            return new TrackingNotification(DateTime.Now, new FactionPointsNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, cityFaction, GainedFractionPoints,
                BonusPremiumGainedFractionPoints, LanguageController.Translation("FACTION_POINTS"), LanguageController.Translation("GAINED")), NotificationType.Faction);
        }
    }
}