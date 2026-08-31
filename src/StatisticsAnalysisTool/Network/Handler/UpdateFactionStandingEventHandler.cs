using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Localization;
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
        await _trackingController.AddNotificationAsync(SetFactionStandingNotification(value.CityFaction, value.GainedFactionFlagPoints.DoubleValue, value.BonusPremiumGainedFractionFlagPoints.DoubleValue));
        _trackingController.DungeonController?.AddValueToDungeon(value.GainedFactionFlagPoints.DoubleValue, ValueType.FactionStanding, value.CityFaction);
        _trackingController.StatisticController?.AddValue(ValueType.FactionStanding, value.GainedFactionFlagPoints.DoubleValue, value.CityFaction);
    }

    private TrackingNotification SetFactionStandingNotification(CityFaction cityFaction, double gainedFractionPoints, double bonusPremiumGainedFractionPoints)
    {
        return new TrackingNotification(DateTime.Now, new FactionFlagPointsNotificationFragment(LocalizationController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, cityFaction, gainedFractionPoints,
            bonusPremiumGainedFractionPoints, LocalizationController.Translation("FACTION_STANDING"), LocalizationController.Translation("GAINED")), LoggingFilterType.Faction);
    }
}