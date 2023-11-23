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

public class UpdateCurrencyEventHandler : EventPacketHandler<UpdateCurrencyEvent>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public UpdateCurrencyEventHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.UpdateCurrency)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(UpdateCurrencyEvent value)
    {
        if (value.CityFaction != CityFaction.Unknown)
        {
            await _gameEventWrapper.TrackingController.AddNotificationAsync(SetFactionPointsNotification(value.CityFaction, value.GainedFactionCoins.DoubleValue, value.BonusPremiumGainedFractionFlagPoints.DoubleValue));
        }

        _gameEventWrapper.LiveStatsTracker.Add(ValueType.FactionPoints, value.GainedFactionCoins.DoubleValue, value.CityFaction);
        _gameEventWrapper.DungeonController?.AddValueToDungeon(value.GainedFactionCoins.DoubleValue, ValueType.FactionPoints, value.CityFaction);
        _gameEventWrapper.StatisticController?.AddValue(ValueType.FactionPoints, value.GainedFactionCoins.DoubleValue);
    }

    private TrackingNotification SetFactionPointsNotification(CityFaction cityFaction, double gainedFractionPoints, double bonusPremiumGainedFractionPoints)
    {
        return new TrackingNotification(DateTime.Now, new FactionPointsNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, cityFaction, gainedFractionPoints,
            bonusPremiumGainedFractionPoints, LanguageController.Translation("FACTION_POINTS"), LanguageController.Translation("GAINED")), LoggingFilterType.Faction);
    }
}