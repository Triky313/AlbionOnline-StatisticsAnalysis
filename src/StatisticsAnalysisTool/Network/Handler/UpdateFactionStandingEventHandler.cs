using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Threading.Tasks;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Handler;

public class UpdateFactionStandingEventHandler : EventPacketHandler<UpdateFactionStandingEvent>
{
    private readonly TrackingController _trackingController;

    public UpdateFactionStandingEventHandler(TrackingController trackingController) : base((int) EventCodes.UpdateFactionStanding)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(UpdateFactionStandingEvent value)
    {
        await _trackingController.AddNotificationAsync(SetFactionFlagPointsNotification(value.CityFaction, value.GainedFactionFlagPoints.DoubleValue, value.BonusPremiumGainedFractionFlagPoints.DoubleValue));
        _trackingController.DungeonController?.AddValueToDungeon(value.GainedFactionFlagPoints.DoubleValue, ValueType.FactionFame, value.CityFaction);
        _trackingController.StatisticController?.AddValue(ValueType.FactionFame, value.GainedFactionFlagPoints.DoubleValue);
    }

    private TrackingNotification SetFactionFlagPointsNotification(CityFaction cityFaction, double gainedFractionPoints, double bonusPremiumGainedFractionPoints)
    {
        return new TrackingNotification(DateTime.Now, new FactionFlagPointsNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, cityFaction, gainedFractionPoints,
            bonusPremiumGainedFractionPoints, LanguageController.Translation("FACTION_FLAG_POINTS"), LanguageController.Translation("GAINED")), LoggingFilterType.Faction);
    }
}