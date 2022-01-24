using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Threading.Tasks;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateFactionStandingEventHandler
    {
        private readonly TrackingController _trackingController;

        public UpdateFactionStandingEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(UpdateFactionStandingEvent value)
        {
            await _trackingController.AddNotificationAsync(SetFactionFlagPointsNotification(value.CityFaction, value.GainedFactionFlagPoints.DoubleValue, value.BonusPremiumGainedFractionFlagPoints.DoubleValue));
            _trackingController.DungeonController?.AddValueToDungeon(value.GainedFactionFlagPoints.DoubleValue, ValueType.FactionFame);
            _trackingController.StatisticController?.AddValue(ValueType.FactionFame, value.GainedFactionFlagPoints.DoubleValue);
        }

        private TrackingNotification SetFactionFlagPointsNotification(CityFaction cityFaction, double GainedFractionPoints, double BonusPremiumGainedFractionPoints)
        {
            return new TrackingNotification(DateTime.Now, new FactionFlagPointsNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, cityFaction, GainedFractionPoints,
                BonusPremiumGainedFractionPoints, LanguageController.Translation("FACTION_FLAG_POINTS"), LanguageController.Translation("GAINED")), NotificationType.Faction);
        }
    }
}