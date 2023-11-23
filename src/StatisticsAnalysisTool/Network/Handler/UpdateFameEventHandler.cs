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

public class UpdateFameEventHandler : EventPacketHandler<UpdateFameEvent>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public UpdateFameEventHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.UpdateFame)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(UpdateFameEvent value)
    {
        await _gameEventWrapper.TrackingController.AddNotificationAsync(SetPveFameNotification(value.TotalPlayerFame.DoubleValue, value.TotalGainedFame,
            value.ZoneFame.DoubleValue, value.PremiumFame, value.SatchelFame.DoubleValue, value.IsBonusFactorActive, value.BonusFactorInPercent));
        _gameEventWrapper.LiveStatsTracker.Add(ValueType.Fame, value.TotalGainedFame);
        _gameEventWrapper.DungeonController?.AddValueToDungeon(value.TotalGainedFame, ValueType.Fame);
        _gameEventWrapper.StatisticController?.AddValue(ValueType.Fame, value.TotalGainedFame);
    }

    private TrackingNotification SetPveFameNotification(double totalPlayerFame, double totalGainedFame, double zoneFame, double premiumFame,
        double satchelFame, bool isBonusFactorActive, double bonusFactorInPercent)
    {
        return new TrackingNotification(DateTime.Now, new FameNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, totalPlayerFame, totalGainedFame,
            LanguageController.Translation("FAME"), PvpPveType.Pve, zoneFame, premiumFame, satchelFame, isBonusFactorActive, bonusFactorInPercent,
            LanguageController.Translation("GAINED")), LoggingFilterType.Fame);
    }
}