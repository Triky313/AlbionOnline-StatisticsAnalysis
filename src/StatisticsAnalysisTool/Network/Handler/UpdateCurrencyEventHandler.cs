using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Threading.Tasks;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Handler;

public class UpdateCurrencyEventHandler : EventPacketHandler<UpdateCurrencyEvent>
{
    private readonly TrackingController _trackingController;
    private readonly LiveStatsTracker _liveStatsTracker;

    public UpdateCurrencyEventHandler(TrackingController trackingController) : base((int) EventCodes.UpdateCurrency)
    {
        _trackingController = trackingController;
        _liveStatsTracker = _trackingController?.LiveStatsTracker;
    }

    protected override async Task OnActionAsync(UpdateCurrencyEvent value)
    {
        if (value.CityFaction != CityFaction.Unknown)
        {
            await _trackingController.AddNotificationAsync(SetFactionPointsNotification(value.CityFaction, value.GainedFactionCoins.DoubleValue, value.BonusPremiumGainedFractionFlagPoints.DoubleValue));
        }

        _liveStatsTracker.Add(ValueType.FactionPoints, value.GainedFactionCoins.DoubleValue, value.CityFaction);
        _trackingController.DungeonController?.AddValueToDungeon(value.GainedFactionCoins.DoubleValue, ValueType.FactionPoints, value.CityFaction);
        _trackingController.StatisticController?.AddValue(ValueType.FactionPoints, value.GainedFactionCoins.DoubleValue);
    }

    private TrackingNotification SetFactionPointsNotification(CityFaction cityFaction, double gainedFractionPoints, double bonusPremiumGainedFractionPoints)
    {
        return new TrackingNotification(DateTime.Now, new FactionPointsNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, cityFaction, gainedFractionPoints,
            bonusPremiumGainedFractionPoints, LanguageController.Translation("FACTION_POINTS"), LanguageController.Translation("GAINED")), LoggingFilterType.Faction);
    }
}