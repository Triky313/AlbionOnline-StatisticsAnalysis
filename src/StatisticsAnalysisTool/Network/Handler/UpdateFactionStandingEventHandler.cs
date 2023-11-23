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
    private readonly IGameEventWrapper _gameEventWrapper;

    public UpdateFactionStandingEventHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.UpdateFactionStanding)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(UpdateFactionStandingEvent value)
    {
        await _gameEventWrapper.TrackingController.AddNotificationAsync(SetFactionFlagPointsNotification(value.CityFaction, value.GainedFactionFlagPoints.DoubleValue, value.BonusPremiumGainedFractionFlagPoints.DoubleValue));
        _gameEventWrapper.DungeonController?.AddValueToDungeon(value.GainedFactionFlagPoints.DoubleValue, ValueType.FactionFame, value.CityFaction);
        _gameEventWrapper.StatisticController?.AddValue(ValueType.FactionFame, value.GainedFactionFlagPoints.DoubleValue);
    }

    private TrackingNotification SetFactionFlagPointsNotification(CityFaction cityFaction, double gainedFractionPoints, double bonusPremiumGainedFractionPoints)
    {
        return new TrackingNotification(DateTime.Now, new FactionFlagPointsNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, cityFaction, gainedFractionPoints,
            bonusPremiumGainedFractionPoints, LanguageController.Translation("FACTION_FLAG_POINTS"), LanguageController.Translation("GAINED")), LoggingFilterType.Faction);
    }
}